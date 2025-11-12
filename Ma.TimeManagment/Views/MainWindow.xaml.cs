using Ma.TimeManagement.Services;
using Ma.TimeManagement.ViewModels;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Ma.TimeManagement.Views
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ApplicationThemeManager.Apply(
    ApplicationTheme.Light,
    WindowBackdropType.Mica
);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private void OpenCreate_Click(object sender, RoutedEventArgs e)
        {
            new CreateWorkItemWindow().ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AzureDevOpsService.Instance.WitClient == null)
            {
                new SettingsWindow().ShowDialog();
            }
            ViewModel.RefreshTasksCommand.Execute(null);
        }
    }
}