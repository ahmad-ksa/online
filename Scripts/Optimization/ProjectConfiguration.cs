using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// إعدادات المشروع - ملف التكوين الشامل
/// </summary>
public class ProjectConfiguration : MonoBehaviour
{
    private static ProjectConfiguration instance;

    [System.Serializable]
    public class GameSettings
    {
        public string GameTitle = "Online Multiplayer Game";
        public string GameVersion = "1.0.0";
        public int MaxPlayers = 100;
        public bool EnableCrossPlatform = true;
        public bool EnableCloudSave = true;
        public bool EnableAnalytics = true;
        public bool EnableAntiCheat = true;
    }

    [System.Serializable]
    public class NetworkSettings
    {
        public string NakamaServerUrl = "localhost:7349";
        public string ServerKey = "your_server_key";
        public int ReconnectAttempts = 5;
        public float ReconnectDelay = 2f;
        public int HeartbeatInterval = 30;
    }

    [System.Serializable]
    public class PlatformSettings
    {
        public bool SteamEnabled = true;
        public string SteamAppId = "480";
        public bool GooglePlayEnabled = true;
        public bool AppleStoreEnabled = true;
        public bool WebEnabled = false;
    }

    [System.Serializable]
    public class GraphicsSettings
    {
        public int DefaultQualityLevel = 3;
        public int MaxFPS = 60;
        public bool VSync = false;
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
    }

    public GameSettings gameSettings = new GameSettings();
    public NetworkSettings networkSettings = new NetworkSettings();
    public PlatformSettings platformSettings = new PlatformSettings();
    public GraphicsSettings graphicsSettings = new GraphicsSettings();

    private bool isInitialized = false;

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

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// تهيئة التكوين
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        ApplySettings();
        LogConfiguration();

        isInitialized = true;

        Debug.Log("ProjectConfiguration Initialized");
    }

    /// <summary>
    /// تطبيق الإعدادات
    /// </summary>
    private void ApplySettings()
    {
        // تطبيق إعدادات الرسومات
        QualitySettings.SetQualityLevel(graphicsSettings.DefaultQualityLevel);
        Application.targetFrameRate = graphicsSettings.MaxFPS;
        QualitySettings.vSyncCount = graphicsSettings.VSync ? 1 : 0;

        // تطبيق الدقة
        Screen.SetResolution(graphicsSettings.ResolutionWidth, graphicsSettings.ResolutionHeight, false);

        Debug.Log("Settings applied successfully");
    }

    /// <summary>
    /// تسجيل التكوين
    /// </summary>
    private void LogConfiguration()
    {
        Debug.Log("=== PROJECT CONFIGURATION ===");
        Debug.Log($"Game: {gameSettings.GameTitle} v{gameSettings.GameVersion}");
        Debug.Log($"Max Players: {gameSettings.MaxPlayers}");
        Debug.Log($"Cross-Platform: {gameSettings.EnableCrossPlatform}");
        Debug.Log($"Cloud Save: {gameSettings.EnableCloudSave}");
        Debug.Log($"Analytics: {gameSettings.EnableAnalytics}");
        Debug.Log($"Anti-Cheat: {gameSettings.EnableAntiCheat}");
        Debug.Log($"Nakama Server: {networkSettings.NakamaServerUrl}");
        Debug.Log($"Steam: {platformSettings.SteamEnabled}");
        Debug.Log($"Quality Level: {graphicsSettings.DefaultQualityLevel}");
        Debug.Log($"Max FPS: {graphicsSettings.MaxFPS}");
        Debug.Log("=============================");
    }

    /// <summary>
    /// حفظ التكوين
    /// </summary>
    public void SaveConfiguration()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("ProjectConfig", json);
        PlayerPrefs.Save();

        Debug.Log("Configuration saved");
    }

    /// <summary>
    /// تحميل التكوين
    /// </summary>
    public void LoadConfiguration()
    {
        string json = PlayerPrefs.GetString("ProjectConfig", "");
        if (!string.IsNullOrEmpty(json))
        {
            JsonUtility.FromJsonOverwrite(json, this);
            Debug.Log("Configuration loaded");
        }
    }

    // Getters
    public static ProjectConfiguration Instance => instance;

    public string GetGameTitle() => gameSettings.GameTitle;
    public string GetGameVersion() => gameSettings.GameVersion;
    public string GetNakamaServerUrl() => networkSettings.NakamaServerUrl;
    public bool IsCrossPlatformEnabled() => gameSettings.EnableCrossPlatform;
    public bool IsCloudSaveEnabled() => gameSettings.EnableCloudSave;
}
