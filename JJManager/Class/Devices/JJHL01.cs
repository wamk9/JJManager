using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp;
using JJManager.Class.App.Input;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;
using HIDClass = JJManager.Class.Devices.Connections.HID;
namespace JJManager.Class.Devices
{
    public class JJHL01 : HIDClass
    {
        private string _color = "ffffff";
        private string _mode = "static";
        public string Mode
        {
            get => _mode;
            set => _mode = value;
        }
        public string Color
        {
            get => _color;
            set => _color = value;
        }


        public JJHL01 (HidDevice hidDevice) : base (hidDevice) 
        {
            RestartClass();
        }
        private void RestartClass()
        {
            _actionSendingData = () => { Task.Run(() => ActionSendingData()); };
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
        }

        private void ActionSendingData()
        {
            while (_isConnected)
            {
                if (!SendData())
                {
                    Disconnect();
                }
            }
        }

        public void UpdateMusicBarData()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();

            try
            {
                int maxPeak = 0;

                foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
                {
                    maxPeak = Math.Max(maxPeak, (int)Math.Round(device.AudioMeterInformation.MasterPeakValue * 255));
                }

                _profile.Update(new JsonObject
                {
                    { "data", new JsonObject
                        {
                            { "order", 0 },
                            { "type", "musicBar" },
                            { "leds_qtd", 10 },
                            { "vol_level", maxPeak },
                            { "max_brightness", 100 },
                            { "color", new JsonArray
                                {
                                    { _color }
                                }
                            }
                        }
                    }
                });
            }
            finally
            {
                //enumerator.Dispose();
            }
        }

        public void UpdateStaticData()
        {
            try
            {
                _profile.Update(new JsonObject
                {
                    { "data", new JsonObject
                        {
                            { "order", 0 },
                            { "type", "static" },
                            { "leds_qtd", 10 },
                            { "max_brightness", 100 },
                            { "color", new JsonArray
                                {
                                    { _color }
                                }
                            }
                        }
                    }
                });
            }
            finally
            {
                //enumerator.Dispose();
            }
        }

        public void SetData(string color)
        {
            _profile.Update(new JsonObject
                {
                    { "data", new JsonObject
                        {
                            { "order", 0 },
                            { "type", "static" },
                            { "leds_qtd", 15 },
                            { "color", new JsonArray
                                {
                                    { color }
                                }
                            }
                        }
                    }
                });
        }

        public bool SendData()
        {
            bool result = false;

            if (_profile.NeedsUpdate)
            {
                _profile.Restart();
                _profile.NeedsUpdate = false;
            }

            //UpdateMusicBarData();
            UpdateStaticData();

            var data = new Dictionary<string, object>
                {
                    { "data", _profile.Data }
                };

            string messageToSend = JsonSerializer.Serialize(data);

            result = SendHIDData(messageToSend, false, 300).Result;

            return result;
        }
    }
}
