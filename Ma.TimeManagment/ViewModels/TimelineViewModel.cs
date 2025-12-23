using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Ma.TimeManagement.ViewModels
{
    public partial class TimelineViewModel :ObservableObject,ITimeLineViewModel
    {
        public const int StartHour = 00;
        public const int EndHour = 23;
        public const int TotalHours = EndHour - StartHour;
        public const int TotalMinutes = TotalHours * 60;

        [ObservableProperty]
        private ObservableCollection<TimelineItem> _items = new();
        [ObservableProperty] private double _zoom = 80; // pixels per hour

        public DateTime DayStart => DateTime.Today.AddHours(StartHour);
        public DateTime DayEnd => DateTime.Today.AddHours(EndHour);

        public double TimelineWidth
        {
            get
            {
                return
                     TotalMinutes * (Zoom / 60.0);
            }
        }

       

        public TimelineViewModel(ILogger<ITimeLineViewModel> logger,ITimeManagementService timeManagementService,IDataService dataService, IDialogService dialogService,IStatusService statusService)
        {
            this.logger = logger;
            this.timeManagementService = timeManagementService;
            this.dataService = dataService;
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.statusService = statusService;
            
            StartTaskCommand = new RelayCommand(async () => await ExecuteStartTaskAsync(new CancellationTokenSource().Token));
            InsertTaskCommand = new RelayCommand(async () => await InsertStartTaskAsync(new CancellationTokenSource().Token));
            EndTaskCommand = new RelayCommand(async () => await ExecuteEndTaskAsync(new CancellationTokenSource().Token));

            _items.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasItems));
            
            ZoomInCommand = new RelayCommand(() => Zoom = Math.Min(Zoom * 1.4, 400));
            ZoomOutCommand = new RelayCommand(() => Zoom = Math.Max(Zoom / 1.4, 20));
            ResetCommand = new RelayCommand(() => Zoom = 80);

            // Background refresh (just like HomeViewModel)

            StrongReferenceMessenger.Default.Register<WorkCalendarItem, string>(this, EnStatusAction.RefreshItem.ToString(), async (r, m) =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                await RefreshItem(m,cts.Token);
            });
            RefreshTasks();
        }

        private async Task RefreshItem(WorkCalendarItem m,CancellationToken cancellationToken)
        {
            await AddTaskToTimelineAsync(m,cancellationToken);
        }

        private void RefreshTasks()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    var refreshed = await dataService.GetWorkCalendarItemsDailyAsync(DateTime.Now,cts.Token);
                    Application.Current?.Dispatcher?.Invoke(async () =>
                    {
                        await AddTasksToTimeline(refreshed,cts.Token);
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, GetType().Name, []);
                    statusService.SendStatus(ex);
                }
            });
        }

        public IRelayCommand ZoomInCommand { get; }
        public IRelayCommand ZoomOutCommand { get; }
        public IRelayCommand ResetCommand { get; }
        public IRelayCommand StartTaskCommand { get; }
        public IRelayCommand EndTaskCommand { get; }
        public IRelayCommand InsertTaskCommand { get; }

        private readonly ILogger<ITimeLineViewModel> logger;
        private readonly ITimeManagementService timeManagementService;
        private readonly IDataService dataService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService statusService;

        public bool HasItems => Items.Count > 0;


        public event EventHandler<WorkItem> TaskStarted;

        private async Task ExecuteEndTaskAsync(CancellationToken cancellationToken)
        {
            await timeManagementService.EndActiveTaskAsync(cancellationToken);
        }

        private async Task ExecuteStartTaskAsync(CancellationToken cancellationToken)
        {
            var vm = new TaskSelectionViewModel();

            // Use cached tasks
            IEnumerable<WorkItem> tasks = await timeManagementService.GetTasksAsync(cancellationToken);
            if (tasks != null)
            {
                foreach (var w in tasks.OrderBy(x => x.ProjectName).ThenBy(x => x.Title))
                    vm.AvailableTasks.Add(w);
            }

            //// Background refresh (just like HomeViewModel)
            //Task.Factory.StartNew(async () =>
            //{
            //    try
            //    {
            //        var refreshed = await _azureDevOpsService.GetTasks();
            //        Application.Current?.Dispatcher?.Invoke(() =>
            //        {
            //            vm.AvailableTasks.Clear();
            //            foreach (var w in refreshed.OrderBy(x => x.ProjectName).ThenBy(x => x.Title))
            //                vm.AvailableTasks.Add(w);
            //        });
            //    }
            //    catch
            //    {
            //        // Ignore refresh errors; keep seed
            //    }
            //});

            var result = _dialogService.ShowDialog(vm);
            if (result == true && vm.SelectedTask != null)
            {
                await timeManagementService.StartNewTaskAsync(vm.SelectedTask, cancellationToken);
            }
        }

        private async Task InsertStartTaskAsync(CancellationToken cancellationToken)
        {
            var vm = new TaskSelectionViewModel();

            // Use cached tasks
            IEnumerable<WorkItem> tasks = await timeManagementService.GetTasksAsync(cancellationToken);
            if (tasks != null)
            {
                foreach (var w in tasks.OrderBy(x => x.ProjectName).ThenBy(x => x.Title))
                    vm.AvailableTasks.Add(w);
            }

            //// Background refresh (just like HomeViewModel)
            //Task.Factory.StartNew(async () =>
            //{
            //    try
            //    {
            //        var refreshed = await _azureDevOpsService.GetTasks();
            //        var CalenderFreeItems = await dataService.GetWorkCalendarFreeItemsDailyAsync();
            //        Application.Current?.Dispatcher?.Invoke(() =>
            //        {
            //            vm.AvailableTasks.Clear();
            //            foreach (var w in refreshed.OrderBy(x => x.ProjectName).ThenBy(x => x.Title))
            //                vm.AvailableTasks.Add(w);
            //            foreach(var w in CalenderFreeItems)
            //            vm.WorkCalendarItems.Add( w);
            //        });
            //    }
            //    catch
            //    {
            //        // Ignore refresh errors; keep seed
            //    }
            //});

            var result = _dialogService.ShowDialog(vm);
            if (result == true && vm.SelectedTask != null && vm.SelectedWorkCalendarItem != null)
            {
                await timeManagementService.InsertNewTaskAsync(vm.SelectedTask,vm.SelectedWorkCalendarItem.StartTime,vm.Duration,cancellationToken);
            }
        }

        //public async Task AddTaskToTimelineAsync(WorkItem task)
        //{
        //    WorkCalendarItem item = new()
        //    {
        //        Title = task.Title,
        //        StartTime = DateTime.Now,
        //        DurationHour = .25
        //    };
        //    await dataService.AddOrUpdateAsync(task.Id,item);
            
        //    await AddTaskToTimelineAsync(item);
        //}

        public async Task AddTasksToTimeline(IEnumerable<WorkCalendarItem> tasks,CancellationToken cancellationToken)
        {
            foreach (WorkCalendarItem task in tasks)
            {
                await AddTaskToTimelineAsync(task,cancellationToken);
            }
            OnPropertyChanged(nameof(HasItems));
        }

        private Random rnd = new Random();
        public async Task AddTaskToTimelineAsync(WorkCalendarItem task,CancellationToken cancellationToken)
        {
            if (task == null) return;

            var item = Items.FirstOrDefault(i => i.TaskId == task.Id);
            if (item == null)
            {
                var workItem = await dataService.GetWorkItemAsync(task.WorkItemID ?? 0,cancellationToken);
                if (workItem == null) return;
                byte[] bytes = [0, 0, 0];
                rnd.NextBytes(bytes);
                Color randomColor = Color.FromRgb(bytes[0], bytes[1], bytes[2]);
                Items.Add(new TimelineItem
                {
                    Title = workItem.Title,
                    ProjectName = workItem.ProjectName,
                    StartTime = task.StartTime,
                    EndTime = task.StartTime.AddHours(task.DurationHour),
                    Background = new SolidColorBrush(randomColor),
                    TaskId = task.Id
                });
            }
            else
            {
                item.StartTime = task.StartTime;
                item.EndTime = task.StartTime.AddHours(task.DurationHour);
                item.TaskId = task.Id;
            }

            SetLineForItems();

            OnPropertyChanged(nameof(HasItems));
        }

        private void SetLineForItems()
        {
            bool most_fix = true;
            while (most_fix)
            {
                most_fix = false;
                foreach (var item in Items.GroupBy(i => i.LineNumber))
                    foreach (var item2 in item)
                    {
                        var match = item.FirstOrDefault(i => item2 != i &&
                        ((i.StartTime >= item2.StartTime && i.StartTime <= item2.EndTime)));
                        if (match!= null)
                        {
                            match.LineNumber++;
                            most_fix = true;
                            break;
                        }
                    }
            }
        }
    }
}