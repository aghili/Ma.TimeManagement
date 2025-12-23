using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface ITimeManagementService
    {
        double ComputeDurationTime(DateTime startTime, DateTime now);
        Task EndActiveTaskAsync(CancellationToken cancellationToken);
        Task<WorkCalendarItem?> GetActiveCalendarItemAsync(CancellationToken cancellationToken);
        Task<IEnumerable<TeamProjectReference>> GetProjectsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<WorkItem>> GetTasksAsync(CancellationToken cancellationToken);
        IEnumerable<TeamProjectReference> GetProjects(CancellationToken cancellationToken);
        IEnumerable<WorkItem> GetTasks(CancellationToken cancellationToken);
        bool HaveActiveWork();
        Task InsertNewTaskAsync(WorkItem selectedTask, DateTime startTime, double duration,CancellationToken cancellationToken);
        Task SetActiveCalendarDurationHourAsync(double durationHour, CancellationToken cancellationToken);
        Task StartNewTaskAsync(WorkItem selectedTask, CancellationToken cancellationToken);
        Task SyncToAzureAsync(CancellationToken cancellationToken);
        Task SyncToAzureExceptActiveOneAsync(CancellationToken cancellationToken);
    }
}