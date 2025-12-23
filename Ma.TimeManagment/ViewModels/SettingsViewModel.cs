using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Ma.TimeManagement.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ILogger<SettingsViewModel> logger;
        private readonly INavigationService _navigationService;
        private readonly ISettingsService settingsService;
        private readonly IMessageService messageService;

        public AzureServerItemModel Server { get; }

        public SettingsViewModel(ILogger<SettingsViewModel> logger,INavigationService navigationService,ISettingsService settingsService,IMessageService messageService)
        {
            this.logger = logger;
            _navigationService = navigationService;
            this.settingsService = settingsService;
            this.messageService = messageService;
            Server = settingsService.FirstServer;
            serverUrl = Server.ServerUrl;
            collection = Server.Collection;
            project = Server.Project;
            _pat = Server.PAT;
            _bypassProxyOnLocal = settingsService.BypassProxyOnLocal;
        }

        [RelayCommand]
        private void NavigateToHome() => _navigationService.NavigateTo<HomeViewModel>();

        [ObservableProperty]
        private string serverUrl ;

        [ObservableProperty]
        private string collection ;

        [ObservableProperty]
        private string project;

        [ObservableProperty]
        private string _pat;

        [ObservableProperty]
        private bool _bypassProxyOnLocal;

        [RelayCommand]
        private void Connect()
        {
            try
            {
                    settingsService.FirstServer = new AzureServerItemModel
                    {
                        ServerUrl = ServerUrl,
                        Collection = Collection,
                        Project = Project,
                        PAT = Pat
                    };
                    settingsService.BypassProxyOnLocal = BypassProxyOnLocal;
                    messageService.RefreshTasks();

                    NavigateToHome();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, GetType().Name, []);
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Error: {ex.Message}"));
            }
        }
    }
}
