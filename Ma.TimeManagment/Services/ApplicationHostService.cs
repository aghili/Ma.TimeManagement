using Ma.TimeManagement.Views;
using Ma.TimeManagement.Windows;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Ma.TimeManagement.Services
{
    //public class ApplicationHostService : IHostedService
    //{
    //    private readonly IServiceProvider _serviceProvider;

    //    private NavigationWindow _navigationWindow;

    //    public ApplicationHostService(IServiceProvider serviceProvider)
    //    {
    //        _serviceProvider = serviceProvider;
    //    }

    //    /// <summary>
    //    /// Triggered when the application host is ready to start the service.
    //    /// </summary>
    //    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    //    public async Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        await HandleActivationAsync();
    //    }

    //    /// <summary>
    //    /// Triggered when the application host is performing a graceful shutdown.
    //    /// </summary>
    //    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    //    public async Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        await Task.CompletedTask;
    //    }

    //    /// <summary>
    //    /// Creates main window during activation.
    //    /// </summary>
    //    private async Task HandleActivationAsync()
    //    {
    //        if (!App.Current.Windows.OfType<MainWindow>().Any())
    //        {
    //            _navigationWindow = (
    //                _serviceProvider.GetService(typeof(NavigationWindow)) as MainWindow
    //            )!;
    //            _navigationWindow!.ShowWindow();

    //            _navigationWindow.Navigate(typeof(Views.Pages.DashboardPage));
    //        }

    //        await Task.CompletedTask;
    //    }
    //}
}
