using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface IDataService
    {
        Task<IEnumerable<TeamProjectReference>> GetTeamProjectsAsync(CancellationToken cancellationToken);
        IEnumerable<TeamProjectReference> GetTeamProjects();
        Task<IEnumerable<WorkItem>> GetWorkItemsAsync(CancellationToken cancellationToken);
        IEnumerable<WorkItem> GetWorkItems();
        Task<WorkItem> AddOrUpdateAsync(Guid ProjectID, WorkItem item,CancellationToken cancellationToken);
        WorkItem AddOrUpdate(Guid ProjectID, WorkItem item);
        Task<WorkCalendarItem> AddOrUpdateAsync(int WorkItemID,WorkCalendarItem item, CancellationToken cancellationToken);
        WorkCalendarItem AddOrUpdate(int WorkItemID, WorkCalendarItem item);
        Task<TeamProjectReference> AddOrUpdateAsync(TeamProjectReference item, CancellationToken cancellationToken);
        TeamProjectReference AddOrUpdate(TeamProjectReference item);
        Task<WorkItem?> GetWorkItemAsync(int Id, CancellationToken cancellationToken);
        WorkItem? GetWorkItem(int Id);
        Task<WorkCalendarItem?> GetWorkCalendarItemAsync(int Id, CancellationToken cancellationToken);
        WorkCalendarItem? GetWorkCalendarItem(int Id);
        Task<List<WorkCalendarItem>> GetWorkCalendarItemsDailyAsync(DateTime date, CancellationToken cancellationToken);
        List<WorkCalendarItem> GetWorkCalendarItemsDaily(DateTime date);
        Task<List<WorkCalendarItem>> GetWorkCalendarItemsRangeAsync(DateTime dateStart,DateTime DateEnd, CancellationToken cancellationToken);
        List<WorkCalendarItem> GetWorkCalendarItemsRange(DateTime dateStart, DateTime DateEnd);
        Task RemoveAsync(TeamProjectReference item, CancellationToken cancellationToken);
        void Remove(TeamProjectReference item);
        Task RemoveAsync(WorkItem item, CancellationToken cancellationToken);
        void Remove(WorkItem item);
        Task<WorkCalendarItem?> GetWorkCalendarItemLastAsync(CancellationToken cancellationToken);
        WorkCalendarItem? GetWorkCalendarItemLast();
        IEnumerable<WorkCalendarItem> GetWorkCalendarItemsNotSynced();
        Task<IEnumerable<WorkCalendarItem>> GetWorkCalendarItemsNotSyncedAsync(CancellationToken cancellationToken);
        Task SetworkCalendarItemSyncedAsync(int id, CancellationToken cancellationToken);
        void SetworkCalendarItemSynced(int id);
        Task SetWorkCalendarItemDurationHourAsync(int workItemID, double durationHour, CancellationToken cancellationToken);
        void SetWorkCalendarItemDurationHour(int ID, double durationHour);
        IEnumerable<WorkCalendarItem> GetWorkCalendarFreeItemsDaily();
        Task<IEnumerable<WorkCalendarItem>> GetWorkCalendarFreeItemsDailyAsync(CancellationToken cancellationToken);
        WorkCalendarItem? GetWorkCalendarItemWithWorkItem(int id);
        Task<WorkCalendarItem?> GetWorkCalendarItemWithWorkItemAsync(int id, CancellationToken cancellationToken);
    }
}