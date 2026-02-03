using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services.Design
{
    public class AzureDevOpsService : IAzureDevOpsService
    {

        private readonly IStatusService statusService;
        private ISettingsService settingsService;
        private readonly IConverterService converterService;
        //private Dictionary<int?,WorkItem> bufferedWorkItems = [];
        //private Dictionary<Guid, TeamProjectReference> bufferedProjects = [];
        private SemaphoreSlim semaphoreInit = new SemaphoreSlim(1);
        private bool Inited = false;
        private ICollection<WorkItemDto> workItems = [];
        private List<TeamProjectReferenceDto> teamProjects;

        public AzureDevOpsService(IStatusService statusService, ISettingsService settingsService, IConverterService converterService)
        {
            this.statusService = statusService;
            this.settingsService = settingsService;
            this.converterService = converterService;

            var server = settingsService.FirstServer;
        }

        public bool IsReady { get => Inited; }

        public ICollection<WorkItemDto> WorkItems
        {
            get => workItems;
        }

        public async Task<ICollection<WorkItemDto>> GetTasksAsync(CancellationToken cancellationToken)
        {
            await RefreshWorkItems(cancellationToken).ConfigureAwait(false);
            return WorkItems;
        }

        public async Task<ICollection<TeamProjectReferenceDto>> GetProjectsAsync(CancellationToken cancellationToken)
        {
            await RefreshProjects(cancellationToken).ConfigureAwait(false);
            return teamProjects;
        }
        public async Task<TeamProjectReferenceDto> GetProjectAsync(Guid Id,CancellationToken cancellationToken)
        {
            return teamProjects.First(i => i.Id == Id);
        }
        private async Task RefreshProjects(CancellationToken cancellationToken)
        {
            if (!Inited)
                try
                {
                    await semaphoreInit.WaitAsync(cancellationToken);
                }
                finally { semaphoreInit.Release(); }
            if (!Inited)
            {
                return;
            }
            List<TeamProjectReferenceDto> Projects = [];

            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Mahak.CoreOps", State = ProjectState.WellFormed, Visibility = ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Mahak.Sales", State = ProjectState.WellFormed, Visibility = ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Mahak.SMS", State = ProjectState.WellFormed, Visibility = ProjectVisibility.Organization });
            Projects.Add(new() { Id = new Guid("00000000-0000-0000-0000-000000000004"), Name = "Mahak.Kiosk", State = ProjectState.WellFormed, Visibility = ProjectVisibility.Organization });

            List<TeamProjectReferenceDto> most_remove = [];

            foreach (var project in teamProjects)
                if (Projects.Any(t => t.Id == project.Id) == false)
                    most_remove.Add(project);

            foreach (var project in most_remove)
                teamProjects.Remove(project);
            foreach (var project in Projects)
                teamProjects.Add(project);
        }

        private async Task RefreshWorkItems(CancellationToken cancellationToken)
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
            List<WorkItemDto> Tasks = [];

            await RefreshProjects(cancellationToken);
            int id = 1;
            foreach (var project in teamProjects)
            {
                List<int> taskIds = [];

                Tasks.Add(new() { Id = id++, CompletedWork = 5, OriginalEstimate = 50, RemainingWork = 20, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 1", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 6, OriginalEstimate = 51, RemainingWork = 21, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 2", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 7, OriginalEstimate = 52, RemainingWork = 22, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 3", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 8, OriginalEstimate = 53, RemainingWork = 23, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 4", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 9, OriginalEstimate = 54, RemainingWork = 24, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 5", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 10, OriginalEstimate = 55, RemainingWork = 25, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 6", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 11, OriginalEstimate = 56, RemainingWork = 26, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 7", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
                Tasks.Add(new() { Id = id++, CompletedWork = 12, OriginalEstimate = 57, RemainingWork = 27, ProjectID = project.Id, ProjectName = project.Name, State = EnWorkState.Active, Title = project.Name + "title 8", Url = string.Empty, WorkItemType = EnWorkItemType.Task });
            }
            List<WorkItemDto> most_remove = [];

            foreach (var task in WorkItems)
                if (Tasks.Any(t => t.Id == task.Id) == false)
                    most_remove.Add(task);

            foreach (var task in most_remove)
                workItems.Remove(task);
        }

        public async Task UpdateWorkItemAsync(int id,WorkItemUpdateDto workItem,CancellationToken cancellationToken)
        {
            return;
        }
   public async Task UpdateWorkItemAsync(int id,WorkItemAddDto workItem,CancellationToken cancellationToken)
        {
            return;
        }
        public async Task<WorkItemDto> GetWorkItemAsync(int TaskId,CancellationToken cancellationToken)
        {
            return WorkItems.First(i => i.Id == TaskId);
        }

        public Task WorkItemAddWorkCompleteAsync(int workItemID, double durationHour, string discussionText,CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<WorkItemDto> CreateWorkItemAsync(WorkItemAddDto workItem,CancellationToken cancellationToken)
        {
            return new();
        }
    }
}