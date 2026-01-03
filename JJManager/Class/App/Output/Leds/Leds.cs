using JJManager.Class.App.Input.MacroKey.Keyboard;
using JJManager.Class.App.Input.MacroKey.Mouse;
using Microsoft.SqlServer.Management.XEvent;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace JJManager.Class.App.Output.Leds
{
    public class Leds : INotifyCollectionChanged
    {
        public enum Side
        {
            None,
            Top,
            Right,
            Left
        }

        public enum LedType
        {
            None,
            RGB,
            PWM
        }

        public enum LedMode
        {
            None,
            RPM,
            Individual,
            Group
        }

        public enum ComparativeType
        {
            None,
            Equals,
            After,
            Before
        }

        public enum VariableType
        {
            None,
            Number,
            Text
        }


        private Side _side = Side.None;
        private LedMode _mode = LedMode.None;
        private LedType _ledType = LedType.None;
        private string _jjproperty = null;
        private string _propertyName = null;
        private string _hexColors = null;
        private List<int> _leds = null;
        private dynamic _valueToActivate = null;
        private int _ledMode = -1;
        private int _ledModeIfEnabled = -1;
        private ComparativeType _comparativeType = ComparativeType.None;
        private VariableType _variableType = VariableType.None;
        private int _order = -1;
        private bool _active = false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public string Property { get => _jjproperty; }
        public string PropertyName { get => _propertyName; }
        public List<int> LedsGrouped { get => _leds; }
        public string Color { get => _hexColors; }
        public int Order { get => _order; set => _order = value; }
        public int Mode { get => _ledMode; set => _ledMode = value; }
        public LedType Type { get => _ledType; set => _ledType = value; }
        public int ModeIfEnabled { get => _ledModeIfEnabled; set => _ledModeIfEnabled = value; }
        public bool Active { get => _active; }

        public string GetActualLedColor
        {
            get
            {
                return _active ? (_ledType == LedType.RGB ? _hexColors : "1") : (_ledType == LedType.RGB ? "#000000" : "0");
            }
        }

        public Leds(LedType ledType)
        {
            _ledType = ledType;
            _leds = new List<int>();
        }

        public Leds(JsonObject json)
        {
            JsonToLeds(json);
        }

        public bool SetActivatedValue(dynamic value)
        {
            // Return false if either value or _valueToActivate is null
            if (_valueToActivate.Equals(null) || value.Equals(null))
            {
                return false;
            }

            // Determine the type to use for comparisons
            Type type = _variableType == VariableType.Number ? typeof(double) : typeof(string);

            // Ensure that both _valueToActivate and value are properly deserialized into their primitive forms
            value = GetPrimitiveValue(value);
            _valueToActivate = GetPrimitiveValue(_valueToActivate);

            // Perform the dynamic equality check
            object actualValue = value.ToString() == "--" && type == typeof(double) ? 0 :
                Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

            object valueToActivate = _valueToActivate.ToString() == "--" && type == typeof(double) ? 0 :
                Convert.ChangeType(_valueToActivate, type, CultureInfo.InvariantCulture);

            // Perform the comparison based on _comparativeType
            switch (_comparativeType)
            {
                case ComparativeType.Equals:
                    // Handle the comparison based on type
                    if (type == typeof(double))
                    {
                        _active = (dynamic)actualValue == (dynamic)valueToActivate;
                        return (double)actualValue == (double)valueToActivate;
                    }

                    _active = Equals(actualValue, valueToActivate);
                    return Equals(actualValue, valueToActivate);

                case ComparativeType.After:
                    // Ensure comparison is for numeric types
                    if (type == typeof(double) || type == typeof(float) || type == typeof(int))
                    {
                        _active = (dynamic)actualValue > (dynamic)valueToActivate;
                        return (dynamic)actualValue > (dynamic)valueToActivate;
                    }
                    throw new InvalidOperationException("Cannot compare 'After' for non-numeric types.");

                case ComparativeType.Before:
                    // Ensure comparison is for numeric types
                    if (type == typeof(double) || type == typeof(float) || type == typeof(int))
                    {
                        _active = (dynamic)actualValue < (dynamic)valueToActivate;
                        return (dynamic)actualValue < (dynamic)valueToActivate;
                    }
                    throw new InvalidOperationException("Cannot compare 'Before' for non-numeric types.");

                default:
                    _active = false;
                    return false;
            }
        }

        private object GetPrimitiveValue(object input)
        {
            // Handle deserialization from different types (this method is adapted to handle JSON input)
            if (input is JsonNode node)
                return node.Deserialize<object>();

            if (input is JsonElement element)
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.String:
                        return element.GetString();
                    case JsonValueKind.Number:
                        return element.TryGetDouble(out var dbl) ? dbl : 0;
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.False:
                        return false;
                    default:
                        return null;
                }
            }

            return input; // Return the input if it's already a primitive type
        }


        public void JsonToLeds(JsonObject json)
        {
            _side = json.ContainsKey("side") && json["side"]?.GetValue<string>() is string s1 ? ToSide(s1) : Side.None;
            _mode = json.ContainsKey("mode") && json["mode"]?.GetValue<string>() is string s2 ? ToLedMode(s2) : LedMode.None;
            _ledType = json.ContainsKey("led_type") && json["led_type"]?.GetValue<string>() is string s3 ? ToLedType(s3) : LedType.None;
            _hexColors = json.ContainsKey("color") && json["color"]?.GetValue<string>() is string s4 ? s4 : "#000000";
            _jjproperty = json.ContainsKey("jj_prop") && json["jj_prop"]?.GetValue<string>() is string s5 ? s5 : null;
            _propertyName = json.ContainsKey("prop_name") && json["prop_name"]?.GetValue<string>() is string s6 ? s6 : null;
            _order = json.ContainsKey("order") && json["order"]?.GetValue<int>() is int o ? o : -1;
            _ledMode = json.ContainsKey("led_mode") && json["led_mode"]?.GetValue<int>() is int l1 ? l1 : -1;
            _ledModeIfEnabled = json.ContainsKey("led_mode_simhub") && json["led_mode_simhub"]?.GetValue<int>() is int l2 ? l2 : -1;

            if (_jjproperty != null)
            {
                DatabaseConnection database = new DatabaseConnection();

                JsonArray jjPropertyJson = database.RunSQLWithResults($"SELECT activate_on FROM simhub_properties WHERE jj_prop = '{_jjproperty}'");

                foreach (JsonObject property in jjPropertyJson)
                {
                    if (property.ContainsKey("activate_on"))
                    {
                        JsonObject activateData = JsonObject.Parse(property["activate_on"].GetValue<string>()).AsObject();

                        _valueToActivate = (activateData.ContainsKey("value") ? activateData["value"].GetValue<string>() : null);
                        _comparativeType = ToComparativeType(activateData.ContainsKey("comparative") ? activateData["comparative"].GetValue<string>() : null);
                        _variableType = ToVariableType(activateData.ContainsKey("type") ? activateData["type"].GetValue<string>() : null);
                    }
                }
            }

            
            if (json.ContainsKey("leds"))
            {
                JsonArray array = json["leds"].AsArray();
                _leds = new List<int>();

                foreach (var item in array)
                {
                    _leds.Add(item.GetValue<int>());
                }
            }
        }

        protected Side ToSide(string value)
        {
            switch (value)
            {
                case "right":
                    return Side.Right;
                case "left":
                    return Side.Left;
                case "top":
                    return Side.Top;
            }

            return Side.None;
        }

        protected LedMode ToLedMode(string value)
        {
            switch (value)
            {
                case "rpm":
                    return LedMode.RPM;
                case "individual":
                    return LedMode.Individual;
                case "group":
                    return LedMode.Group;
            }

            return LedMode.None;
        }

        protected LedType ToLedType(string value)
        {
            switch (value.ToLower())
            {
                case "rgb":
                    return LedType.RGB;
                case "pwm":
                    return LedType.PWM;
            }

            return LedType.None;
        }

        protected ComparativeType ToComparativeType(string value)
        {
            switch (value.ToLower())
            {
                case "equals":
                    return ComparativeType.Equals;
                case "after":
                    return ComparativeType.After;
                case "before":
                    return ComparativeType.Before;
            }

            return ComparativeType.None;
        }

        protected VariableType ToVariableType(string value)
        {
            switch (value.ToLower())
            {
                case "number":
                    return VariableType.Number;
                case "text":
                    return VariableType.Text;
            }

            return VariableType.None;
        }

        public JsonObject objectToJson()
        {
            JsonObject json = new JsonObject();

            json["side"] = _side.ToString().ToLower();
            json["mode"] = _mode.ToString().ToLower();
            json["led_type"] = _ledType.ToString().ToLower();
            json["led_mode"] = _ledMode;
            json["jj_prop"] = _jjproperty;
            json["prop_name"] = _propertyName;
            json["color"] = _hexColors;
            json["order"] = _order;
            json["led_mode_simhub"] = _ledModeIfEnabled;

            JsonArray leds = new JsonArray();

            if (_leds != null)
            {
                foreach (int item in _leds)
                {
                    leds.Add(item);
                }
            }

            json["leds"] = leds;

            return json;
        }

        public bool Update(JsonObject data)
        {
            if (data.ContainsKey("side"))
            {
                _side = ToSide(data["side"].GetValue<string>());
            }

            if (data.ContainsKey("mode"))
            {
                _mode = ToLedMode(data["mode"].GetValue<string>());
            }

            if (data.ContainsKey("led_mode"))
            {
                _ledMode = data["led_mode"].GetValue<int>();
            }

            if (data.ContainsKey("order"))
            {
                _order = data["order"].GetValue<int>();
            }

            if (data.ContainsKey("led_mode_simhub"))
            {
                _ledModeIfEnabled = data["led_mode_simhub"].GetValue<int>();
            }

            if (data.ContainsKey("led_type"))
            {
                _ledType = ToLedType(data["led_type"].GetValue<string>());
            }

            if (data.ContainsKey("jj_prop"))
            {
                _jjproperty = data["jj_prop"].GetValue<string>();
            }

            if (data.ContainsKey("color"))
            {
                _hexColors = data["color"].GetValue<string>();
            }

            if (data.ContainsKey("leds"))
            {
                if (_leds == null)
                {
                    _leds = new List<int>();
                }

                _leds.Clear();

                foreach (int led in data["leds"].AsArray().Select(v => (int)v))
                {
                    _leds.Add(led);
                }
            }

            return true;
        }

        //public void OrderActions(List<MacroKeyAction> actions = null)
        //{
        //    // Sort by Value
        //    List<MacroKeyAction> sorted = actions ?? _actions.OrderBy(item => item.Order).ToList();
        //    uint newOrder = 0;

        //    // Clear the original collection and repopulate with the sorted items (Need clean ListView)
        //    _actions.Clear();

        //    foreach (MacroKeyAction action in sorted)
        //    {
        //        action.Order = newOrder;
        //        _actions.Add(action);
        //        newOrder++;
        //    }
        //}

        public bool Execute()
        {
            return true;
        }

        public JsonArray TranslateToDevice(Dictionary<string, int> ledModeList , JsonObject SimHubLastValues = null)
        {
            List<string> ledModesNecessary = new List<string>
            {
                "off",
                "on",
                "blink",
                "pulse",
                "simhub"
            };

            if (!ledModesNecessary.All(x => ledModeList.ContainsKey(x)))
            {
                return new JsonArray();
            }

            JsonArray jsonLedValues = new JsonArray();
            JsonArray jsonLedValue = new JsonArray();

            foreach (int ledPos in _leds)
            {
                if (_ledType == Leds.LedType.PWM || _ledType == Leds.LedType.RGB)
                {
                    jsonLedValue.Add(_ledType == Leds.LedType.PWM ? "pwm" : "rgb");
                    jsonLedValue.Add(ledPos);

                    bool isSimhubMode = _ledMode == ledModeList["simhub"];

                    if (isSimhubMode)
                    {
                        bool isActivated = (isSimhubMode && _jjproperty != null && SimHubLastValues != null && SimHubLastValues.ContainsKey(_jjproperty) ? SetActivatedValue(SimHubLastValues[_jjproperty].GetValue<dynamic>()) : false);
                        //Console.WriteLine(_jjproperty + " " + SimHubLastValues[_jjproperty].GetValue<dynamic>());
                        if (_ledType == Leds.LedType.PWM)
                        {
                            jsonLedValue.Add(isActivated ? _ledModeIfEnabled : ledModeList["off"]);
                        }
                        else if (_ledType == Leds.LedType.RGB)
                        {
                            jsonLedValue.Add(isActivated ? _hexColors : "#000000");
                        }
                    }
                    else
                    {
                        if (_ledType == Leds.LedType.PWM)
                        {
                            jsonLedValue.Add(_ledMode);
                        }
                        else if (_ledType == Leds.LedType.RGB)
                        {
                            jsonLedValue.Add(_hexColors);
                        }
                    }
                }

                jsonLedValues.Add(jsonLedValue.DeepClone().AsArray());
                jsonLedValue.Clear();
            }
            
            return jsonLedValues;
        }
    }
}
