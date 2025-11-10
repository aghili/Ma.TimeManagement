using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Ma.TimeManagement
{
    public partial class MainWindow : Window
    {
        private string _baseUrl;
        private string _authHeader;
        private dynamic _selectedTask;
        private DispatcherTimer _timer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private TimeSpan _savedTime = TimeSpan.Zero;
        private DateTime _lastSaveTime;
        private bool _isPausedDueToIdle = false;
        private HttpClient _httpClient = new HttpClient();
        private ObservableCollection<dynamic> _tasks = new ObservableCollection<dynamic>(); // For reordering

        private const int AutoSaveIntervalMinutes = 15; // Auto-save every 15 min
        private const int IdleThresholdSeconds = 300; // Pause after 5 min idle

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            TasksListView.ItemsSource = _tasks;

            // Handle window minimize to tray
            StateChanged += (s, e) => { if (WindowState == WindowState.Minimized) Hide(); };
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var server = ServerUrlTextBox.Text.Trim();
                var collection = CollectionTextBox.Text.Trim();
                var project = ProjectTextBox.Text.Trim();
                var pat = PatPasswordBox.Password.Trim();

                _baseUrl = $"{server}/{collection}/{project}/_apis";
                _authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));

                // Fetch assigned Tasks (WIQL query)
                var wiql = new { query = "SELECT [System.Id], [System.Title], [System.State], [Microsoft.VSTS.Scheduling.CompletedWork] FROM workitems WHERE [System.TeamProject] = @project AND [System.WorkItemType] = 'Task' AND [System.AssignedTo] = @me AND [System.State] <> 'Closed'" };
                var response = await PostAsync("/wit/wiql?api-version=7.1", wiql);
                var workItems = JsonSerializer.Deserialize<Dictionary<string, object>>(response)["workItems"];

                // Get details for each
                _tasks.Clear();
                foreach (JsonElement idObj in (JsonElement)workItems)
                {
                    var id = idObj.GetProperty("id").GetInt32();
                    var task = JsonSerializer.Deserialize<dynamic>(await GetAsync($"/wit/workitems/{id}?api-version=7.1"));
                    _tasks.Add(task);
                }

                StatusTextBlock.Text = "Status: Tasks fetched successfully!";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        private async void CreateWorkItemButton_Click(object sender, RoutedEventArgs e)
        {
            var type = WorkItemTypeComboBox.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "Task";
            var title = NewTitleTextBox.Text.Trim();
            var description = NewDescriptionTextBox.Text.Trim();
            var parentId = ParentIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Title is required.");
                return;
            }

            try
            {
                var patch = new List<object>
                {
                    new { op = "add", path = "/fields/System.Title", value = title }
                };
                if (!string.IsNullOrEmpty(description))
                {
                    patch.Add(new { op = "add", path = "/fields/System.Description", value = description });
                }
                if (!string.IsNullOrEmpty(parentId))
                {
                    patch.Add(new
                    {
                        op = "add",
                        path = "/relations/-",
                        value = new
                        {
                            rel = "System.LinkTypes.Hierarchy-Reverse",
                            url = $"{_baseUrl}/wit/workitems/{parentId}"
                        }
                    });
                }

                var url = $"/wit/workitems/${type}?api-version=7.1";
                var result = await PostPatchAsync(url, patch.ToArray(), "Post"); // Use Post for create

                var created = JsonSerializer.Deserialize<dynamic>(result);
                StatusTextBlock.Text = $"Created {type} ID: {created.id}";

                // Clear inputs and refresh list
                NewTitleTextBox.Text = "";
                NewDescriptionTextBox.Text = "";
                ParentIdTextBox.Text = "";
                ConnectButton_Click(null, null);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error creating: {ex.Message}";
            }
        }

        private void MoveToTopButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListView.SelectedItem is dynamic selected)
            {
                _tasks.Remove(selected);
                _tasks.Insert(0, selected);
                TasksListView.SelectedItem = selected;
                StartButton_Click(null, null); // Auto-start timer
            }
            else
            {
                MessageBox.Show("Select a task first.");
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedTask = TasksListView.SelectedItem as dynamic;
            if (_selectedTask == null) { MessageBox.Show("Select a Task first."); return; }

            ResetTimerState();
            _timer.Start();
            UpdateButtonStates(isRunning: true);
            StatusTextBlock.Text = $"Tracking Task #{_selectedTask.id}: {_selectedTask.fields["System.Title"]}";
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPausedDueToIdle)
            {
                _isPausedDueToIdle = false;
                _timer.Start();
                UpdateButtonStates(isRunning: true);
                StatusTextBlock.Text = $"Resumed tracking Task #{_selectedTask.id}";
                (Application.Current as App)._notifyIcon.ShowBalloonTip(5000, "Resumed", "Timer resumed after idle pause.", System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await StopAndSaveAsync(manualStop: true);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            TimerTextBlock.Text = _elapsedTime.ToString(@"hh\:mm\:ss");

            var notifyIcon = (Application.Current as App)._notifyIcon;
            notifyIcon.ToolTipText = $"Task #{_selectedTask.id}: {TimerTextBlock.Text}";

            // Auto-save check
            if ((DateTime.Now - _lastSaveTime).TotalMinutes >= AutoSaveIntervalMinutes)
            {
                _ = SaveIncrementAsync(); // Fire and forget
            }

            // Idle detection
            var idleSeconds = GetIdleTimeSeconds();
            if (idleSeconds > IdleThresholdSeconds && !_isPausedDueToIdle)
            {
                _isPausedDueToIdle = true;
                _timer.Stop();
                UpdateButtonStates(isRunning: false);
                _ = SaveIncrementAsync();
                StatusTextBlock.Text = "Timer paused due to inactivity.";
                notifyIcon.ShowBalloonTip(5000, "Idle Detected", "Timer paused after 5 min inactivity. Resume when ready.", System.Windows.Forms.ToolTipIcon.Warning);
            }
        }

        private async Task StopAndSaveAsync(bool manualStop)
        {
            _timer.Stop();
            await SaveIncrementAsync();

            StatusTextBlock.Text = $"Stopped and saved for Task #{_selectedTask.id}";
            ResetTimerState();
            UpdateButtonStates(isRunning: false);

            if (manualStop) ConnectButton_Click(null, null);
        }

        private async Task SaveIncrementAsync()
        {
            var increment = _elapsedTime - _savedTime;
            if (increment.TotalHours <= 0) return;

            try
            {
                var currentTask = JsonSerializer.Deserialize<dynamic>(await GetAsync($"/wit/workitems/{_selectedTask.id}?api-version=7.1"));
                var currentCompleted = currentTask.fields.ContainsKey("Microsoft.VSTS.Scheduling.CompletedWork")
                    ? (double)currentTask.fields["Microsoft.VSTS.Scheduling.CompletedWork"]
                    : 0.0;

                var patch = new[]
                {
                    new { op = "add", path = "/fields/Microsoft.VSTS.Scheduling.CompletedWork", value = currentCompleted + increment.TotalHours }
                };
                await PostPatchAsync($"/wit/workitems/{_selectedTask.id}?api-version=7.1", patch, "Patch");

                _savedTime = _elapsedTime;
                _lastSaveTime = DateTime.Now;
                StatusTextBlock.Text = $"Auto-saved {increment.TotalHours:F2} hours to Task #{_selectedTask.id}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error saving: {ex.Message}";
            }
        }

        private void ResetTimerState()
        {
            _elapsedTime = TimeSpan.Zero;
            _savedTime = TimeSpan.Zero;
            _lastSaveTime = DateTime.Now;
            _isPausedDueToIdle = false;
            TimerTextBlock.Text = "00:00:00";
        }

        private void UpdateButtonStates(bool isRunning)
        {
            StartButton.IsEnabled = !isRunning && !_isPausedDueToIdle;
            ResumeButton.IsEnabled = _isPausedDueToIdle;
            StopButton.IsEnabled = isRunning;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private static uint GetIdleTimeSeconds()
        {
            var info = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
            GetLastInputInfo(ref info);
            return ((uint)Environment.TickCount - info.dwTime) / 1000;
        }

        private async Task<string> GetAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _baseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> PostAsync(string path, object body)
        {
            return await PostPatchAsync(path, body, "Post");
        }

        private async Task<string> PatchAsync(string path, object body)
        {
            return await PostPatchAsync(path, body, "Patch");
        }

        private async Task<string> PostPatchAsync(string path, object body, string method)
        {
            var request = new HttpRequestMessage(method == "Patch" ? HttpMethod.Patch : HttpMethod.Post, _baseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json-patch+json");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}