using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Ma.TimeManagement.ViewModels
{
    public partial class CreateWorkItemViewModel : ObservableObject
    {
        public CreateWorkItemViewModel(ILogger<CreateWorkItemViewModel> logger,INavigationService navigationService,IAzureDevOpsService azureDevOpsService,IMessageService messageService)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.azureDevOpsService = azureDevOpsService;
            this.messageService = messageService;
        }
        
        [RelayCommand]
        private void NavigateToHome() => navigationService.NavigateTo<HomeViewModel>();
        
        [ObservableProperty]
        private int _selectedTypeIndex = 0;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _parentId;
        private readonly ILogger<CreateWorkItemViewModel> logger;
        private readonly INavigationService navigationService;
        private readonly IAzureDevOpsService azureDevOpsService;
        private readonly IMessageService messageService;

        [RelayCommand(CanExecute = nameof(CanCreate))]
        private async Task Create(CancellationToken cancellationToken)
        {
            var type = SelectedTypeIndex == 0 ? EnWorkItemType.Task : EnWorkItemType.UserStory;
            try
            {
                var workItem = new WorkItemAddDto()
                {
                    Title = Title,
                    Discution = Description,
                    WorkItemType = type,
                };
                var created = await azureDevOpsService.CreateWorkItemAsync(workItem,cancellationToken);
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Created {type} ID: {created.Id}"));
                messageService.RefreshTasks();
                NavigateToHome();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, GetType().Name, []);
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Error creating: {ex.Message}"));
            }
        }

        private bool CanCreate() => !string.IsNullOrEmpty(Title);
    }
}
