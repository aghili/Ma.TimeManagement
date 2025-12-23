using Ma.TimeManagement.Models;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Ma.TimeManagement.Services
{
    public class AzureDevOpsService : IAzureDevOpsService
    {
        private readonly ICurrentUserPatService _patService;

        private readonly ILogger<IAzureDevOpsService> logger;
        private readonly IStatusService statusService;
        private ISettingsService settingsService;
        private readonly IConverterService converterService;
        private SemaphoreSlim semaphoreInit = new SemaphoreSlim(1);

        public AzureDevOpsService(ILogger<IAzureDevOpsService> logger, ICurrentUserPatService patService, IStatusService statusService, ISettingsService settingsService, IConverterService converterService)
        {
            this.logger = logger;
            _patService = patService;
            this.statusService = statusService;
            this.settingsService = settingsService;
            this.converterService = converterService;
        }

        private async Task<VssConnection> CreateConnectionAsync(CancellationToken cancellationToken)
        {
            var pat = await _patService.GetPatAsync(cancellationToken); // ← automatically gets current user's PAT
            try
            {
                if (string.IsNullOrEmpty(pat))
                {
                    statusService.SendStatus(EnBalloonIcon.Info, "Initialize", "Set Personal Access Token in Settings!");
                    throw new Exception("Set Personal Access Token in Settings!");
                }
                semaphoreInit.Wait(cancellationToken);
                var uri = new Uri($"https://cicd-server/MahakSolutions");
                var credentials = new VssBasicCredential(string.Empty, pat);
                var connection = new Microsoft.VisualStudio.Services.WebApi.VssConnection(uri, credentials, new Microsoft.VisualStudio.Services.WebApi.VssClientHttpRequestSettings() { BypassProxyOnLocal = settingsService.BypassProxyOnLocal });
                connection.Settings.BypassProxyOnLocal = settingsService.BypassProxyOnLocal;
                return connection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, GetType().Name, []);
                statusService.SendStatus(EnBalloonIcon.Info, "Initialize", "Server is not response or connection informations is wrong!");
                throw;
            }
            finally
            {
                semaphoreInit.Release();
            }
        }

        public bool IsReady { get => true; }

        public async Task<ICollection<WorkItemDto>> GetTasksAsync(CancellationToken cancellationToken)
        {
            return await RefreshWorkItems(cancellationToken).ConfigureAwait(false);
        }

        public async Task<ICollection<TeamProjectReferenceDto>> GetProjectsAsync(CancellationToken cancellationToken)
        {
            return await RefreshProjects(cancellationToken).ConfigureAwait(false);
        }

        private async Task<ICollection<TeamProjectReferenceDto>> RefreshProjects(CancellationToken cancellationToken)
        {
            List<TeamProjectReferenceDto> Projects = [];
#if !DISABLEAZURE
            using var connection = await CreateConnectionAsync(cancellationToken);
            using var prgClient = connection.GetClient<Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient>(cancellationToken);
            var projects = await prgClient.GetProjects(null, null, null, null, null, null);
            foreach (var project in projects)
            {
                Projects.Add(converterService.ConvertTo(project));
            }
#else
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Mahak.CoreOps", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Mahak.Sales", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Mahak.SMS", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000004"), Name = "Mahak.Kiosk", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
#endif
            return Projects;
        }

        private async Task<ICollection<WorkItemDto>> RefreshWorkItems(CancellationToken cancellationToken)
        {
            List<WorkItemDto> Tasks = [];
            var projects = await RefreshProjects(cancellationToken);
            int id = 1;

            using var connection = await CreateConnectionAsync(cancellationToken);
            using var WitClient = connection.GetClient<WorkItemTrackingHttpClient>(cancellationToken);


            foreach (var project in projects)
            {
                List<int> taskIds = [];
                var wiql = new Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.Wiql
                {
                    Query = $"SELECT [System.Id]" +
                          $"FROM workitems WHERE [System.TeamProject] = '{project.Name}' AND [System.WorkItemType] = 'Task' " +
                          $"AND [System.AssignedTo] = @me AND [System.State] <> 'Closed'"
                };
#if !DISABLEAZURE
                var queryResult = await WitClient.QueryByWiqlAsync(wiql);

                foreach (var workItemRef in queryResult.WorkItems)
                {
                    taskIds.Add(workItemRef.Id);
                }
                if (taskIds.Count == 0)
                    continue;
                var tasks = await WitClient.GetWorkItemsAsync(taskIds, expand: Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemExpand.Fields);
                Tasks.AddRange(converterService.ConvertTo(project.Id,tasks));
#else
                Tasks.Add(new() { Id = id++, CompletedWork = 5, OriginalEstimate = 50, RemainingWork = 20, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 1", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 6, OriginalEstimate = 51, RemainingWork = 21, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 2",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 7, OriginalEstimate = 52, RemainingWork = 22, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 3",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 8, OriginalEstimate = 53, RemainingWork = 23, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 4",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 9, OriginalEstimate = 54, RemainingWork = 24, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 5",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 10, OriginalEstimate = 55, RemainingWork = 25, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 6", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 11, OriginalEstimate = 56, RemainingWork = 26, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 7", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 12, OriginalEstimate = 57, RemainingWork = 27, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 8",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
#endif
            }
            List<WorkItemDto> most_remove = [];

            return Tasks;
        }

        public async Task AddDiscutionToTaskAsync(string Text,Guid Project,int TaskId,CancellationToken cancellationToken)
        {
            using var connection = await CreateConnectionAsync(cancellationToken);
            using var WitClient = connection.GetClient<WorkItemTrackingHttpClient>(cancellationToken);
            await WitClient.AddCommentAsync(new Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.CommentCreate()
            {
                Text = ""
            },Project,TaskId);
        }

        public async Task UpdateWorkItemAsync(JsonPatchDocument patch, int TaskId,CancellationToken cancellationToken)
        {
            using var connection = await CreateConnectionAsync(cancellationToken);
            using var WitClient = connection.GetClient<WorkItemTrackingHttpClient>(cancellationToken);
            await WitClient.UpdateWorkItemAsync(patch, TaskId);
            statusService.SendStatus(EnBalloonIcon.Info, "Sync", $"{TaskId}#: Update Work Item.");
        }

        public async Task<WorkItemDto?> GetWorkItemAsync(int TaskId,CancellationToken cancellationToken)
        {
            var workItem = (await RefreshWorkItems(cancellationToken)).FirstOrDefault(i=>i.Id == TaskId);
            return workItem;
        }

        public async Task<WorkItemDto> CreateWorkItemAsync(string title, EnWorkState State, double originalEstimate, double RemainingWork, double CompletedWork, EnWorkItemType WorkItemType, Guid projectId, string discution,CancellationToken cancellationToken)
        {
            //WorkItemDto task = converterService.ConvertTo(await WitClient.CreateWorkItemAsync(patch, ProjectID, type));
            //return task;
            //var patch = new JsonPatchDocument();
            //patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = Title });

            //if (!string.IsNullOrEmpty(Description))
            //{
            //    patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Description", Value = Description });
            //}

            //if (!string.IsNullOrEmpty(ParentId) && int.TryParse(ParentId, out int parentId))
            //{
            //    var parent = await azureDevOpsService.GetWorkItemAsync(parentId, cancellationToken);
            //    patch.Add(new JsonPatchOperation
            //    {
            //        Operation = Operation.Add,
            //        Path = "/relations/-",
            //        Value = new WorkItemRelation
            //        {
            //            Rel = "System.LinkTypes.Hierarchy-Reverse",
            //            Url = parent.Url
            //        }
            //    });
            //}
            return new();
        }
        public async Task WorkItemAddWorkCompleteAsync(int workItemID, double durationHour,string discussionText,CancellationToken cancellationToken)
        {
            var currentTask = await GetWorkItemAsync(workItemID,cancellationToken);
            var currentCompleted = currentTask.CompletedWork;
            var currentRemainingWork = currentTask.RemainingWork;
            var TotalHours = converterService.ConvertHourToRounded(currentCompleted + durationHour);
            var remainingWork = (currentRemainingWork - TotalHours);
            remainingWork = converterService.ConvertHourToRounded(remainingWork);


            var patch = new JsonPatchDocument();
            patch.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.Scheduling.CompletedWork",
                Value = TotalHours
            });
            patch.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.Scheduling.RemainingWork",
                Value = remainingWork < 0 ? 0 : remainingWork
            });
            patch.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.History",
                Value = discussionText
            });
            statusService.SendStatus(EnBalloonIcon.Info, "Sync", $"{workItemID}#: Update CompletedWork to {TotalHours} and RemainingWork to {remainingWork}.");
            await UpdateWorkItemAsync(patch, workItemID,cancellationToken);
        }

        public Task<TeamProjectReferenceDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}