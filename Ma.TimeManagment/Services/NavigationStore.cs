using CommunityToolkit.Mvvm.ComponentModel;

namespace Ma.TimeManagement.Services
{
    public class NavigationStore : ObservableObject
    {
        private ObservableObject _currentViewModel;

        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }
    }
}
