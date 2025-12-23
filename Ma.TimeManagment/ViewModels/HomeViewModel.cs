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
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Policy;

namespace Ma.TimeManagement.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly ILogger<HomeViewModel> logger;
        private readonly ITimeManagementService timeManagementService;
        private readonly IDataService dataService;
        private readonly INavigationService _navigationService;
     
        public HomeViewModel(ILogger<HomeViewModel> logger,ITimeManagementService timeManagementService,IDataService dataService,INavigationService navigationService, IStatusService statusService)
        {
            this.logger = logger;
            this.timeManagementService = timeManagementService;
            this.dataService = dataService;
            _navigationService = navigationService;
            StatusService = statusService;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.IsEnabled = true;
            Task.Factory.StartNew(async () =>
            {
                Tasks = [.. await timeManagementService.GetTasksAsync(new CancellationTokenSource().Token)];
                OnPropertyChanged(nameof(Tasks));
            });

            StrongReferenceMessenger.Default.Register<StatusActionModel, string>(this, EnStatusAction.RefreshTasks.ToString(), async (r, m) =>
            {
                await RefreshTasks(new CancellationTokenSource().Token);
            });
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

        //[ObservableProperty]
        //private bool _isStopEnabled;

        private DispatcherTimer _timer;
        private WorkCalendarItem _activeTaskCalander;
        public WorkCalendarItem ActiveTaskCalander { set
            {
                _activeTaskCalander = value;
                WorkItemID = ActiveTaskCalander?.WorkItem?.Id.ToString() ?? "XXXX";
                WorkItemTitle = ActiveTaskCalander?.WorkItem?.Title ?? "Task did not started yet.";
            }
            get => _activeTaskCalander; }

        [ObservableProperty]
        private string _workItemID;
        [ObservableProperty]
        private string _workItemTitle;

        //private TimeSpan _elapsedTime = TimeSpan.Zero;
        //private TimeSpan _savedTime = TimeSpan.Zero;
        //private DateTime _lastSaveTime = DateTime.Now;
        //private bool _isPausedDueToIdle = false;

        //private const int AutoSaveIntervalMinutes = 15;
        //private const int IdleThresholdSeconds = 300;

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
            //MoveToTopCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task Start(CancellationToken cancellationToken)
        {
            //ResetTimerState();
            //_timer.Start();
            await timeManagementService.StartNewTaskAsync(SelectedTask,cancellationToken);
            Status = $"Tracking Task #{SelectedTask.Id}: {SelectedTask.Title}";
            UpdateButtonStates();
            await RefreshTasks(cancellationToken);
        }
        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task ShowTask(CancellationToken cancellationToken)
        {
            //ResetTimerState();
            //_timer.Start();
            var url_splits = SelectedTask.Url.Split(['/']);
            var url = $"https://{url_splits[2]}/{url_splits[3]}/{SelectedTask.ProjectName}/_workitems/edit/{SelectedTask.Id}";
            
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private bool CanStart() => SelectedTask != null && !timeManagementService.HaveActiveWork();

        //[RelayCommand(CanExecute = nameof(CanResume))]
        //private void Resume()
        //{
        //    _isPausedDueToIdle = false;
        //    _timer.Start();
        //    Status = $"Resumed tracking Task #{SelectedTask.Id}";
        //    StatusService.SendStatus(BalloonIcon.Info, "Resumed", "Timer resumed after idle pause.");
        //    UpdateButtonStates();
        //}

        //private bool CanResume() => _isPausedDueToIdle;

        [RelayCommand(CanExecute = nameof(CanStop))]
        private async Task Stop(CancellationToken cancellationToken)
        {
            //_timer.Stop();
            //await SaveIncrementAsync();
            await timeManagementService.EndActiveTaskAsync(cancellationToken);
            Status = $"Stopped and saved for Task #{SelectedTask?.Id}";
            //ResetTimerState();
            UpdateButtonStates();
            await RefreshTasks(cancellationToken);
        }

        private bool CanStop() => timeManagementService.HaveActiveWork();

        //[RelayCommand(CanExecute = nameof(CanMoveToTop))]
        //private void MoveToTop()
        //{
        //    Tasks.Remove(SelectedTask);
        //    Tasks.Insert(0, SelectedTask);
        //    Start();
        //}

        //private bool CanMoveToTop() => SelectedTask != null;

        [RelayCommand]
        private async Task RefreshTasks(CancellationToken cancellationToken)
        {
            try
            {

                Tasks = [.. await timeManagementService.GetTasksAsync(cancellationToken)];
                OnPropertyChanged(nameof(Tasks));
                Status = "Status: Tasks fetched successfully!";
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
            }
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            WorkCalendarItem? item =  await timeManagementService.GetActiveCalendarItemAsync(cts.Token);
            double duration = 0;
            if (item != null)
            {
                if (ActiveTaskCalander == null || item.Id != ActiveTaskCalander.Id)
                {
                    ActiveTaskCalander = await dataService.GetWorkCalendarItemWithWorkItemAsync(item.Id,cts.Token);
                    Status = $"{ActiveTaskCalander.WorkItem?.Id}#:STARTED , {ActiveTaskCalander.WorkItem?.Title}";
                }
                duration = (DateTime.Now - ActiveTaskCalander.StartTime).TotalSeconds;
            }
            else
            {
                ActiveTaskCalander = null;
                Status = "";
            }
            var _elapsedTime = TimeSpan.FromSeconds(duration);
            TimerText = _elapsedTime.ToString(@"hh\:mm\:ss");

            //StatusService.SendStatus(BalloonIcon.Info, "Tasks", $"Task #{SelectedTask.Id}: {TimerText}");

            //if ((DateTime.Now - _lastSaveTime).TotalMinutes >= AutoSaveIntervalMinutes)
            //{
            //    SaveIncrementAsync();
            //}

            //var idleSeconds = GetIdleTimeSeconds();
            //if (idleSeconds > IdleThresholdSeconds && !_isPausedDueToIdle)
            //{
            //    //_isPausedDueToIdle = true;
            //    //_timer.Stop();
            //    //SaveIncrementAsync();
            //    //Status = "Timer paused due to inactivity.";
            //    //notifyIcon.ShowBalloonTip("Idle Detected", "Timer paused after 5 min inactivity. Resume when ready.", BalloonIcon.Warning);
            //    //UpdateButtonStates();
            //}
        }

        //private async Task SaveIncrementAsync()
        //{
        //    var increment = _elapsedTime - _savedTime;
        //    if (increment.TotalHours <= 0) return;
        //    try
        //    {
        //        //todo: Sync Calender Item to stop Time
        //        _savedTime = _elapsedTime;
        //        _lastSaveTime = DateTime.Now;
        //        Status = $"Auto-saved {increment.TotalHours:F2} hours to Task #{SelectedTask.Id}";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, GetType().Name, []);
        //        Status = $"Error saving: {ex.Message}";
        //    }
        //}

        private void ResetTimerState()
        {
            //_elapsedTime = TimeSpan.Zero;
            //_savedTime = TimeSpan.Zero;
            //_lastSaveTime = DateTime.Now;
            //_isPausedDueToIdle = false;
            TimerText = "00:00:00";
        }

        private void UpdateButtonStates()
        {
            //IsResumeEnabled = _isPausedDueToIdle;
            //IsStopEnabled = _timer.IsEnabled;
            StartCommand.NotifyCanExecuteChanged();
            //MoveToTopCommand.NotifyCanExecuteChanged();
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