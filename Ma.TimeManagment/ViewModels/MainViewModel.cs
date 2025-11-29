// MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Ma.TimeManagement.ViewModels
{

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject currentViewModel;
        [ObservableProperty]
        private string message;
        private readonly ILogger<MainViewModel> logger;
        private readonly INavigationStore _navigationStore;
        private readonly INavigationService _navigationService;

        public MainViewModel(ILogger<MainViewModel> logger,INavigationStore navigationStore, INavigationService navigationService)
        {
            this.logger = logger;
            _navigationStore = navigationStore;
            _navigationService = navigationService;

            CurrentViewModel = _navigationStore.CurrentViewModel;
            _navigationStore.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(NavigationStore.CurrentViewModel))
                {
                    CurrentViewModel = _navigationStore.CurrentViewModel;
                }
            };

            // Default navigation
            _navigationService.NavigateTo<HomeViewModel>();

            StrongReferenceMessenger.Default.Register<StatusModel, string>(this, EnStatusAction.Message.ToString(), (r, m) =>
            {
                Message = $"{m.Title}:{m.Description}";
            });

        }

        [RelayCommand]
        private void NavigateHome()
        {
            try
            {
                _navigationService.NavigateTo<HomeViewModel>();
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        [RelayCommand]
        private void NavigateTimeline()
        {
            try
            {
                _navigationService.NavigateTo<TimelineViewModel>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, GetType().Name, []);
                Console.WriteLine(ex);
            }
        }

        [RelayCommand]
        private void NavigateSettings() {
            try
            {
                _navigationService.NavigateTo<SettingsViewModel>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, GetType().Name, []);
                Console.WriteLine(ex);
            }
        }

        [RelayCommand]
        private void Minimize()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
    }
}