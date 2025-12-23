using System.Windows;

namespace Ma.TimeManagement.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent the window from actually closing (and stopping the app)
            e.Cancel = true;

            // Hide the window instead
            this.Hide();
        }
    }
}
