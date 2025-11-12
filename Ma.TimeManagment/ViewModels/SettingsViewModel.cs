using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ma.TimeManagement.Services;
using Ma.TimeManagement.Views;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ma.TimeManagement.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string serverUrl = "https://cicd-server";

        [ObservableProperty]
        private string collection = "DefaultCollection";

        [ObservableProperty]
        private string project;

        [ObservableProperty]
        private string _pat;

        [RelayCommand]
        private void Connect()
        {
            try
            {
                var uri = new Uri($"{ServerUrl}/{Collection}");
                var credentials = new VssBasicCredential(string.Empty, Pat);
                AzureDevOpsService.Instance.Initialize(uri, credentials, Project);
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Connected successfully!"));
                (Application.Current.MainWindow as MainWindow)?.ViewModel.RefreshTasksCommand.Execute(null);
                Application.Current.Dispatcher.Invoke(() => Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault()?.Close());
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Error: {ex.Message}"));
            }
        }
    }
}
