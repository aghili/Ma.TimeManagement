using Ma.TimeManagement.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ma.TimeManagement.Services
{
    public class TimeEngineService : BackgroundService
    {
        private readonly IDataService dataService;
        private readonly IStatusService statusService;
        private readonly IAzureDevOpsService azureDevOpsService;
        private readonly ILogger<TimeEngineService> _logger;
        private readonly IConverterService converterService;
        private int _executionCount;
        public TimeEngineService(ILogger<TimeEngineService> logger,IConverterService converterService, IDataService dataService, IStatusService statusService, IAzureDevOpsService azureDevOpsService)
        {
            _logger = logger;
            this.converterService = converterService;
            this.dataService = dataService;
            this.statusService = statusService;
            this.azureDevOpsService = azureDevOpsService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            // When the timer should have no due-time, then do the work once now.
            await DoWork(stoppingToken);

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWork(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await SyncToAzure(null);

            await base.StopAsync(cancellationToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            try
            {
                WorkCalendarItem? WorkCalendarItemLast = await dataService.GetWorkCalendarItemLastAsync();
                await SyncToAzure(WorkCalendarItemLast);

                if (WorkCalendarItemLast != null)
                {
                    double DurationHour = ComputeDurationTime(WorkCalendarItemLast.StartTime, DateTime.Now);
                    if (DurationHour != WorkCalendarItemLast.DurationHour)
                    {
                        await dataService.SetWorkCalendarItemDurationHourAsync(WorkCalendarItemLast.Id, DurationHour);
                        statusService.RefreshItem(WorkCalendarItemLast);
                    }
                }
            }
            catch (OperationCanceledException) {
                await SyncToAzure(null);
            }
            catch (Exception ex)
            {
                statusService.SendStatus(ex);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            _logger.LogInformation("Timed Hosted Service is working.");
        }

        private async Task SyncToAzure(WorkCalendarItem? WorkCalendarItemLast)
        {
            IEnumerable<WorkCalendarItem> workCalendarItems = await dataService.GetWorkCalendarItemsNotSyncedAsync();
            IEnumerable<WorkCalendarItem> Durations = [.. workCalendarItems];
            if (WorkCalendarItemLast != null)
                Durations = [.. Durations.Where(i => i.Id != i.Id)];
            var DurationHours = Durations.GroupBy(i => i.WorkItemID).Select(i => new { WorkItemID = i.Key, DurationHour = i.Sum(d => d.DurationHour) }).ToList();
            foreach (var item in DurationHours)
            {
                if (item.WorkItemID != null)
                    continue;
                await azureDevOpsService.WorkItemAddWorkCompleteAsync(item.WorkItemID ?? 0, item.DurationHour);
                foreach (var workCalendarItem in workCalendarItems.Where(i => i.WorkItemID == item.WorkItemID))
                    await dataService.SetworkCalendarItemSyncedAsync(workCalendarItem.Id);
            }
        }

        private double ComputeDurationTime(DateTime startTime, DateTime now)
        {
            var increment = now - startTime;
            var TotalHours =converterService.ConvertHourToRounded(increment.TotalHours);
            return TotalHours;
        }
    }
}