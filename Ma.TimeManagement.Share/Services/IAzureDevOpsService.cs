using Ma.TimeManagement.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Ma.TimeManagement.Services
{
    public interface IAzureDevOpsService
    {
        Task<WorkItemDto> CreateWorkItemAsync(string title, EnWorkState State, double originalEstimate, double RemainingWork, double CompletedWork, EnWorkItemType WorkItemType, Guid projectId, string discution,CancellationToken cancellationToken);
        Task<ICollection<TeamProjectReferenceDto>> GetProjectsAsync(CancellationToken cancellationToken);
        Task<ICollection<WorkItemDto>> GetTasksAsync(CancellationToken cancellationToken);
        Task<WorkItemDto?> GetWorkItemAsync(int TaskId, CancellationToken cancellationToken);
        Task UpdateWorkItemAsync(JsonPatchDocument patch, int TaskId,CancellationToken cancellationToken);
        Task WorkItemAddWorkCompleteAsync(int workItemID, double durationHour, string discussionText, CancellationToken cancellationToken);
        Task<TeamProjectReferenceDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken);
    }
}