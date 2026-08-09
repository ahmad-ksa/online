using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// عميل Nakama - اتصال مع الخادم الحقيقي
/// </summary>
public class NakamaClient : MonoBehaviour
{
    private static NakamaClient instance;

    [SerializeField] private NakamaConfig nakamaConfig;
    private bool isConnected = false;
    private bool isAuthenticated = false;
    private string currentPlayerId = "";
    private string currentSessionToken = "";

    // محاكاة الاتصال (TODO: استبدل بـ Nakama SDK الحقيقي)
    private class FakeNakamaSession
    {
        public string playerId;
        public string token;
        public string username;
    }

    private FakeNakamaSession currentSession;

    // Events
    public event System.Action OnConnected;
    public event System.Action OnDisconnected;
    public event System.Action<string> OnConnectionError;
    public event System.Action<string> OnAuthenticationSuccess;
    public event System.Action<string> OnAuthenticationError;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (nakamaConfig == null)
        {
            nakamaConfig = Resources.Load<NakamaConfig>("NakamaConfig");
            if (nakamaConfig == null)
            {
                Logger.LogError("NakamaConfig not found in Resources!", "NakamaClient");
                return;
            }
        }

        // تحقق من الإعدادات
        if (!nakamaConfig.ValidateSettings())
        {
            Logger.LogError("Nakama config validation failed!", "NakamaClient");
            return;
        }

        if (nakamaConfig.DebugMode)
        {
            nakamaConfig.PrintSettings();
        }

        Logger.Log($"NakamaClient initialized - Connecting to {nakamaConfig.GetServerUrl()}", "NakamaClient");
    }

    /// <summary>
    /// الاتصال بـ Nakama Server
    /// </summary>
    public async Task<bool> Connect()
    {
        try
        {
            Logger.Log($"Attempting to connect to {nakamaConfig.GetServerUrl()}...", "NakamaClient");

            int retries = 0;
            while (retries < nakamaConfig.MaxRetries)
            {
                try
                {
                    // محاكاة الاتصال
                    await Task.Delay(500);

                    // TODO: استبدل بـ Nakama SDK الحقيقي
                    // var client = new Client("http", nakamaConfig.ServerHost, nakamaConfig.ServerPort);
                    // var socket = client.NewSocket();
                    // await socket.ConnectAsync(session);

                    isConnected = true;
                    OnConnected?.Invoke();
                    Logger.Log("✓ Connected to Nakama Server!", "NakamaClient");
                    return true;
                }
                catch (System.Exception ex)
                {
                    retries++;
                    Logger.LogWarning($"Connection attempt {retries} failed: {ex.Message}", "NakamaClient");

                    if (retries < nakamaConfig.MaxRetries)
                    {
                        await Task.Delay((int)(nakamaConfig.RetryDelay * 1000));
                    }
                }
            }

            isConnected = false;
            OnConnectionError?.Invoke("Failed to connect after " + nakamaConfig.MaxRetries + " attempts");
            Logger.LogError("Failed to connect to Nakama Server!", "NakamaClient");
            return false;
        }
        catch (System.Exception ex)
        {
            Logger.LogCritical("Connection error", ex, "NakamaClient");
            OnConnectionError?.Invoke(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// تسجيل الدخول
    /// </summary>
    public async Task<bool> Login(string email, string password)
    {
        if (!isConnected)
        {
            Logger.LogError("Not connected to server!", "NakamaClient");
            OnAuthenticationError?.Invoke("Not connected to server");
            return false;
        }

        try
        {
            Logger.Log($"Logging in as {email}...", "NakamaClient");

            // محاكاة تسجيل الدخول
            await Task.Delay(1000);

            // TODO: استبدل بـ Nakama SDK الحقيقي
            // var session = await client.AuthenticateEmailAsync(email, password);

            currentSession = new FakeNakamaSession
            {
                playerId = System.Guid.NewGuid().ToString(),
                token = System.Guid.NewGuid().ToString(),
                username = email.Split('@')[0]
            };

            isAuthenticated = true;
            currentPlayerId = currentSession.playerId;
            currentSessionToken = currentSession.token;

            OnAuthenticationSuccess?.Invoke(currentSession.username);
            Logger.Log($"✓ Authentication successful! Player ID: {currentPlayerId}", "NakamaClient");
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Login failed: {ex.Message}", "NakamaClient");
            OnAuthenticationError?.Invoke(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// تسجيل لاعب جديد
    /// </summary>
    public async Task<bool> Register(string email, string password, string username)
    {
        if (!isConnected)
        {
            Logger.LogError("Not connected to server!", "NakamaClient");
            OnAuthenticationError?.Invoke("Not connected to server");
            return false;
        }

        try
        {
            Logger.Log($"Registering new player: {username}...", "NakamaClient");

            // محاكاة التسجيل
            await Task.Delay(1000);

            // TODO: استبدل بـ Nakama SDK الحقيقي
            // var account = await client.AuthenticateEmailAsync(email, password, username, create: true);

            currentSession = new FakeNakamaSession
            {
                playerId = System.Guid.NewGuid().ToString(),
                token = System.Guid.NewGuid().ToString(),
                username = username
            };

            isAuthenticated = true;
            currentPlayerId = currentSession.playerId;
            currentSessionToken = currentSession.token;

            OnAuthenticationSuccess?.Invoke(username);
            Logger.Log($"✓ Registration successful! Player ID: {currentPlayerId}", "NakamaClient");
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Registration failed: {ex.Message}", "NakamaClient");
            OnAuthenticationError?.Invoke(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// إرسال رسالة إلى السيرفر
    /// </summary>
    public async Task<bool> SendMessage(NetworkMessage message)
    {
        if (!isAuthenticated)
        {
            Logger.LogError("Not authenticated!", "NakamaClient");
            return false;
        }

        try
        {
            if (nakamaConfig.LogNetworkMessages)
            {
                Logger.LogDebug($"Sending message: {message.messageType}", "NakamaClient");
            }

            // محاكاة الإرسال
            await Task.Delay(100);

            // TODO: استبدل بـ Nakama SDK الحقيقي
            // await socket.SendMatchStateAsync(matchId, opCode, JsonUtility.ToJson(message.data));

            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to send message: {ex.Message}", "NakamaClient");
            return false;
        }
    }

    /// <summary>
    /// الحصول على قائمة اللاعبين النشطين
    /// </summary>
    public async Task<List<string>> GetActivePlayers()
    {
        try
        {
            // محاكاة الحصول على اللاعبين
            await Task.Delay(200);

            // TODO: استبدل بـ Nakama SDK الحقيقي
            return new List<string> { currentPlayerId };
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to get active players: {ex.Message}", "NakamaClient");
            return new List<string>();
        }
    }

    /// <summary>
    /// قطع الاتصال
    /// </summary>
    public void Disconnect()
    {
        isConnected = false;
        isAuthenticated = false;
        currentPlayerId = "";
        currentSessionToken = "";
        OnDisconnected?.Invoke();
        Logger.Log("Disconnected from Nakama Server", "NakamaClient");
    }

    // Getters
    public bool IsConnected => isConnected;
    public bool IsAuthenticated => isAuthenticated;
    public string CurrentPlayerId => currentPlayerId;
    public string ServerUrl => nakamaConfig.GetServerUrl();
    public NakamaConfig Config => nakamaConfig;
    public static NakamaClient Instance => instance;
}
