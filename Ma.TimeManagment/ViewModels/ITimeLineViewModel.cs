using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ma.TimeManagement.Models;
using System.Collections.ObjectModel;

namespace Ma.TimeManagement.ViewModels
{
    public interface ITimeLineViewModel
    {
        public double TimelineWidth { get; }
        public ObservableCollection<TimelineItem> Items { set; get; }

        public double Zoom { set; get; }

        public IRelayCommand ZoomInCommand { get; }
        public IRelayCommand ZoomOutCommand { get; }
        public IRelayCommand ResetCommand { get; }
        public IRelayCommand StartTaskCommand { get; }
        public IRelayCommand InsertTaskCommand { get; }

    }
}