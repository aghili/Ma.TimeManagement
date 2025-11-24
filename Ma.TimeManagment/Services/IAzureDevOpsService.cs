using Ma.TimeManagement.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Ma.TimeManagement.Services
{
    public interface IAzureDevOpsService
    {
        IEnumerable<WorkItem> WorkItems { get; }

        Task<WorkItem> CreateWorkItemAsync(JsonPatchDocument patch, Guid guid, string type);
        Task<IEnumerable<TeamProjectReference>> GetProjects();
        Task<IEnumerable<WorkItem>> GetTasks();
        Task<WorkItem> GetWorkItemAsync(int TaskId);
        void Initialize(string ServerUrl, string Collection, string project, string Pat);
        Task UpdateWorkItemAsync(JsonPatchDocument patch, int TaskId);
        Task WorkItemAddWorkCompleteAsync(int workItemID, double durationHour);
    }
}