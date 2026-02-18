using JJManager.Core.Connections.SimHub;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Others;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JJManager.Core.Profile;

/// <summary>
/// Represents a device configuration profile
/// Contains inputs, outputs, and custom data
/// </summary>
public class DeviceProfile
{
    #region Fields

    private Guid _id = Guid.Empty;
    private string _name = "Perfil Padrão";
    private Guid _productId = Guid.Empty;
    private bool _needsUpdate = true;
    private bool _changed;
    private JsonObject _data = new();
    private List<ProfileInput> _inputs = new();
    private List<ProfileOutput> _outputs = new();

    #endregion

    #region Properties

    /// <summary>
    /// Profile ID from database
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    /// Profile name
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _changed = true;
        }
    }

    /// <summary>
    /// Product ID this profile belongs to
    /// </summary>
    public Guid ProductId
    {
        get => _productId;
        set => _productId = value;
    }

    /// <summary>
    /// Whether this profile needs to be sent to the device
    /// </summary>
    public bool NeedsUpdate
    {
        get => _needsUpdate;
        set => _needsUpdate = value;
    }

    /// <summary>
    /// Whether this profile has unsaved changes
    /// </summary>
    public bool Changed
    {
        get => _changed;
        set => _changed = value;
    }

    /// <summary>
    /// Custom JSON data for device-specific settings
    /// </summary>
    public JsonObject Data
    {
        get => _data;
        set
        {
            _data = value ?? new JsonObject();
            _changed = true;
        }
    }

    /// <summary>
    /// List of input configurations
    /// </summary>
    public List<ProfileInput> Inputs
    {
        get => _inputs;
        set => _inputs = value ?? new();
    }

    /// <summary>
    /// List of output configurations
    /// </summary>
    public List<ProfileOutput> Outputs
    {
        get => _outputs;
        set => _outputs = value ?? new();
    }

    #endregion

    #region Constructor
    public DeviceProfile()
    {
    }

    public DeviceProfile(string name, Guid productId)
    {
        _name = name;
        _productId = productId;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Mark the profile as needing to be sent to the device
    /// </summary>
    public void MarkAsNeedsUpdate()
    {
        _needsUpdate = true;
    }

    /// <summary>
    /// Clear the needs update flag after sending to device
    /// </summary>
    public void ClearNeedsUpdate()
    {
        _needsUpdate = false;
    }

    /// <summary>
    /// Update profile data from JSON object
    /// </summary>
    public void Update(JsonObject updateData)
    {
        if (updateData == null)
            return;

        if (updateData.TryGetPropertyValue("name", out var nameNode))
        {
            _name = nameNode?.GetValue<string>() ?? _name;
        }

        if (updateData.TryGetPropertyValue("data", out var dataNode))
        {
            if (dataNode is JsonObject dataObj)
            {
                _data = dataObj.Deserialize<JsonObject>() ?? new JsonObject();
            }
        }

        _changed = true;
        _needsUpdate = true;
    }

    /// <summary>
    /// Get a value from the data object
    /// </summary>
    public T? GetDataValue<T>(string key, T? defaultValue = default)
    {
        if (_data.TryGetPropertyValue(key, out var node) && node != null)
        {
            try
            {
                return node.GetValue<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// Try to get a value from the data object
    /// </summary>
    public bool TryGetDataValue<T>(string key, out T? value)
    {
        value = default;
        if (_data.TryGetPropertyValue(key, out var node) && node != null)
        {
            try
            {
                // Special handling for arrays
                if (typeof(T) == typeof(double[]))
                {
                    if (node is JsonArray jsonArray)
                    {
                        var result = new double[jsonArray.Count];
                        for (int i = 0; i < jsonArray.Count; i++)
                        {
                            result[i] = jsonArray[i]?.GetValue<double>() ?? 0;
                        }
                        value = (T)(object)result;
                        return true;
                    }
                    return false;
                }

                value = node.GetValue<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Set a value in the data object
    /// </summary>
    public void SetDataValue<T>(string key, T value)
    {
        _data[key] = JsonValue.Create(value);
        _changed = true;
    }

    /// <summary>
    /// Get an input by index
    /// </summary>
    public ProfileInput? GetInput(int index)
    {
        return index >= 0 && index < _inputs.Count ? _inputs[index] : null;
    }

    /// <summary>
    /// Get an output by index
    /// </summary>
    public ProfileOutput? GetOutput(int index)
    {
        return index >= 0 && index < _outputs.Count ? _outputs[index] : null;
    }

    /// <summary>
    /// Add an input configuration
    /// </summary>
    public void AddInput(ProfileInput input)
    {
        _inputs.Add(input);
        _changed = true;
    }

    /// <summary>
    /// Add an output configuration
    /// </summary>
    public void AddOutput(ProfileOutput output)
    {
        _outputs.Add(output);
        _changed = true;
    }

    /// <summary>
    /// Remove an input configuration
    /// </summary>
    public bool RemoveInput(ProfileInput input)
    {
        var result = _inputs.Remove(input);
        if (result) _changed = true;
        return result;
    }

    /// <summary>
    /// Remove an output configuration
    /// </summary>
    public bool RemoveOutput(ProfileOutput output)
    {
        var result = _outputs.Remove(output);
        if (result) _changed = true;
        return result;
    }

    /// <summary>
    /// Add or update an LED output for a specific LED index
    /// </summary>
    public ProfileOutput AddOrUpdateLedOutput(int ledIndex, LedConfiguration ledConfig)
    {
        // Find existing output or create new one
        var existingOutput = _outputs.FirstOrDefault(o =>
            o.Mode == OutputMode.Leds &&
            o.Led?.LedsGrouped?.Contains(ledIndex) == true &&
            o.Led?.Property == ledConfig.Property);

        if (existingOutput != null)
        {
            // Update existing
            existingOutput.Led = ledConfig;
            existingOutput.Configuration = LedConfigToJson(ledConfig);
        }
        else
        {
            // Create new output with next available index
            var newIndex = _outputs.Count > 0 ? _outputs.Max(o => o.Index) + 1 : 0;
            existingOutput = new ProfileOutput
            {
                Index = newIndex,
                Mode = OutputMode.Leds,
                Led = ledConfig,
                Configuration = LedConfigToJson(ledConfig)
            };
            _outputs.Add(existingOutput);
        }

        _changed = true;
        _needsUpdate = true;
        return existingOutput;
    }

    /// <summary>
    /// Remove an LED output by index and property
    /// </summary>
    public bool RemoveLedOutput(int ledIndex, string property)
    {
        var output = _outputs.FirstOrDefault(o =>
            o.Mode == OutputMode.Leds &&
            o.Led?.LedsGrouped?.Contains(ledIndex) == true &&
            o.Led?.Property == property);

        if (output != null)
        {
            _outputs.Remove(output);
            _changed = true;
            _needsUpdate = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove an LED output by output index
    /// </summary>
    public bool RemoveLedOutputByIndex(int outputIndex)
    {
        var output = _outputs.FirstOrDefault(o => o.Index == outputIndex);
        if (output != null)
        {
            _outputs.Remove(output);
            _changed = true;
            _needsUpdate = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get all LED outputs for a specific LED index
    /// </summary>
    public IEnumerable<ProfileOutput> GetLedOutputsForLed(int ledIndex)
    {
        return _outputs.Where(o =>
            o.Mode == OutputMode.Leds &&
            o.Led?.LedsGrouped?.Contains(ledIndex) == true);
    }

    /// <summary>
    /// Duplicate an LED output to another LED
    /// </summary>
    public ProfileOutput? DuplicateLedOutput(int sourceOutputIndex, int targetLedIndex)
    {
        var sourceOutput = _outputs.FirstOrDefault(o => o.Index == sourceOutputIndex);
        if (sourceOutput?.Led == null)
            return null;

        var newLedConfig = new LedConfiguration
        {
            Property = sourceOutput.Led.Property,
            PropertyName = sourceOutput.Led.PropertyName,
            Color = sourceOutput.Led.Color,
            Order = _outputs.Count(o => o.Led?.LedsGrouped?.Contains(targetLedIndex) == true),
            Mode = sourceOutput.Led.Mode,
            ModeIfEnabled = sourceOutput.Led.ModeIfEnabled,
            Brightness = sourceOutput.Led.Brightness,
            BlinkSpeed = sourceOutput.Led.BlinkSpeed,
            PulseDelay = sourceOutput.Led.PulseDelay,
            LedsGrouped = new List<int> { targetLedIndex },
            ValueToActivate = sourceOutput.Led.ValueToActivate,
            Comparative = sourceOutput.Led.Comparative,
            VarType = sourceOutput.Led.VarType
        };

        return AddOrUpdateLedOutput(targetLedIndex, newLedConfig);
    }

    /// <summary>
    /// Convert LedConfiguration to JSON for storage
    /// </summary>
    private static JsonObject LedConfigToJson(LedConfiguration led)
    {
        var json = new JsonObject
        {
            ["property"] = led.Property,
            ["property_name"] = led.PropertyName,
            ["color"] = led.Color,
            ["order"] = led.Order,
            ["mode"] = led.Mode,
            ["mode_if_enabled"] = led.ModeIfEnabled,
            ["brightness"] = led.Brightness,
            ["blink_speed"] = led.BlinkSpeed,
            ["pulse_delay"] = led.PulseDelay,
            ["value_to_activate"] = led.ValueToActivate,
            ["comparative"] = led.Comparative.ToString(),
            ["var_type"] = led.VarType.ToString()
        };

        var ledsArray = new JsonArray();
        if (led.LedsGrouped != null)
        {
            foreach (var ledIdx in led.LedsGrouped)
            {
                ledsArray.Add(ledIdx);
            }
        }
        json["leds"] = ledsArray;

        return json;
    }

    /// <summary>
    /// Parse LedConfiguration from JSON
    /// </summary>
    public static LedConfiguration? JsonToLedConfig(JsonObject? json)
    {
        if (json == null)
            return null;

        var config = new LedConfiguration
        {
            Property = json["property"]?.GetValue<string>() ?? string.Empty,
            PropertyName = json["property_name"]?.GetValue<string>() ?? string.Empty,
            Color = json["color"]?.GetValue<string>() ?? "#000000",
            Order = json["order"]?.GetValue<int>() ?? 0,
            Mode = json["mode"]?.GetValue<int>() ?? 0,
            ModeIfEnabled = json["mode_if_enabled"]?.GetValue<int>() ?? 1,
            Brightness = json["brightness"]?.GetValue<int>() ?? 100,
            BlinkSpeed = json["blink_speed"]?.GetValue<int>() ?? 5,
            PulseDelay = json["pulse_delay"]?.GetValue<int>() ?? 5,
            ValueToActivate = json["value_to_activate"]?.GetValue<string>()
        };

        // Parse comparative
        if (json.TryGetPropertyValue("comparative", out var compNode) && compNode != null)
        {
            var compStr = compNode.GetValue<string>();
            if (Enum.TryParse<ComparativeType>(compStr, out var compType))
            {
                config.Comparative = compType;
            }
        }

        // Parse var_type
        if (json.TryGetPropertyValue("var_type", out var varNode) && varNode != null)
        {
            var varStr = varNode.GetValue<string>();
            if (Enum.TryParse<VariableType>(varStr, out var varType))
            {
                config.VarType = varType;
            }
        }

        // Parse leds array
        if (json.TryGetPropertyValue("leds", out var ledsNode) && ledsNode is JsonArray ledsArray)
        {
            config.LedsGrouped = new List<int>();
            foreach (var item in ledsArray)
            {
                if (item != null)
                {
                    config.LedsGrouped.Add(item.GetValue<int>());
                }
            }
        }

        return config;
    }
    #endregion
}

/// <summary>
/// Input configuration for a profile
/// </summary>
public class ProfileInput
{
    public int Index { get; set; }
    public InputMode Mode { get; set; }
    public InputType Type { get; set; }
    public JsonObject? Configuration { get; set; }
    public bool UpdateSessionsToControl { get; set; }
}

/// <summary>
/// Output configuration for a profile
/// </summary>
public class ProfileOutput
{
    public int Index { get; set; }
    public OutputMode Mode { get; set; }
    public JsonObject? Configuration { get; set; }
    public LedConfiguration? Led { get; set; }
}

/// <summary>
/// LED configuration for outputs
/// </summary>
public class LedConfiguration
{
    public string Property { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public int Order { get; set; }
    public int Mode { get; set; }
    public int ModeIfEnabled { get; set; }
    public int Brightness { get; set; } = 100;
    public int BlinkSpeed { get; set; } = 5;
    public int PulseDelay { get; set; } = 5;
    public List<int>? LedsGrouped { get; set; }

    // Activation settings
    public string? ValueToActivate { get; set; }
    public ComparativeType Comparative { get; set; } = ComparativeType.Equals;
    public VariableType VarType { get; set; } = VariableType.Text;

    private bool _active;
    public bool Active => _active;

    /// <summary>
    /// Get the actual LED color (color if active, black if not)
    /// </summary>
    public string GetActualLedColor => _active ? Color : "#000000";

    /// <summary>
    /// Check if the LED should be activated based on the SimHub value
    /// </summary>
    public bool SetActivatedValue(dynamic value)
    {
        if (string.IsNullOrEmpty(PropertyName) || ValueToActivate == null)
        {
            _active = false;
            return false;
        }

        // Check if value is null or undefined (handles JsonElement which is a struct)
        if (IsNullOrUndefined(value))
        {
            _active = false;
            return false;
        }

        try
        {
            // Get primitive value
            object? actualValue = GetPrimitiveValue(value);
            object? targetValue = GetPrimitiveValue(ValueToActivate);

            if (actualValue == null || targetValue == null)
            {
                _active = false;
                return false;
            }

            // Convert to appropriate type for comparison
            if (VarType == VariableType.Number)
            {
                double actual = Convert.ToDouble(actualValue.ToString() == "--" ? 0 : actualValue, CultureInfo.InvariantCulture);
                double target = Convert.ToDouble(targetValue.ToString() == "--" ? 0 : targetValue, CultureInfo.InvariantCulture);
                
                _active = Comparative switch
                {
                    ComparativeType.Equals => actual == target,
                    ComparativeType.NotEquals => actual != target,
                    ComparativeType.GreaterThan => actual > target,
                    ComparativeType.GreaterOrEquals => actual >= target,
                    ComparativeType.LessThan => actual < target,
                    ComparativeType.LessOrEquals => actual <= target,
                    _ => false
                };
            }
            else
            {
                string actual = actualValue.ToString() ?? "";
                string target = targetValue.ToString() ?? "";

                _active = Comparative switch
                {
                    ComparativeType.Equals => actual.Equals(target, StringComparison.OrdinalIgnoreCase),
                    ComparativeType.NotEquals => !actual.Equals(target, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
            }

            return _active;
        }
        catch
        {
            _active = false;
            return false;
        }
    }

    /// <summary>
    /// Check if a dynamic value is null or undefined (handles JsonElement struct)
    /// </summary>
    private static bool IsNullOrUndefined(object? input)
    {
        if (input == null)
            return true;

        if (input is JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ||
                   element.ValueKind == JsonValueKind.Undefined;
        }

        if (input is JsonNode node)
        {
            try
            {
                // Try to get the underlying JsonElement
                var jsonElement = node.GetValue<JsonElement>();
                return jsonElement.ValueKind == JsonValueKind.Null ||
                       jsonElement.ValueKind == JsonValueKind.Undefined;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private static object? GetPrimitiveValue(object? input)
    {
        if (input == null) return null;

        if (input is JsonNode node)
        {
            try
            {
                return node.GetValue<object>();
            }
            catch
            {
                return null;
            }
        }

        if (input is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetDouble(out var dbl) ? dbl : 0,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => null
            };
        }

        return input;
    }
}

public enum VariableType
{
    None,
    Number,
    Text
}

public enum InputMode
{
    None,
    MacroKey,
    AudioController,
    AudioPlayer
}

public enum InputType
{
    Digital,
    Analog
}

public enum OutputMode
{
    None,
    Leds,
    DashboardLeds
}
