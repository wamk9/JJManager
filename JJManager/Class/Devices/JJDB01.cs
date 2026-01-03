using HidSharp;
using JJManager.Class.App;
using JJManager.Class.App.Output;
using JJManager.Class.App.Output.Leds;
using JJManager.Properties;
using Newtonsoft.Json.Linq;
using STBootLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Markup;
using static JJManager.Class.App.Output.Leds.Leds;
using HIDClass = JJManager.Class.Devices.Connections.HID;

namespace JJManager.Class.Devices
{
    public class JJDB01 : HIDClass
    {
        private SimHubWebsocket _simHubSync = null;
        private int HIDInfoLimit = 64 * 8 - 1;
        private JsonObject _requestedData = null;
        private readonly int _connectionTimeoutLimit = 10;
        private int _actualConnectionTimeout = 0;
        private string _dashboardId = "default";
        private int _dashboardActualPage = -1;
        private bool _sendAllDashData = false;
        private uint _qtdPassedAllData = 0;
        private uint _limitPassedAllData = 1;
        private bool _lastGameRunningState = false;
        private JsonArray _DashboardPagesVariables = null;
        private Channel<(JsonObject, JsonObject)> _incomingQueue;
        private CancellationTokenSource _cts;
        private Task _pollTask;
        private Task _processTask;
        private Task _sendTask;
        private List<string> jsonsToSend = new List<string>();


        public JJDB01(HidDevice hidDevice) : base(hidDevice)
        {
            _reportId = 0x00;

            _cmds = new HashSet<ushort>
            {
                0x0000, // Send LED Data
                0x0001, // Request/Send Dashboard Data
                0x0002, // Receive Actual Dashboard Layout
                0x0003, // Receive Actual Dashboard Page
                0xffff  // Request/Send Device Info
            };

            RestartClass();
        }

        private void RestartClass()
        {
            _DashboardPagesVariables = JsonNode.Parse(Properties.Resources.JJDB01_dashboard_vars).AsArray();
            _actionSendingData = () => { Task.Run(async () => await ActionSendingData()); };
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
            
            if (_cts != null)
            {
                _cts.Cancel();
            }

            _cts = new CancellationTokenSource();
    //        _incomingQueue = Channel.CreateBounded<(JsonObject, JsonObject)>(
    //new BoundedChannelOptions(1)
    //{
    //    SingleWriter = true,
    //    SingleReader = true,
    //    FullMode = BoundedChannelFullMode.DropOldest // keep only latest
    //});
    //        // bounded=1 → keeps only the latest data, avoids backlog

    //        _pollTask = Task.Run(async () => { while (_isConnected) { await PollSimHubPlugin(_cts.Token); } });
    //        _processTask = Task.Run(async () => { while (_isConnected) { await ProcessJJDB01Json(_cts.Token); } });
    //        _sendTask = Task.Run(async () => { while (_isConnected) { await ProcessSendQueue(_cts.Token); } });

    //        Task.WhenAll(new List<Task> { _pollTask, _processTask, _sendTask });
        }


        private List<string> GetFIFOJson()
        {
            return jsonsToSend;
        }

        private async Task ProcessSendQueue(CancellationToken token)
        {

            try
            {
                List<string> jsons = GetFIFOJson().ToList();

                if (jsons.Count == 0)
                {
                    return;
                }

                foreach (var jsonString in jsons)
                {
                    if (jsonString.StartsWith("{\"led_data\":"))
                    {
                        Console.WriteLine("Enviou Led");
                        await SendHIDData(jsonString, false, 0, 50, 0).ContinueWith((result) =>
                        {
                            if (!result.Result)
                            {
                                throw new Exception($"Falha ao enviar '{jsonString}' via HID para o dashboard JJDB-01 de ID '{_connId}'");
                            }
                        });
                        Console.WriteLine("ok");
                    }
                    else
                    {
                        Console.WriteLine("Enviou Dash");
                        string requestedJsonString = await RequestHIDData(jsonString, false, 0, 2000, 0);
                        if (!string.IsNullOrEmpty(requestedJsonString))
                        {
                            Console.WriteLine("Recebeu resposta");
                            try
                            {
                                JsonObject requestedJson = JsonObject.Parse(requestedJsonString).AsObject();

                                _dashboardId = requestedJson.ContainsKey("id") ? requestedJson["id"].GetValue<string>() : "default";
                                _dashboardActualPage = requestedJson.ContainsKey("pag") ? requestedJson["pag"].GetValue<int>() : 0;
                                _sendAllDashData = requestedJson.ContainsKey("data") && requestedJson["data"].GetValue<uint>() == 1;
                            }
                            catch (Exception)
                            {
                                // JsonObject Failed
                            }
                        }
                        Console.WriteLine("OK");
                    }
                }
                Console.WriteLine("Enviou");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Send] Error: {ex}");
            }
        }

