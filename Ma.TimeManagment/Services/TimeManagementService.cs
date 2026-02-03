using Ma.TimeManagement.Models;
using Ma.TimeManagement.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ma.TimeManagement.Services
{

    public class TimeManagementService : ITimeManagementService
    {
        private readonly ILogger<ITimeManagementService> _logger;
        private readonly IDialogService dialogService;
        private readonly IConverterService converterService;
        private readonly IDataService dataService;
        private readonly IStatusService statusService;
        private readonly IMessageService messageService;
        private readonly IAzureDevOpsService azureDevOpsService;

        public TimeManagementService(ILogger<ITimeManagementService> logger,IDialogService dialogService, IConverterService converterService, IDataService dataService, IStatusService statusService,IMessageService messageService, IAzureDevOpsService azureDevOpsService)
        {
            _logger = logger;
            this.dialogService = dialogService;
            this.converterService = converterService;
            this.dataService = dataService;
            this.statusService = statusService;
            this.messageService = messageService;
            this.azureDevOpsService = azureDevOpsService;
        }

        public double ComputeDurationTime(DateTime startTime, DateTime now)
        {
            var increment = now - startTime;
            var TotalHours = converterService.ConvertHourToRounded(increment.TotalHours);
            return TotalHours;
        }


        public async Task<IEnumerable<WorkItem>> GetTasksAsync(CancellationToken cancellationToken)
        {
            return await RefreshWorkItems(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TeamProjectReference>> GetProjectsAsync(CancellationToken cancellationToken)
        {
            return await RefreshProjects(cancellationToken).ConfigureAwait(false);
        }
        public IEnumerable<WorkItem> GetTasks(CancellationToken cancellationToken)
        {
            IEnumerable<WorkItem> items = [];
            App.Current.Dispatcher.Invoke(async () =>
            {
                items = await RefreshWorkItems(cancellationToken).ConfigureAwait(false);
            });
            return items;
        }

        public IEnumerable<TeamProjectReference> GetProjects(CancellationToken cancellationToken)
        {
            IEnumerable<TeamProjectReference> items = [];
            App.Current.Dispatcher.Invoke(async () =>
            {
                items = await RefreshProjects(cancellationToken).ConfigureAwait(false);
            });
            return items;
        }

        private async Task<IEnumerable<TeamProjectReference>> RefreshProjects(CancellationToken cancellationToken)
        {
            List<TeamProjectReference> Projects = [];
#if !DISABLEAZURE
            var projects = await azureDevOpsService.GetProjectsAsync(cancellationToken);
#else
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Mahak.CoreOps", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Mahak.Sales", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Mahak.SMS", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000004"), Name = "Mahak.Kiosk", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
#endif

            List<TeamProjectReference> most_remove = [];

            foreach (var project in await dataService.GetTeamProjectsAsync(cancellationToken))
                if (Projects.Any(t => t.Id == project.Id) == false)
                    most_remove.Add(project);

            foreach (var project in most_remove)
                await dataService.RemoveAsync(project, cancellationToken);
            foreach (var project in Projects)
                await dataService.AddOrUpdateAsync(project,cancellationToken);
            return Projects;
        }

        private async Task<IEnumerable<WorkItem>> RefreshWorkItems(CancellationToken cancellationToken)
        {
            IEnumerable<WorkItemDto> Tasks = [];

#if !DISABLEAZURE

            var projects = await azureDevOpsService.GetProjectsAsync(cancellationToken);

            foreach (var project in projects)
                await dataService.AddOrUpdateAsync(converterService.ConvertTo(project),cancellationToken);

           Tasks = await azureDevOpsService.GetTasksAsync(cancellationToken);
#else
            int id = 1;
            foreach (var project in await dataService.GetTeamProjectsAsync())
            {
                List<int> taskIds = [];
                var wiql = new Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.Wiql
                {
                    Query = $"SELECT [System.Id]" +
                          $"FROM workitems WHERE [System.TeamProject] = '{project.Name}' AND [System.WorkItemType] = 'Task' " +
                          $"AND [System.AssignedTo] = @me AND [System.State] <> 'Closed'"
                };
                Tasks.Add(new() { Id = id++, CompletedWork = 5, OriginalEstimate = 50, RemainingWork = 20, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 1", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 6, OriginalEstimate = 51, RemainingWork = 21, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 2",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 7, OriginalEstimate = 52, RemainingWork = 22, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 3",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 8, OriginalEstimate = 53, RemainingWork = 23, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 4",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 9, OriginalEstimate = 54, RemainingWork = 24, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 5",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 10, OriginalEstimate = 55, RemainingWork = 25, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 6", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 11, OriginalEstimate = 56, RemainingWork = 26, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 7", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 12, OriginalEstimate = 57, RemainingWork = 27, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 8",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
            }
#endif
            foreach (var task in Tasks)
                await dataService.AddOrUpdateAsync(task.ProjectID, converterService.ConvertTo(task), cancellationToken);


            List<WorkItem> most_remove = [];

            foreach (var task in await dataService.GetWorkItemsAsync(cancellationToken))
                if (Tasks.Any(t => t.Id == task.Id) == false)
                    most_remove.Add(task);

            foreach (var task in most_remove)
                await dataService.RemoveAsync(task, cancellationToken);

            return await dataService.GetWorkItemsAsync(cancellationToken);
        }


        public async Task<WorkCalendarItem?> GetActiveCalendarItemAsync(CancellationToken cancellationToken)
        {
            return await dataService.GetWorkCalendarItemLastAsync(cancellationToken);
        }

        public async Task SetActiveCalendarDurationHourAsync(double durationHour,CancellationToken cancellationToken)
        {
            var item = await GetActiveCalendarItemAsync(cancellationToken);
            if (item != null)
            {
                item.DurationHour = converterService.ConvertHourToRounded(durationHour);
                await dataService.SetWorkCalendarItemDurationHourAsync(item.Id, item.DurationHour,cancellationToken);
                messageService.RefreshWorkCalendarItem(item.Id);
            }
        }

        public async Task SyncToAzureAsync(CancellationToken cancellationToken)
        {
            await SyncToAzureExceptAsync(null,cancellationToken);
        }
        private async Task SyncToAzureExceptAsync(WorkCalendarItem? WorkCalendarItemLast,CancellationToken cancellationToken)
        {
            IEnumerable<WorkCalendarItem> workCalendarItems = await dataService.GetWorkCalendarItemsNotSyncedAsync(cancellationToken);
            IEnumerable<WorkCalendarItem> Durations = [.. workCalendarItems];

            if (WorkCalendarItemLast != null)
                Durations = [.. Durations.Where(i => WorkCalendarItemLast.Id != i.Id)];
            var DurationHours = Durations.GroupBy(i => i.WorkItemID).Select(i => new { WorkItemID = i.Key, DurationHour = i.Sum(d => d.DurationHour) }).ToList();
            if (DurationHours.Count() > 0 && DurationHours.First().WorkItemID != null)
                statusService.SendStatus(EnBalloonIcon.Info, "Sync", $"Start sync times for works:{string.Join(',', DurationHours.Select(i => i.WorkItemID))}");
            foreach (var item in DurationHours)
            {
                if (item.WorkItemID == null)
                    continue;
                statusService.SendStatus(EnBalloonIcon.Info, "Sync", $"Start sync CompletedWork for {item.WorkItemID}# with duration {item.DurationHour}");
                
                TaskDiscussionViewModel tvm = new TaskDiscussionViewModel();

                var task  = await dataService.GetWorkItemAsync(item.WorkItemID ?? 0, cancellationToken);
                if (task == null)
                    continue;
                tvm.Task =  task;
                await azureDevOpsService.WorkItemAddWorkCompleteAsync(item.WorkItemID ?? 0, item.DurationHour,tvm.Discussion,cancellationToken);
                foreach (var workCalendarItem in workCalendarItems.Where(i => i.WorkItemID == item.WorkItemID))
                    await dataService.SetworkCalendarItemSyncedAsync(workCalendarItem.Id, cancellationToken);
            }
        }

        public async Task SyncToAzureExceptActiveOneAsync(CancellationToken cancellationToken)
        {
            var item = await GetActiveCalendarItemAsync(cancellationToken);

            await SyncToAzureExceptAsync(item,cancellationToken);
        }

        public async Task InsertNewTaskAsync(WorkItem selectedTask, DateTime startTime, double duration,CancellationToken cancellationToken)
        {
            var CalendarItem = new WorkCalendarItem()
            {
                DurationHour = duration,
                StartTime = startTime,
            };
            var item = await dataService.AddOrUpdateAsync(selectedTask.Id,CalendarItem, cancellationToken);
            await SyncToAzureExceptActiveOneAsync(cancellationToken);
           messageService.RefreshWorkCalendarItem(item.Id);
        }

        public async Task StartNewTaskAsync(WorkItem selectedTask,CancellationToken cancellationToken)
        {
            var CalendarItem = new WorkCalendarItem()
            {
                DurationHour = .25,
                StartTime = DateTime.Now,
            };
            var item = await dataService.AddOrUpdateAsync(selectedTask.Id, CalendarItem,cancellationToken);
            await SyncToAzureExceptActiveOneAsync(cancellationToken);
            messageService.RefreshWorkCalendarItem(item.Id);
        }

        public async Task EndActiveTaskAsync(CancellationToken cancellationToken)
        {
            await SyncToAzureAsync(cancellationToken);
        }

        public bool HaveActiveWork()
        {
            return dataService.GetWorkCalendarItemLast() != null;
        }
    }
}