using CommunityToolkit.Mvvm.ComponentModel;
using Ma.TimeManagement.Models;
using System.Collections.ObjectModel;

namespace Ma.TimeManagement.ViewModels
{
    public partial class TaskSelectionViewModel : ObservableObject
    {
        public ObservableCollection<WorkItem> AvailableTasks { get; } =[];

        public ObservableCollection<WorkCalendarItem> WorkCalendarItems { get; } = [];

        [ObservableProperty]
        private WorkItem _selectedTask;
        [ObservableProperty]
        private WorkCalendarItem? _selectedWorkCalendarItem;
    }
}