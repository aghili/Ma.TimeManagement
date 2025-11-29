using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Ma.TimeManagement.Data;
using Ma.TimeManagement.Services;
using Ma.TimeManagement.ViewModels;
using Ma.TimeManagement.Views;
using Ma.TimeManagement.Windows;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using Wpf.Ui.Appearance;

namespace Ma.TimeManagement
{
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging

        private static IHost _host;

        public App()
        {
            var staticDataInstance = new StaticDataService();
            _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(staticDataInstance.PathConfiguration); })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection") ?? new SqliteConnectionStringBuilder() { DataSource = staticDataInstance.PathFullDatabase, Cache = SqliteCacheMode.Shared, Pooling = true }.ConnectionString;
                context.Configuration["DefaultConnection"] = connectionString;

                services.AddSingleton<IStaticDataService>(staticDataInstance);
                services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(connectionString));
                services.AddTransient<IDataService, DataService>();
                services.AddTransient<ITimeManagementService, TimeManagementService>();

                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<INavigationStore, NavigationStore>();
#if DEBUGWITHDISABLEAZURE
                services.AddSingleton<IAzureDevOpsService, Services.Design.AzureDevOpsService>();
#else
                services.AddSingleton<IAzureDevOpsService, Services.AzureDevOpsService>();

#endif                
                services.AddSingleton<ISettingsService, SettingsService>();

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<HomeViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddSingleton<TimelineViewModel>();

                services.AddTransient<IDialogService, DialogService>();
                //services.AddHostedService<ApplicationHostService>();

                //services.AddSingleton<IConverterService, ConverterService>();

                //services.AddSingleton<IStaticSettingService, StaticSettingService>();

                services.AddSingleton<IStatusService, StatusService>();
                services.AddSingleton<IConverterService, ConverterService>();

                //services.AddSingleton<IEnvironmentService, EnvironmentService>();

                // Service containing navigation, same as INavigationWindow... but without window

                // Main window with navigation
                services.AddSingleton<MainWindow>();

                services.AddSingleton(sp => new MainWindow
                {
                    DataContext = sp.GetRequiredService<MainViewModel>()
                });

                services.AddHostedService<TimeEngineService>();

            }).Build();
            using (var scope = _host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var dbContextFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

                using var applicationDbContext = dbContextFactory.CreateDbContext();

                applicationDbContext.Database.Migrate();
            }

            SetupExceptionHandling();
        }

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await _host.StartAsync();
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
         
            //ApplicationThemeManager.ApplySystemTheme();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        protected override async void OnExit(ExitEventArgs e)
        {
            var statusService = GetService<IStatusService>();
            statusService.Stop();
            base.OnExit(e);

            // Signal the host to stop when the WPF application exits
            await _host.StopAsync();

            // Dispose of the host
            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        public void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var status_service = GetService<IStatusService>();
            if (status_service != null)
            {
                status_service.SendStatus(e.Exception);
                e.Handled = true;
            }
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            var status_service = GetService<IStatusService>();
            var logger = GetService<ILogger>();
            string message = $"Unhandled exception ({source})";
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, GetType().Name, []);
                status_service.SendStatus(ex);
            }
            finally
            {
                status_service.SendStatus(BalloonIcon.Error,"Error", $"{message}\n{exception.Message}");
            }
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