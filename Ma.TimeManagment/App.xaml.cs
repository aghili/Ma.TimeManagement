using System;
using System.Drawing;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Wpf.Ui.Appearance;

namespace Ma.TimeManagement
{
    public partial class App : Application
    {
        public TaskbarIcon _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create tray icon
            _notifyIcon = new TaskbarIcon
            {
                Icon = SystemIcons.Application, // Add an icon file to project (or use SystemIcons.Application)
                ToolTipText = "Ma.TimeManagement - Time Tracking",
                Visibility = Visibility.Visible
            };

            // Context menu for tray
            var contextMenu = new System.Windows.Controls.ContextMenu();
            var showMenuItem = new System.Windows.Controls.MenuItem { Header = "Show Window" };
            showMenuItem.Click += (s, args) => MainWindow.ShowAndActivate();
            var exitMenuItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
            exitMenuItem.Click += (s, args) => Shutdown();
            contextMenu.Items.Add(showMenuItem);
            contextMenu.Items.Add(exitMenuItem);
            _notifyIcon.ContextMenu = contextMenu;

            // Double-click tray to show window
            _notifyIcon.TrayMouseDoubleClick += (s, args) => MainWindow.ShowAndActivate();

            ApplicationThemeManager.ApplySystemTheme();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }

    public static class WindowExtensions
    {
        public static void ShowAndActivate(this Window window)
        {
            if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
            window.Show();
            window.Activate();
        }
    }
}