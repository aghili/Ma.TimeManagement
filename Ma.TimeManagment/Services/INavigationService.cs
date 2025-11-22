using CommunityToolkit.Mvvm.ComponentModel;

namespace Ma.TimeManagement.Services
{
    public interface INavigationService
    {
        void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
    }
}
