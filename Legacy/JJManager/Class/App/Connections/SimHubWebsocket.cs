using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.IdentityModel.Tokens;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using STBootLib;
using System.IdentityModel.Tokens;
using System.Net;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using SharpDX;
using JJManager.Class.Devices;
using JJManager.Class.App.Output.Leds;
using JJManager.Class.App.Input;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using MaterialSkin.Controls;
using Microsoft.Win32;
using System.Diagnostics;
using JJManager.Class.App.Output;

namespace JJManager.Class.App
{
    public class SimHubWebsocket
    {
        private ClientWebSocket _WebSocket = null;
        private string _IpAddress = "localhost";
        private string _DeviceName = "";
        private int _Port = 2920;
        private JsonObject _lastValues = null;
        private JsonObject _lastValuesUpdated = null;
        private Dictionary<int, string> ledColorsRight = new Dictionary<int, string>();
        private Dictionary<int, string> ledColorsLeft = new Dictionary<int, string>();
        private CancellationTokenSource _ctsConnection = new CancellationTokenSource();
        private CancellationTokenSource _ctsSendRequest = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        private CancellationTokenSource _ctsReceiveRequest = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        private static bool _cancelAutoConnIfEnabled = false;
        public bool IsConnected
        {
            get => _WebSocket.State == WebSocketState.Open;
        }

        public static bool CancelAutoConnIfEnabled
        {
            get => _cancelAutoConnIfEnabled;
        }
        public SimHubWebsocket(int port, string deviceName)
        {
            _Port = port;
            _DeviceName = (deviceName.Length == 0 ? "JJMANAGER_CONNECTION" : deviceName);
            WebSocketConfigInit();
            _lastValues = new JsonObject();
            _lastValuesUpdated = new JsonObject();
        }

        public void WebSocketConfigInit()
        {
            _WebSocket = new ClientWebSocket();
            _WebSocket.Options.SetRequestHeader("device", _DeviceName);
        }

        public JsonObject LastValues { get => _lastValues; }
        public JsonObject LastValuesUpdated { get => _lastValuesUpdated; }

