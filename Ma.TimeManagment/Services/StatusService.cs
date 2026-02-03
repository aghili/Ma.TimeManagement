using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hardcodet.Wpf.TaskbarNotification;
using Ma.TimeManagement.Models;
using System.Drawing;
using System.IO;
using System.Windows;

namespace Ma.TimeManagement.Services
{
    public class ThemeService : IThemeService
    {
        public void SetLightTheme()
        {
            Application.Current.ThemeMode = ThemeMode.Light;
        }

        public void SetDarkTheme()
        {
            Application.Current.ThemeMode = ThemeMode.Dark;
        }

        public void SetDefaultTheme()
        {
            Application.Current.ThemeMode = ThemeMode.System;
        }

    }
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

        public void SendStatus(EnBalloonIcon icon,string title, string description)
        {
            StatusModel message = new(icon, description, title);
            
            _notifyIcon.ShowBalloonTip(title, description, (BalloonIcon)icon);
            StrongReferenceMessenger.Default.Send(message,EnStatusAction.Message.ToString());
        }

        public void SendStatus(Exception ex)
        {
            SendStatus(EnBalloonIcon.Error, "Exception", ex.Message);
        }

        public void SendStatus(string status)
        {
            SendStatus(EnBalloonIcon.Info, "Info", status);
        }

        public void Stop()
        {
            _notifyIcon?.Dispose();
        }
    }
}