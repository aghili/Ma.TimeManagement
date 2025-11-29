using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hardcodet.Wpf.TaskbarNotification;
using Ma.TimeManagement.Models;
using Ma.TimeManagement.ViewModels;
using Ma.TimeManagement.Windows;
using System.Drawing;
using System.IO;
using System.Windows;

namespace Ma.TimeManagement.Services
{
    public class StatusService : IStatusService
    {
        public TaskbarIcon _notifyIcon;

        public StatusService()
        {
            // Create tray icon
            _notifyIcon = new TaskbarIcon
            {
                Icon = new Icon(new MemoryStream(Properties.Resources.Icon)), // Add an icon file to project (or use SystemIcons.Application)
                ToolTipText = "Ma.TimeManagement - Time Tracking",
                
                Visibility = Visibility.Visible
            };

            // Context menu for tray
            var contextMenu = new System.Windows.Controls.ContextMenu();
            var showMenuItem = new System.Windows.Controls.MenuItem { Header = "Show Window" };
            showMenuItem.Click += (s, args) => App.Current.MainWindow.ShowAndActivate();
            var exitMenuItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
            exitMenuItem.Click += (s, args) => App.Current.Shutdown();
            contextMenu.Items.Add(showMenuItem);
            contextMenu.Items.Add(exitMenuItem);
            _notifyIcon.ContextMenu = contextMenu;
            _notifyIcon.DoubleClickCommand = new RelayCommand(() => { App.Current.MainWindow.ShowAndActivate(); });

            // Double-click tray to show window
            _notifyIcon.TrayMouseDoubleClick += (s, args) => App.Current.MainWindow.ShowAndActivate();

        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
        }

        public void SendStatus(BalloonIcon icon,string title, string description)
        {
            StatusModel message = new(icon,title, description);
            
            _notifyIcon.ShowBalloonTip(title, description, icon);
            StrongReferenceMessenger.Default.Send(message,EnStatusAction.Message.ToString());
        }

        public void RefreshTasks()
        {
            StatusActionModel message = new(EnStatusAction.RefreshTasks);
            StrongReferenceMessenger.Default.Send(message,EnStatusAction.RefreshTasks.ToString());
        }

        public void RefreshItem(WorkCalendarItem item)
        {
            StrongReferenceMessenger.Default.Send(item, EnStatusAction.RefreshItem.ToString());
        }

        public void RegisterRefreshTasks(object Host,Action value)
        {
            StrongReferenceMessenger.Default.Register<StatusModel,string>(Host,EnStatusAction.RefreshTasks.ToString(), (r, m) =>
            {
                value.Invoke();
            });
        }

        public void SendStatus(Exception ex)
        {
            SendStatus(BalloonIcon.Error, "Exception", ex.Message);
        }

        public void SendStatus(string status)
        {
            SendStatus(BalloonIcon.Info, "Info", status);
        }

        public void Stop()
        {
            _notifyIcon?.Dispose();
        }
    }
}