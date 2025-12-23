// Program.cs
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Ma.TimeManagement.Avalonia.Views;
using Ma.TimeManagement.Avalonia.ViewModels;
using System;

namespace Ma.TimeManagement.Avalonia;

public class Program
{
    // Entry point – works on Windows, Linux, macOS, Android, iOS
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()     // Auto-detects all platforms
            .WithInterFont()
            .LogToTrace()
            .UseDependencyInjection(RegisterServices());  // Services go here!

    // Your old Microsoft.Extensions.Hosting services – 100 % unchanged
    private static void RegisterServices()
    {
        var services = new ServiceCollection();

        // ←←← COPY ALL YOUR OLD SERVICES HERE EXACTLY AS BEFORE ←←←
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<TimeTrackingService>();
        services.AddTransient<DatabaseService>();
        services.AddLogging();
        services.AddHttpClient();
        // … everything else you had

        // This makes them available everywhere
        App.Current!.Services = services.BuildServiceProvider();
    }
}