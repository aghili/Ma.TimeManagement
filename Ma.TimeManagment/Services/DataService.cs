using Ma.TimeManagement.Data;
using Ma.TimeManagement.Models;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Ma.TimeManagement.Services
{
    public class DataService : IDataService
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbContextFactory;
        private readonly IConverterService converterService;
        private readonly IStatusService statusService;

        public DataService(IDbContextFactory<ApplicationDbContext> dbContextFactory,IConverterService converterService, IStatusService statusService)
        {
            this.dbContextFactory = dbContextFactory;
            this.converterService = converterService;
            this.statusService = statusService;
        }

        public async Task<IEnumerable<TeamProjectReference>> GetTeamProjectsAsync()
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return [.. applicationDbContext.TeamProjects];
        }

        public IEnumerable<TeamProjectReference> GetTeamProjects()
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            return [.. applicationDbContext.TeamProjects];
        }

        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync()
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return [.. applicationDbContext.WorkItems];
        }

        public IEnumerable<WorkItem> GetWorkItems()
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            return [.. applicationDbContext.WorkItems];
        }

        public async Task<TeamProjectReference> AddOrUpdateAsync(TeamProjectReference item)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            TeamProjectReference? entity;
            if ((entity = await applicationDbContext.TeamProjects.FindAsync(item.Id)) != null)
            {
               SetProperties(item, entity);
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
            return item;
        }
        public TeamProjectReference AddOrUpdate(TeamProjectReference item)
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            TeamProjectReference? entity;
            if ((entity = applicationDbContext.TeamProjects.Find(item.Id)) != null)
            {
                SetProperties(item, entity);
            }
            else
                applicationDbContext.TeamProjects.Add(item);
            try
            {
                applicationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                statusService.SendStatus(ex);
            }
            return item;
        }

        private static void SetProperties(TeamProjectReference item, TeamProjectReference entity)
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

        public async Task<WorkItem> AddOrUpdateAsync(Guid ProjectID, WorkItem item)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            WorkItem? entity;
            if ((entity = await applicationDbContext.WorkItems.FindAsync(item.Id)) != null)
            {
                SetProperties(item, entity);
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
            return item;
        }

        public WorkItem AddOrUpdate(Guid ProjectID, WorkItem item)
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            WorkItem? entity;
            if ((entity = applicationDbContext.WorkItems.Find(item.Id)) != null)
            {
                SetProperties(item, entity);
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
                applicationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                statusService.SendStatus(ex);
            }
            return item;
        }

        private static void SetProperties(WorkItem item, WorkItem entity)
        {
            entity.CompletedWork = item.CompletedWork;
            entity.Url = item.Url;
            entity.Title = item.Title;
            entity.OriginalEstimate = item.OriginalEstimate;
            entity.RemainingWork = item.RemainingWork;
            entity.State = item.State;
            entity.WorkItemType = item.WorkItemType;
        }

        public async Task<WorkItem?> GetWorkItemAsync(int taskId)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return await applicationDbContext.WorkItems.FindAsync(taskId);
        }
        public WorkItem? GetWorkItem(int taskId)
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            return applicationDbContext.WorkItems.Find(taskId);
        }

        public async Task RemoveAsync(TeamProjectReference item)
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
        public void Remove(TeamProjectReference item)
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            var entity = applicationDbContext.TeamProjects
                .OrderBy(i => i.Id)
                .Include(i => i.workItems)
                .ThenInclude(j => j.WorkCalendarItems)
                .FirstOrDefault(i => i.Id == item.Id);
            if (entity == null)
                return;
            applicationDbContext.Remove(entity);
            applicationDbContext.SaveChanges();
        }

        public async Task RemoveAsync(WorkItem item)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            var entity = await applicationDbContext.WorkItems.FindAsync(item.Id);
            if (entity == null)
                return;
            applicationDbContext.Remove(entity);
            await applicationDbContext.SaveChangesAsync();
        }
        public void Remove(WorkItem item)
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            var entity = applicationDbContext.WorkItems.Find(item.Id);
            if (entity == null)
                return;
            applicationDbContext.Remove(entity);
            applicationDbContext.SaveChanges();
        }


        public async Task<WorkCalendarItem> AddOrUpdateAsync(int WorkItemID, WorkCalendarItem item)
        {
            PerpareItem(item);
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            WorkCalendarItem? entity;
            if ((entity = await applicationDbContext.WorkCalendarItems.FindAsync(item.Id)) != null)
            {
                SetProperties(item, entity);
            }
            else
            {
                var project = applicationDbContext.WorkItems
                     .Include(j => j.WorkCalendarItems)
                     .Where(i => i.Id == WorkItemID)
                     .FirstOrDefault();
                project.WorkCalendarItems.Add(item);
            }

            try
            {
                await applicationDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                statusService.SendStatus(ex);
            }
            return item;
        }

        public WorkCalendarItem AddOrUpdate(int WorkItemID, WorkCalendarItem item)
        {
            PerpareItem(item);
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            WorkCalendarItem? entity;
            if ((entity = applicationDbContext.WorkCalendarItems.Find(item.Id)) != null)
            {
                SetProperties(item, entity);
            }
            else
            {
                var project = applicationDbContext.WorkItems
                     .Include(j => j.WorkCalendarItems)
                     .Where(i => i.Id == WorkItemID)
                     .FirstOrDefault();
                project.WorkCalendarItems.Add(item);
            }

            try
            {
                applicationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                statusService.SendStatus(ex);
            }
            return item;
        }

        private static void SetProperties(WorkCalendarItem item, WorkCalendarItem entity)
        {
            entity.DurationHour = item.DurationHour;
            entity.WorkItemID = item.WorkItemID;
            entity.StartTime = item.StartTime;
            entity.Title = item.Title;
            entity.Description = item.Description;
        }

        private void PerpareItem(WorkCalendarItem item)
        {
            item.Description = item.Description ?? "";
            item.Title = item.Title ?? "";
        }

        public async Task<WorkCalendarItem?> GetWorkCalendarItemAsync(int Id)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return await applicationDbContext.WorkCalendarItems.FindAsync(Id);
        }
        public WorkCalendarItem? GetWorkCalendarItem(int Id)
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            return applicationDbContext.WorkCalendarItems.Find(Id);
        }

        public async Task<List<WorkCalendarItem>> GetWorkCalendarItemsDailyAsync(DateTime date)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return await applicationDbContext.WorkCalendarItems
                .Where(i => i.StartTime >= DateTime.Now.Date && i.StartTime <DateTime.Now.AddDays(1).Date)
                .ToListAsync();
        }
        public List<WorkCalendarItem> GetWorkCalendarItemsDaily(DateTime date)
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            return [.. applicationDbContext.WorkCalendarItems.Where(i => i.StartTime >= DateTime.Now.Date && i.StartTime < DateTime.Now.AddDays(1).Date)];
        }
        public async Task<List<WorkCalendarItem>> GetWorkCalendarItemsRangeAsync(DateTime dateStart, DateTime DateEnd)
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return await applicationDbContext.WorkCalendarItems
                .Where(i => i.StartTime >= dateStart && i.StartTime < DateEnd)
                .ToListAsync();
        }
        public List<WorkCalendarItem> GetWorkCalendarItemsRange(DateTime dateStart, DateTime DateEnd)
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            return [.. applicationDbContext.WorkCalendarItems.Where(i => i.StartTime >= dateStart && i.StartTime < DateEnd)];
        }

        public async Task<WorkCalendarItem?> GetWorkCalendarItemLastAsync()
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return await applicationDbContext.WorkCalendarItems.Where(i => i.StartTime.Date == DateTime.Today).OrderBy(i=>i.Id).LastOrDefaultAsync();
        }

        public WorkCalendarItem? GetWorkCalendarItemLast()
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            return applicationDbContext.WorkCalendarItems.Where(i => i.StartTime.Date == DateTime.Today).OrderBy(i => i.Id).LastOrDefault();
        }

        public IEnumerable<WorkCalendarItem> GetWorkCalendarItemsNotSynced()
        {
            using var applicationDbContext = dbContextFactory.CreateDbContext();
            return [.. applicationDbContext.WorkCalendarItems.Where(i => i.Synced == false)];
        }

        public async Task<IEnumerable<WorkCalendarItem>> GetWorkCalendarItemsNotSyncedAsync()
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            return await applicationDbContext.WorkCalendarItems.Where(i => i.Synced == false).ToListAsync();
        }

        public async Task SetworkCalendarItemSyncedAsync(int id)
        {
            var item = await GetWorkCalendarItemAsync(id);
            if (item == null)
                return;
            item.Synced = true;
            await AddOrUpdateAsync(item.WorkItemID ?? 0, item);
        }

        public void SetworkCalendarItemSynced(int id)
        {
            var item = GetWorkCalendarItem(id);
            if (item == null)
                return;
            item.Synced = true;
            AddOrUpdate(item.WorkItemID ?? 0, item);
        }

        public async Task SetWorkCalendarItemDurationHourAsync(int ID, double durationHour)
        {
            var item = await GetWorkCalendarItemAsync(ID);
            if (item == null)
                return;
            item.DurationHour = durationHour;
            await AddOrUpdateAsync(item.WorkItemID ?? 0, item);
        }
        public void SetWorkCalendarItemDurationHour(int ID, double durationHour)
        {
            var item = GetWorkCalendarItem(ID);
            if (item == null)
                return;
            item.DurationHour = durationHour;
            AddOrUpdate(item.WorkItemID ?? 0, item);
        }

        public IEnumerable<WorkCalendarItem> GetWorkCalendarFreeItemsDaily()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<WorkCalendarItem>> GetWorkCalendarFreeItemsDailyAsync()
        {
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
            var Tasks = await applicationDbContext.WorkCalendarItems
                .Where(i => i.StartTime >= DateTime.Today && i.StartTime < DateTime.Now.AddDays(1).Date)
                .OrderBy(i=>i.StartTime)
                .ToListAsync();
            if (Tasks.Count == 0)
                return [new() { StartTime = DateTime.Today.AddHours(8) }];
            List<WorkCalendarItem> results = [];
            DateTime lastDate = DateTime.Today.AddHours(8);
            double Duration = 0;
            foreach (var item in Tasks)
            {
                Duration = (item.StartTime - lastDate).TotalHours;
                if (Duration <= 0)
                    continue;
                results.Add(new(){ StartTime =lastDate,DurationHour =converterService.ConvertHourToRounded(Duration) });
                lastDate = item.StartTime.AddHours(item.DurationHour);
            }
            Duration = (DateTime.Today.AddHours(22)- lastDate).TotalHours;
            results.Add(new() { StartTime = lastDate, DurationHour = converterService.ConvertHourToRounded(Duration) });
            return results;
        }
    }
}