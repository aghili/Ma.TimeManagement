using Ma.TimeManagement.Services;
using Ma.TimeManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Ma.TimeManagement.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as HomeViewModel).RefreshTasksCommand.Execute(null);
        }

        private void TasksListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (DataContext as HomeViewModel).ShowTaskCommand.Execute(null);
        }
    }
}