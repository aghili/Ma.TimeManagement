using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface IDataService
    {
        Task<IEnumerable<TeamProjectReference>> GetTeamProjects();
        Task<IEnumerable<WorkItem>> GetWorkItems();

        Task AddOrUpdate(Guid ProjectID, WorkItem item);
        Task AddOrUpdate(TeamProjectReference item);
        Task<WorkItem?> GetWorkItem(int taskId);
        Task Remove(TeamProjectReference item);
        Task Remove(WorkItem item);
    }
}