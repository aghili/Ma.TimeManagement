

using Hardcodet.Wpf.TaskbarNotification;
using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface IStatusService
    {
        void RefreshItem(WorkCalendarItem item);
        void RefreshTasks();
        void SendStatus(string status);
        void SendStatus(BalloonIcon Icon,string title, string description);
        void SendStatus(Exception ex);
    }
}