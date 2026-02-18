using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using JJManager.Core.Devices.Abstractions;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Interfaces.Services;
using JJManager.Infrastructure.Services;
using JJManager.Desktop.Services;
using JJManager.Desktop.ViewModels;
using JJManager.Desktop.Views;
using JJManager.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace JJManager.Desktop;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }
    public static ILocalizationService? Localization => Services?.GetService<ILocalizationService>();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            Debug.WriteLine("=== OnFrameworkInitializationCompleted START ===");

            // Configure services
            Debug.WriteLine("Configuring services...");
            var services = new ServiceCollection();
            ConfigureServices(services);
            Debug.WriteLine("Building service provider...");
            Services = services.BuildServiceProvider();
            Debug.WriteLine("Service provider built successfully.");

            // Set default accent color resource BEFORE any async operations
            if (Current?.Resources != null)
            {
                Current.Resources["AccentColor"] = Avalonia.Media.Color.Parse("#6200EE");
                Current.Resources["AccentBrush"] = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6200EE"));
                Current.Resources["HeaderBackground"] = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6200EE"));
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Debug.WriteLine("Creating MainWindow...");

                // Avoid duplicate validations from both Avalonia and the CommunityToolkit
                BindingPlugins.DataValidators.RemoveAt(0);

                Debug.WriteLine("Getting MainWindowViewModel from DI...");
                var viewModel = Services.GetRequiredService<MainWindowViewModel>();
                Debug.WriteLine($"MainWindowViewModel created: {viewModel != null}");

                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel
                };
                Debug.WriteLine("MainWindow created and DataContext set.");

                // Initialize services asynchronously after window is shown
                desktop.MainWindow.Opened += async (s, e) =>
                {
                    try
                    {
                        Debug.WriteLine("MainWindow Opened event - initializing services...");
                        await InitializeServicesAsync();
                        Debug.WriteLine("Services initialized successfully.");

                        // Start device discovery after window is shown
                        Debug.WriteLine("Starting device discovery...");
                        await viewModel.InitializeAsync();
                        Debug.WriteLine("Device discovery started.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error initializing services: {ex}");
                    }
                };
            }

            Debug.WriteLine("Calling base.OnFrameworkInitializationCompleted...");
            base.OnFrameworkInitializationCompleted();
            Debug.WriteLine("=== OnFrameworkInitializationCompleted END ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"!!! CRITICAL ERROR in OnFrameworkInitializationCompleted: {ex}");
            var errorPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "JJManager_Error.txt");
            System.IO.File.WriteAllText(errorPath, $"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}\n\nInner Exception:\n{ex.InnerException}");
            throw;
        }
    }

    private async Task InitializeServicesAsync()
    {
        if (Services == null) return;

        try
        {
            // Initialize database first
            var database = Services.GetRequiredService<IAppConfigService>();
            await database.InitializeAsync();

            // Initialize localization
            var localization = Services.GetRequiredService<ILocalizationService>();
            var savedLanguage = await database.GetConfigAsync("language");
            await localization.InitializeAsync(savedLanguage);

            // Initialize theme
            var theme = Services.GetRequiredService<IThemeService>();
            var savedTheme = await database.GetConfigAsync("theme_mode");
            var savedAccent = await database.GetConfigAsync("accent_color");
            theme.Initialize(savedTheme, savedAccent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing services: {ex}");
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure (EF Core + SQLite + Repositories)
        // Path is determined by the Infrastructure layer based on OS
        var dbPath = DependencyInjection.GetDefaultDatabasePath();
        services.AddInfrastructure(dbPath);

        // Probe
        services.AddSingleton<IDeviceProbe<HidSharp.HidDevice>, HidDeviceProbe>();

        // Register services
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IDeviceService, DeviceService>();

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DevicesViewModel>();
        services.AddTransient<UpdatesViewModel>();
        services.AddTransient<SettingsViewModel>();
    }
}
