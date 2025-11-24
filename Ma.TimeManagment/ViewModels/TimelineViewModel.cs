using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Ma.TimeManagement.ViewModels
{
    public partial class TimelineViewModel :ObservableObject,ITimeLineViewModel
    {
        public const int StartHour = 8;
        public const int EndHour = 22;
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

       

        public TimelineViewModel(IAzureDevOpsService azureDevOpsService,IDataService dataService, IDialogService dialogService,IStatusService statusService)
        {
            _azureDevOpsService = azureDevOpsService ?? throw new ArgumentNullException(nameof(azureDevOpsService));
            this.dataService = dataService;
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.statusService = statusService;
            StartTaskCommand = new RelayCommand(async () => await ExecuteStartTaskAsync());
            _items.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasItems));
            
            ZoomInCommand = new RelayCommand(() => Zoom = Math.Min(Zoom * 1.4, 400));
            ZoomOutCommand = new RelayCommand(() => Zoom = Math.Max(Zoom / 1.4, 20));
            ResetCommand = new RelayCommand(() => Zoom = 80);

            // Background refresh (just like HomeViewModel)

            StrongReferenceMessenger.Default.Register<WorkCalendarItem, string>(this, EnStatusAction.RefreshItem.ToString(), (r, m) =>
            {
                RefreshItem(m);
            });
            RefreshTasks();
        }

        private void RefreshItem(WorkCalendarItem m)
        {
            throw new NotImplementedException();
        }

        private void RefreshTasks()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var refreshed = await dataService.GetWorkCalendarItemsDailyAsync(DateTime.Now);
                    Application.Current?.Dispatcher?.Invoke(async () =>
                    {
                        await AddTasksToTimeline(refreshed);
                    });
                }
                catch (Exception ex)
                {
                    statusService.SendStatus(ex);
                }
            });
        }

        public IRelayCommand ZoomInCommand { get; }
        public IRelayCommand ZoomOutCommand { get; }
        public IRelayCommand ResetCommand { get; }
        public IRelayCommand StartTaskCommand { get; }
        private readonly IAzureDevOpsService _azureDevOpsService;
        private readonly IDataService dataService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService statusService;

        public bool HasItems => Items.Count > 0;

        public event EventHandler<WorkItem> TaskStarted;

        private async Task ExecuteStartTaskAsync()
        {
            var vm = new TaskSelectionViewModel();

            // Use cached tasks
            if (_azureDevOpsService.WorkItems != null)
            {
                foreach (var w in _azureDevOpsService.WorkItems.OrderBy(x => x.ProjectName).ThenBy(x => x.Title))
                    vm.AvailableTasks.Add(w);
            }

            // Background refresh (just like HomeViewModel)
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var refreshed = await _azureDevOpsService.GetTasks();
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        vm.AvailableTasks.Clear();
                        foreach (var w in refreshed.OrderBy(x => x.ProjectName).ThenBy(x => x.Title))
                            vm.AvailableTasks.Add(w);
                    });
                }
                catch
                {
                    // Ignore refresh errors; keep seed
                }
            });

            var result = _dialogService.ShowDialog(vm);
            if (result == true && vm.SelectedTask != null)
            {
                await AddTaskToTimelineAsync(vm.SelectedTask);
                TaskStarted?.Invoke(this, vm.SelectedTask);
            }
        }

        public async Task AddTaskToTimelineAsync(WorkItem task)
        {
            WorkCalendarItem item = new()
            {
                Title = task.Title,
                StartTime = DateTime.Now,
                DurationHour = .25
            };
            await dataService.AddOrUpdateAsync(task.Id,item);
            
            await AddTaskToTimelineAsync(item);
        }

        public async Task AddTasksToTimeline(IEnumerable<WorkCalendarItem> tasks)
        {
            foreach (WorkCalendarItem task in tasks)
            {
                if (task == null) return;
                
                var item = Items.FirstOrDefault(i => i.TaskId == task.Id);
                if (item == null)
                {
                    var workItem = await dataService.GetWorkItemAsync(task.WorkItemID ?? 0);
                    if (workItem == null) return;
                    Items.Add(new TimelineItem
                    {
                        Title = workItem.Title,
                        ProjectName = workItem.ProjectName,
                        StartTime = task.StartTime,
                        EndTime = task.StartTime.AddHours(task.DurationHour),
                        Background = new SolidColorBrush(Colors.MediumSeaGreen),
                        TaskId = task.Id
                    });
                }else
                {
                    item.StartTime = task.StartTime;
                    item.EndTime = task.StartTime.AddHours(task.DurationHour);
                    item.Background = new SolidColorBrush(Colors.MediumSeaGreen);
                    item.TaskId = task.Id;
                }
            }
            OnPropertyChanged(nameof(HasItems));
        }

        public async Task AddTaskToTimelineAsync(WorkCalendarItem task)
        {
            if (task == null) return;
            var workItem = await dataService.GetWorkItemAsync(task.WorkItemID ?? 0);
            if (workItem == null) return;
            Items.Add(new TimelineItem
            {
                Title = workItem.Title,
                ProjectName = workItem.ProjectName,
                StartTime = task.StartTime,
                EndTime = task.StartTime.AddHours(task.DurationHour),
                Background = new SolidColorBrush(Colors.MediumSeaGreen),
                TaskId = task.Id
            });
            OnPropertyChanged(nameof(HasItems));
        }
    }
}