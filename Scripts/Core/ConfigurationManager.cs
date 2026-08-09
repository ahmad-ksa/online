using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// مدير الإعدادات - يتعامل مع جميع إعدادات اللعبة والسيرفر
/// </summary>
public class ConfigurationManager : MonoBehaviour
{
    private static ConfigurationManager instance;
    private GameConfiguration gameConfig;

    // Events
    public static event Action<GameConfiguration> OnConfigurationLoaded;
    public static event Action<string, object> OnSettingChanged;

    // Custom Settings
    private Dictionary<string, object> customSettings = new Dictionary<string, object>();

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

    /// <summary>
    /// تهيئة ConfigurationManager
    /// </summary>
    public static void Initialize(GameConfiguration config)
    {
        if (instance != null)
        {
            Debug.Log("ConfigurationManager already initialized");
            return;
        }

        GameObject configObject = new GameObject("ConfigurationManager");
        var manager = configObject.AddComponent<ConfigurationManager>();
        manager.InitializeConfig(config);
        DontDestroyOnLoad(configObject);

        Debug.Log("ConfigurationManager Initialized");
    }

    private void InitializeConfig(GameConfiguration config)
    {
        if (isInitialized)
            return;

        gameConfig = config;

        // تحميل الإعدادات المخصصة
        LoadCustomSettings();

        OnConfigurationLoaded?.Invoke(gameConfig);
        isInitialized = true;
    }

    /// <summary>
    /// تحميل الإعدادات المخصصة
    /// </summary>
    private void LoadCustomSettings()
    {
        // تحميل من PlayerPrefs أو ملفات
        Debug.Log("Loading custom settings...");
    }

    /// <summary>
    /// تعيين إعداد
    /// </summary>
    public void SetSetting(string key, object value)
    {
        if (customSettings.ContainsKey(key))
        {
            customSettings[key] = value;
        }
        else
        {
            customSettings.Add(key, value);
        }

        OnSettingChanged?.Invoke(key, value);
        SaveSettings();

        Debug.Log($"Setting changed: {key} = {value}");
    }

    /// <summary>
    /// الحصول على إعداد
    /// </summary>
    public object GetSetting(string key, object defaultValue = null)
    {
        if (customSettings.ContainsKey(key))
        {
            return customSettings[key];
        }

        return defaultValue;
    }

    /// <summary>
    /// الحصول على إعداد (Generic)
    /// </summary>
    public T GetSetting<T>(string key, T defaultValue = default)
    {
        if (customSettings.ContainsKey(key))
        {
            try
            {
                return (T)customSettings[key];
            }
            catch
            {
                return defaultValue;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// حفظ الإعدادات
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            // يمكن حفظ في PlayerPrefs أو ملف JSON
            Debug.Log("Settings saved");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save settings: {ex.Message}");
        }
    }

    // Getters
    public static ConfigurationManager Instance => instance;
    public GameConfiguration GameConfig => gameConfig;
    public bool IsInitialized => isInitialized;
}

/// <summary>
/// إعدادات اللعبة الأساسية
/// </summary>
[CreateAssetMenu(fileName = "GameConfiguration", menuName = "Game/Configuration")]
public class GameConfiguration : ScriptableObject
{
    [Header("Basic Info")]
    public string GameTitle = "My Online Game";
    public string GameVersion = "1.0.0";
    public string GameDescription = "An awesome multiplayer game";

    [Header("Server Configuration")]
    public string NakamaServerHost = "localhost";
    public int NakamaServerPort = 7349;
    public int NakamaHTTPPort = 7350;
    public string NakamaServerKey = "defaultkey";
    public bool UseSSL = false;

    [Header("Game Rules")]
    public int MaxPlayersPerGame = 10;
    public int MinPlayersToStart = 2;
    public float GameDuration = 600f; // 10 دقائق
    public bool RequireAuthentication = true;

    [Header("Gameplay Settings")]
    [SerializeField]
    private List<string> sceneNames = new List<string>();
    [SerializeField]
    private List<string> characterIds = new List<string>();
    [SerializeField]
    private List<GameMode> gameModes = new List<GameMode>();

