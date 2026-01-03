using JJManager.Class.App.Input.MacroKey.Keyboard;
using JJManager.Class.App.Input.MacroKey.Mouse;
using Microsoft.SqlServer.Management.XEvent;
using Newtonsoft.Json.Linq;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace JJManager.Class.App.Output.DashboardLeds
{
    public class DashboardLeds : INotifyCollectionChanged
    {
        public enum Side
        {
            None,
            Top,
            Right,
            Left
        }

        public enum Mode
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
        private Mode _mode = Mode.None;
        private string _jjproperty = null;
        private string _hexColors = null;
        private List<string> _leds = null;
        private dynamic _valueToActivate = null;
        private ComparativeType _comparativeType = ComparativeType.None;
        private VariableType _variableType = VariableType.None;
        private int _order = -1;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public string Property { get => _jjproperty; }
        public List<string> Leds { get => _leds; }
        public string Color { get => _hexColors; }
        public int Order { get => _order ; set => _order = value; }

        public DashboardLeds()
        {
            _leds = new List<string>();
        }

        public DashboardLeds(JsonObject json)
        {
            JsonToLeds(json);
        }

        public bool CheckIfIsActivated(dynamic value)
        {
            if (_valueToActivate == null || value == null)
            {
                return false;
            }


            Type type = null;

            // Set the appropriate type based on _variableType
            switch (_variableType)
            {
                case VariableType.Number:
                    type = typeof(double);
                    break;

                case VariableType.Text:
                    type = typeof(string);
                    break;
            }

            // Convert values to the correct type
            object actualValue = value.ToString() == "--" && type == typeof(double) ? 0 : Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            object ValueToActivate = _valueToActivate.ToString() == "--" && type == typeof(double) ? 0 : Convert.ChangeType(_valueToActivate, type, CultureInfo.InvariantCulture);

            // Perform comparison based on _comparativeType
            switch (_comparativeType)
            {
                case ComparativeType.Equals:
                    if (type == typeof(double))
                        return Math.Abs((double)actualValue - (double)ValueToActivate) < 0.0001;
                    return string.Equals(actualValue.ToString().Trim(), ValueToActivate.ToString().Trim(), StringComparison.OrdinalIgnoreCase);

                case ComparativeType.After:
                    // Ensure you are comparing numeric values correctly
                    if (type == typeof(double) || type == typeof(float) || type == typeof(int))
                    {
                        return (dynamic)actualValue > (dynamic)ValueToActivate;
                    }
                    throw new InvalidOperationException("Cannot compare 'After' for non-numeric types.");

                case ComparativeType.Before:
                    // Ensure you are comparing numeric values correctly
                    if (type == typeof(double) || type == typeof(float) || type == typeof(int))
                    {
                        return (dynamic)actualValue < (dynamic)ValueToActivate;
                    }
                    throw new InvalidOperationException("Cannot compare 'Before' for non-numeric types.");

                default:
                    return false;
            }
        }

        public void JsonToLeds(JsonObject json)
        {
            _side = json.ContainsKey("side") && json["side"]?.GetValue<string>() is string s1 ? ToSide(s1) : Side.None;
            _mode = json.ContainsKey("mode") && json["mode"]?.GetValue<string>() is string s2 ? ToMode(s2) : Mode.None;
            _hexColors = json.ContainsKey("color") && json["color"]?.GetValue<string>() is string s3 ? s3 : "#000000";
            _jjproperty = json.ContainsKey("jj_prop") && json["jj_prop"]?.GetValue<string>() is string s4 ? s4 : null;
            _order = json.ContainsKey("order") && json["order"]?.GetValue<int>() is int o ? o : -1;

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

            /*
            if (json.ContainsKey("leds"))
            {
                JsonArray array = json["leds"].AsArray();
                _leds = new List<string>();

                foreach (var item in array)
                {
                    _leds.Add(item.GetValue<string>());
                }
            }*/
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

        protected Mode ToMode(string value)
        {
            switch (value)
            {
                case "rpm":
                    return Mode.RPM;
                case "individual":
                    return Mode.Individual;
                case "group":
                    return Mode.Group;
            }

            return Mode.None;
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
            json["jj_prop"] = _jjproperty;
            json["color"] = _hexColors;
            json["order"] = _order;

            JsonArray leds = new JsonArray();

            if (_leds != null)
            {
                foreach (string item in _leds)
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
                _mode = ToMode(data["mode"].GetValue<string>());
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
                    _leds = new List<string>();
                }

                _leds.Clear();

                foreach (string led in data["leds"].AsArray().Select(v => (string)v))
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
    }
}
