using Hardcodet.Wpf.TaskbarNotification;
using Ma.TimeManagement.Services;
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
using CommunityToolkit.Mvvm.Messaging;
using Ma.TimeManagement.Models;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace Ma.TimeManagement.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly AzureDevOpsService azureDevOpsService;

        public HomeViewModel(INavigationService navigationService, AzureDevOpsService azureDevOpsService, IStatusService statusService)
        {
            _navigationService = navigationService;
            this.azureDevOpsService = azureDevOpsService;
            StatusService = statusService;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            Tasks = [.. azureDevOpsService.WorkItems];
            Task.Factory.StartNew(async () =>
            {
                Tasks = [.. await azureDevOpsService.GetTasks()];
                OnPropertyChanged(nameof(Tasks));
            });

            //StatusService.RegisterRefreshTasks(this,async ()=> { await RefreshTasks(); });
            //WeakReferenceMessenger.Default.Register<HomeViewModel, string>(this, EnStatusAction.RefreshTasks.ToString(), async (r, m) =>
            //{
            //    await RefreshTasks();
            //});
        }

        [RelayCommand]
        private void NavigateToSettings() => _navigationService.NavigateTo<SettingsViewModel>();

        public ObservableCollection<WorkItem> Tasks { get; set; } = new ObservableCollection<WorkItem>();
        public IStatusService StatusService { get; }

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
            Status = $"Tracking Task #{SelectedTask.Id}: {SelectedTask.Title}";
            UpdateButtonStates();
        }

        private bool CanStart() => SelectedTask != null && !_timer.IsEnabled && !_isPausedDueToIdle;

        [RelayCommand(CanExecute = nameof(CanResume))]
        private void Resume()
        {
            _isPausedDueToIdle = false;
            _timer.Start();
            Status = $"Resumed tracking Task #{SelectedTask.Id}";
            StatusService.SendStatus(BalloonIcon.Info, "Resumed", "Timer resumed after idle pause.");
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
        private async Task RefreshTasks()
        {
            try
            {


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

            //StatusService.SendStatus(BalloonIcon.Info, "Tasks", $"Task #{SelectedTask.Id}: {TimerText}");

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
                var currentTask = await azureDevOpsService.GetWorkItemAsync(SelectedTask.Id);
                var currentCompleted = currentTask.CompletedWork;
                var currentRemainingWork = currentTask.RemainingWork;
                var TotalHours = Math.Round((currentCompleted + increment.TotalHours) * 4, MidpointRounding.ToPositiveInfinity) / 4;
                var remainingWork = (currentRemainingWork - TotalHours);
                remainingWork = Math.Round(remainingWork * 4, MidpointRounding.ToPositiveInfinity) / 4;
               

                var patch = new JsonPatchDocument();
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Scheduling.CompletedWork",
                    Value =TotalHours
                });
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Scheduling.RemainingWork",
                    Value = remainingWork < 0 ? 0 : remainingWork
                });
                await azureDevOpsService.UpdateWorkItemAsync(patch, SelectedTask.Id);

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
            StopCommand.NotifyCanExecuteChanged();
        }

        private static uint GetIdleTimeSeconds()
        {
            var info = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
            GetLastInputInfo(ref info);
            return ((uint)Environment.TickCount - info.dwTime) / 1000;
        }
    }
}