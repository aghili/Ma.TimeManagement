using CommunityToolkit.Mvvm.ComponentModel;

namespace Ma.TimeManagement.Services
{
    public class NavigationStore : ObservableObject, INavigationStore
    {
        private ObservableObject _currentViewModel;

        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }
    }
}
