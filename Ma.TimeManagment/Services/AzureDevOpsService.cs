using Ma.TimeManagement.Models;
using Ma.TimeManagement.OpenAPIService;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Ma.TimeManagement.Services
{
    internal class AzureDevOpsService : IAzureDevOpsService
    {
        private MaTimeManagmentApiClient clientmaTimeManagement;
        private readonly IConverterService converterService;

        public AzureDevOpsService(MaTimeManagmentApiClient maTimeManagementApi,IConverterService converterService)
        {
            this.clientmaTimeManagement = maTimeManagementApi;
            this.converterService = converterService;
        }

        public async Task<Models.WorkItemDto> CreateWorkItemAsync(string title, Models.EnWorkState State, double originalEstimate, double RemainingWork, double CompletedWork, Models.EnWorkItemType WorkItemType, Guid projectId, string discution,CancellationToken cancellationToken)
        {
            var workItemDto = new WorkItemAddDto() { Title = title, State = State, OriginalEstimate = originalEstimate, RemainingWork = RemainingWork, CompletedWork = CompletedWork, WorkItemType = WorkItemType, ProjectID = projectId, Discution = discution };
            return await clientmaTimeManagement.WorkItemsPOSTAsync(workItemDto,cancellationToken);
        }

        public async Task<Models.TeamProjectReferenceDto> GetProjectAsync(Guid id, CancellationToken cancellationToken)
        {
            return await clientmaTimeManagement.ProjectsGETAsync(id,cancellationToken);
        }

        public async Task<ICollection<Models.TeamProjectReferenceDto>> GetProjectsAsync(CancellationToken cancellationToken)
        {
            return await clientmaTimeManagement.ProjectsAllAsync(cancellationToken);
        }

        public async Task<ICollection<Models.WorkItemDto>> GetTasksAsync(CancellationToken cancellationToken)
        {
            return await clientmaTimeManagement.WorkItemsAllAsync(cancellationToken);
        }

        public async Task<Models.WorkItemDto> GetWorkItemAsync(int TaskId, CancellationToken cancellationToken)
        {
            return await clientmaTimeManagement.WorkItemsGETAsync(TaskId,cancellationToken);
        }

        public Task UpdateWorkItemAsync(JsonPatchDocument patch, int TaskId,CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task WorkItemAddWorkCompleteAsync(int workItemID, double durationHour, string discussionText,CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
