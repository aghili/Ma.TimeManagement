using Ma.TimeManagement.Models;
using Ma.TimeManagement.ViewModels;
using System.Windows;

namespace Ma.TimeManagement.Views
{
    public partial class TaskDiscussionDialog : Window
    {
        public TaskDiscussionDialog()
        {
            InitializeComponent();
        }

        private void OnAccept(object sender, RoutedEventArgs e)
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
            { 
                (DataContext as TaskSelectionViewModel).Duration = (e.AddedItems[0] as WorkCalendarItem).DurationHour;
                (DataContext as TaskSelectionViewModel).TimeStart = (e.AddedItems[0] as WorkCalendarItem).StartTime;
            }
        }
    }
}