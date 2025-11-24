using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Ma.TimeManagement.Services
{
    public interface INavigationStore: INotifyPropertyChanged, INotifyPropertyChanging
    {
        ObservableObject CurrentViewModel { get; set; }
    }
}