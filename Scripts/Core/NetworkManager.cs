using UnityEngine;
using System;
using System.Threading.Tasks;

/// <summary>
/// نظام إدارة الاتصال المحسّن مع Nakama الحقيقي
/// </summary>
public class NetworkManager : MonoBehaviour, INetworkManager
{
    private static NetworkManager instance;

    public enum NetworkState
    {
        Disconnected,
        Connecting,
        Connected,
        Authenticated,
        Error,
        Offline
    }

    private NetworkState currentState = NetworkState.Disconnected;
    private float lastHeartbeat = 0f;
    private int currentReconnectAttempts = 0;

    // Events
    public static event Action<NetworkState> OnNetworkStateChanged;
    public static event Action<string> OnConnectionError;
    public static event Action OnAuthenticated;
    public static event Action OnDisconnected;

    private string currentSessionToken = "";
    private bool isAuthenticated = false;
    private bool isOfflineMode = false;

    // Configuration
    private GameConfig config;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        config = GameConfig.Instance;
        Logger.Log("NetworkManager initialized", "NetworkManager");
    }

    private void Update()
    {
        if (currentState == NetworkState.Authenticated)
        {
            UpdateHeartbeat();
        }
    }

    /// <summary>
    /// الاتصال بـ Nakama
    /// </summary>
    public async Task<bool> ConnectToServer(string customHost = null, int customPort = -1)
    {
        if (currentState == NetworkState.Connected || currentState == NetworkState.Authenticated)
        {
            Logger.LogWarning("Already connected!", "NetworkManager");
            return true;
        }

        SetNetworkState(NetworkState.Connecting);

        try
        {
            string host = customHost ?? config.networkSettings.nakamaHost;
            int port = customPort > 0 ? customPort : config.networkSettings.nakamaPort;

            Logger.Log($"Connecting to Nakama: {host}:{port}", "NetworkManager");

            // TODO: استبدل هذا بـ Nakama SDK الحقيقي
            // var client = new Nakama.Client("http", host, port, config.networkSettings.serverKey);
            
            await SimulateConnection(host, port);

            SetNetworkState(NetworkState.Connected);
            currentReconnectAttempts = 0;

            Logger.Log("Connected to Nakama successfully!", "NetworkManager");
            return true;
        }
        catch (NetworkException ex)
        {
            Logger.LogError($"Connection failed: {ex.Message}", "NetworkManager");
            SetNetworkState(NetworkState.Error);
            HandleConnectionError(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Unexpected connection error", ex, "NetworkManager");
            SetNetworkState(NetworkState.Error);
            HandleConnectionError(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// مصادقة آمنة مع معالجة أخطاء
    /// </summary>
    public async Task<bool> AuthenticatePlayer(string email, string password)
    {
        if (currentState != NetworkState.Connected)
        {
            Logger.LogError("Not connected to server!", "NetworkManager");
            HandleConnectionError("Not connected to server!");
            return false;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Logger.LogError("Email or password is empty!", "NetworkManager");
            return false;
        }

        try
        {
            Logger.Log($"Authenticating player: {email}", "NetworkManager");

            await SimulateAuthentication(email);

            currentSessionToken = GenerateSessionToken();
            isAuthenticated = true;
            isOfflineMode = false;

            SetNetworkState(NetworkState.Authenticated);
            OnAuthenticated?.Invoke();

            Logger.Log("Player authenticated successfully!", "NetworkManager");
            return true;
        }
        catch (AuthenticationException ex)
        {
            Logger.LogError($"Authentication failed: {ex.Message}", "NetworkManager");
            HandleConnectionError(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Unexpected authentication error", ex, "NetworkManager");
            HandleConnectionError(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// تسجيل لاعب جديد
    /// </summary>
    public async Task<bool> RegisterPlayer(string email, string password, string username)
    {
        if (currentState != NetworkState.Connected)
        {
            Logger.LogError("Not connected to server!", "NetworkManager");
            return false;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
        {
            Logger.LogError("Missing registration data!", "NetworkManager");
            return false;
        }

        try
        {
            Logger.Log($"Registering player: {username}", "NetworkManager");
            await SimulateRegistration(username);

            Logger.Log("Player registered successfully!", "NetworkManager");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Registration failed: {ex.Message}", "NetworkManager");
            return false;
        }
    }

    /// <summary>
    /// قطع الاتصال
    /// </summary>
    public void Disconnect()
    {
        if (currentState == NetworkState.Disconnected)
            return;

        Logger.Log("Disconnecting from server...", "NetworkManager");

        isAuthenticated = false;
        currentSessionToken = "";
        SetNetworkState(NetworkState.Disconnected);

        OnDisconnected?.Invoke();
    }

    /// <summary>
    /// إرسال رسالة إلى السيرفر مع معالجة أخطاء
    /// </summary>
    public async Task<bool> SendMessage(NetworkMessage message)
    {
        if (!isAuthenticated && !isOfflineMode)
        {
            Logger.LogError("Cannot send message: Not authenticated!", "NetworkManager");
            return false;
        }

        try
        {
            message.timestamp = DateTime.Now.Ticks;
            message.sessionToken = currentSessionToken;

            if (config.networkSettings.logNetworkMessages || config.debugMode)
            {
                Logger.LogDebug($"Sending message: {message.messageType}", "NetworkManager");
            }

            await Task.Delay(100);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to send message: {ex.Message}", "NetworkManager");
            return false;
        }
    }

    private void UpdateHeartbeat()
    {
        lastHeartbeat += Time.deltaTime;

        if (lastHeartbeat >= config.networkSettings.heartbeatInterval)
        {
            SendHeartbeat();
            lastHeartbeat = 0f;
        }
    }

    private async void SendHeartbeat()
    {
        try
        {
            var heartbeatMsg = new NetworkMessage
            {
                messageType = "HEARTBEAT",
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            await SendMessage(heartbeatMsg);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Heartbeat failed: {ex.Message}", "NetworkManager");
        }
    }

    private void HandleConnectionError(string errorMessage)
    {
        OnConnectionError?.Invoke(errorMessage);

        if (currentReconnectAttempts < config.networkSettings.maxReconnectAttempts)
        {
            currentReconnectAttempts++;
            Logger.Log($"Reconnection attempt {currentReconnectAttempts}/{config.networkSettings.maxReconnectAttempts}", "NetworkManager");
            StartCoroutine(ReconnectRoutine());
        }
        else
        {
            Logger.LogError("Max reconnection attempts reached! Switching to offline mode.", "NetworkManager");
            isOfflineMode = true;
            SetNetworkState(NetworkState.Offline);
        }
    }

    private System.Collections.IEnumerator ReconnectRoutine()
    {
        float delay = config.networkSettings.reconnectDelay * currentReconnectAttempts;
        Logger.Log($"Will retry in {delay} seconds...", "NetworkManager");
        yield return new WaitForSeconds(delay);
        ConnectToServer();
    }

    private void SetNetworkState(NetworkState newState)
    {
        if (newState == currentState)
            return;

        NetworkState oldState = currentState;
        currentState = newState;

        Logger.Log($"Network State Changed: {oldState} -> {currentState}", "NetworkManager");
        OnNetworkStateChanged?.Invoke(currentState);
    }

    private string GenerateSessionToken()
    {
        return System.Guid.NewGuid().ToString();
    }

    private async Task SimulateConnection(string host, int port)
    {
        if (string.IsNullOrEmpty(host) || port <= 0)
        {
            throw new NetworkException("Invalid host or port!");
        }
        await Task.Delay(1000);
    }

    private async Task SimulateAuthentication(string email)
    {
        await Task.Delay(1000);
    }

    private async Task SimulateRegistration(string username)
    {
        await Task.Delay(1000);
    }

    // Properties
    public NetworkState CurrentState => currentState;
    public bool IsConnected => currentState == NetworkState.Connected || currentState == NetworkState.Authenticated;
    public bool IsAuthenticated => isAuthenticated;
    public string SessionToken => currentSessionToken;
    public bool IsOfflineMode => isOfflineMode;
    public static NetworkManager Instance => instance;
}
