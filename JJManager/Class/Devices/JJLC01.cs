using HidSharp;
using JJManager.Class.App.Input;
using System.Linq;
using System.Text.Json.Nodes;
using System;
using System.Threading.Tasks;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using AudioSwitcher.AudioApi.CoreAudio;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;
using System.Windows;
using LiveCharts.Wpf;
using System.Globalization;

namespace JJManager.Class.Devices
{
    public class JJLC01 : HIDClass
    {
        private volatile bool _requesting = false;
        private volatile bool _sending = false;
        private readonly int _connectionTimeoutLimit = 2;
        private int _actualConnectionTimeout = 0;
        private SynchronizationContext _syncContext;
        private App.Profile.Profile _profileInDevice = null;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _classNeedsInitialize = true;

        public JJLC01(HidDevice hidDevice) : base(hidDevice)
        {
            RestartClass();
        }

        public void RestartClass()
        {
            _actionReceivingData = () => { Task.Run(async () => await ActionReceivingData(_cts.Token)); };
            _actionSendingData = () => { Task.Run(async () => await ActionSendingData(_cts.Token)); };
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
            _syncContext = SynchronizationContext.Current; // Capture UI thread context
        }

        private async Task ActionReceivingData(CancellationToken token)
        {
            try
            {
                if (_isConnected)
                {
                    _profileInDevice = new App.Profile.Profile(this, "Perfil Ativo Ao Conectar", true);
                    await RequestData(true);
                    _profile = _profileInDevice;
                }

                while (!token.IsCancellationRequested && _isConnected)
                {
                    if (!_profile.NeedsUpdate)
                    {
                        _profile.Restart();
                    }

                    await RequestData();
                    await Task.Delay(50, token); // prevent tight loop
                }
            }
            catch (OperationCanceledException)
            {
            
            }
            finally
            {
                Pages.Devices.JJLC01.NotifyDisconnectedDevice(_connId);
                //await RestartClass();
            }
        }

        private async Task ActionSendingData(CancellationToken token)
        {
            try
            {
                _profile.Restart();

                while (!token.IsCancellationRequested && _isConnected)
                {
                    if (_profile.NeedsUpdate)
                    {
                        _profile.Restart();
                        _profile.NeedsUpdate = false;
                    }

                    await SendData();
                    await Task.Delay(1000, token);
                }
            }
            catch (OperationCanceledException) 
            {

            }
            finally
            {
                Pages.Devices.JJLC01.NotifyDisconnectedDevice(_connId);
                //await RestartClass();
                //_classNeedsInitialize = false;
            }
        }

        private async Task SendData()
        {
            try
            {
                _sending = true;

                JsonArray jsonArray = new JsonArray();
                Input[] profileInputs = _profile.Inputs.ToArray();

                string messageToSend = _profile.Data.ToJsonString();
                //Console.WriteLine(messageToSend);

                for (int i = 0; i < 2; i++)
                {
                    await SendHIDData(messageToSend, false, 500, 200);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                _sending = false;
            }
        }
        public async Task RequestData(bool sendToActualProfile = false)
        {
            try
            {
                _requesting = true;
                for (int i = 0; i < _connectionTimeoutLimit; i++)
                {
                    string requestedJsonString = await RequestHIDData(new JsonObject { { "request", new JsonArray { { "actual_data" } } } }.ToJsonString(), false, 100, 100);

                    Console.WriteLine(requestedJsonString);

                    if (!string.IsNullOrEmpty(requestedJsonString))
                    {
                        JsonObject requestedJson = (JsonObject)JsonNode.Parse(requestedJsonString);

                        if (requestedJson.ContainsKey("pot_percent"))
                        {
                            Pages.Devices.JJLC01.UpdatePotPercent(requestedJson["pot_percent"].GetValue<int>());
                        }

                        if (requestedJson.ContainsKey("kg_pressed"))
                        {
                            Pages.Devices.JJLC01.UpdateKgPressed(requestedJson["kg_pressed"].GetValue<double>());
                        }

                        if (requestedJson.ContainsKey("raw"))
                        {
                            Pages.Devices.JJLC01.UpdateRawData(requestedJson["raw"].GetValue<int>());
                        }

                        if (requestedJson.ContainsKey("fine_offset"))
                        {
                            Pages.Devices.JJLC01.UpdateFineOffset(requestedJson["fine_offset"].GetValue<int>());
                        }

                        if (requestedJson.ContainsKey("adc") && sendToActualProfile)
                        {
                            JsonArray adcArray = requestedJson["adc"].AsArray();

                            double[] adcValues = adcArray.Select(x => x.GetValue<double>()).ToArray();

                            string jsonString = adcArray.ToString();

                            // Parse the JSON string into a JsonObject
                            var jsonObjectFromString = JsonArray.Parse(jsonString.Replace('"', ' '))[0];

                            JsonObject jsonToUpdate = new JsonObject
                                {
                                    {
                                        "data",  new JsonObject
                                        {
                                            { "jjlc01_data", new JsonObject { {"adc", requestedJson["adc"].AsArray().DeepClone() } } }
                                        }
                                    }
                                };

                            if (_profile.Id == _profileInDevice.Id)
                            {
                                _profile.Update(jsonToUpdate);
                            }
                            else
                            {
                                _profileInDevice.Update(jsonToUpdate);
                            }
                        
                            //Pages.Devices.JJLC01.UpdateSeries(adcValues);
                        }

                        _actualConnectionTimeout = 0;
                        break;
                    }
                    else
                    {
                        _actualConnectionTimeout++;

                        if (_actualConnectionTimeout >= _connectionTimeoutLimit)
                        {
                            Disconnect();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _requesting = false;
            }
        }
    }
}
