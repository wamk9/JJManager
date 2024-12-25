using HidSharp;
using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using JoystickSharpDXClass = SharpDX.DirectInput.Joystick;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Class.Devices.Connections
{
    public class Joystick : JJDevice
    {
        private JoystickSharpDXClass _joystick = null;

        public Joystick(JoystickSharpDXClass joystick) : base()
        {
            _joystick = joystick;
            _productName = _joystick.Properties.ProductName;
            _type = Type.Joystick;
            _connId = _joystick.Properties.ClassGuid.GetHashCode().ToString();
            GetProductID();
            _profile = new ProfileClass(this);
            GetUserProductID();
        }

        protected (bool, JoystickState) ReceiveJoystickData()
        {
            bool result = false;
            JoystickState currentState = null;

            try
            {
                _joystick.Acquire();
                _joystick.Poll();

                currentState = _joystick.GetCurrentState(); //only show the last state

                _joystick.Unacquire();
            }
            catch (SharpDXException ex)
            {
                if ((ex.ResultCode == ResultCode.NotAcquired) || (ex.ResultCode == ResultCode.InputLost))
                {
                    Log.Insert("Joystick", "Ocorreu um problema relacionado ao SharpDX", ex);
                }
            }
            catch (Exception ex)
            {
                Log.Insert("Joystick", "Ocorreu um problema relacionado a busca dos dados de inputs de joystick", ex);
            }

            return (result, currentState);
        }

        protected int GetAxisPercent (int axisRaw)
        {
            return (axisRaw > -1 ?  (axisRaw * 100 / 65530) : -1);
        }
    }
}
