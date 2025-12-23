namespace Ma.TimeManagement.Services
{
    public interface IMessageService
    {
        void RefreshWorkCalendarItem(int WorkCalendarItemId);
        void RefreshTasks();
    }
}