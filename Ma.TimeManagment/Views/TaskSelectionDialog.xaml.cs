using Ma.TimeManagement.Models;
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

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                txt_duration.Text = (e.AddedItems[0] as WorkCalendarItem).DurationHour.ToString();
        }
    }
}