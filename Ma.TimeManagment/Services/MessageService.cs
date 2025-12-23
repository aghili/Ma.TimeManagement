using CommunityToolkit.Mvvm.Messaging;
using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public class MessageService : IMessageService
    {
        private readonly IDataService dataService;

        public MessageService(IDataService dataService)
        {
            this.dataService = dataService;
        }

        public void RefreshTasks()
        {
            StatusActionModel message = new(EnStatusAction.RefreshTasks);
            StrongReferenceMessenger.Default.Send(message, EnStatusAction.RefreshTasks.ToString());
        }

        public void RefreshWorkCalendarItem(int WorkCalendarItemId)
        {
            var item = dataService.GetWorkCalendarItem(WorkCalendarItemId);
            if (item != null)
            {
                StrongReferenceMessenger.Default.Send(item, EnStatusAction.RefreshItem.ToString());
            }
        }

        public void RegisterRefreshTasks(object Host, Action value)
        {
            StrongReferenceMessenger.Default.Register<StatusModel, string>(Host, EnStatusAction.RefreshTasks.ToString(), (r, m) =>
            {
                value.Invoke();
            });
        }
    }
}