using Ma.TimeManagement.Data;
using Ma.TimeManagement.Models;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ma.TimeManagement.Services
{
    public class DataService : IDataService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbContextFactory;
        private readonly IStatusService statusService;

        public DataService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IStatusService statusService)
        {
            this.dbContextFactory = dbContextFactory;
            this.statusService = statusService;
        }

        public async Task<IEnumerable<TeamProjectReference>> GetTeamProjects()
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return [.. applicationDbContext.TeamProjects];
        }

        public async Task<IEnumerable<WorkItem>> GetWorkItems()
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return [.. applicationDbContext.WorkItems];
        }

        public async Task AddOrUpdate(TeamProjectReference item)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            TeamProjectReference? entity;
            if ((entity = applicationDbContext.TeamProjects.Find(item.Id)) != null)
            {
                entity.RequiredPermissions = item.RequiredPermissions;
                entity.Name = item.Name;
                entity.Description = item.Description;
                entity.Revision = item.Revision;
                entity.Visibility = item.Visibility;
                entity.Abbreviation = item.Abbreviation;
                entity.DefaultTeamImageUrl = item.DefaultTeamImageUrl;
                entity.LastUpdateTime = item.LastUpdateTime;
                entity.NamespaceId = item.NamespaceId;
                entity.State = item.State;
                entity.Token = item.Token;
                entity.Url = item.Url;
            }
            else
                applicationDbContext.TeamProjects.Add(item);
            try
            {
                await applicationDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                statusService.SendStatus(ex);
            }
        }

        public async Task AddOrUpdate(Guid ProjectID, WorkItem item)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            WorkItem? entity;
            if ((entity = applicationDbContext.WorkItems.Find(item.Id)) != null)
            {
                entity.CompletedWork = item.CompletedWork;
                entity.Url = item.Url;
                entity.Title = item.Title;
                entity.OriginalEstimate = item.OriginalEstimate;
                entity.RemainingWork = item.RemainingWork;
                entity.State = item.State;
                entity.WorkItemType = item.WorkItemType;
            }
            else
            {
                var project = applicationDbContext.TeamProjects
                     .Include(j => j.workItems)
                     .Where(i => i.Id == ProjectID)
                     .FirstOrDefault();
                project.workItems.Add(item);
            }

            try
            {
                await applicationDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                statusService.SendStatus(ex);
            }
        }

        public async Task<WorkItem?> GetWorkItem(int taskId)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return await applicationDbContext.WorkItems.FindAsync(taskId);
        }

        public async Task Remove(TeamProjectReference item)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            var entity = await applicationDbContext.TeamProjects
                .OrderBy(i=>i.Id)
                .Include(i=>i.workItems)
                .ThenInclude(j=>j.WorkCalendarItems)
                .FirstOrDefaultAsync(i=>i.Id == item.Id);
            if (entity == null)
                return;
            applicationDbContext.Remove(entity);
            await applicationDbContext.SaveChangesAsync();
        }

        public async Task Remove(WorkItem item)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            var entity = await applicationDbContext.WorkItems.FindAsync(item.Id);
            if (entity == null)
                return;
            applicationDbContext.Remove(entity);
            await applicationDbContext.SaveChangesAsync();
        }
    }
}