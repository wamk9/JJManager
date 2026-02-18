using JJManager.Core.Connections.SimHub;

namespace JJManager.Desktop.Services;

/// <summary>
/// Service to provide SimHub properties for UI.
/// Delegates to SimHubPropertyItem static methods in Core.
/// </summary>
public class SimHubPropertiesService
{
    private static SimHubPropertiesService? _instance;
    private static readonly object _lock = new();

    public static SimHubPropertiesService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SimHubPropertiesService();
                }
            }
            return _instance;
        }
    }

    private SimHubPropertiesService() { }

    /// <summary>
    /// Gets all SimHub properties from Core.
    /// </summary>
    public List<SimHubPropertyItem> GetProperties()
    {
        return SimHubPropertyItem.GetAllProperties();
    }

    /// <summary>
    /// Gets only SimHub properties that can be used as LED triggers.
    /// </summary>
    public List<SimHubPropertyItem> GetLedTriggerProperties()
    {
        return SimHubPropertyItem.GetAllProperties()
            .Where(p => p.LedTrigger)
            .ToList();
    }
}
