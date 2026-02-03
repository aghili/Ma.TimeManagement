using CommunityToolkit.Mvvm.ComponentModel;
using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.ViewModels
{
    public partial class TaskDiscussionViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _discussion = string.Empty;
        [ObservableProperty]
        private WorkItem _task;
    }
}