        private async Task ActionSendingData()
        {
            while (_isConnected)
            {
                await SendData();
            }
        }

        public async Task SendData()
        {
            bool forceDisconnection = false;
            Console.WriteLine("Entrou");
            try
            {
                if (_profile.NeedsUpdate)
                {
                    _profile.Restart();
                    _profile.NeedsUpdate = false;
                }

                bool needsSimHub = true;//_profile.Outputs.Any(x => x.Led?.Mode == -1);

                if (needsSimHub)
                {
                    if (_simHubSync == null)
                    {
                        _simHubSync = new SimHubWebsocket(2920, "JJDB01_" + _connId);
                    }

                    if (!_simHubSync.IsConnected)
                    {
                        _simHubSync.StartCommunication();
                    }

                    var (success, message) = _simHubSync.RequestMessage(
                        new JsonObject {
                        {
                            "request", new JsonArray {
                                { "SimHubLastData" }
                            }
                        }
                        }.ToJsonString(),
                        0
                    );

                    if (!success)
                    {
                        throw new WebSocketException("Falha ao fazer requisição ao plugin 'JJManager Sync' do SimHub");
                    }
                }

                if (!needsSimHub && _simHubSync != null)
                {
                    _simHubSync.StopCommunication();
                    _simHubSync = null;
                }













                JsonArray ledsData = new JsonArray();
                int brightness = (_profile.Data.ContainsKey("brightness") ? _profile.Data["brightness"].GetValue<int>() : 100);

                Dictionary<string, int> ledModeList = new Dictionary<string, int>
                {
                    { "simhub", -1 },
                    { "off", 0 },
                    { "on", 1 },
                    { "blink", 2 },
                    { "pulse", 3 }
                };

                for (int i = 0; i < 16; i++)
                {
                    _profile.OrderOutputsBy(Output.OutputMode.Leds, i);
                }

                Dictionary<int, bool> ledIsActivated = new Dictionary<int, bool>();

                List<List<byte>> ledsToSend = new List<List<byte>>();

                foreach (Output output in _profile.Outputs.Where(x => x.Led != null).OrderByDescending(x => x.Led.Order))
                {
                    bool lastIsActive = output.Led.Active;

                    output.Led.SetActivatedValue(_simHubSync?.LastValues?[output.Led.Property]?.GetValue<dynamic>() ?? false);

                    if (!lastIsActive || ledsToSend.Any(x => x[2] == (byte)output.Led.LedsGrouped.FirstOrDefault()))
                    {
                        continue;
                    }

                    var ledBytes = new List<byte>
                    {
                        (byte)(0x0000 >> 8),    // High byte
                        (byte)(0x0000 & 0xFF)  // Low byte
                    };

                    ledBytes.AddRange(Encoding.ASCII.GetBytes(output.Led.LedsGrouped.FirstOrDefault().ToString("D2")));

                    // Adiciona a string de cor como bytes (ex: "#FF00FF")
                    ledBytes.AddRange(Encoding.ASCII.GetBytes(output.Led.GetActualLedColor));

                    // Terminador (nova linha)
                    ledBytes.Add(0x0A);

                    ledsToSend.Add(ledBytes);
                }

                for (int i = 0; i < 16; i++) // Actual LEDs on JJDB-01
                {
                    byte[] iStr = Encoding.ASCII.GetBytes(i.ToString("D2"));

                    if (!ledsToSend.Any(x =>
                        x[2] == iStr[0] &&
                        x[3] == iStr[1]))
                    {
                        var ledBytes = new List<byte>
                        {
                            (byte)(0x0000 >> 8),    // High byte
                            (byte)(0x0000 & 0xFF),  // Low byte
                        };

                        ledBytes.AddRange(Encoding.ASCII.GetBytes(i.ToString("D2")));

                        // Adiciona a string de cor como bytes (ex: "#FF00FF")
                        ledBytes.AddRange(Encoding.ASCII.GetBytes("#000000"));

                        // Terminador (nova linha)
                        ledBytes.Add(0x0A);

                        ledsToSend.Add(ledBytes);
                    }
                }


                //foreach (var (output, ledDataTranslated) in _profile.Outputs
                //    .Where(x => x.Led != null)
                //    .OrderByDescending(x => x.Led.Order)
                //    .SelectMany(output => output.Led.TranslateToDevice(ledModeList, _simHubSync?.LastValues ?? null).Select(ledData => (output, ledData)))

                //)
                //{
                //    JsonArray ledArray = ledDataTranslated.AsArray();
                //    if (ledArray.Count != 3) continue;

                //    int ledIndex = ledArray[1].GetValue<int>();
                //    bool isActivated = output.Led.CheckIfIsActivated(
                //        _simHubSync?.LastValues[output.Led.Property]?.GetValue<dynamic>() ?? false
                //    );

                //    if (isActivated)
                //    {
                //        ledsData.Add(ledArray.DeepClone().AsArray());
                //    }

                //    ledIsActivated[ledIndex] = ledIsActivated.TryGetValue(ledIndex, out bool existing)
                //        ? existing || isActivated
                //        : isActivated;
                //}

                //HashSet<int> inactiveLedIndices = ledIsActivated
                //    .Where(kvp => !kvp.Value)
                //    .Select(kvp => kvp.Key)
                //    .ToHashSet();

                //for (short i = 0; i < 16; i++)
                //{
                //    var latestLed = _profile.Outputs
                //        .Where(x => x.Led != null && x.Led.Order == i)
                //        .OrderByDescending(x => x.) // or use Order / Index
                //        .Select(x => x.Led)
                //        .FirstOrDefault();
                //}

                //foreach (var output in _profile.Outputs.Where(x => x.Led != null))
                //{
                //    var ledType = output.Led.Type.ToString().ToLower();

                //    foreach (int ledPos in output.Led.LedsGrouped)
                //    {
                //        if (!inactiveLedIndices.Contains(ledPos))
                //            continue;

                //        var offValue = output.Led.Type == Leds.LedType.PWM ? (object) 0 : "#000000";
                //        ledsData.Add(new JsonArray { ledType, ledPos, offValue });
                //    }
                //}

                //JsonObject data = new JsonObject
                //{
                //    {
                //        "led_data" , new JsonObject
                //        {
                //            { "config", new JsonObject
                //                {
                //                    { "brightness", brightness},
                //                }
                //            },
                //            { "leds", new JsonArray() }
                //        }
                //    }
                //};

                //JsonObject dataToCount = data.DeepClone().AsObject();
                //JsonObject dataToConvert = null;

                //string jsonTmp = string.Empty;
                //bool lastNeedsSave = false;

                //for (int i = 0; i < ledsData.Count; i++)
                //{
                //    var currentLed = ledsData[i].DeepClone().AsArray();
                //    dataToCount["led_data"]["leds"].AsArray().Add(currentLed);

                //    string serialized = JsonSerializer.Serialize(dataToCount);

                //    if (serialized.Length < HIDInfoLimit)
                //    {
                //        dataToConvert = dataToCount.DeepClone().AsObject();
                //        lastNeedsSave = true;
                //    }
                //    else
                //    {
                //        // Remove last added LED (rollback)
                //        dataToCount["led_data"]["leds"].AsArray().RemoveAt(
                //            dataToCount["led_data"]["leds"].AsArray().Count - 1
                //        );

                //        // Send previous valid set
                //        jsonsToSend.Add(JsonSerializer.Serialize(dataToConvert));

                //        // Start new set
                //        dataToCount = data.DeepClone().AsObject();
                //        dataToCount["led_data"]["leds"].AsArray().Add(currentLed);
                //    }

                //    if ((i + 1) == ledsData.Count && lastNeedsSave)
                //    {
                //        jsonsToSend.Add(JsonSerializer.Serialize(dataToConvert));
                //    }
                //}

                // Isso é ref a pegada de ID e paginação
                //string dashboardActualId = await RequestHIDBytes(new List<byte>
                //{
                //    (byte)(0x0002 >> 8),    // High byte
                //    (byte)(0x0002 & 0xFF),  // Low byte
                //    0x0A
                //}, false, 0, 20, 0);

                //int.TryParse(await RequestHIDBytes(new List<byte>
                //    {
                //        (byte)(0x0003 >> 8),    // High byte
                //        (byte)(0x0003 & 0xFF),  // Low byte
                //        0x0A
                //    }, false, 0, 20, 0), out int dashboardActualPage);

                // Check if GameRunning changed from 0 to 1 (game started/resumed)
                bool currentGameRunningState = false;
                if (_simHubSync?.LastValues != null && _simHubSync.LastValues.ContainsKey("GameRunning"))
                {
                    string gameRunningValue = _simHubSync.LastValues["GameRunning"]?.GetValue<string>() ?? "0";
                    currentGameRunningState = gameRunningValue != "0";
                }

                if (!_lastGameRunningState && currentGameRunningState)
                {
                    // Game just started/resumed - force resend all data
                    _sendAllDashData = true;
                    _qtdPassedAllData = 0;
                }
                _lastGameRunningState = currentGameRunningState;

                //if (dashboardActualId != _dashboardId || _dashboardActualPage != dashboardActualPage)
                //{
                //    _sendAllDashData = true;
                //    _qtdPassedAllData = 0;
                //    _dashboardId = dashboardActualId.Count(x => (byte) x != 0x00) > 0 ? dashboardActualId : _dashboardId;
                //    _dashboardActualPage = dashboardActualPage;
                //}

                _sendAllDashData = _sendAllDashData && _limitPassedAllData > _qtdPassedAllData;
                _qtdPassedAllData = _sendAllDashData ? _qtdPassedAllData + 1 : 0;

                // MODIFIED: Always send ALL updated data, regardless of current page
                // This ensures the screen has all telemetry data cached and available
                TranslateToDeviceScreen(
                    _simHubSync?.LastValuesUpdated ?? new JsonObject(),  // Always send updated values
                    null,  // null = send all properties (don't filter by page)
                    out List<List<byte>> translatedDashMessage,
                    HIDInfoLimit
                );

                Console.WriteLine("VaiEnviar");

                await SendHIDBytes(ledsToSend, false, 0, 2000, 5);
                await SendHIDBytes(translatedDashMessage, false, 0, 2000, 5);

                //await SendHIDBytes(new List<byte>
                //{
                //    (byte)(0x0005 >> 8),    // High byte
                //    (byte)(0x0005 & 0xFF),  // Low byte
                //    0x0A
                //}, false, 0, 0, 0);

                Console.WriteLine("Enviei Tudo");



                //_sendAllDashData = false;
                //jsonsToSend.AddRange(translatedDashMessage);
                //Console.WriteLine("Vai enviar");
                //foreach (string jsonString in jsonsToSend)
                //{
                //    if (jsonString.StartsWith("{\"led_data\":"))
                //    {
                //        Console.WriteLine("Enviou Led");
                //        await SendHIDData(jsonString, false, 0, 50, 0).ContinueWith((result) =>
                //        {
                //            if (!result.Result)
                //            {
                //                throw new Exception($"Falha ao enviar '{jsonString}' via HID para o dashboard JJDB-01 de ID '{_connId}'");
                //            }
                //        });
                //        Console.WriteLine("ok");
                //    }
                //    else
                //    {
                //        Console.WriteLine("Enviou Dash");
                //        string requestedJsonString = await RequestHIDData(jsonString, false, 0, 2000, 0);
                //        if (!string.IsNullOrEmpty(requestedJsonString))
                //        {
                //            Console.WriteLine("Recebeu resposta");
                //            try
                //            {
                //                JsonObject requestedJson = JsonObject.Parse(requestedJsonString).AsObject();

                //                _dashboardId = requestedJson.ContainsKey("id") ? requestedJson["id"].GetValue<string>() : "default";
                //                _dashboardActualPage = requestedJson.ContainsKey("pag") ? requestedJson["pag"].GetValue<int>() : 0;
                //                _sendAllDashData = requestedJson.ContainsKey("data") && requestedJson["data"].GetValue<uint>() == 1;
                //            }
                //            catch (Exception)
                //            {
                //                // JsonObject Failed
                //            }
                //        }
                //        Console.WriteLine("OK");
                //    }

                //}
                Console.WriteLine("Enviou");
            }
            catch (WebSocketException)
            {
                // Called when SimHub connection fails, without log.
                if (SimHubWebsocket.CancelAutoConnIfEnabled)
                {
                    AutoConnect = false;
                }

                forceDisconnection = true;
            }
            catch (Exception ex)
            {
                Log.Insert("JJDB01", "Ocorreu um problema ao enviar dados para o seu dashboard", ex);
                forceDisconnection = true;
            }
            finally
            {
                if (forceDisconnection)
                {
                    Disconnect();
                }
            }
        }

