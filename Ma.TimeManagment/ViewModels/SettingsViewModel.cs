using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using System.Windows;

namespace Ma.TimeManagement.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly AzureDevOpsService azureDevOpsService;
        private readonly SettingsService settingsService;
        private readonly IStatusService statusService;

        public AzureServerItemModel Server { get; }

        public SettingsViewModel(INavigationService navigationService,AzureDevOpsService azureDevOpsService,SettingsService settingsService,IStatusService statusService)
        {
            _navigationService = navigationService;
            this.azureDevOpsService = azureDevOpsService;
            this.settingsService = settingsService;
            this.statusService = statusService;
            Server = settingsService.FirstServer;
            serverUrl = Server.ServerUrl;
            collection = Server.Collection;
            project = Server.Project;
            _pat = Server.PAT;
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

        [RelayCommand]
        private void Connect()
        {
            try
            {
                azureDevOpsService.Initialize(ServerUrl, Collection, Project,Pat);
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Connected successfully!"));
                settingsService.FirstServer = new AzureServerItemModel
                {
                    ServerUrl = ServerUrl,
                    Collection = Collection,
                    Project = Project,
                    PAT = Pat
                };
                statusService.RefreshTasks();

                NavigateToHome();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Error: {ex.Message}"));
            }
        }
    }
}
