using JJManager.Core.Others;
using JJManager.Core.Profile;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JJManager.Core.Connections.SimHub
{
    public enum SimHubFieldType
    {
        None,
        Text,
        Boolean,
        Number
    }

    public class SimHubPropertyItem
    {
        public string PropertyKey { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public SimHubFieldType FieldType { get; }
        public List<object>? FieldValue { get; }
        public ushort? Command { get; }

        /// <summary>
        /// Indicates if this property can be used as a LED trigger.
        /// Properties like SpeedUnit that are usually fixed should have this set to false.
        /// </summary>
        public bool LedTrigger { get; }

        /// <summary>
        /// Suggested values for non-boolean fields (e.g., gear values: -1, 0, 1, 2, ..., R, N)
        /// </summary>
        public List<string>? SuggestedValues { get; }

        /// <summary>
        /// Current value received from SimHub (updated at runtime)
        /// </summary>
        public object? CurrentValue { get; set; }

        public List<ComparativeType>? SupportedComparatives => FieldType == SimHubFieldType.Number ?
        [
            ComparativeType.Equals,
            ComparativeType.NotEquals,
            ComparativeType.GreaterThan,
            ComparativeType.GreaterOrEquals,
            ComparativeType.LessThan,
            ComparativeType.LessOrEquals
        ] :
        [
            ComparativeType.Equals,
            ComparativeType.NotEquals
        ];

        public SimHubPropertyItem(string key, string displayName, string category, SimHubFieldType fieldType, ushort? command = null, List<object>? fieldValue = null, bool ledTrigger = true, List<string>? suggestedValues = null)
        {
            PropertyKey = key;
            DisplayName = displayName;
            Category = category;
            FieldType = fieldType;
            Command = command;
            FieldValue = fieldValue;
            LedTrigger = ledTrigger;
            SuggestedValues = suggestedValues;
        }

        public override string ToString() => DisplayName;

        #region Static Methods - Value Formatting

        /// <summary>
        /// Format a SimHub JSON value for sending to device.
        /// Handles doubles with proper formatting (max 2 decimal places, no scientific notation).
        /// </summary>
        public static string FormatValue(JsonNode? value)
        {
            if (value == null)
                return string.Empty;

            try
            {
                var kind = value.GetValueKind();

                switch (kind)
                {
                    case JsonValueKind.Number:
                        var doubleVal = value.GetValue<double>();
                        // Check if it's actually an integer
                        if (Math.Abs(doubleVal % 1) < 0.0001)
                            return ((int)doubleVal).ToString();
                        // Format with max 2 decimal places, invariant culture
                        return doubleVal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

                    case JsonValueKind.True:
                        return "1";

                    case JsonValueKind.False:
                        return "0";

                    case JsonValueKind.String:
                        return value.GetValue<string>() ?? string.Empty;

                    default:
                        return value.ToString().Trim();
                }
            }
            catch
            {
                return value.ToString().Trim();
            }
        }

        #endregion

        #region Static Methods - Property Dictionary

        private static List<SimHubPropertyItem>? _cachedProperties;
        private static Dictionary<string, ushort>? _commandLookup;
        private static readonly object _lock = new();

        /// <summary>
        /// Get all SimHub properties from JSON configuration.
        /// </summary>
        public static List<SimHubPropertyItem> GetAllProperties()
        {
            EnsureLoaded();
            return _cachedProperties!;
        }

        /// <summary>
        /// Try to get the HID command ID for a SimHub property name.
        /// </summary>
        public static bool TryGetCommandId(string propertyName, out ushort commandId)
        {
            EnsureLoaded();
            return _commandLookup!.TryGetValue(propertyName, out commandId);
        }

        /// <summary>
        /// Check if a property name is supported.
        /// </summary>
        public static bool IsPropertySupported(string propertyName)
        {
            EnsureLoaded();
            return _commandLookup!.ContainsKey(propertyName);
        }

        private static void EnsureLoaded()
        {
            if (_cachedProperties == null)
            {
                lock (_lock)
                {
                    if (_cachedProperties == null)
                    {
                        LoadFromJson();
                    }
                }
            }
        }

        private static void LoadFromJson()
        {
            _cachedProperties = new List<SimHubPropertyItem>();
            _commandLookup = new Dictionary<string, ushort>();

            try
            {
                var jsonContent = LoadJsonContent();
                if (string.IsNullOrEmpty(jsonContent))
                {
                    Console.WriteLine("[SimHubPropertyItem] Failed to load JSON");
                    return;
                }

                var jsonDoc = JsonDocument.Parse(jsonContent);
                var propertiesArray = jsonDoc.RootElement.GetProperty("properties");

                foreach (var prop in propertiesArray.EnumerateArray())
                {
                    var key = prop.GetProperty("key").GetString() ?? string.Empty;
                    var category = prop.GetProperty("category").GetString() ?? "General";
                    var fieldTypeStr = prop.GetProperty("fieldType").GetString() ?? "None";
                    var commandStr = prop.TryGetProperty("command", out var cmdProp) ? cmdProp.GetString() : null;
                    var ledTrigger = prop.TryGetProperty("ledTrigger", out var ltProp) && ltProp.GetBoolean();

                    // Parse suggested values
                    List<string>? suggestedValues = null;
                    if (prop.TryGetProperty("suggestedValues", out var svArray))
                    {
                        suggestedValues = new List<string>();
                        foreach (var val in svArray.EnumerateArray())
                        {
                            var strVal = val.GetString();
                            if (!string.IsNullOrEmpty(strVal))
                                suggestedValues.Add(strVal);
                        }
                    }

                    // Parse field type
                    var fieldType = fieldTypeStr switch
                    {
                        "Boolean" => SimHubFieldType.Boolean,
                        "Number" => SimHubFieldType.Number,
                        "Text" => SimHubFieldType.Text,
                        _ => SimHubFieldType.None
                    };

                    // Parse command
                    ushort? command = null;
                    if (!string.IsNullOrEmpty(commandStr))
                    {
                        try
                        {
                            command = Convert.ToUInt16(commandStr, 16);
                            _commandLookup[key] = command.Value;
                        }
                        catch { }
                    }

                    _cachedProperties.Add(new SimHubPropertyItem(
                        key, key, category, fieldType, command, null, ledTrigger, suggestedValues
                    ));
                }

                Console.WriteLine($"[SimHubPropertyItem] Loaded {_cachedProperties.Count} properties");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SimHubPropertyItem] Error loading: {ex.Message}");
            }
        }

        private static string? LoadJsonContent()
        {
            // Try file paths
            var paths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Assets", "Config", "SimHubProperties.json"),
                Path.Combine(AppContext.BaseDirectory, "Config", "SimHubProperties.json"),
                Path.Combine(AppContext.BaseDirectory, "SimHubProperties.json")
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }

            // Try embedded resource
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "JJManager.Core.Connections.SimHub.SimHubProperties.json";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }
            catch { }

            return null;
        }

        #endregion
    }
}