        public bool TranslateToDeviceScreen(JsonObject messageReceived, JsonArray varsToUse, out List<List<byte>> translatedMessage, int limitChars = 64 * 7)
        {
            translatedMessage = new List<List<byte>>();

            if (messageReceived.Count == 0)
            {    
                return true;
            }

            foreach (var kvp in messageReceived)
            {
                string propertyName = kvp.Key;
                JsonNode value = kvp.Value;

                if ((varsToUse != null && varsToUse.Any(x => x?.GetValue<string>() == propertyName)) || varsToUse == null)
                {
                    var dict = JsonNode.Parse(Resources.JJPropertyDictionary).AsObject();
                    var match = dict.FirstOrDefault(x => string.Equals(x.Value?.ToString(), propertyName, StringComparison.OrdinalIgnoreCase));

                    // SKIP if property not found in dictionary
                    if (match.Key == null)
                    {
                        continue;  // Ignore properties not in JJPropertyDictionary
                    }

                    // parse hex like "0x0001"
                    ushort cmdJJProperty = Convert.ToUInt16(match.Key, 16);

                    byte[] message = System.Text.Encoding.ASCII.GetBytes(value?.DeepClone().ToString().Trim());
                    int chunkSize = 27;
                    int numberChunks = (message.Length + chunkSize - 1) / chunkSize;

                    for (int i = 0; i < numberChunks; i++)
                    {
                        List<byte> splittedMessage = message.Skip(chunkSize * i).Take(chunkSize).ToList();

                        List<byte> messageConverted = new List<byte>
                        {
                            (byte)(_cmds.ElementAt(1) >> 8),    // High byte
                            (byte)(_cmds.ElementAt(1) & 0xFF),  // Low byte
                            (byte)(cmdJJProperty >> 8),    // High byte
                            (byte)(cmdJJProperty & 0xFF)  // Low byte
                        };


                        messageConverted.AddRange(splittedMessage);

                        // Flags: 0x2001 = END, 0x1001 = CONTINUE
                        ushort flag = (i + 1) == numberChunks ? (ushort)0x2001 : (ushort)0x1001;
                        messageConverted.Add((byte)(flag >> 8));   // FLAG_H
                        messageConverted.Add((byte)(flag & 0xFF)); // FLAG_L

                        translatedMessage.Add(messageConverted);
                    }
                }
            }

            return true;
        }

        public override bool Disconnect()
        {
            if (_simHubSync != null)
            {
                _simHubSync.StopCommunication();
                _simHubSync = null;
            }

            return base.Disconnect();
        }
    }
}
