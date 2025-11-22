using Ma.TimeManagement.ViewModels;
using System.Windows.Controls;

namespace Ma.TimeManagement.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void PatBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as SettingsViewModel).Pat = (sender as PasswordBox).Password;
        }
    }
}
