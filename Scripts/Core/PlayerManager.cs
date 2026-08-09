using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// مدير اللاعب - يتعامل مع بيانات اللاعب والملف الشخصي
/// </summary>
public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;

    // Player Data
    private PlayerProfile currentPlayerProfile;
    private PlayerStats currentPlayerStats;
    private PlayerSettings playerSettings;

    // Events
    public static event Action<PlayerProfile> OnPlayerProfileLoaded;
    public static event Action<PlayerStats> OnPlayerStatsUpdated;
    public static event Action<string> OnPlayerDataSaved;

    // Storage
    private const string PLAYER_PROFILE_KEY = "PlayerProfile";
    private const string PLAYER_STATS_KEY = "PlayerStats";
    private const string PLAYER_SETTINGS_KEY = "PlayerSettings";

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
    /// تهيئة PlayerManager
    /// </summary>
    public static void Initialize()
    {
        if (instance != null)
        {
            Debug.Log("PlayerManager already initialized");
            return;
        }

        GameObject playerObject = new GameObject("PlayerManager");
        instance = playerObject.AddComponent<PlayerManager>();
        DontDestroyOnLoad(playerObject);

        instance.InitializeLocal();
        Debug.Log("PlayerManager Initialized");
    }

    private void InitializeLocal()
    {
        if (isInitialized)
            return;

        // تحميل البيانات المحلية
        LoadPlayerDataLocal();

        // إنشاء ملف شخصي جديد إذا لم يكن موجوداً
        if (currentPlayerProfile == null)
        {
            currentPlayerProfile = new PlayerProfile();
            Debug.Log("New player profile created");
        }

        if (currentPlayerStats == null)
        {
            currentPlayerStats = new PlayerStats();
            Debug.Log("New player stats created");
        }

        if (playerSettings == null)
        {
            playerSettings = new PlayerSettings();
            Debug.Log("New player settings created");
        }

        isInitialized = true;
    }

    /// <summary>
    /// تحميل بيانات اللاعب المحلية
    /// </summary>
    private void LoadPlayerDataLocal()
    {
        try
        {
            // تحميل الملف الشخصي
            string profileJson = PlayerPrefs.GetString(PLAYER_PROFILE_KEY, "");
            if (!string.IsNullOrEmpty(profileJson))
            {
                currentPlayerProfile = JsonUtility.FromJson<PlayerProfile>(profileJson);
            }

            // تحميل الإحصائيات
            string statsJson = PlayerPrefs.GetString(PLAYER_STATS_KEY, "");
            if (!string.IsNullOrEmpty(statsJson))
            {
                currentPlayerStats = JsonUtility.FromJson<PlayerStats>(statsJson);
            }

            // تحميل الإعدادات
            string settingsJson = PlayerPrefs.GetString(PLAYER_SETTINGS_KEY, "");
            if (!string.IsNullOrEmpty(settingsJson))
            {
                playerSettings = JsonUtility.FromJson<PlayerSettings>(settingsJson);
            }

            Debug.Log("Player data loaded from local storage");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load player data: {ex.Message}");
        }
    }

    /// <summary>
    /// حفظ بيانات اللاعب محلياً
    /// </summary>
    public void SavePlayerDataLocal()
    {
        try
        {
            string profileJson = JsonUtility.ToJson(currentPlayerProfile);
            PlayerPrefs.SetString(PLAYER_PROFILE_KEY, profileJson);

            string statsJson = JsonUtility.ToJson(currentPlayerStats);
            PlayerPrefs.SetString(PLAYER_STATS_KEY, statsJson);

            string settingsJson = JsonUtility.ToJson(playerSettings);
            PlayerPrefs.SetString(PLAYER_SETTINGS_KEY, settingsJson);

            PlayerPrefs.Save();

            Debug.Log("Player data saved to local storage");
            OnPlayerDataSaved?.Invoke("LOCAL_SAVE");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save player data: {ex.Message}");
        }
    }

    /// <summary>
    /// تحديث الملف الشخصي
    /// </summary>
    public void UpdatePlayerProfile(PlayerProfile newProfile)
    {
        currentPlayerProfile = newProfile;
        OnPlayerProfileLoaded?.Invoke(currentPlayerProfile);
        SavePlayerDataLocal();

        Debug.Log($"Player profile updated: {currentPlayerProfile.playerName}");
    }

    /// <summary>
    /// تحديث الإحصائيات
    /// </summary>
    public void UpdatePlayerStats(PlayerStats newStats)
    {
        currentPlayerStats = newStats;
        OnPlayerStatsUpdated?.Invoke(currentPlayerStats);
        SavePlayerDataLocal();

        Debug.Log("Player stats updated");
    }

    /// <summary>
    /// إضافة XP
    /// </summary>
    public void AddXP(int amount)
    {
        currentPlayerStats.totalXP += amount;
        
        // تحديث المستوى
        int newLevel = (currentPlayerStats.totalXP / 1000) + 1;
        if (newLevel > currentPlayerStats.currentLevel)
        {
            currentPlayerStats.currentLevel = newLevel;
            Debug.Log($"Level up! New level: {newLevel}");
        }

        OnPlayerStatsUpdated?.Invoke(currentPlayerStats);
        SavePlayerDataLocal();
    }

    /// <summary>
    /// إضافة عملات
    /// </summary>
    public void AddCurrency(int amount, CurrencyType currencyType)
    {
        if (currencyType == CurrencyType.Coins)
        {
            currentPlayerStats.coins += amount;
        }
        else if (currencyType == CurrencyType.Gems)
        {
            currentPlayerStats.gems += amount;
        }

        SavePlayerDataLocal();
    }

    /// <summary>
    /// استنزاف عملات
    /// </summary>
    public bool SpendCurrency(int amount, CurrencyType currencyType)
    {
        if (currencyType == CurrencyType.Coins)
        {
            if (currentPlayerStats.coins >= amount)
            {
                currentPlayerStats.coins -= amount;
                SavePlayerDataLocal();
                return true;
            }
        }
        else if (currencyType == CurrencyType.Gems)
        {
            if (currentPlayerStats.gems >= amount)
            {
                currentPlayerStats.gems -= amount;
                SavePlayerDataLocal();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// فتح إنجاز
    /// </summary>
    public void UnlockAchievement(string achievementId)
    {
        if (!currentPlayerStats.UnlockedAchievements.Contains(achievementId))
        {
            currentPlayerStats.UnlockedAchievements.Add(achievementId);
            SavePlayerDataLocal();

            Debug.Log($"Achievement unlocked: {achievementId}");
        }
    }

    /// <summary>
    /// فتح شخصية
    /// </summary>
    public void UnlockCharacter(string characterId)
    {
        if (!currentPlayerStats.UnlockedCharacters.Contains(characterId))
        {
            currentPlayerStats.UnlockedCharacters.Add(characterId);
            SavePlayerDataLocal();

            Debug.Log($"Character unlocked: {characterId}");
        }
    }

    /// <summary>
    /// تعيين الشخصية المختارة
    /// </summary>
    public void SetSelectedCharacter(string characterId)
    {
        if (currentPlayerStats.UnlockedCharacters.Contains(characterId))
        {
            currentPlayerStats.selectedCharacterId = characterId;
            SavePlayerDataLocal();

            Debug.Log($"Character selected: {characterId}");
        }
    }

    // Getters
    public static PlayerManager Instance => instance;
    public PlayerProfile CurrentProfile => currentPlayerProfile;
    public PlayerStats CurrentStats => currentPlayerStats;
    public PlayerSettings Settings => playerSettings;
    public bool IsInitialized => isInitialized;
}

// ==================== Data Classes ====================

[System.Serializable]
public class PlayerProfile
{
    public string playerId = System.Guid.NewGuid().ToString();
    public string playerName = "Player";
    public string email = "";
    public string profilePictureUrl = "";
    public string bio = "";
    public long createdAt = System.DateTime.Now.Ticks;
    public long lastLoginAt = System.DateTime.Now.Ticks;
    public bool isVerified = false;
}

[System.Serializable]
public class PlayerStats
{
    public int currentLevel = 1;
    public int totalXP = 0;
    public int coins = 0;
    public int gems = 0;

    public int totalMatches = 0;
    public int totalWins = 0;
    public int totalLosses = 0;

    public float winRatio => totalMatches > 0 ? (float)totalWins / totalMatches : 0f;

    public string selectedCharacterId = "default";
    
    [SerializeField]
    private List<string> unlockedCharacters = new List<string> { "default" };
    
    [SerializeField]
    private List<string> unlockedAchievements = new List<string>();

    public List<string> UnlockedCharacters => unlockedCharacters;
    public List<string> UnlockedAchievements => unlockedAchievements;
}

[System.Serializable]
public class PlayerSettings
{
    [SerializeField]
    public float masterVolume = 0.8f;
    [SerializeField]
    public float musicVolume = 0.6f;
    [SerializeField]
    public float sfxVolume = 0.8f;
    [SerializeField]
    public float voiceVolume = 0.8f;

    [SerializeField]
    public int graphicsQuality = 2; // 0: Low, 1: Medium, 2: High
    [SerializeField]
    public float brightness = 1f;
    [SerializeField]
    public int fps = 60;

    [SerializeField]
    public string language = "en";
    [SerializeField]
    public bool subtitles = true;
    [SerializeField]
    public bool hapticFeedback = true;

    [SerializeField]
    public bool allowFriendRequests = true;
    [SerializeField]
    public bool allowMessages = true;
    [SerializeField]
    public bool profilePublic = true;
}

public enum CurrencyType
{
    Coins,
    Gems
}
