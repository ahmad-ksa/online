using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// إعدادات الاتصال بـ Nakama Server - قابل للتعديل من Inspector
/// </summary>
[CreateAssetMenu(fileName = "NakamaConfig", menuName = "Game/Nakama Config")]
public class NakamaConfig : ScriptableObject
{
    [Header("Server Settings")]
    [SerializeField] private string serverHost = "45.76.130.15";
    [SerializeField] private int serverPort = 7349;
    [SerializeField] private bool useSSL = false;
    [SerializeField] private string serverKey = "defaultkey";

    [Header("Connection Settings")]
    [SerializeField] private float connectionTimeout = 30f;
    [SerializeField] private int maxRetries = 5;
    [SerializeField] private float retryDelay = 2f;

    [Header("Game Settings")]
    [SerializeField] private string gameName = "OnlineGame";
    [SerializeField] private int maxPlayersPerMatch = 100;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool logNetworkMessages = true;

    // Properties
    public string ServerHost => serverHost;
    public int ServerPort => serverPort;
    public bool UseSSL => useSSL;
    public string ServerKey => serverKey;
    public float ConnectionTimeout => connectionTimeout;
    public int MaxRetries => maxRetries;
    public float RetryDelay => retryDelay;
    public string GameName => gameName;
    public int MaxPlayersPerMatch => maxPlayersPerMatch;
    public bool DebugMode => debugMode;
    public bool LogNetworkMessages => logNetworkMessages;

    /// <summary>
    /// الحصول على URL الخادم الكامل
    /// </summary>
    public string GetServerUrl()
    {
        string protocol = useSSL ? "https" : "http";
        return $"{protocol}://{serverHost}:{serverPort}";
    }

    /// <summary>
    /// التحقق من صحة الإعدادات
    /// </summary>
    public bool ValidateSettings()
    {
        if (string.IsNullOrEmpty(serverHost))
        {
            Logger.LogError("Server host is empty!", "NakamaConfig");
            return false;
        }

        if (serverPort <= 0 || serverPort > 65535)
        {
            Logger.LogError("Server port is invalid!", "NakamaConfig");
            return false;
        }

        if (connectionTimeout <= 0)
        {
            Logger.LogError("Connection timeout must be positive!", "NakamaConfig");
            return false;
        }

        if (maxRetries < 0)
        {
            Logger.LogError("Max retries cannot be negative!", "NakamaConfig");
            return false;
        }

        return true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// طباعة الإعدادات في Console (للـ Debug)
    /// </summary>
    public void PrintSettings()
    {
        if (!debugMode)
            return;

        Debug.Log($"\n==== Nakama Configuration ====");
        Debug.Log($"Server URL: {GetServerUrl()}");
        Debug.Log($"Max Retries: {maxRetries}");
        Debug.Log($"Retry Delay: {retryDelay}s");
        Debug.Log($"Connection Timeout: {connectionTimeout}s");
        Debug.Log($"Game Name: {gameName}");
        Debug.Log($"Max Players: {maxPlayersPerMatch}");
        Debug.Log($"===========================\n");
    }
#endif
}
