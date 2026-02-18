using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;

namespace JJManager.Core.Connections.SimHub;

/// <summary>
/// WebSocket client for connecting to SimHub's JJManager Sync plugin
/// Cross-platform implementation
/// </summary>
public class SimHubWebsocket : IDisposable
{
    #region Fields

    private ClientWebSocket? _webSocket;
    private readonly string _ipAddress = "localhost";
    private readonly string _deviceName;
    private readonly int _port = 2920;
    private JsonObject _lastValues = new();
    private JsonObject _lastValuesUpdated = new();
    private CancellationTokenSource? _ctsConnection;
    private bool _isDisposed;

    #endregion

    #region Properties

    public bool IsConnected => _webSocket?.State == WebSocketState.Open || false;
    public JsonObject LastValues => _lastValues;
    public JsonObject LastValuesUpdated => _lastValuesUpdated;

    #endregion

    #region Constructor

    public SimHubWebsocket(int port, string deviceName)
    {
        _port = port;
        _deviceName = string.IsNullOrEmpty(deviceName) ? "JJMANAGER_CONNECTION" : deviceName;
        InitializeWebSocket();
    }

    private void InitializeWebSocket()
    {
        _webSocket = new ClientWebSocket();
        _webSocket.Options.SetRequestHeader("device", _deviceName);
    }

    #endregion

    #region Connection Methods

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_webSocket?.State == WebSocketState.Open)
                return true;

            if (_webSocket?.State != WebSocketState.None)
            {
                // Need to recreate the WebSocket
                _webSocket?.Dispose();
                InitializeWebSocket();
            }
            
            _ctsConnection = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var uri = new Uri($"ws://{_ipAddress}:{_port}/jjmanager/?device={_deviceName}");

            using var timeoutCts = new CancellationTokenSource(300);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _ctsConnection.Token, timeoutCts.Token);

            await _webSocket!.ConnectAsync(uri, linkedCts.Token);

            return _webSocket.State == WebSocketState.Open;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            HashSet<WebSocketState> statesWithoutClose = new HashSet<WebSocketState>
            {
                WebSocketState.Aborted,
                WebSocketState.Closed,
                WebSocketState.CloseSent,
                WebSocketState.CloseReceived
            };

            if (_webSocket != null)
            {
                if (statesWithoutClose.Contains(_webSocket.State))
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await _webSocket.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        cts.Token);
                }
            }

            _ctsConnection?.Cancel();
            _webSocket?.Dispose();
            _webSocket = null;
        }
        catch
        {
            // Ignore close errors
        }
    }

    #endregion

    #region Request Methods

    /// <summary>
    /// Request SimHub data and update LastValues
    /// </summary>
    public async Task<(bool Success, JsonObject Data)> RequestDataAsync(CancellationToken cancellationToken = default)
    {
        if (_webSocket?.State != WebSocketState.Open)
            return (false, new JsonObject());

        try
        {
            // Build request
            var request = new JsonObject
            {
                { "request", new JsonArray { "SimHubLastData" } }
            };

            // Send request
            var sendBuffer = Encoding.UTF8.GetBytes(request.ToJsonString());
            using var sendCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedSendCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, sendCts.Token);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(sendBuffer),
                WebSocketMessageType.Text,
                true,
                linkedSendCts.Token);

            // Receive response
            var receiveBuffer = new byte[1024 * 8];
            using var receiveCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedReceiveCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, receiveCts.Token);

            var result = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(receiveBuffer),
                linkedReceiveCts.Token);

            if (!result.EndOfMessage)
                return (false, new JsonObject());

            // Parse response
            var jsonData = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
            var jsonResponse = JsonNode.Parse(jsonData.Replace("\n", "").Trim())?.AsObject();

            if (jsonResponse == null)
                return (false, new JsonObject());

            // Update LastValues and LastValuesUpdated
            UpdateLastValues(jsonResponse);

            return (true, jsonResponse);
        }
        catch (Exception)
        {
            return (false, new JsonObject());
        }
    }

    private void UpdateLastValues(JsonObject jsonResponse)
    {
        _lastValuesUpdated = new JsonObject();

        var simHubNode = jsonResponse["data"]?["SimHubLastData"];
        if (simHubNode is not JsonObject simHubObj)
            return;

        foreach (var kvp in simHubObj)
        {
            string key = kvp.Key.Replace("prop.", "").Replace("bbox.", "");
            JsonNode? value = kvp.Value;

            bool isUpdated = !_lastValues.ContainsKey(key) ||
                            _lastValues.ContainsKey(key) &&
                             _lastValues[key]?.ToJsonString() != value?.ToJsonString() ||
                             key == "GameRunning";

            if (isUpdated)
            {
                _lastValuesUpdated[key] = value?.DeepClone();
                _lastValues[key] = value?.DeepClone();
            }
        }
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _ctsConnection?.Cancel();
                _ctsConnection?.Dispose();
                _webSocket?.Dispose();
            }
            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
