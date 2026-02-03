using Ma.TimeManagement.Models;
using Ma.TimeManagement.OpenAPIService;

namespace Ma.TimeManagement.Services
{

    internal class AzureDevOpsService : IAzureDevOpsService
    {
        private MaTimeManagementApiClient clientmaTimeManagement;
        private readonly IConverterService converterService;

        public AzureDevOpsService(MaTimeManagementApiClient maTimeManagementApi,IConverterService converterService)
        {
            this.clientmaTimeManagement = maTimeManagementApi;
            this.converterService = converterService;
        }

        public async Task<Models.WorkItemDto> CreateWorkItemAsync(WorkItemAddDto workItem,CancellationToken cancellationToken)
        {
            return await clientmaTimeManagement.ApiWorkItemsPostAsync(workItem,cancellationToken);
        }

        public async Task<TeamProjectReferenceDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken)
        {
            // Allow null to match IAzureDevOpsService signature (Task<T?>)
            return await clientmaTimeManagement.ApiProjectsGetAsync(id,cancellationToken);
        }

        public async Task<ICollection<Models.TeamProjectReferenceDto>> GetProjectsAsync(CancellationToken cancellationToken)
        {
            return await clientmaTimeManagement.ApiProjectsGetAsync(cancellationToken);
        }

        public async Task<ICollection<Models.WorkItemDto>> GetTasksAsync(CancellationToken cancellationToken)
        {
            return await clientmaTimeManagement.ApiWorkItemsGetAsync(cancellationToken);
        }

        public async Task<Models.WorkItemDto?> GetWorkItemAsync(int TaskId, CancellationToken cancellationToken)
        {
            // Allow null to match IAzureDevOpsService signature (Task<T?>)
            return await clientmaTimeManagement.ApiWorkItemsGetAsync(TaskId,cancellationToken);
        }

        public async Task UpdateWorkItemAsync(int TaskId,WorkItemAddDto workItem, CancellationToken cancellationToken)
        {
            await clientmaTimeManagement.ApiWorkItemsPutAsync(TaskId, workItem, cancellationToken);
        }

        public async Task UpdateWorkItemAsync(int TaskId,WorkItemUpdateDto workItem, CancellationToken cancellationToken)
        {
            await clientmaTimeManagement.ApiWorkItemsPatchAsync(TaskId, workItem, cancellationToken);
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

            await clientmaTimeManagement.ApiWorkItemsPatchAsync(workItemID, workItemUpdate , cancellationToken);

        }
    }
}
