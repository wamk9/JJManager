using JJManager.Class.App.Input.MacroKey.Keyboard;
using JJManager.Class.App.Input.MacroKey.Mouse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace JJManager.Class.App.Input.MacroKey
{
    public class MacroKey
    {
        private ObservableCollection<MacroKeyAction> _actions = null;

        public ObservableCollection<MacroKeyAction> Actions
        {
            get => _actions;
            set => _actions = value;
        }

        public MacroKey() 
        { 
            _actions = new ObservableCollection<MacroKeyAction>();
        }

        public MacroKey(List<MacroKeyAction> actions)
        {
            OrderActions(actions);
        }

        public MacroKey(JsonObject json)
        {
            _actions = new ObservableCollection<MacroKeyAction>();
            if (json.ContainsKey("actions"))
            {
                foreach (JsonObject action in json["actions"].AsArray())
                {
                    _actions.Add(new MacroKeyAction(action));
                }

                OrderActions();
            }
        }

        public JsonObject ActionsToJson()
        {
            JsonArray json = new JsonArray();
            OrderActions();

            foreach (MacroKeyAction action in _actions)
            {
                json.Add(action.ToJson());
            }

            return new JsonObject() 
            {
                { "actions", json }
            };
        }

        public bool Update(int order, MacroKeyAction action)
        {
            // OBS: Remove and after insert because of observable collection
            if (order > -1 && order < _actions.Count && _actions.Count > 0) 
            {
                _actions.RemoveAt(order);
                action.Order = (uint) order;
            }
            else
            {
                action.Order = (uint) _actions.Count;
            }


            if (_actions.Count == 0)
            {
                _actions.Add(action);
            }
            else
            {
                _actions.Insert((int) action.Order, action);
            }

            return true;
        }

        public void OrderActions(List<MacroKeyAction> actions = null)
        {
            // Sort by Value
            List<MacroKeyAction> sorted = actions ?? _actions.OrderBy(item => item.Order).ToList();
            uint newOrder = 0;

            // Clear the original collection and repopulate with the sorted items (Need clean ListView)
            _actions.Clear();

            foreach (MacroKeyAction action in sorted)
            {
                action.Order = newOrder;
                _actions.Add(action);
                newOrder++;
            }
        }

        public bool Execute()
        {
            foreach(MacroKeyAction action in _actions)
            {
                if (action.Data != null)
                {
                    switch (action.Type)
                    {
                        case MacroKeyAction.ActionType.Mouse:
                        
                            MouseController mouseController = new MouseController();

                            if (action.Data.ContainsKey("mouseX"))
                            {
                                mouseController.MoveMouseBy(action.Data["mouseX"].GetValue<int>(), 0);
                            }

                            if (action.Data.ContainsKey("mouseY"))
                            {
                                mouseController.MoveMouseBy(0, action.Data["mouseY"].GetValue<int>());
                            }

                            if (action.Data.ContainsKey("mouseClick"))
                            {
                                switch (action.Data["mouseClick"].GetValue<string>())
                                {
                                    case "right":
                                        mouseController.ClickRightButton();
                                        break;
                                    case "middle":
                                        mouseController.ClickMiddleButton();
                                        break;
                                    case "left":
                                        mouseController.ClickLeftButton();
                                        break;
                                }
                            }
                        break;
                        case MacroKeyAction.ActionType.KeyboardKey:
                            KeyboardController keyboardKeyController = new KeyboardController();

                            if (action.Data.ContainsKey("keyboardKeyCode") && action.Data.ContainsKey("keyboardKeyAction"))
                            {
                                switch (action.Data["keyboardKeyAction"].GetValue<string>())
                                {
                                    case "press/release":
                                        keyboardKeyController.PressKey(action.Data["keyboardKeyCode"].GetValue<ushort>());
                                        break;
                                    case "release":
                                        keyboardKeyController.ReleaseKey(action.Data["keyboardKeyCode"].GetValue<ushort>());
                                        break;
                                    case "press":
                                        keyboardKeyController.PressKey(action.Data["keyboardKeyCode"].GetValue<ushort>(), true);
                                        break;
                                }
                            }

                            break;
                        case MacroKeyAction.ActionType.KeyboardText:
                            KeyboardController keyboardTextController = new KeyboardController();

                            if (action.Data.ContainsKey("keyboardText"))
                            {
                                keyboardTextController.WriteText(action.Data["keyboardText"].GetValue<string>());
                            }

                            break;
                        case MacroKeyAction.ActionType.Wait:
                            if (action.Data.ContainsKey("waitTime"))
                            {
                                Task.Run(() => { Thread.Sleep(action.Data["waitTime"].GetValue<int>()); }).Wait();
                            }

                            break;
                    }
                }
            }

            return true;
        }
    }
}
