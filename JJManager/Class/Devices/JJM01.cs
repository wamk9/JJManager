using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp;
using JJManager.Class.App.Input;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJM01 : HIDClass
    {
        private CoreAudioController coreAudioController = new CoreAudioController();
        private volatile bool _requesting = false;
        private volatile bool _sending = false;
        private readonly int _connectionTimeoutLimit = 10;
        private int _actualConnectionTimeout = 0;

        public JJM01(HidDevice hidDevice) : base (hidDevice)
        {
            _actionReceivingData = () => { Task.Run(async () => await ActionReceivingData()); };
            _actionSendingData = () => { Task.Run(async () => await ActionSendingData()); };
        }

        private async Task ActionReceivingData()
        {
            if (_isConnected)
            {
                UpdateCoreAudioController(true);
                await SendInputs();
            }
            while (_isConnected)
            {
                //await UpdateCoreAudioController();
                if (!_profile.NeedsUpdate)
                {
                    await RequestData();
                }
            }
        }

        private async Task ActionSendingData()
        {
            while (_isConnected)
            {
                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    UpdateCoreAudioController(true);
                    await SendInputs();
                    _profile.NeedsUpdate = false;
                }

                UpdateCoreAudioController();
                await Task.Delay(1000);
            }
        }

        public async Task SendInputs()
        {
            _sending = true;

            try
            {
                JsonArray jsonArray = new JsonArray();
                Input[] profileInputs = _profile.Inputs.ToArray();

                foreach (Input inputToSend in profileInputs)
                {
                    string messageToSend = new JsonObject
                    {
                        { "data", new JsonObject
                            {
                                { "order", inputToSend.Id },
                                { "name", inputToSend.Name }
                            }
                        }
                    }.ToJsonString();
                    //Console.WriteLine(messageToSend);

                    for (int i = 0; i < 2; i++)
                    {
                        await SendHIDData(messageToSend, false, 500, 200);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _sending = false;
            }
        }

        public void UpdateCoreAudioController(bool forceUpdate = false)
        {
            bool createNewCoreAudioController = forceUpdate;

            if (!createNewCoreAudioController)
            {
                for (int i = 0; i < _profile.Inputs.Count; i++)
                {
                    if (_profile.Inputs[i]?.AudioController?.AudioCoreNeedsRestart ?? false)
                    {
                        createNewCoreAudioController = true;
                        break;
                    }
                }
            }

            if (createNewCoreAudioController)
            {
                coreAudioController = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                coreAudioController = new CoreAudioController();

                for (int i = 0; i < _profile.Inputs.Count; i++)
                {
                    if (_profile.Inputs[i]?.AudioController != null)
                    {
                        _profile.Inputs[i].AudioController.ResetCoreAudioController(coreAudioController).Wait();
                        _profile.Inputs[i].AudioController.AudioCoreNeedsRestart = false;
                        _profile.Inputs[i].Execute();
                    }
                }
            }
        }

        public async Task RequestData()
        {
            //Wait to acquire the semaphore
            if (_sending || _requesting || coreAudioController == null)
            {
                return;
            }

            try
            {
                _requesting = true;
                for (int i = 0; i < _connectionTimeoutLimit; i++)
                {
                    string requestedJsonString = await RequestHIDData(new JsonObject { { "request", new JsonArray { { "input-all" } } } }.ToJsonString(), false, 100, 100);

                    //Console.WriteLine(requestedJsonString);

                    if (!string.IsNullOrEmpty(requestedJsonString))
                    {
                        JsonObject requestedJson = JsonObject.Parse(requestedJsonString).AsObject();

                        if (requestedJson.ContainsKey("input-all"))
                        {
                            for (int x = 0; x < requestedJson["input-all"].AsArray().Count; x++)
                            {
                                if (_profile.Inputs[x].AudioController != null && _profile.Inputs[x].AudioController.SettedVolume != requestedJson["input-all"][x].GetValue<int>())
                                {
                                    _profile.Inputs[x].AudioController.SettedVolume = requestedJson["input-all"][x].GetValue<int>();
                                    _profile.Inputs[x].Execute();
                                }
                            }
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
            finally
            {
                _requesting = false;
            }
        }
    }
}
