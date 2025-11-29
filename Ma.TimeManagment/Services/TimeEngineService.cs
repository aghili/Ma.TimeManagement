using Ma.TimeManagement.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ma.TimeManagement.Services
{
    public class TimeEngineService : BackgroundService
    {
        private readonly ILogger<TimeEngineService> logger;
        private readonly ITimeManagementService timeManagementService;
        private readonly IStatusService statusService;

        public TimeEngineService(ILogger<TimeEngineService> logger,ITimeManagementService timeManagementService,IStatusService statusService)
        {
            this.logger = logger;
            this.timeManagementService = timeManagementService;
            this.statusService = statusService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Timed Hosted Service running.");

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
                logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await timeManagementService.SyncToAzureAsync();

            await base.StopAsync(cancellationToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            try
            {

                var WorkCalendarItemLast = await timeManagementService.GetActiveCalendarItemAsync();

                await timeManagementService.SyncToAzureExceptActiveOneAsync();

                if (WorkCalendarItemLast != null)
                {
                    double DurationHour = timeManagementService.ComputeDurationTime(WorkCalendarItemLast.StartTime, DateTime.Now);
                    if (DurationHour != WorkCalendarItemLast.DurationHour)
                    {
                        await timeManagementService.SetActiveCalendarDurationHourAsync(DurationHour);
                    }
                }
            }
            catch (OperationCanceledException) {
                await timeManagementService.SyncToAzureAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, GetType().Name, []);
                statusService.SendStatus(ex);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            logger.LogInformation("Timed Hosted Service is working.");
        }
    }
}