using Hardcodet.Wpf.TaskbarNotification;
using Ma.TimeManagement.Services;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.Services.WebApi.Patch;

namespace Ma.TimeManagement.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<WorkItem> Tasks { get; } = new ObservableCollection<WorkItem>();

        [ObservableProperty]
        private WorkItem _selectedTask;

        [ObservableProperty]
        private string _timerText = "00:00:00";

        [ObservableProperty]
        private string _status = "Status: Ready";

        [ObservableProperty]
        private bool _isResumeEnabled;

        [ObservableProperty]
        private bool _isStopEnabled;

        private DispatcherTimer _timer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private TimeSpan _savedTime = TimeSpan.Zero;
        private DateTime _lastSaveTime = DateTime.Now;
        private bool _isPausedDueToIdle = false;

        private const int AutoSaveIntervalMinutes = 15;
        private const int IdleThresholdSeconds = 300;

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public MainViewModel()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
        }

        partial void OnSelectedTaskChanged(WorkItem value)
        {
            StartCommand.NotifyCanExecuteChanged();
            MoveToTopCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private void Start()
        {
            ResetTimerState();
            _timer.Start();
            Status = $"Tracking Task #{SelectedTask.Id}: {SelectedTask.Fields["System.Title"]}";
            UpdateButtonStates();
        }

        private bool CanStart() => SelectedTask != null && !_timer.IsEnabled && !_isPausedDueToIdle;

        [RelayCommand(CanExecute = nameof(CanResume))]
        private void Resume()
        {
            _isPausedDueToIdle = false;
            _timer.Start();
            Status = $"Resumed tracking Task #{SelectedTask.Id}";
            (Application.Current as App)._notifyIcon.ShowBalloonTip("Resumed", "Timer resumed after idle pause.", BalloonIcon.Info);
            UpdateButtonStates();
        }

        private bool CanResume() => _isPausedDueToIdle;

        [RelayCommand(CanExecute = nameof(CanStop))]
        private async Task Stop()
        {
            _timer.Stop();
            await SaveIncrementAsync();
            Status = $"Stopped and saved for Task #{SelectedTask.Id}";
            ResetTimerState();
            UpdateButtonStates();
            await RefreshTasks();
        }

        private bool CanStop() => _timer.IsEnabled;

        [RelayCommand(CanExecute = nameof(CanMoveToTop))]
        private void MoveToTop()
        {
            Tasks.Remove(SelectedTask);
            Tasks.Insert(0, SelectedTask);
            Start();
        }

        private bool CanMoveToTop() => SelectedTask != null;

        [RelayCommand]
        private void Minimize()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        [RelayCommand]
        private async Task RefreshTasks()
        {
            try
            {
                var wiql = new Wiql
                {
                    Query = $"SELECT [System.Id], [System.Title], [System.State], [Microsoft.VSTS.Scheduling.CompletedWork] " +
                            $"FROM workitems WHERE [System.TeamProject] = '{AzureDevOpsService.Instance.Project}' AND [System.WorkItemType] = 'Task' " +
                            $"AND [System.AssignedTo] = @me AND [System.State] <> 'Closed'"
                };
                var queryResult = await AzureDevOpsService.Instance.WitClient.QueryByWiqlAsync(wiql);

                Tasks.Clear();
                foreach (var workItemRef in queryResult.WorkItems)
                {
                    var task = await AzureDevOpsService.Instance.WitClient.GetWorkItemAsync(workItemRef.Id, expand: WorkItemExpand.Fields);
                    Tasks.Add(task);
                }

                Status = "Status: Tasks fetched successfully!";
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            TimerText = _elapsedTime.ToString(@"hh\:mm\:ss");

            var notifyIcon = (Application.Current as App)._notifyIcon;
            notifyIcon.ToolTipText = $"Task #{SelectedTask.Id}: {TimerText}";

            if ((DateTime.Now - _lastSaveTime).TotalMinutes >= AutoSaveIntervalMinutes)
            {
                SaveIncrementAsync();
            }

            var idleSeconds = GetIdleTimeSeconds();
            if (idleSeconds > IdleThresholdSeconds && !_isPausedDueToIdle)
            {
                //_isPausedDueToIdle = true;
                //_timer.Stop();
                //SaveIncrementAsync();
                //Status = "Timer paused due to inactivity.";
                //notifyIcon.ShowBalloonTip("Idle Detected", "Timer paused after 5 min inactivity. Resume when ready.", BalloonIcon.Warning);
                //UpdateButtonStates();
            }
        }

        private async Task SaveIncrementAsync()
        {
            var increment = _elapsedTime - _savedTime;
            if (increment.TotalHours <= 0) return;

            try
            {
                var currentTask = await AzureDevOpsService.Instance.WitClient.GetWorkItemAsync(SelectedTask.Id??0);
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

                await AzureDevOpsService.Instance.WitClient.UpdateWorkItemAsync(patch, SelectedTask.Id ?? 0);

                _savedTime = _elapsedTime;
                _lastSaveTime = DateTime.Now;
                Status = $"Auto-saved {increment.TotalHours:F2} hours to Task #{SelectedTask.Id}";
            }
            catch (Exception ex)
            {
                Status = $"Error saving: {ex.Message}";
            }
        }

        private void ResetTimerState()
        {
            _elapsedTime = TimeSpan.Zero;
            _savedTime = TimeSpan.Zero;
            _lastSaveTime = DateTime.Now;
            _isPausedDueToIdle = false;
            TimerText = "00:00:00";
        }

        private void UpdateButtonStates()
        {
            IsResumeEnabled = _isPausedDueToIdle;
            IsStopEnabled = _timer.IsEnabled;
            StartCommand.NotifyCanExecuteChanged();
            MoveToTopCommand.NotifyCanExecuteChanged();
        }

        private static uint GetIdleTimeSeconds()
        {
            var info = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
            GetLastInputInfo(ref info);
            return ((uint)Environment.TickCount - info.dwTime) / 1000;
        }
    }
}