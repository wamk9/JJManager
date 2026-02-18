using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JJManager.Desktop.Services;
using System;
using System.Collections.ObjectModel;
using JJManager.Core.Interfaces.Services;
namespace JJManager.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDeviceService _deviceService;
    private readonly IAppConfigService _appConfigService;

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _statusMessage = "Pronto";

    [ObservableProperty]
    private int _connectedDeviceCount;

    [ObservableProperty]
    private bool _hasConnectedDevices;

    [ObservableProperty]
    private DevicesViewModel _devicesViewModel;

    [ObservableProperty]
    private UpdatesViewModel _updatesViewModel;

    [ObservableProperty]
    private SettingsViewModel _settingsViewModel;

    public MainWindowViewModel(
        IDeviceService deviceService,
        IAppConfigService appConfigService,
        DevicesViewModel devicesViewModel,
        UpdatesViewModel updatesViewModel,
        SettingsViewModel settingsViewModel)
    {
        _deviceService = deviceService;
        _appConfigService = appConfigService;
        _devicesViewModel = devicesViewModel;
        _updatesViewModel = updatesViewModel;
        _settingsViewModel = settingsViewModel;

        // Subscribe to device changes
        _deviceService.DevicesChanged += OnDevicesChanged;

        // Note: Initialize() should be called after window is shown
    }

    public async Task InitializeAsync()
    {
        try
        {
            StatusMessage = "Iniciando descoberta de dispositivos...";
            await _deviceService.StartDiscoveryAsync();
            _devicesViewModel.StartAutoRefresh();
            StatusMessage = "Pronto";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in MainWindowViewModel.Initialize: {ex}");
        }
    }

    private void OnDevicesChanged(object? sender, EventArgs e)
    {
        ConnectedDeviceCount = _deviceService.ConnectedDevices.Count;
        HasConnectedDevices = ConnectedDeviceCount > 0;
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        StatusMessage = value switch
        {
            0 => "Gerencie seus dispositivos JohnJohn3D",
            1 => "Verifique atualizações de firmware e software",
            2 => "Personalize as configurações do JJManager",
            _ => "Pronto"
        };
    }
}
