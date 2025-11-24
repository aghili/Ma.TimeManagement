using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Ma.TimeManagement.Services
{
    public class NavigationService : INavigationService
    {
        private readonly INavigationStore _navigationStore;
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(INavigationStore navigationStore, IServiceProvider serviceProvider)
        {
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
        {
            var viewModel = _serviceProvider.GetService<TViewModel>();
            _navigationStore.CurrentViewModel = viewModel;
        }
    }
}
