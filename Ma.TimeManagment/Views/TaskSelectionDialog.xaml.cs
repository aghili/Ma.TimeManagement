using System.Windows;

namespace Ma.TimeManagement.Views
{
    public partial class TaskSelectionDialog : Window
    {
        public TaskSelectionDialog()
        {
            InitializeComponent();
        }

        private void OnStart(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}