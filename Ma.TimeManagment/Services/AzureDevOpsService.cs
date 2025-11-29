using Ma.TimeManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Ma.TimeManagement.Services
{
    public class AzureDevOpsService : IAzureDevOpsService
    {
        private WorkItemTrackingHttpClient WitClient;

        private Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient prgClient;

        private List<string> Projects { get; set; }

        private readonly ILogger<IAzureDevOpsService> logger;
        private readonly IStatusService statusService;
        private ISettingsService settingsService;
        private readonly IDataService dataService;
        private readonly IConverterService converterService;
        //private Dictionary<int?,WorkItem> bufferedWorkItems = [];
        //private Dictionary<Guid, TeamProjectReference> bufferedProjects = [];
        private SemaphoreSlim semaphoreInit = new SemaphoreSlim(1);
        private bool Inited = false;
        private IEnumerable<WorkItem> workItems = [];
        private IEnumerable<TeamProjectReference> teamProjects;

        public AzureDevOpsService(ILogger<IAzureDevOpsService> logger,IStatusService statusService, ISettingsService settingsService, IDataService dataService, IConverterService converterService)
        {
            this.logger = logger;
            this.statusService = statusService;
            this.settingsService = settingsService;
            this.dataService = dataService;
            this.converterService = converterService;

            var server = settingsService.FirstServer;
            Task.Factory.StartNew(() =>
            {
                Initialize(server.ServerUrl, server.Collection, server.Project, server.PAT);
            }, TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void Initialize(string ServerUrl, string Collection, string project, string Pat)
        {
            try
            {
                if (string.IsNullOrEmpty(Pat))
                {
                    statusService.SendStatus(Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info, "Initialize", "Set Personal Access Token in Settings!");
                    return;
                }
                    CancellationTokenSource cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(10000);
                semaphoreInit.Wait(cancellationToken.Token);
                var uri = new Uri($"{ServerUrl}/{Collection}");
                var credentials = new VssBasicCredential(string.Empty, Pat);
                var connection = new Microsoft.VisualStudio.Services.WebApi.VssConnection(uri, credentials);
#if !DISABLEAZURE
                WitClient = connection.GetClient<WorkItemTrackingHttpClient>(cancellationToken.Token);
                prgClient = connection.GetClient<Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient>(cancellationToken.Token);
#else
#endif
                Inited = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, GetType().Name, []);
                statusService.SendStatus(ex);
            }
            finally
            {
                semaphoreInit.Release();
            }
        }

        public IEnumerable<WorkItem> WorkItems
        {
            get => workItems;
        }

        public async Task<IEnumerable<WorkItem>> GetTasks()
        {
            await RefreshWorkItems().ConfigureAwait(false);
            workItems = await dataService.GetWorkItemsAsync();
            return WorkItems;
        }

        public async Task<IEnumerable<TeamProjectReference>> GetProjects()
        {
            await RefreshProjects().ConfigureAwait(false);
            teamProjects = await dataService.GetTeamProjectsAsync();
            return teamProjects;
        }

        private async Task RefreshProjects()
        {
            if (!Inited)
                try
                {
                    CancellationTokenSource cancellationToken = new CancellationTokenSource();
                    cancellationToken.CancelAfter(10000);
                    await semaphoreInit.WaitAsync(cancellationToken.Token);
                }
                finally { semaphoreInit.Release(); }
            if (!Inited)
            {
                return;
            }
            List<TeamProjectReference> Projects = [];
#if !DISABLEAZURE
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

            List<TeamProjectReference> most_remove = [];

            foreach (var project in await dataService.GetTeamProjectsAsync())
                if (Projects.Any(t => t.Id == project.Id) == false)
                    most_remove.Add(project);

            foreach (var project in most_remove)
                await dataService.RemoveAsync(project);
            foreach (var project in Projects)
                await dataService.AddOrUpdateAsync(project);
        }

        private async Task RefreshWorkItems()
        {
            if (!Inited)
                try
                {
                    await semaphoreInit.WaitAsync();
                }
                finally
                {
                    semaphoreInit.Release();
                }
            List<WorkItem> Tasks = [];

            await RefreshProjects();
            int id = 1;
            foreach (var project in await dataService.GetTeamProjectsAsync())
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
                Tasks.AddRange(converterService.ConvertTo(tasks));
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
                foreach (var task in Tasks)
                    await dataService.AddOrUpdateAsync(project.Id, task);
            }
            List<WorkItem> most_remove = [];

            foreach (var task in await dataService.GetWorkItemsAsync())
                if (Tasks.Any(t => t.Id == task.Id) == false)
                    most_remove.Add(task);

            foreach (var task in most_remove)
                await dataService.RemoveAsync(task);
        }

        public async Task AddDiscutionToTaskAsync(string Text,Guid Project,int TaskId)
        {
            await WitClient.AddCommentAsync(new Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.CommentCreate()
            {
                Text = ""
            },Project,TaskId);
        }

        public async Task UpdateWorkItemAsync(JsonPatchDocument patch, int TaskId)
        {
            await WitClient.UpdateWorkItemAsync(patch, TaskId);
            statusService.SendStatus(Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info, "Sync", $"{TaskId}#: Update Work Item.");
        }

        public async Task<WorkItem> GetWorkItemAsync(int TaskId)
        {
            WorkItem task = converterService.ConvertTo(await WitClient.GetWorkItemAsync(TaskId));
            await dataService.AddOrUpdateAsync(task.ProjectID, task);
            return await dataService.GetWorkItemAsync(TaskId) ?? task;
        }

        public async Task<WorkItem> CreateWorkItemAsync(JsonPatchDocument patch, Guid ProjectID, string type)
        {
            WorkItem task = converterService.ConvertTo(await WitClient.CreateWorkItemAsync(patch, ProjectID, type));
            await dataService.AddOrUpdateAsync(ProjectID, task);
            return await dataService.GetWorkItemAsync(task.Id) ?? task;
        }

        public async Task WorkItemAddWorkCompleteAsync(int workItemID, double durationHour,string discussionText)
        {
            var currentTask = await GetWorkItemAsync(workItemID);
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
            statusService.SendStatus(Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info, "Sync", $"{workItemID}#: Update CompletedWork to {TotalHours} and RemainingWork to {remainingWork}.");
            await UpdateWorkItemAsync(patch, workItemID);
        }
    }
}