using Ma.TimeManagement.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Ma.TimeManagement.Services
{
    public interface IAzureDevOpsService
    {
        Task<WorkItemDto> CreateWorkItemAsync(WorkItemAddDto workItem,CancellationToken cancellationToken);
        Task<ICollection<TeamProjectReferenceDto>> GetProjectsAsync(CancellationToken cancellationToken);
        Task<ICollection<WorkItemDto>> GetTasksAsync(CancellationToken cancellationToken);
        Task<WorkItemDto?> GetWorkItemAsync(int id, CancellationToken cancellationToken);
        Task UpdateWorkItemAsync(int id,WorkItemUpdateDto workItem, CancellationToken cancellationToken);
        Task WorkItemAddWorkCompleteAsync(int id, double durationHour, string discussionText, CancellationToken cancellationToken);
        Task<TeamProjectReferenceDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateWorkItemAsync(int id, WorkItemAddDto workItem, CancellationToken cancellationToken);
    }
}