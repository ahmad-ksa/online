using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// تكامل Steam - يتعامل مع ميزات Steam للعبة
/// </summary>
public class SteamIntegration : MonoBehaviour
{
    private static SteamIntegration instance;

    private string steamAppId = "0";
    private string steamUserId = "";
    private bool isSteamInitialized = false;

    // Steam Features
    private List<SteamAchievement> steamAchievements = new List<SteamAchievement>();
    private List<SteamStatistic> steamStatistics = new List<SteamStatistic>();
    private Dictionary<string, int> userStats = new Dictionary<string, int>();

    // Events
    public static event Action OnSteamInitialized;
    public static event Action<string> OnAchievementUnlocked;
    public static event Action<string> OnStatisticUpdated;

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
    /// تهيئة Steam
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        InitializeSteam();
        CreateSteamAchievements();
        CreateSteamStatistics();

        isInitialized = true;

        Debug.Log("SteamIntegration Initialized");
    }

    /// <summary>
    /// تهيئة Steam API
    /// </summary>
    private void InitializeSteam()
    {
        try
        {
            // في بيئة حقيقية، هنا تتصل بـ Steamworks SDK
            // مثلاً: Steamworks.SteamClient.Init(480);

            isSteamInitialized = true;
            steamAppId = "480"; // Default AppID

            Debug.Log("Steam initialized successfully");
            OnSteamInitialized?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Steam: {ex.Message}");
            isSteamInitialized = false;
        }
    }

    /// <summary>
    /// إنشاء إنجازات Steam
    /// </summary>
    private void CreateSteamAchievements()
    {
        steamAchievements = new List<SteamAchievement>
        {
            new SteamAchievement
            {
                AchievementId = "ACH_FIRST_KILL",
                Name = "First Blood",
                Description = "احصل على أول قتل",
                IconPath = "images/achievements/first_blood.png",
                IsUnlocked = false
            },

            new SteamAchievement
            {
                AchievementId = "ACH_LEVEL_10",
                Name = "Novice",
                Description = "وصول للمستوى 10",
                IconPath = "images/achievements/novice.png",
                IsUnlocked = false
            },

            new SteamAchievement
            {
                AchievementId = "ACH_LEVEL_50",
                Name = "Master",
                Description = "وصول للمستوى 50",
                IconPath = "images/achievements/master.png",
                IsUnlocked = false
            },

            new SteamAchievement
            {
                AchievementId = "ACH_WINSTREAK_10",
                Name = "Unstoppable",
                Description = "فز 10 مرات متتالية",
                IconPath = "images/achievements/unstoppable.png",
                IsUnlocked = false
            },

            new SteamAchievement
            {
                AchievementId = "ACH_100_KILLS",
                Name = "Serial Killer",
                Description = "احصل على 100 قتل",
                IconPath = "images/achievements/serial_killer.png",
                IsUnlocked = false
            }
        };

        Debug.Log($"Created {steamAchievements.Count} Steam achievements");
    }

    /// <summary>
    /// إنشاء إحصائيات Steam
    /// </summary>
    private void CreateSteamStatistics()
    {
        steamStatistics = new List<SteamStatistic>
        {
            new SteamStatistic { StatId = "total_kills", Name = "Total Kills", Value = 0 },
            new SteamStatistic { StatId = "total_deaths", Name = "Total Deaths", Value = 0 },
            new SteamStatistic { StatId = "total_wins", Name = "Total Wins", Value = 0 },
            new SteamStatistic { StatId = "total_losses", Name = "Total Losses", Value = 0 },
            new SteamStatistic { StatId = "max_level", Name = "Max Level", Value = 1 },
            new SteamStatistic { StatId = "playtime_hours", Name = "Playtime (Hours)", Value = 0 },
            new SteamStatistic { StatId = "total_spending", Name = "Total Spending ($)", Value = 0 }
        };

        Debug.Log($"Created {steamStatistics.Count} Steam statistics");
    }

    /// <summary>
    /// فتح إنجاز
    /// </summary>
    public async Task<bool> UnlockAchievement(string achievementId)
    {
        if (!isSteamInitialized)
        {
            Debug.LogWarning("Steam not initialized!");
            return false;
        }

        var achievement = steamAchievements.FirstOrDefault(a => a.AchievementId == achievementId);

        if (achievement == null)
        {
            Debug.LogWarning($"Achievement not found: {achievementId}");
            return false;
        }

        if (achievement.IsUnlocked)
        {
            Debug.LogWarning("Achievement already unlocked!");
            return false;
        }

        try
        {
            Debug.Log($"Unlocking Steam achievement: {achievementId}");

            await Task.Delay(300);

            // في بيئة حقيقية:
            // Steamworks.SteamUserStats.SetAchievement(achievementId);
            // Steamworks.SteamUserStats.StoreStats();

            achievement.IsUnlocked = true;
            achievement.UnlockedAt = DateTime.Now;

            OnAchievementUnlocked?.Invoke(achievementId);

            Debug.Log($"Achievement unlocked: {achievement.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to unlock achievement: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تحديث إحصائية
    /// </summary>
    public async Task<bool> UpdateStatistic(string statId, int value)
    {
        if (!isSteamInitialized)
        {
            Debug.LogWarning("Steam not initialized!");
            return false;
        }

        var stat = steamStatistics.FirstOrDefault(s => s.StatId == statId);

        if (stat == null)
        {
            Debug.LogWarning($"Statistic not found: {statId}");
            return false;
        }

        try
        {
            Debug.Log($"Updating Steam stat: {statId} = {value}");

            await Task.Delay(200);

            // في بيئة حقيقية:
            // Steamworks.SteamUserStats.SetStat(statId, value);
            // Steamworks.SteamUserStats.StoreStats();

            stat.Value = value;
            userStats[statId] = value;

            OnStatisticUpdated?.Invoke(statId);

            Debug.Log($"Statistic updated: {stat.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update statistic: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// الحصول على الإنجازات
    /// </summary>
    public List<SteamAchievement> GetAchievements()
    {
        return new List<SteamAchievement>(steamAchievements);
    }

    /// <summary>
    /// الحصول على الإنجازات المفتوحة
    /// </summary>
    public List<SteamAchievement> GetUnlockedAchievements()
    {
        return steamAchievements.Where(a => a.IsUnlocked).ToList();
    }

    /// <summary>
    /// الحصول على نسبة الإنجازات
    /// </summary>
    public float GetAchievementPercentage()
    {
        if (steamAchievements.Count == 0)
            return 0;

        float unlocked = steamAchievements.Count(a => a.IsUnlocked);
        return (unlocked / steamAchievements.Count) * 100f;
    }

    /// <summary>
    /// الحصول على الإحصائيات
    /// </summary>
    public List<SteamStatistic> GetStatistics()
    {
        return new List<SteamStatistic>(steamStatistics);
    }

    /// <summary>
    /// الحصول على إحصائية محددة
    /// </summary>
    public int GetStatistic(string statId)
    {
        var stat = steamStatistics.FirstOrDefault(s => s.StatId == statId);
        return stat?.Value ?? 0;
    }

    /// <summary>
    /// فتح صفحة المتجر في Steam
    /// </summary>
    public void OpenSteamStore()
    {
        if (!isSteamInitialized)
        {
            Debug.LogWarning("Steam not initialized!");
            return;
        }

        try
        {
            // في بيئة حقيقية:
            // Steamworks.SteamFriends.ActivateGameOverlayToStore(480);

            Debug.Log("Opening Steam store overlay");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open store: {ex.Message}");
        }
    }

    /// <summary>
    /// فتح قائمة الأصدقاء في Steam
    /// </summary>
    public void OpenSteamFriends()
    {
        if (!isSteamInitialized)
        {
            Debug.LogWarning("Steam not initialized!");
            return;
        }

        try
        {
            // في بيئة حقيقية:
            // Steamworks.SteamFriends.ActivateGameOverlay("friends");

            Debug.Log("Opening Steam friends overlay");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open friends: {ex.Message}");
        }
    }

    // Getters
    public static SteamIntegration Instance => instance;
    public bool IsSteamInitialized => isSteamInitialized;
    public string SteamAppId => steamAppId;
}

// ==================== Data Classes ====================

[System.Serializable]
public class SteamAchievement
{
    public string AchievementId;
    public string Name;
    public string Description;
    public string IconPath;
    public bool IsUnlocked;
    public DateTime UnlockedAt;
}

[System.Serializable]
public class SteamStatistic
{
    public string StatId;
    public string Name;
    public int Value;
}
