using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface IStatusService
    {
        void SendStatus(string status);
        void SendStatus(EnBalloonIcon Icon,string title, string description);
        void SendStatus(Exception ex);
        void Stop();
    }
}