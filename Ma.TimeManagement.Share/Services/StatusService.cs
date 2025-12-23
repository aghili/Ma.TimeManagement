using Ma.TimeManagement.Models;
using System.Drawing;

namespace Ma.TimeManagement.Services
{
    public class StatusServiceConsole : IStatusService
    {
        public void SendStatus(EnBalloonIcon icon,string title, string description)
        {
        }

        public void RefreshTasks()
        {
        }

        public void RefreshWorkCalendarItem(int WorkCalendarItemId)
        {
        }

        public void RegisterRefreshTasks(object Host,Action value)
        {
        }

        public void SendStatus(Exception ex)
        {
        }

        public void SendStatus(string status)
        {
        }

        public void Stop()
        {
        }
    }
}