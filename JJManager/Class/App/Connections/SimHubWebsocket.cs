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

namespace JJManager.Class.App
{
    public class SimHubWebsocket
    {
        private ClientWebSocket _WebSocket = null;
        private string _IpAddress = "localhost";
        private string _DeviceName = "";
        private int _Port = 2920;
        public bool IsConnected
        {
            get => _WebSocket.State == WebSocketState.Open;
        }
        public SimHubWebsocket(int port, string deviceName)
        {
            _Port = port;
            _DeviceName = (deviceName.Length == 0 ? "JJMANAGER_CONNECTION" : deviceName);
            _WebSocket = new ClientWebSocket();
            _WebSocket.Options.SetRequestHeader("device", _DeviceName);
        }

        public void StartCommunication()
        {
            try
            {
                if (_WebSocket != null && _WebSocket.State == WebSocketState.None)
                {
                    //_WebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(1);
                    _WebSocket.ConnectAsync(new Uri($"ws://{_IpAddress}:{_Port}/jjmanager/?device={_DeviceName}"), CancellationToken.None).Wait(3000);
                }
            }
            catch (WebSocketException ex)
            {


                Log.Insert("Websocket", "Connection error", ex);
            }
            catch (OperationCanceledException)
            {

            }
            catch (AggregateException ex)
            {
                Log.Insert("Websocket", "Connection error", ex);

            }
            catch (Exception ex)
            {
                Log.Insert("Websocket", "Unexpected error", ex);
            }
        }

        

        public void StopCommunication()
        {
            if (_WebSocket.State == WebSocketState.Open || _WebSocket.State == WebSocketState.CloseSent || _WebSocket.State == WebSocketState.CloseReceived)
            {
                Task.Run(async () =>
                {
                    await _WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }).Wait();

                _WebSocket.Dispose();
            }
        }
        public (bool, JsonObject) RequestMessage(string request = "")
        {
            JsonObject jsonToSend = new JsonObject();
            var senderBuffer = new byte[1024 * 4];
            var receivedBuffer = new byte[1024 * 4];
            WebSocketReceiveResult result = null;
            try
            {
                while (_WebSocket.State == WebSocketState.Open)
                {
                   if (!string.IsNullOrEmpty(request))
                    {
                        senderBuffer = Encoding.UTF8.GetBytes(request);
                        _WebSocket.SendAsync(new ArraySegment<byte>(senderBuffer), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                        request = null;
                    }
                   
                    result = _WebSocket.ReceiveAsync(new ArraySegment<byte>(receivedBuffer), CancellationToken.None).Result;

                    if (result.EndOfMessage)
                    {
                        try
                        {
                            var jsonData = Encoding.UTF8.GetString(receivedBuffer, 0, result.Count);
                            jsonToSend = JsonNode.Parse(jsonData).AsObject();
                            break;
                        }
                        catch (System.Text.Json.JsonException)
                        {
                            jsonToSend.Add("data", "invalid_json");
                            break;
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            catch (WebSocketException)
            {
                // Handle exception
            }

            if (jsonToSend.Count == 0)
            {
                MessageBox.Show("Conexão com o SimHub não disponível, verifique se você possui o JJManager Sync ativo no mesmo ou se este encontra-se em execução.", "Falha de Conexão");
            }

            return (jsonToSend.Count > 0, jsonToSend);
        }


        public bool TranslateToButtonBoxHID(JsonObject messageReceived, out string translatedMessage, ref bool acceptedOldVersionPlugin, int brightness_limit = 100)
        {
            JsonObject jsonResult = messageReceived;
            JsonObject jsonValues = new JsonObject();
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
                    if (simHubLastData.ContainsKey("led_mode") && simHubLastData["led_mode"].GetValue<int>() == 3)
                    {
                        jsonValues.Add("led_mode", (simHubLastData.ContainsKey("led_mode") ? simHubLastData["led_mode"].GetValue<int>() : 0));
                        jsonValues.Add("brightness", (simHubLastData.ContainsKey("brightness") ? simHubLastData["brightness"].GetValue<int>() : 0));
                    }
                    else
                    {
                        jsonValues.Add("led_mode", (simHubLastData.ContainsKey("led_mode") ? simHubLastData["led_mode"].GetValue<int>() : 0));
                        jsonValues.Add("brightness", (simHubLastData.ContainsKey("brightness") ? Math.Min((simHubLastData.ContainsKey("brightness") ? simHubLastData["brightness"].GetValue<int>() : 0), brightness_limit) : 0));
                    }
                }
                else if (jsonResult.ContainsKey("status") && jsonResult["status"].ToString() == "offline")
                {
                    MessageBox.Show("Não foi possível se conectar ao SimHub, verifique se você está com o plugin de sincronismo instalado e com o SimHub em execução.");
                    translatedMessage = "";
                    return false;
                }
                else if (jsonResult.ContainsKey("status") && jsonResult["status"].ToString() == "app_error")
                {
                    MessageBox.Show("Ocorreu um erro ao sincronizar com o SimHub, contate o suporte para avaliar o acontecimento.");
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
                { "data", jsonValues }
            }).ToJsonString();
            return true;
        }

        public Version GetPluginVersion()
        {
            Version version = null;

            Task.Run(async () =>
            {
                StartCommunication();
                var (success, result) =  RequestMessage("{\"request\": [\"PluginVersion\"]}");

                JsonObject jsonResult = result;

                if (success && jsonResult != null &&
                    jsonResult.ContainsKey("status") && jsonResult["status"].ToString() == "online" &&
                    (jsonResult.ContainsKey("data") && jsonResult["data"] is JsonObject dataObject) &&
                    (dataObject.ContainsKey("PluginVersion")))
                {
                    version = new Version(jsonResult["plugin_version"].ToString());
                }

                StopCommunication();
            }).Wait();

            return version;
        }

    }
}
