using UnityEngine;

/// <summary>
/// ملف إعدادات اللعبة الآمن - ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config")]
public class GameConfig : ScriptableObject
{
    [System.Serializable]
    public class NetworkSettings
    {
        [Header("Nakama Server")]
        public string nakamaHost = "play.nakama.dev";
        public int nakamaPort = 7349;
        public string serverKey = ""; // تُملأ في وقت التشغيل
        public bool useSSL = true;

        [Header("Connection")]
        public int connectionTimeout = 10;
        public int maxReconnectAttempts = 5;
        public float reconnectDelay = 5f;
        public float heartbeatInterval = 30f;
    }

    [System.Serializable]
    public class GameSettings
    {
        public string gameTitle = "My Awesome Game";
        public string gameVersion = "1.0.0";
        public int maxPlayers = 100;
        public bool enableCrossPlatform = true;
        public bool enableCloudSave = true;
        public bool enableAnalytics = true;
        public bool enableAntiCheat = true;
    }

    [System.Serializable]
    public class GraphicsSettings
    {
        public int defaultQualityLevel = 3;
        public int maxFPS = 60;
        public bool vSync = true;
        public int defaultWidth = 1920;
        public int defaultHeight = 1080;
    }

    [System.Serializable]
    public class PlatformSettings
    {
        [Header("Steam")]
        public bool steamEnabled = false;
        public string steamAppID = "";

        [Header("Mobile")]
        public bool googlePlayEnabled = false;
        public bool appleStoreEnabled = false;

        [Header("Web")]
        public bool webEnabled = false;
    }

    [SerializeField]
    public NetworkSettings networkSettings = new NetworkSettings();

    [SerializeField]
    public GameSettings gameSettings = new GameSettings();

    [SerializeField]
    public GraphicsSettings graphicsSettings = new GraphicsSettings();

    [SerializeField]
    public PlatformSettings platformSettings = new PlatformSettings();

    [Header("Debug")]
    public bool debugMode = false;
    public bool enableDebugConsole = true;
    public bool logNetworkMessages = false;

    private static GameConfig instance;

    /// <summary>
    /// الحصول على instance
    /// </summary>
    public static GameConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameConfig>("GameConfig");
                if (instance == null)
                {
                    Logger.LogWarning("GameConfig not found in Resources/GameConfig.asset", "GameConfig");
                    instance = CreateInstance<GameConfig>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// التحقق من صحة الإعدادات
    /// </summary>
    public bool ValidateSettings()
    {
        if (string.IsNullOrEmpty(networkSettings.nakamaHost))
        {
            Logger.LogError("Nakama host is empty!", "GameConfig");
            return false;
        }

        if (networkSettings.nakamaPort <= 0 || networkSettings.nakamaPort > 65535)
        {
            Logger.LogError("Invalid Nakama port!", "GameConfig");
            return false;
        }

        if (string.IsNullOrEmpty(gameSettings.gameTitle))
        {
            Logger.LogWarning("Game title is empty", "GameConfig");
        }

        return true;
    }

    /// <summary>
    /// حفظ الإعدادات في EditorPrefs (للتطوير فقط)
    /// </summary>
    public void SaveToPrefs()
    {
        #if UNITY_EDITOR
        PlayerPrefs.SetString("Nakama_Host", networkSettings.nakamaHost);
        PlayerPrefs.SetInt("Nakama_Port", networkSettings.nakamaPort);
        PlayerPrefs.SetInt("DebugMode", debugMode ? 1 : 0);
        PlayerPrefs.Save();
        Logger.Log("Settings saved to PlayerPrefs", "GameConfig");
        #endif
    }

    /// <summary>
    /// تحميل الإعدادات من EditorPrefs
    /// </summary>
    public void LoadFromPrefs()
    {
        #if UNITY_EDITOR
        networkSettings.nakamaHost = PlayerPrefs.GetString("Nakama_Host", networkSettings.nakamaHost);
        networkSettings.nakamaPort = PlayerPrefs.GetInt("Nakama_Port", networkSettings.nakamaPort);
        debugMode = PlayerPrefs.GetInt("DebugMode", 0) == 1;
        Logger.Log("Settings loaded from PlayerPrefs", "GameConfig");
        #endif
    }
}
