using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.XEvent;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static JJManager.Class.App.Input.Input;
using System.Windows.Forms;
using JJManager.Class.App.Input.MacroKey.Keyboard;

namespace JJManager.Class.App.Input.MacroKey
{
    public class MacroKeyAction : INotifyCollectionChanged
    {
        public enum ActionType
        {
            None,
            Mouse,
            KeyboardKey,
            KeyboardText,
            Wait
        }

        
        private ActionType _actionType = ActionType.None;
        private uint _order = 0;
        private JsonObject _data = null;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ActionType Type 
        {
            get => _actionType;
        }
        public string TypeList
        {
            get => _actionType.ToString();
        }
        public JsonObject Data
        {
            get => _data;
        }
        public uint Order
        {
            get => _order;
            set => _order = value;
        }

        public string Description
        {
            get => DescriptionToList();
        }

        private string DescriptionToList()
        {
            List<string> descriptionPart = new List<string>();

            if (_actionType == ActionType.None)
            {
                descriptionPart.Add("Macrokey sem ação definida");
            }
            else if (_actionType == ActionType.Mouse)
            {
                if (_data.ContainsKey("mouseX"))
                {
                    int mouseX = _data["mouseX"].GetValue<int>();

                    if (mouseX > 0)
                    {
                        descriptionPart.Add("Movimentará o mouse " + mouseX + " pixels para direita");
                    }
                    else if (mouseX < 0)
                    {
                        descriptionPart.Add("Movimentará o mouse " + Math.Abs(mouseX) + " pixels para esquerda");
                    }
                }

                if (_data.ContainsKey("mouseY"))
                {
                    int mouseY = _data["mouseY"].GetValue<int>();

                    if (mouseY > 0)
                    {
                        descriptionPart.Add("Movimentará o mouse" + mouseY + " pixels para cima");
                    }
                    else if (mouseY < 0)
                    {
                        descriptionPart.Add("Movimentará o mouse" + Math.Abs(mouseY) + " pixels para baixo");
                    }
                }

                if (_data.ContainsKey("mouseClick"))
                {
                    string mouseClick = _data["mouseClick"].GetValue<string>();

                    if (mouseClick == "right")
                    {
                        descriptionPart.Add("Clicará com o botão esquerdo do mouse");
                    }
                    else if (mouseClick == "middle")
                    {
                        descriptionPart.Add("Clicará com o botão do meio do mouse");
                    }
                    else if (mouseClick == "left")
                    {
                        descriptionPart.Add("Clicará com o botão direito do mouse");
                    }
                }
            }
            else if (_actionType == ActionType.KeyboardKey)
            {
                if (_data.ContainsKey("keyboardKeyAction") && _data.ContainsKey("keyboardKeyCode"))
                {
                    switch (_data["keyboardKeyAction"].GetValue<string>())
                    {
                        case "press":
                            descriptionPart.Add("Irá pressionar e manterá a tecla " + KeyboardController.GetFriendlyKeyNameFromScanCode(_data["keyboardKeyCode"].GetValue<ushort>()));
                            break;
                        case "release":
                            descriptionPart.Add("Irá soltar a tecla " + KeyboardController.GetFriendlyKeyNameFromScanCode(_data["keyboardKeyCode"].GetValue<ushort>()));
                            break;
                        case "press/release":
                            descriptionPart.Add("Irá pressionar e soltar a tecla " + KeyboardController.GetFriendlyKeyNameFromScanCode(_data["keyboardKeyCode"].GetValue<ushort>()));
                            break;
                    }
                }
            }
            else if (_actionType == ActionType.KeyboardText)
            {
                if (_data.ContainsKey("keyboardText"))
                {
                    descriptionPart.Add("Digitará onde estiver em foco '" + _data["keyboardText"].GetValue<string>() + "'.");
                }
            }
            else if (_actionType == ActionType.Wait)
            {
                if (_data.ContainsKey("waitTime"))
                {
                    descriptionPart.Add("Irá esperar por aproximadamente " + _data["waitTime"].GetValue<uint>() + "ms até executar a próxima ação.");
                }
            }


            return string.Join("; ", descriptionPart.ToArray());
        }

        public MacroKeyAction(uint order, ActionType type, JsonObject data) 
        {
            _order = order;
            _actionType = type;
            _data = data;
        }

        public MacroKeyAction(JsonObject json)
        {
            _order = json.ContainsKey("order") ? json["order"].GetValue<uint>() : 0;
            _actionType = json.ContainsKey("actionType") ? ToActionType(json["actionType"].GetValue<string>()) : ActionType.None;
            _data = json.ContainsKey("data") ? JsonObject.Parse(json["data"].GetValue<string>()).AsObject() : new JsonObject();
        }

        public JsonObject ToJson()
        {
            return new JsonObject()
            {
                { "order", _order },
                { "actionType", _actionType.ToString().ToLower() },
                { "data", _data.ToJsonString() }
            };
        }

        protected ActionType ToActionType(string value)
        {
            switch (value)
            {
                case "mouse":
                    return ActionType.Mouse;
                case "keyboardkey":
                    return ActionType.KeyboardKey;
                case "keyboardtext":
                    return ActionType.KeyboardText;
                case "wait":
                    return ActionType.Wait;
            }

            return ActionType.None;
        }
    }
}
