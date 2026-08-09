using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير الإنجازات - يتعامل مع الإنجازات والشارات
/// </summary>
public class AchievementManager : MonoBehaviour
{
    private static AchievementManager instance;

    private List<Achievement> allAchievements = new List<Achievement>();
    private Dictionary<string, AchievementProgress> achievementProgress = 
        new Dictionary<string, AchievementProgress>();

    // Events
    public static event Action<Achievement> OnAchievementUnlocked;
    public static event Action<Achievement> OnAchievementProgressUpdated;
    public static event Action<int> OnProgressMade;

    // Storage Keys
    private const string ACHIEVEMENTS_KEY = "AchievementsProgress";

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
    /// تهيئة مدير الإنجازات
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        CreateDefaultAchievements();
        LoadProgressLocal();

        isInitialized = true;

        Debug.Log("AchievementManager Initialized");
    }

    /// <summary>
    /// إنشاء الإنجازات الافتراضية
    /// </summary>
    private void CreateDefaultAchievements()
    {
        allAchievements = new List<Achievement>
        {
            // إنجازات البداية
            new Achievement
            {
                AchievementId = "first_blood",
                Name = "First Blood",
                Description = "احصل على أول قتل",
                Icon = "first_blood",
                Points = 10,
                IsHidden = false,
                Category = "Combat"
            },

            // إنجازات القتل
            new Achievement
            {
                AchievementId = "kill_10",
                Name = "Killer",
                Description = "احصل على 10 قتل",
                Icon = "killer",
                Points = 25,
                IsHidden = false,
                Category = "Combat",
                TargetValue = 10
            },

            new Achievement
            {
                AchievementId = "kill_100",
                Name = "Serial Killer",
                Description = "احصل على 100 قتل",
                Icon = "serial_killer",
                Points = 50,
                IsHidden = false,
                Category = "Combat",
                TargetValue = 100
            },

            // إنجازات الترتيب
            new Achievement
            {
                AchievementId = "level_10",
                Name = "Novice",
                Description = "وصول للمستوى 10",
                Icon = "novice",
                Points = 15,
                IsHidden = false,
                Category = "Progression",
                TargetValue = 10
            },

            new Achievement
            {
                AchievementId = "level_50",
                Name = "Master",
                Description = "وصول للمستوى 50 (الحد الأقصى)",
                Icon = "master",
                Points = 100,
                IsHidden = false,
                Category = "Progression",
                TargetValue = 50
            },

            // إنجازات الأصدقاء
            new Achievement
            {
                AchievementId = "friend_5",
                Name = "Social Butterfly",
                Description = "أضف 5 أصدقاء",
                Icon = "social_butterfly",
                Points = 20,
                IsHidden = false,
                Category = "Social",
                TargetValue = 5
            },

            // إنجازات الدردشة
            new Achievement
            {
                AchievementId = "messages_50",
                Name = "Chatterbox",
                Description = "أرسل 50 رسالة",
                Icon = "chatterbox",
                Points = 15,
                IsHidden = false,
                Category = "Social",
                TargetValue = 50
            },

            // إنجازات الفوز
            new Achievement
            {
                AchievementId = "wins_10",
                Name = "Winner",
                Description = "فز 10 مرات",
                Icon = "winner",
                Points = 30,
                IsHidden = false,
                Category = "Combat",
                TargetValue = 10
            },

            // إنجازات مخفية
            new Achievement
            {
                AchievementId = "secret_master",
                Name = "Secret Master",
                Description = "???",
                Icon = "secret",
                Points = 50,
                IsHidden = true,
                Category = "Secret"
            }
        };

        // إنشاء تقدم لكل إنجاز
        foreach (var achievement in allAchievements)
        {
            achievementProgress[achievement.AchievementId] = new AchievementProgress
            {
                AchievementId = achievement.AchievementId,
                IsUnlocked = false,
                Progress = 0,
                TargetValue = achievement.TargetValue,
                UnlockedAt = DateTime.MinValue
            };
        }

        Debug.Log($"Created {allAchievements.Count} achievements");
    }

    /// <summary>
    /// الحصول على جميع الإنجازات
    /// </summary>
    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(allAchievements);
    }

    /// <summary>
    /// الحصول على الإنجازات المفتوحة فقط
    /// </summary>
    public List<Achievement> GetUnlockedAchievements()
    {
        return allAchievements
            .Where(a => achievementProgress[a.AchievementId].IsUnlocked)
            .ToList();
    }

    /// <summary>
    /// الحصول على الإنجازات المتبقية
    /// </summary>
    public List<Achievement> GetLockedAchievements()
    {
        return allAchievements
            .Where(a => !achievementProgress[a.AchievementId].IsUnlocked)
            .ToList();
    }

    /// <summary>
    /// تحديث تقدم الإنجاز
    /// </summary>
    public void UpdateAchievementProgress(string achievementId, int progressAmount)
    {
        if (!achievementProgress.ContainsKey(achievementId))
        {
            Debug.LogWarning($"Achievement not found: {achievementId}");
            return;
        }

        var progress = achievementProgress[achievementId];

        if (progress.IsUnlocked)
            return; // تم فتحه بالفعل

        progress.Progress += progressAmount;

        OnAchievementProgressUpdated?.Invoke(
            allAchievements.First(a => a.AchievementId == achievementId)
        );

        // التحقق من الفتح
        CheckAchievementUnlock(achievementId);

        SaveProgressLocal();
    }

    /// <summary>
    /// التحقق من فتح الإنجاز
    /// </summary>
    private void CheckAchievementUnlock(string achievementId)
    {
        var achievement = allAchievements.FirstOrDefault(a => a.AchievementId == achievementId);
        if (achievement == null)
            return;

        var progress = achievementProgress[achievementId];

        if (progress.Progress >= achievement.TargetValue)
        {
            UnlockAchievement(achievementId);
        }
    }

    /// <summary>
    /// فتح إنجاز
    /// </summary>
    public async Task<bool> UnlockAchievement(string achievementId)
    {
        if (!achievementProgress.ContainsKey(achievementId))
            return false;

        var progress = achievementProgress[achievementId];

        if (progress.IsUnlocked)
            return false; // تم فتحه بالفعل

        try
        {
            Debug.Log($"Unlocking achievement: {achievementId}");

            await Task.Delay(300);

            progress.IsUnlocked = true;
            progress.UnlockedAt = DateTime.Now;

            var achievement = allAchievements.First(a => a.AchievementId == achievementId);

            // إضافة نقاط
            PlayerManager.Instance.AddXP(achievement.Points * 10);

            OnAchievementUnlocked?.Invoke(achievement);
            OnProgressMade?.Invoke(achievement.Points);

            SaveProgressLocal();

            Debug.Log($"Achievement unlocked: {achievement.Name} (+{achievement.Points} points)");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to unlock achievement: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// الحصول على تقدم إنجاز محدد
    /// </summary>
    public AchievementProgress GetAchievementProgress(string achievementId)
    {
        if (achievementProgress.ContainsKey(achievementId))
            return achievementProgress[achievementId];

        return null;
    }

    /// <summary>
    /// الحصول على إحصائيات الإنجازات
    /// </summary>
    public AchievementStats GetAchievementStats()
    {
        var unlockedCount = achievementProgress.Count(p => p.Value.IsUnlocked);
        var totalPoints = allAchievements
            .Where(a => achievementProgress[a.AchievementId].IsUnlocked)
            .Sum(a => a.Points);

        return new AchievementStats
        {
            TotalAchievements = allAchievements.Count,
            UnlockedAchievements = unlockedCount,
            TotalPoints = totalPoints,
            CompletionPercentage = (float)unlockedCount / allAchievements.Count * 100
        };
    }

    /// <summary>
    /// حفظ التقدم محلياً
    /// </summary>
    private void SaveProgressLocal()
    {
        try
        {
            var progressList = achievementProgress.Values.ToList();
            string json = JsonUtility.ToJson(new AchievementProgressList { progress = progressList });
            PlayerPrefs.SetString(ACHIEVEMENTS_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save achievements: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل التقدم محلياً
    /// </summary>
    private void LoadProgressLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(ACHIEVEMENTS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var progressList = JsonUtility.FromJson<AchievementProgressList>(json);
                foreach (var prog in progressList.progress)
                {
                    if (achievementProgress.ContainsKey(prog.AchievementId))
                    {
                        achievementProgress[prog.AchievementId] = prog;
                    }
                }

                Debug.Log("Achievement progress loaded");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load achievements: {ex.Message}");
        }
    }

    // Getters
    public static AchievementManager Instance => instance;
}

// ==================== Data Classes ====================

[System.Serializable]
public class Achievement
{
    public string AchievementId;
    public string Name;
    public string Description;
    public string Icon;
    public int Points;
    public bool IsHidden;
    public string Category;
    public int TargetValue = 1;
}

[System.Serializable]
public class AchievementProgress
{
    public string AchievementId;
    public bool IsUnlocked;
    public int Progress;
    public int TargetValue;
    public DateTime UnlockedAt;
}

[System.Serializable]
public class AchievementStats
{
    public int TotalAchievements;
    public int UnlockedAchievements;
    public int TotalPoints;
    public float CompletionPercentage;
}

[System.Serializable]
public class AchievementProgressList
{
    public List<AchievementProgress> progress = new List<AchievementProgress>();
}
