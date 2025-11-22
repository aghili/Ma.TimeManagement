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
    }
}