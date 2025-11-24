using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface IDataService
    {
        Task<IEnumerable<TeamProjectReference>> GetTeamProjectsAsync();
        IEnumerable<TeamProjectReference> GetTeamProjects();
        Task<IEnumerable<WorkItem>> GetWorkItemsAsync();
        IEnumerable<WorkItem> GetWorkItems();
        Task AddOrUpdateAsync(Guid ProjectID, WorkItem item);
        void AddOrUpdate(Guid ProjectID, WorkItem item);
        Task AddOrUpdateAsync(int WorkItemID,WorkCalendarItem item);
        void AddOrUpdate(int WorkItemID, WorkCalendarItem item);
        Task AddOrUpdateAsync(TeamProjectReference item);
        void AddOrUpdate(TeamProjectReference item);
        Task<WorkItem?> GetWorkItemAsync(int Id);
        WorkItem? GetWorkItem(int Id);
        Task<WorkCalendarItem?> GetWorkCalendarItemAsync(int Id);
        WorkCalendarItem? GetWorkCalendarItem(int Id);
        Task<List<WorkCalendarItem>> GetWorkCalendarItemsDailyAsync(DateTime date);
        List<WorkCalendarItem> GetWorkCalendarItemsDaily(DateTime date);
        Task<List<WorkCalendarItem>> GetWorkCalendarItemsRangeAsync(DateTime dateStart,DateTime DateEnd);
        List<WorkCalendarItem> GetWorkCalendarItemsRange(DateTime dateStart, DateTime DateEnd);
        Task RemoveAsync(TeamProjectReference item);
        void Remove(TeamProjectReference item);
        Task RemoveAsync(WorkItem item);
        void Remove(WorkItem item);
        Task<WorkCalendarItem?> GetWorkCalendarItemLastAsync();
        WorkCalendarItem? GetWorkCalendarItemLast();
        IEnumerable<WorkCalendarItem> GetWorkCalendarItemsNotSynced();
        Task<IEnumerable<WorkCalendarItem>> GetWorkCalendarItemsNotSyncedAsync();
        Task SetworkCalendarItemSyncedAsync(int id);
        void SetworkCalendarItemSynced(int id);
        Task SetWorkCalendarItemDurationHourAsync(int workItemID, double durationHour);
        void SetWorkCalendarItemDurationHour(int ID, double durationHour);
    }
}