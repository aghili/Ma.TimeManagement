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

        public async Task<Models.WorkItemDto> CreateWorkItemAsync(WorkItemAddDto workItem,CancellationToken cancellationToken)
        {
            return await clientmaTimeManagement.WorkItemsPOSTAsync(workItem,cancellationToken);
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

        public async Task UpdateWorkItemAsync(int TaskId,WorkItemAddDto workItem, CancellationToken cancellationToken)
        {
            await clientmaTimeManagement.WorkItemsPUTAsync(TaskId, workItem, cancellationToken);
        }
  public async Task UpdateWorkItemAsync(int TaskId,WorkItemUpdateDto workItem, CancellationToken cancellationToken)
        {
            await clientmaTimeManagement.WorkItemsPATCHAsync(TaskId, workItem, cancellationToken);
        }

        public async Task WorkItemAddWorkCompleteAsync(int workItemID, double durationHour, string discussionText,CancellationToken cancellationToken)
        {
            var workItem = await GetWorkItemAsync(workItemID, cancellationToken);
            if (workItem == null)
                return;
            var currentCompleted = workItem.CompletedWork;
            var currentRemainingWork = workItem.RemainingWork;
            var TotalHours = converterService.ConvertHourToRounded(currentCompleted + durationHour);
            var remainingWork = (currentRemainingWork - TotalHours);
            remainingWork = converterService.ConvertHourToRounded(remainingWork);

            var workItemUpdate = new WorkItemUpdateDto()
            {
                CompletedWork = TotalHours,
                RemainingWork = remainingWork < 0 ? 0 : remainingWork,
                Discution = discussionText
            };

            await clientmaTimeManagement.WorkItemsPATCHAsync(workItemID, workItemUpdate , cancellationToken);

        }
    }
}
