using CommunityToolkit.Mvvm.ComponentModel;
using Ma.TimeManagement.Models;
using System.Collections.ObjectModel;

namespace Ma.TimeManagement.ViewModels
{
    public partial class TaskSelectionViewModel : ObservableObject
    {
        public ObservableCollection<WorkItem> AvailableTasks { get; } = new ObservableCollection<WorkItem>();

        [ObservableProperty]
        private WorkItem _selectedTask;
    }
}