        public static string GetSimHubInstallPath()
        {
            // Try both 64-bit and 32-bit registry paths
            string[] possibleKeys = new[]
            {
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{019253FE-5A17-42BE-A6B8-D71A729FA5DE}_is1"
            };

            foreach (string keyPath in possibleKeys)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        object pathValue = key.GetValue("InstallLocation");
                        if (pathValue != null)
                        {
                            return pathValue.ToString();
                        }
                    }
                }
            }

            return null; // Not found
        }

        private bool ShowMessageToOpenSimHub()
        {
            DialogResult result = Pages.App.MessageBox.Show(null, "Falha de conexão", $"Conexão com o SimHub não disponível, verifique se você possui o JJManager Sync ativo no mesmo ou se este encontra-se em execução.{Environment.NewLine}{Environment.NewLine}Deseja que o JJManager tente abrir o SimHub?", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string simHubPath = GetSimHubInstallPath();
                    if (!string.IsNullOrEmpty(simHubPath))
                    {
                        if (Process.GetProcessesByName("SimHubWPF").Length == 0)
                        {
                            Process.Start(Path.Combine(simHubPath, "SimHubWPF.exe"));
                        }

                        Task.Delay(TimeSpan.FromSeconds(15)).Wait(); // delay to wait SimHub...
                        return true;
                    }
                    else
                    {
                        Pages.App.MessageBox.Show(null, "SimHub Não Encontrado", "Caminho do SimHub não encontrado no registro do Windows.");
                        _cancelAutoConnIfEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Insert("Websocket", "Falha ao tentar iniciar o SimHub", ex);
                }
            }
            else
            {
                _cancelAutoConnIfEnabled = true;
            }

            return false;
        }

        public void StartCommunication(bool showDialog = true)
        {
            try
            {
                if (_WebSocket?.State == WebSocketState.None)
                {
                    _ctsConnection = (_ctsConnection?.IsCancellationRequested  == true ? new CancellationTokenSource() : _ctsConnection) ?? new CancellationTokenSource();
                    _WebSocket?.ConnectAsync(new Uri($"ws://{_IpAddress}:{_Port}/jjmanager/?device={_DeviceName}"), _ctsConnection.Token).Wait(5000);
                    
                    if (_WebSocket.State != WebSocketState.Open)
                    {
                        _ctsConnection.Cancel();
                        _ctsConnection.Dispose();
                        _ctsConnection = null;
                        throw new WebSocketException("Connection error");
                    }

                    //_WebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(1);
                }
            }
            catch (WebSocketException ex)
            {
                if (!(showDialog && ShowMessageToOpenSimHub()))
                {
                    Log.Insert("Websocket", "Connection error", ex);
                    throw ex;
                }
                else
                {
                    StartCommunication();
                }
            }
            catch (OperationCanceledException ex)
            {
                if (!(showDialog && ShowMessageToOpenSimHub()))
                {
                    Log.Insert("Websocket", "Cancelled operation", ex);
                    throw ex;
                }
                else
                {
                    StartCommunication();
                }
            }
            catch (AggregateException ex)
            {
                if (!(showDialog && ShowMessageToOpenSimHub()))
                {
                    Log.Insert("Websocket", "Connection error", ex);
                    throw ex;
                }
                else
                {
                    StartCommunication();
                }
            }
            catch (Exception ex)
            {
                if (!(showDialog && ShowMessageToOpenSimHub()))
                {
                    Log.Insert("Websocket", "Unexpected error", ex);
                    throw ex;
                }
                else
                {
                    StartCommunication();
                }
            }
        }

        

        public void StopCommunication()
        {
            try
            {
                WebSocketState[] listOfStatesToClose =
                {
                    WebSocketState.Open,
                    WebSocketState.Connecting,
                    WebSocketState.CloseSent,
                    WebSocketState.CloseReceived
                };

                if (listOfStatesToClose.Contains(_WebSocket.State))
                {
                    _WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token).GetAwaiter().GetResult();
                }

                _ctsConnection?.Cancel();
                _ctsConnection?.Dispose();
                _ctsConnection = null;
                _WebSocket?.Abort();
                _WebSocket?.Dispose();
            }
            catch
            {
                // Only to don't get double try
            }
        }
        public (bool, JsonObject) RequestMessage(string request = "", int delay = 300)
        {
            JsonObject jsonToSend = new JsonObject();
            var senderBuffer = new byte[1024 * 4];
            var receivedBuffer = new byte[1024 * 4];
            WebSocketReceiveResult result = null;
            try
            {
                Thread.Sleep(delay);

                while (_WebSocket.State == WebSocketState.Open)
                {
                   if (!string.IsNullOrEmpty(request))
                    {
                        _ctsSendRequest = (_ctsSendRequest?.IsCancellationRequested == true ? new CancellationTokenSource(TimeSpan.FromSeconds(5)) : _ctsSendRequest) ?? new CancellationTokenSource(TimeSpan.FromSeconds(5));

                        senderBuffer = Encoding.UTF8.GetBytes(request);
                        _WebSocket.SendAsync(new ArraySegment<byte>(senderBuffer), WebSocketMessageType.Text, true, _ctsSendRequest.Token).GetAwaiter().GetResult();
                        request = null;
                    }

                    _ctsReceiveRequest = (_ctsReceiveRequest?.IsCancellationRequested == true ? new CancellationTokenSource(TimeSpan.FromSeconds(5)) : _ctsReceiveRequest) ?? new CancellationTokenSource(TimeSpan.FromSeconds(5));

                    result = _WebSocket.ReceiveAsync(new ArraySegment<byte>(receivedBuffer), _ctsReceiveRequest.Token).GetAwaiter().GetResult();

                    if (result.EndOfMessage)
                    {
                        try
                        {
                            var jsonData = Encoding.UTF8.GetString(receivedBuffer, 0, result.Count);
                            jsonToSend = JsonNode.Parse(jsonData.Replace("\n", "").Trim()).AsObject();
                            break;
                        }
                        catch (System.Text.Json.JsonException)
                        {
                            jsonToSend.Add("data", "invalid_json");
                            break;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                throw ex;
                // Handle exception
            }
            catch (SocketException ex)
            {
                throw ex;
                // Handle exception
            }
            catch (AggregateException ex)
            {
                throw ex;
            }
            finally
            {
                _ctsSendRequest?.Cancel();
                _ctsSendRequest?.Dispose();
                _ctsSendRequest = null;
                _ctsReceiveRequest?.Cancel();
                _ctsReceiveRequest?.Dispose();
                _ctsReceiveRequest = null;
            }
            //if (jsonToSend.Count == 0)
            //{
            //    Pages.App.MessageBox.Show((MaterialForm) null, "Falha de conexão", "Conexão com o SimHub não disponível, verifique se você possui o JJManager Sync ativo no mesmo ou se este encontra-se em execução.");

            //}

            JsonObject actualValues = _lastValues.DeepClone().AsObject();
            _lastValuesUpdated.Clear();

            var simHubNode = jsonToSend?["data"]?["SimHubLastData"];
            if (simHubNode is JsonObject simHubObj)
            {
                foreach (var kvp in simHubObj)
                {
                    string key = kvp.Key.Replace("prop.", "").Replace("bbox.", "");
                    JsonNode value = kvp.Value;

                    bool isUpdated = !_lastValues.ContainsKey(key) ||
                                     (_lastValues.ContainsKey(key) &&
                                      _lastValues[key]?.ToJsonString() != value?.ToJsonString()) || 
                                      key == "GameRunning";

                    if (isUpdated)
                    {
                        _lastValuesUpdated.Add(key, value.DeepClone());
                        _lastValues[key] = value.DeepClone(); // update only after checking
                    }
                }
            }

            return (jsonToSend.Count > 0, jsonToSend);
        }


        public bool TranslateToButtonBoxHID(JsonObject messageReceived, out string translatedMessage, string deviceTag, int brightness_limit = 100)
        {
            JsonObject jsonResult = messageReceived;
            JsonObject jsonValues = new JsonObject();
            Form activeForm = null;

            try
            {
                if ((jsonResult.ContainsKey("status") && jsonResult["status"].ToString() == "online") &&
                    (jsonResult.ContainsKey("data") && jsonResult["data"] is JsonObject dataObject) &&
                    (dataObject.ContainsKey("SimHubLastData") && dataObject["SimHubLastData"] is JsonObject simHubLastData))
                {
                    /*if (jsonResult["status"].ToString() == "old_version")
                    {
                        MessageBox.Show("Parece que seu plugin 'JJManager Sync (Integração SimHub)' encontra-se desatualizado, recomendamos instalar a nova versão para garantir o funcionamento do sincronismo, para tal vá até a aba 'Atualizações' e instale a nova versão.");
                        acceptedOldVersionPlugin = true;
                    }*/

                    if (simHubLastData.ContainsKey("bbox.pulse_delay"))
                    {
                        jsonValues.Add("pulse_delay", (simHubLastData.ContainsKey("bbox.pulse_delay") ? simHubLastData["bbox.pulse_delay"].GetValue<int>() : 30)); // 0 to 255
                    }

                    if (simHubLastData.ContainsKey("bbox.blink_speed"))
                    {
                        jsonValues.Add("blink_speed", (simHubLastData.ContainsKey("bbox.blink_speed") ? simHubLastData["bbox.blink_speed"].GetValue<int>() : 150)); // 0 to 255
                    }

                    if (simHubLastData.ContainsKey("bbox.led_mode"))
                    {
                        jsonValues.Add("led_mode", (simHubLastData.ContainsKey("bbox.led_mode") ? simHubLastData["bbox.led_mode"].GetValue<int>() : 0)); // 0 to 255
                    }

                    if (simHubLastData.ContainsKey("bbox.brightness"))
                    {
                        jsonValues.Add("brightness", (simHubLastData.ContainsKey("bbox.brightness") ? Math.Min((simHubLastData.ContainsKey("bbox.brightness") ? simHubLastData["bbox.brightness"].GetValue<int>() : 0), brightness_limit) : 0)); // 0 to 100
                    }

                    //if (simHubLastData.ContainsKey("bbox.led_mode") && (simHubLastData["bbox.led_mode"].GetValue<int>() == 3 || simHubLastData["bbox.led_mode"].GetValue<int>() == 2))
                    //{
                    //    jsonValues.Add("led_mode", (simHubLastData.ContainsKey("bbox.led_mode") ? simHubLastData["bbox.led_mode"].GetValue<int>() : 0));
                    //    jsonValues.Add("brightness", (simHubLastData.ContainsKey("bbox.brightness") ? simHubLastData["bbox.brightness"].GetValue<int>() : 0));
                    //}
                    //else
                    //{
                    //    jsonValues.Add("led_mode", (simHubLastData.ContainsKey("bbox.led_mode") ? simHubLastData["bbox.led_mode"].GetValue<int>() : 0));
                    //    jsonValues.Add("brightness", (simHubLastData.ContainsKey("bbox.brightness") ? Math.Min((simHubLastData.ContainsKey("bbox.brightness") ? simHubLastData["bbox.brightness"].GetValue<int>() : 0), brightness_limit) : 0));
                    //}
                }
                else if (jsonResult.ContainsKey("status") && jsonResult["status"].ToString() == "offline")
                {
                    Pages.App.MessageBox.Show(activeForm as MaterialForm, "Erro de Conexão", "Não foi possível se conectar ao SimHub, verifique se você está com o plugin de sincronismo instalado e com o SimHub em execução.");
                    translatedMessage = "";
                    return false;
                }
                else if (jsonResult.ContainsKey("status") && jsonResult["status"].ToString() == "app_error")
                {
                    Pages.App.MessageBox.Show(activeForm as MaterialForm, "Erro de Sincronização", "Ocorreu um erro ao sincronizar com o SimHub, contate o suporte para avaliar o acontecimento.");
                    translatedMessage = "";
                    return false;
                }
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                jsonValues.Clear();
                jsonValues.Add("led_mode", 0);
                jsonValues.Add("brightness", 0);
            }

            translatedMessage = (new JsonObject
            {
                { deviceTag.Length > 0 ? deviceTag + "_data" : "data", jsonValues }
            }).ToJsonString();
            return true;
        }
    }
}
