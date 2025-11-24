using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ma.TimeManagement.Services;
using Ma.TimeManagement.Views;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.Windows;
using System.Windows.Navigation;

namespace Ma.TimeManagement.ViewModels
{
    public partial class CreateWorkItemViewModel : ObservableObject
    {
        public CreateWorkItemViewModel(INavigationService navigationService,IAzureDevOpsService azureDevOpsService,IStatusService statusService)
        {
            this.navigationService = navigationService;
            this.azureDevOpsService = azureDevOpsService;
            this.statusService = statusService;
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
        private readonly INavigationService navigationService;
        private readonly IAzureDevOpsService azureDevOpsService;
        private readonly IStatusService statusService;

        [RelayCommand(CanExecute = nameof(CanCreate))]
        private async Task Create()
        {
            var type = SelectedTypeIndex == 0 ? "Task" : "User Story";
            try
            {
                var patch = new JsonPatchDocument();
                patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = Title });

                if (!string.IsNullOrEmpty(Description))
                {
                    patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Description", Value = Description });
                }

                if (!string.IsNullOrEmpty(ParentId) && int.TryParse(ParentId, out int parentId))
                {
                    var parent = await azureDevOpsService.GetWorkItemAsync(parentId);
                    patch.Add(new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/relations/-",
                        Value = new WorkItemRelation
                        {
                            Rel = "System.LinkTypes.Hierarchy-Reverse",
                            Url = parent.Url
                        }
                    });
                }

                var created = await azureDevOpsService.CreateWorkItemAsync(patch,Guid.NewGuid(), type);
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Created {type} ID: {created.Id}"));
                statusService.RefreshTasks();
                NavigateToHome();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Error creating: {ex.Message}"));
            }
        }

        private bool CanCreate() => !string.IsNullOrEmpty(Title);
    }
}
