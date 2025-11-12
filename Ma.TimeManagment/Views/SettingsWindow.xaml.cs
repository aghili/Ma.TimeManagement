using Ma.TimeManagement.ViewModels;
using System.Windows;

namespace Ma.TimeManagement.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}
