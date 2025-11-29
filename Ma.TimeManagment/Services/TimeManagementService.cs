using Ma.TimeManagement.Models;
using Ma.TimeManagement.ViewModels;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Ma.TimeManagement.Services
{
    public class TimeManagementService : ITimeManagementService
    {
        private readonly ILogger<ITimeManagementService> _logger;
        private readonly IDialogService dialogService;
        private readonly IConverterService converterService;
        private readonly IDataService dataService;
        private readonly IStatusService statusService;
        private readonly IAzureDevOpsService azureDevOpsService;

        public TimeManagementService(ILogger<ITimeManagementService> logger,IDialogService dialogService, IConverterService converterService, IDataService dataService, IStatusService statusService, IAzureDevOpsService azureDevOpsService)
        {
            _logger = logger;
            this.dialogService = dialogService;
            this.converterService = converterService;
            this.dataService = dataService;
            this.statusService = statusService;
            this.azureDevOpsService = azureDevOpsService;
        }

        public double ComputeDurationTime(DateTime startTime, DateTime now)
        {
            var increment = now - startTime;
            var TotalHours = converterService.ConvertHourToRounded(increment.TotalHours);
            return TotalHours;
        }

        public async Task<WorkCalendarItem?> GetActiveCalendarItemAsync()
        {
            return await dataService.GetWorkCalendarItemLastAsync();
        }

        public async Task SetActiveCalendarDurationHourAsync(double durationHour)
        {
            var item = await GetActiveCalendarItemAsync();
            if (item != null)
            {
                item.DurationHour = converterService.ConvertHourToRounded(durationHour);
                await dataService.SetWorkCalendarItemDurationHourAsync(item.Id, item.DurationHour);
                statusService.RefreshItem(item);
            }
        }

        public async Task SyncToAzureAsync()
        {
            await SyncToAzureExceptAsync();
        }
        private async Task SyncToAzureExceptAsync(WorkCalendarItem? WorkCalendarItemLast = null)
        {
            IEnumerable<WorkCalendarItem> workCalendarItems = await dataService.GetWorkCalendarItemsNotSyncedAsync();
            IEnumerable<WorkCalendarItem> Durations = [.. workCalendarItems];

            if (WorkCalendarItemLast != null)
                Durations = [.. Durations.Where(i => WorkCalendarItemLast.Id != i.Id)];
            var DurationHours = Durations.GroupBy(i => i.WorkItemID).Select(i => new { WorkItemID = i.Key, DurationHour = i.Sum(d => d.DurationHour) }).ToList();
            if (DurationHours.Count() > 0 && DurationHours.First().WorkItemID != null)
                statusService.SendStatus(Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info, "Sync", $"Start sync times for works:{string.Join(',', DurationHours.Select(i => i.WorkItemID))}");
            foreach (var item in DurationHours)
            {
                if (item.WorkItemID == null)
                    continue;
                statusService.SendStatus(Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info, "Sync", $"Start sync CompletedWork for {item.WorkItemID}# with duration {item.DurationHour}");
                
                TaskDiscussionViewModel tvm = new TaskDiscussionViewModel();

                var task  = await dataService.GetWorkItemAsync(item.WorkItemID ?? 0);
                if (task == null)
                    continue;
                tvm.Task =  task;
                dialogService.ShowDialog(tvm);
                await azureDevOpsService.WorkItemAddWorkCompleteAsync(item.WorkItemID ?? 0, item.DurationHour,tvm.Discussion);
                foreach (var workCalendarItem in workCalendarItems.Where(i => i.WorkItemID == item.WorkItemID))
                    await dataService.SetworkCalendarItemSyncedAsync(workCalendarItem.Id);
            }
        }

        public async Task SyncToAzureExceptActiveOneAsync()
        {
            var item = await GetActiveCalendarItemAsync();

            await SyncToAzureExceptAsync(item);
        }

        public async Task InsertNewTaskAsync(WorkItem selectedTask, DateTime startTime, double duration)
        {
            var CalendarItem = new WorkCalendarItem()
            {
                DurationHour = duration,
                StartTime = startTime,
            };
            var item = await dataService.AddOrUpdateAsync(selectedTask.Id,CalendarItem);
            await SyncToAzureExceptActiveOneAsync();
           statusService.RefreshItem(item);
        }

        public async Task StartNewTaskAsync(WorkItem selectedTask)
        {
            var CalendarItem = new WorkCalendarItem()
            {
                DurationHour = .25,
                StartTime = DateTime.Now,
            };
            var item = await dataService.AddOrUpdateAsync(selectedTask.Id, CalendarItem);
            await SyncToAzureExceptActiveOneAsync();
            statusService.RefreshItem(item);
        }

        public async Task EndActiveTaskAsync()
        {
            await SyncToAzureAsync();
        }

        public bool HaveActiveWork()
        {
            return dataService.GetWorkCalendarItemLast() != null;
        }
    }
}