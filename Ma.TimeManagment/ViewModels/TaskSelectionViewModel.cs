using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        [NotifyCanExecuteChangedFor(nameof(dummy_funcCommand))]
        private WorkCalendarItem? _selectedWorkCalendarItem;

        [RelayCommand(CanExecute =nameof(SelectedWorkCalendarItemChange))]
        private void dummy_func()
        {
            return;
        }
        private bool SelectedWorkCalendarItemChange()
        {
            Duration = SelectedWorkCalendarItem.DurationHour;
            return true;
        }
        [ObservableProperty]
        private double _duration = 0;
        [ObservableProperty]
        private DateTime _timeStart = DateTime.Today.AddHours(8);
    }
}