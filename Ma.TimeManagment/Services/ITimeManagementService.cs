using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface ITimeManagementService
    {
        double ComputeDurationTime(DateTime startTime, DateTime now);
        Task EndActiveTaskAsync();
        Task<WorkCalendarItem?> GetActiveCalendarItemAsync();

        bool HaveActiveWork();
        Task InsertNewTaskAsync(WorkItem selectedTask, DateTime startTime, double duration);
        Task SetActiveCalendarDurationHourAsync(double durationHour);
        Task StartNewTaskAsync(WorkItem selectedTask);
        Task SyncToAzureAsync();
        Task SyncToAzureExceptActiveOneAsync();
    }
}