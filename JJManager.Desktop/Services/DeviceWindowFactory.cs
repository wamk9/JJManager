using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using JJManager.Core.Devices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace JJManager.Desktop.Services;

/// <summary>
/// Factory for creating device-specific configuration windows dynamically.
/// Uses naming convention: {DeviceClassName}Window and {DeviceClassName}ViewModel
/// </summary>
public static class DeviceWindowFactory
{
    private const string ViewModelsNamespace = "JJManager.Desktop.ViewModels.Devices";
    private const string ViewsNamespace = "JJManager.Desktop.Views.Devices";

    /// <summary>
    /// Opens the configuration window for the specified device as a modal dialog.
    /// Dynamically finds and instantiates the Window and ViewModel based on DeviceClassName.
    /// Services are resolved automatically from the DI container via ActivatorUtilities.
    /// </summary>
    /// <param name="device">The device to configure</param>
    /// <param name="parentWindow">Optional parent window. If null, uses MainWindow.</param>
    /// <returns>True if window was opened successfully, false otherwise</returns>
    public static async Task<bool> OpenDeviceWindowAsync(JJDevice device, Window? parentWindow = null)
    {
        var deviceClassName = device.DeviceClassName;
        Debug.WriteLine($"DeviceWindowFactory: Opening window for {deviceClassName}");

        if (string.IsNullOrEmpty(deviceClassName) || deviceClassName == "Unknown")
        {
            Debug.WriteLine($"DeviceWindowFactory: Device class name is unknown, cannot open window");
            return false;
        }

        try
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Find Window type: {DeviceClassName}Window
            var windowTypeName = $"{ViewsNamespace}.{deviceClassName}Window";
            var windowType = assembly.GetType(windowTypeName);

            if (windowType == null)
            {
                Debug.WriteLine($"DeviceWindowFactory: Window type not found: {windowTypeName}");
                return false;
            }

            // Find ViewModel type: {DeviceClassName}ViewModel
            var viewModelTypeName = $"{ViewModelsNamespace}.{deviceClassName}ViewModel";
            var viewModelType = assembly.GetType(viewModelTypeName);

            if (viewModelType == null)
            {
                Debug.WriteLine($"DeviceWindowFactory: ViewModel type not found: {viewModelTypeName}");
                return false;
            }

            // Create window instance
            var window = Activator.CreateInstance(windowType) as Window;
            if (window == null)
            {
                Debug.WriteLine($"DeviceWindowFactory: Failed to create window instance");
                return false;
            }

            // Create close action that calls Cleanup on ViewModel
            Action closeAction = () =>
            {
                // Call Cleanup method on ViewModel if it exists
                var viewModel = window.DataContext;
                if (viewModel != null)
                {
                    var cleanupMethod = viewModelType.GetMethod("Cleanup");
                    cleanupMethod?.Invoke(viewModel, null);
                }
                window.Close();
            };

            // Create ViewModel instance using ActivatorUtilities
            // Services (IDeviceProfileService, etc.) are resolved from DI container automatically
            // Only device and closeAction are passed as explicit parameters
            if (App.Services == null)
            {
                Debug.WriteLine($"DeviceWindowFactory: App.Services is null, cannot create ViewModel");
                return false;
            }

            var viewModelInstance = ActivatorUtilities.CreateInstance(App.Services, viewModelType, device, closeAction);
            if (viewModelInstance == null)
            {
                Debug.WriteLine($"DeviceWindowFactory: Failed to create ViewModel instance");
                return false;
            }

            window.DataContext = viewModelInstance;

            // Get parent window (use MainWindow if not specified)
            var owner = parentWindow;
            if (owner == null && App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                owner = desktop.MainWindow;
            }

            // Show as modal dialog (blocks parent window)
            if (owner != null)
            {
                await window.ShowDialog(owner);
            }
            else
            {
                // Fallback to non-modal if no parent available
                window.Show();
            }

            Debug.WriteLine($"DeviceWindowFactory: Successfully opened {deviceClassName} window");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeviceWindowFactory: Error opening window: {ex.Message}");
            Debug.WriteLine($"DeviceWindowFactory: Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Checks if a configuration window exists for the specified device class.
    /// </summary>
    /// <param name="deviceClassName">The device class name</param>
    /// <returns>True if window and ViewModel types exist</returns>
    public static bool HasDeviceWindow(string deviceClassName)
    {
        if (string.IsNullOrEmpty(deviceClassName) || deviceClassName == "Unknown")
            return false;

        var assembly = Assembly.GetExecutingAssembly();

        var windowTypeName = $"{ViewsNamespace}.{deviceClassName}Window";
        var viewModelTypeName = $"{ViewModelsNamespace}.{deviceClassName}ViewModel";

        var windowType = assembly.GetType(windowTypeName);
        var viewModelType = assembly.GetType(viewModelTypeName);

        return windowType != null && viewModelType != null;
    }
}
