using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Ma.TimeManagement
{
    public partial class MainWindow : Window
    {
        private WorkItemTrackingHttpClient _witClient;
        private string _project;
        private WorkItem _selectedTask;
        private DispatcherTimer _timer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private TimeSpan _savedTime = TimeSpan.Zero;
        private DateTime _lastSaveTime;
        private bool _isPausedDueToIdle = false;
        private ObservableCollection<WorkItem> _tasks = new ObservableCollection<WorkItem>(); // For reordering

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
                _project = ProjectTextBox.Text.Trim();
                var pat = PatPasswordBox.Password.Trim();

                var uri = new Uri($"{server}/{collection}");
                var credentials = new VssBasicCredential(string.Empty, pat);
                var connection = new VssConnection(uri, credentials);
                _witClient = connection.GetClient<WorkItemTrackingHttpClient>();

                // Fetch assigned Tasks (WIQL query)
                var wiql = new Wiql
                {
                    Query = $"SELECT [System.Id], [System.Title], [System.State], [Microsoft.VSTS.Scheduling.CompletedWork] " +
                            $"FROM workitems WHERE [System.TeamProject] = '{_project}' AND [System.WorkItemType] = 'Task' " +
                            $"AND [System.AssignedTo] = @me AND [System.State] <> 'Closed'"
                };
                var queryResult = await _witClient.QueryByWiqlAsync(wiql);

                // Get details for each
                _tasks.Clear();
                foreach (var workItemRef in queryResult.WorkItems)
                {
                    var task = await _witClient.GetWorkItemAsync(workItemRef.Id, expand: WorkItemExpand.Fields);
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
            var parentIdStr = ParentIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Title is required.");
                return;
            }

            try
            {
                var patch = new JsonPatchDocument();
                patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = title });

                if (!string.IsNullOrEmpty(description))
                {
                    patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Description", Value = description });
                }

                if (!string.IsNullOrEmpty(parentIdStr) && int.TryParse(parentIdStr, out int parentId))
                {
                    var parent = await _witClient.GetWorkItemAsync(parentId);
                    patch.Add(new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/relations/-",
                        Value = new WorkItemRelation
                        {
                            Rel = "System.LinkTypes.Hierarchy-Reverse",
                            Url = parent.Url
                        }
                    });
                }

                var created = await _witClient.CreateWorkItemAsync(patch, _project, type);
                StatusTextBlock.Text = $"Created {type} ID: {created.Id}";

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
            if (TasksListView.SelectedItem is WorkItem selected)
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
            if (TasksListView.SelectedItem is WorkItem selectedTask)
            {
                _selectedTask = selectedTask;
            }
            else
            {
                MessageBox.Show("Select a Task first.");
                return;
            }

            ResetTimerState();
            _timer.Start();
            UpdateButtonStates(isRunning: true);
            StatusTextBlock.Text = $"Tracking Task #{_selectedTask.Id}: {_selectedTask.Fields["System.Title"]}";
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPausedDueToIdle)
            {
                _isPausedDueToIdle = false;
                _timer.Start();
                UpdateButtonStates(isRunning: true);
                StatusTextBlock.Text = $"Resumed tracking Task #{_selectedTask.Id}";
                (Application.Current as App)._notifyIcon.ShowBalloonTip("Resumed", "Timer resumed after idle pause.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
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
            notifyIcon.ToolTipText = $"Task #{_selectedTask.Id}: {TimerTextBlock.Text}";

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
                notifyIcon.ShowBalloonTip("Idle Detected", "Timer paused after 5 min inactivity. Resume when ready.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }
        }

        private async Task StopAndSaveAsync(bool manualStop)
        {
            _timer.Stop();
            await SaveIncrementAsync();

            StatusTextBlock.Text = $"Stopped and saved for Task #{_selectedTask.Id}";
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
                var currentTask = await _witClient.GetWorkItemAsync(_selectedTask.Id ?? 0);
                var currentCompleted = currentTask.Fields.ContainsKey("Microsoft.VSTS.Scheduling.CompletedWork")
                    ? (double)currentTask.Fields["Microsoft.VSTS.Scheduling.CompletedWork"]
                    : 0.0;

                var patch = new JsonPatchDocument();
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Scheduling.CompletedWork",
                    Value = currentCompleted + increment.TotalHours
                });

                await _witClient.UpdateWorkItemAsync(patch, _selectedTask.Id ?? 0);

                _savedTime = _elapsedTime;
                _lastSaveTime = DateTime.Now;
                StatusTextBlock.Text = $"Auto-saved {increment.TotalHours:F2} hours to Task #{_selectedTask.Id}";
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
    }
}