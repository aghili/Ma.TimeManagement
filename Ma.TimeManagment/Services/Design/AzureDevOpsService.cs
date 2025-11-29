using Ma.TimeManagement.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Ma.TimeManagement.Services.Design
{
    public class AzureDevOpsService : IAzureDevOpsService
    {
        private List<string> Projects { get; set; }

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

        public AzureDevOpsService(IStatusService statusService, ISettingsService settingsService, IDataService dataService, IConverterService converterService)
        {
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
            Inited = true;
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

            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Mahak.CoreOps", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Mahak.Sales", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Mahak.SMS", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000004"), Name = "Mahak.Kiosk", State = Microsoft.TeamFoundation.Core.WebApi.ProjectState.WellFormed, Visibility = Microsoft.TeamFoundation.Core.WebApi.ProjectVisibility.Organization });

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

                Tasks.Add(new() { Id = id++, CompletedWork = 5, OriginalEstimate = 50, RemainingWork = 20, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 1", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 6, OriginalEstimate = 51, RemainingWork = 21, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 2",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 7, OriginalEstimate = 52, RemainingWork = 22, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 3",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 8, OriginalEstimate = 53, RemainingWork = 23, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 4",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 9, OriginalEstimate = 54, RemainingWork = 24, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 5",Url=string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 10, OriginalEstimate = 55, RemainingWork = 25, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 6", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 11, OriginalEstimate = 56, RemainingWork = 26, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name+"title 7", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 12, OriginalEstimate = 57, RemainingWork = 27, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 8",Url=string.Empty, WorkItemType = EnWorkItemType.Task });

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

        public async Task UpdateWorkItemAsync(JsonPatchDocument patch, int TaskId)
        {
        }

        public async Task<WorkItem> GetWorkItemAsync(int TaskId)
        {
            return await dataService.GetWorkItemAsync(TaskId);
        }

        public async Task<WorkItem> CreateWorkItemAsync(JsonPatchDocument patch, Guid guid, string type)
        {
            return new();
        }

        public Task WorkItemAddWorkCompleteAsync(int workItemID, double durationHour, string discussionText)
        {
            return Task.CompletedTask;
        }
    }
}