    [Header("Network Settings")]
    public int ConnectionTimeout = 10;
    public int MaxReconnectAttempts = 5;
    public float HeartbeatInterval = 30f;
    public int MessageBufferSize = 1000;

    [Header("Economy")]
    public int StartingCoins = 100;
    public int StartingGems = 0;
    public float CoinMultiplier = 1f;
    public float XPMultiplier = 1f;

    [Header("Features")]
    public bool EnableVoiceChat = true;
    public bool EnableCrossPlay = true;
    public bool EnableProgressiveWeb = false;
    public bool EnableAnalytics = true;
    public bool EnableAntiCheat = true;

    [Header("Platform Settings")]
    public bool SupportMobile = true;
    public bool SupportDesktop = true;
    public bool SupportSteam = true;

    // Getters
    public List<string> SceneNames => sceneNames;
    public List<string> CharacterIds => characterIds;
    public List<GameMode> GameModes => gameModes;

    /// <summary>
    /// إضافة Scene
    /// </summary>
    public void AddScene(string sceneName)
    {
        if (!sceneNames.Contains(sceneName))
        {
            sceneNames.Add(sceneName);
            Debug.Log($"Scene added: {sceneName}");
        }
    }

    /// <summary>
    /// إضافة شخصية
    /// </summary>
    public void AddCharacter(string characterId)
    {
        if (!characterIds.Contains(characterId))
        {
            characterIds.Add(characterId);
            Debug.Log($"Character added: {characterId}");
        }
    }

    /// <summary>
    /// إضافة نمط لعب
    /// </summary>
    public void AddGameMode(GameMode mode)
    {
        if (!gameModes.Contains(mode))
        {
            gameModes.Add(mode);
            Debug.Log($"Game mode added: {mode.ModeName}");
        }
    }

    /// <summary>
    /// الحصول على نمط لعب
    /// </summary>
    public GameMode GetGameMode(string modeId)
    {
        foreach (var mode in gameModes)
        {
            if (mode.ModeId == modeId)
                return mode;
        }

        return null;
    }

    /// <summary>
    /// الحصول على شخصية
    /// </summary>
    public bool IsCharacterUnlocked(string characterId)
    {
        return characterIds.Contains(characterId);
    }
}

/// <summary>
/// أنماط اللعب
/// </summary>
[System.Serializable]
public class GameMode
{
    public string ModeId = "mode_1";
    public string ModeName = "Deathmatch";
    public string ModeDescription = "Eliminate all opponents!";
    public int MaxPlayers = 10;
    public int MinPlayers = 2;
    public float Duration = 600f;
    [SerializeField]
    private List<string> allowedMaps = new List<string>();
    public List<string> AllowedMaps => allowedMaps;

    public GameMode()
    {
        ModeId = System.Guid.NewGuid().ToString().Substring(0, 8);
    }

    /// <summary>
    /// إضافة خريطة
    /// </summary>
    public void AddMap(string mapName)
    {
        if (!allowedMaps.Contains(mapName))
        {
            allowedMaps.Add(mapName);
        }
    }

    /// <summary>
    /// إزالة خريطة
    /// </summary>
    public void RemoveMap(string mapName)
    {
        if (allowedMaps.Contains(mapName))
        {
            allowedMaps.Remove(mapName);
        }
    }
}

/// <summary>
/// إعدادات الشخصية
/// </summary>
[System.Serializable]
public class CharacterConfiguration
{
    public string CharacterId = "character_1";
    public string CharacterName = "Knight";
    public string CharacterDescription = "A strong warrior";
    [SerializeField]
    private List<string> availableSkins = new List<string>();
    public float Health = 100f;
    public float Speed = 5f;
    public float AttackDamage = 10f;

    public List<string> AvailableSkins => availableSkins;

    /// <summary>
    /// إضافة skin
    /// </summary>
    public void AddSkin(string skinId)
    {
        if (!availableSkins.Contains(skinId))
        {
            availableSkins.Add(skinId);
        }
    }
}

/// <summary>
/// إعدادات الخريطة
/// </summary>
[System.Serializable]
public class MapConfiguration
{
    public string MapId = "map_1";
    public string MapName = "Forest";
    public string SceneName = "ForestScene";
    public string Description = "A dense forest map";
    public int MaxPlayers = 10;
}
