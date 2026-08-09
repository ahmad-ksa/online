using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// مدير الشبكة - يتعامل مع الاتصال بـ Nakama والتواصل مع السيرفر
/// </summary>
public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;

    // Network States
    public enum NetworkState
    {
        Disconnected,
        Connecting,
        Connected,
        Authenticated,
        Error
    }

    private NetworkState currentState = NetworkState.Disconnected;
    private float lastHeartbeat = 0f;
    private float heartbeatInterval = 30f; // كل 30 ثانية

    // Events
    public static event Action<NetworkState> OnNetworkStateChanged;
    public static event Action<string> OnConnectionError;
    public static event Action OnAuthenticated;
    public static event Action OnDisconnected;

    // Nakama Configuration
    [SerializeField] private string nakamaHost = "localhost";
    [SerializeField] private int nakamaPort = 7349;
    [SerializeField] private string serverKey = "defaultkey";
    [SerializeField] private bool useSSL = false;

    // Connection Settings
    [SerializeField] private int connectionTimeout = 10;
    [SerializeField] private int maxReconnectAttempts = 5;
    private int currentReconnectAttempts = 0;

    // Nakama Session
    private string currentSessionToken = "";
    private bool isAuthenticated = false;

    // Message Queue
    private Queue<NetworkMessage> messageQueue = new Queue<NetworkMessage>();
    private bool isProcessingMessages = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // معالجة الرسائل المتبقية
        ProcessMessageQueue();

        // Heartbeat
        UpdateHeartbeat();
    }

    /// <summary>
    /// تهيئة NetworkManager
    /// </summary>
    public static void Initialize()
    {
        if (instance != null)
        {
            Debug.Log("NetworkManager already initialized");
            return;
        }

        GameObject networkObject = new GameObject("NetworkManager");
        instance = networkObject.AddComponent<NetworkManager>();
        DontDestroyOnLoad(networkObject);

        Debug.Log("NetworkManager Initialized");
    }

    /// <summary>
    /// الاتصال بـ Nakama
    /// </summary>
    public async Task<bool> ConnectToServer(string customHost = null, int customPort = -1)
    {
        if (currentState == NetworkState.Connected || currentState == NetworkState.Authenticated)
        {
            Debug.LogWarning("Already connected!");
            return true;
        }

        SetNetworkState(NetworkState.Connecting);

        try
        {
            // استخدام الإعدادات المخصصة إذا تم توفيرها
            string host = customHost ?? nakamaHost;
            int port = customPort > 0 ? customPort : nakamaPort;

            Debug.Log($"Connecting to Nakama: {host}:{port}");

            // محاكاة الاتصال (سيتم استبداله بـ Nakama SDK الفعلي)
            await Task.Delay(1000);

            SetNetworkState(NetworkState.Connected);
            currentReconnectAttempts = 0;

            Debug.Log("Connected to Nakama successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection failed: {ex.Message}");
            SetNetworkState(NetworkState.Error);
            HandleConnectionError(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// مصادقة اللاعب
    /// </summary>
    public async Task<bool> AuthenticatePlayer(string email, string password)
    {
        if (currentState != NetworkState.Connected)
        {
            HandleConnectionError("Not connected to server!");
            return false;
        }

        try
        {
            Debug.Log($"Authenticating player: {email}");

            // محاكاة المصادقة (سيتم استبداله بـ Nakama SDK الفعلي)
            await Task.Delay(1000);

            // في الواقع، سيتم الحصول على session token من Nakama
            currentSessionToken = GenerateSessionToken();
            isAuthenticated = true;

            SetNetworkState(NetworkState.Authenticated);
            OnAuthenticated?.Invoke();

            Debug.Log("Player authenticated successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Authentication failed: {ex.Message}");
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
            HandleConnectionError("Not connected to server!");
            return false;
        }

        try
        {
            Debug.Log($"Registering player: {username}");

            // محاكاة التسجيل (سيتم استبداله بـ Nakama SDK الفعلي)
            await Task.Delay(1000);

            Debug.Log("Player registered successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Registration failed: {ex.Message}");
            HandleConnectionError(ex.Message);
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

        Debug.Log("Disconnecting from server...");

        isAuthenticated = false;
        currentSessionToken = "";
        SetNetworkState(NetworkState.Disconnected);

        OnDisconnected?.Invoke();
    }

    /// <summary>
    /// إرسال رسالة إلى السيرفر
    /// </summary>
    public async Task<bool> SendMessage(NetworkMessage message)
    {
        if (!isAuthenticated)
        {
            Debug.LogError("Cannot send message: Not authenticated!");
            return false;
        }

        try
        {
            message.timestamp = DateTime.Now.Ticks;
            message.sessionToken = currentSessionToken;

            Debug.Log($"Sending message: {message.messageType}");

            // إضافة للطابور
            messageQueue.Enqueue(message);

            // إرسال فوري (محاكاة)
            await Task.Delay(100);

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send message: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// معالجة طابور الرسائل
    /// </summary>
    private void ProcessMessageQueue()
    {
        if (messageQueue.Count == 0 || isProcessingMessages)
            return;

        isProcessingMessages = true;

        try
        {
            while (messageQueue.Count > 0)
            {
                var message = messageQueue.Dequeue();
                ProcessMessage(message);
            }
        }
        finally
        {
            isProcessingMessages = false;
        }
    }

    /// <summary>
    /// معالجة رسالة واحدة
    /// </summary>
    private void ProcessMessage(NetworkMessage message)
    {
        Debug.Log($"Processing message: {message.messageType}");
        // سيتم تنفيذ معالجة محددة حسب نوع الرسالة
    }

    /// <summary>
    /// تحديث Heartbeat
    /// </summary>
    private void UpdateHeartbeat()
    {
        if (currentState != NetworkState.Authenticated)
            return;

        lastHeartbeat += Time.deltaTime;

        if (lastHeartbeat >= heartbeatInterval)
        {
            SendHeartbeat();
            lastHeartbeat = 0f;
        }
    }

    /// <summary>
    /// إرسال Heartbeat للسيرفر
    /// </summary>
    private async void SendHeartbeat()
    {
        try
        {
            var heartbeatMsg = new NetworkMessage
            {
                messageType = "HEARTBEAT",
                data = new Dictionary<string, object>()
            };

            await SendMessage(heartbeatMsg);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Heartbeat failed: {ex.Message}");
        }
    }

    /// <summary>
    /// معالجة أخطاء الاتصال
    /// </summary>
    private void HandleConnectionError(string errorMessage)
    {
        OnConnectionError?.Invoke(errorMessage);

        // محاولة إعادة الاتصال
        if (currentReconnectAttempts < maxReconnectAttempts)
        {
            currentReconnectAttempts++;
            Debug.Log($"Reconnection attempt {currentReconnectAttempts}/{maxReconnectAttempts}");
            StartCoroutine(ReconnectRoutine());
        }
        else
        {
            Debug.LogError("Max reconnection attempts reached!");
            Disconnect();
        }
    }

    /// <summary>
    /// روتين إعادة الاتصال
    /// </summary>
    private System.Collections.IEnumerator ReconnectRoutine()
    {
        yield return new WaitForSeconds(5f); // انتظر 5 ثوان قبل المحاولة
        ConnectToServer();
    }

    /// <summary>
    /// تغيير حالة الشبكة
    /// </summary>
    private void SetNetworkState(NetworkState newState)
    {
        if (newState == currentState)
            return;

        NetworkState oldState = currentState;
        currentState = newState;

        Debug.Log($"Network State Changed: {oldState} -> {currentState}");
        OnNetworkStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// توليد Session Token (محاكاة)
    /// </summary>
    private string GenerateSessionToken()
    {
        return System.Guid.NewGuid().ToString();
    }

    // Getters
    public static NetworkManager Instance => instance;
    public NetworkState CurrentState => currentState;
    public bool IsConnected => currentState == NetworkState.Connected;
    public bool IsAuthenticated => isAuthenticated;
    public string SessionToken => currentSessionToken;
    public int MessageQueueCount => messageQueue.Count;
}

/// <summary>
/// رسالة الشبكة
/// </summary>
[System.Serializable]
public class NetworkMessage
{
    public string messageType;
    public Dictionary<string, object> data;
    public long timestamp;
    public string sessionToken;
    public string messageId = System.Guid.NewGuid().ToString();
}
