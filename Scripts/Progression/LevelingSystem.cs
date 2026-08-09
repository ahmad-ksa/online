using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// نظام المستويات - يتعامل مع XP والترقيات والمستويات
/// </summary>
public class LevelingSystem : MonoBehaviour
{
    private static LevelingSystem instance;

    // Level System
    private Dictionary<int, LevelConfig> levelConfigs = new Dictionary<int, LevelConfig>();
    private int currentLevel = 1;
    private int currentXP = 0;
    private int totalXP = 0;

    // Level Rewards
    private List<LevelReward> levelRewards = new List<LevelReward>();

    // Events
    public static event Action<int, int> OnXPGained;
    public static event Action<int> OnLevelUp;
    public static event Action<LevelReward> OnLevelRewardReceived;
    public static event Action OnPrestigeReady;

    // Storage Keys
    private const string LEVEL_DATA_KEY = "LevelData";
    private const string XP_DATA_KEY = "XPData";

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
    /// تهيئة نظام المستويات
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        CreateLevelConfigs();
        LoadLevelDataLocal();

        isInitialized = true;

        Debug.Log("LevelingSystem Initialized");
    }

    /// <summary>
    /// إنشاء إعدادات المستويات
    /// </summary>
    private void CreateLevelConfigs()
    {
        // المستويات 1-50
        for (int i = 1; i <= 50; i++)
        {
            int xpRequired = i * 1000; // كل مستوى يحتاج 1000 XP أكثر من السابق

            levelConfigs[i] = new LevelConfig
            {
                Level = i,
                XPRequired = xpRequired,
                RewardCoins = i * 100,
                RewardGems = i * 10,
                RewardPrestige = 1
            };

            // إضافة جوائز خاصة كل 5 مستويات
            if (i % 5 == 0)
            {
                levelRewards.Add(new LevelReward
                {
                    Level = i,
                    RewardType = "special_item",
                    RewardName = $"Milestone_{i}",
                    RewardValue = 1
                });
            }
        }

        Debug.Log($"Created {levelConfigs.Count} level configs");
    }

    /// <summary>
    /// إضافة XP
    /// </summary>
    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        currentXP += amount;
        totalXP += amount;

        OnXPGained?.Invoke(amount, currentXP);

        // التحقق من الترقية
        CheckLevelUp();

        SaveLevelDataLocal();
    }

    /// <summary>
    /// التحقق من الترقية
    /// </summary>
    private void CheckLevelUp()
    {
        if (!levelConfigs.ContainsKey(currentLevel + 1))
            return; // لا توجد مستويات أخرى

        int xpNeeded = levelConfigs[currentLevel + 1].XPRequired;

        while (currentXP >= xpNeeded && currentLevel < 50)
        {
            currentLevel++;
            currentXP -= xpNeeded;

            // تطبيق جوائز المستوى
            ApplyLevelRewards(currentLevel);

            OnLevelUp?.Invoke(currentLevel);

            Debug.Log($"Level Up! New level: {currentLevel}");

            if (levelConfigs.ContainsKey(currentLevel + 1))
            {
                xpNeeded = levelConfigs[currentLevel + 1].XPRequired;
            }
        }
    }

    /// <summary>
    /// تطبيق جوائز المستوى
    /// </summary>
    private void ApplyLevelRewards(int level)
    {
        if (!levelConfigs.ContainsKey(level))
            return;

        var config = levelConfigs[level];

        // إضافة العملات
        PlayerManager.Instance.AddCurrency(config.RewardCoins, CurrencyType.Coins);
        PlayerManager.Instance.AddCurrency(config.RewardGems, CurrencyType.Gems);

        Debug.Log($"Level {level} rewards: +{config.RewardCoins} coins, +{config.RewardGems} gems");

        // التحقق من الجوائز الخاصة
        var specialReward = levelRewards.Find(r => r.Level == level);
        if (specialReward != null)
        {
            OnLevelRewardReceived?.Invoke(specialReward);
            Debug.Log($"Special reward: {specialReward.RewardName}");
        }
    }

    /// <summary>
    /// الحصول على XP المطلوب للمستوى التالي
    /// </summary>
    public int GetXPForNextLevel()
    {
        if (!levelConfigs.ContainsKey(currentLevel + 1))
            return 0;

        return levelConfigs[currentLevel + 1].XPRequired - currentXP;
    }

    /// <summary>
    /// الحصول على نسبة تقدم المستوى
    /// </summary>
    public float GetLevelProgress()
    {
        if (!levelConfigs.ContainsKey(currentLevel + 1))
            return 1f;

        int xpNeeded = levelConfigs[currentLevel + 1].XPRequired;
        return (float)currentXP / xpNeeded;
    }

    /// <summary>
    /// الحصول على معلومات المستوى الحالي
    /// </summary>
    public LevelInfo GetCurrentLevelInfo()
    {
        return new LevelInfo
        {
            Level = currentLevel,
            CurrentXP = currentXP,
            TotalXP = totalXP,
            XPForNextLevel = GetXPForNextLevel(),
            LevelProgress = GetLevelProgress(),
            MaxLevel = 50
        };
    }

    /// <summary>
    /// الحصول على معلومات مستوى محدد
    /// </summary>
    public LevelConfig GetLevelConfig(int level)
    {
        if (levelConfigs.ContainsKey(level))
            return levelConfigs[level];

        return null;
    }

    /// <summary>
    /// تعيين المستوى مباشرة (للاختبار)
    /// </summary>
    public void SetLevel(int level)
    {
        if (level < 1 || level > 50)
        {
            Debug.LogWarning("Invalid level!");
            return;
        }

        currentLevel = level;
        currentXP = 0;

        SaveLevelDataLocal();
        Debug.Log($"Level set to: {level}");
    }

    /// <summary>
    /// حفظ بيانات المستوى
    /// </summary>
    private void SaveLevelDataLocal()
    {
        try
        {
            var data = new LevelData
            {
                CurrentLevel = currentLevel,
                CurrentXP = currentXP,
                TotalXP = totalXP
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(LEVEL_DATA_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save level data: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل بيانات المستوى
    /// </summary>
    private void LoadLevelDataLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(LEVEL_DATA_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonUtility.FromJson<LevelData>(json);
                currentLevel = data.CurrentLevel;
                currentXP = data.CurrentXP;
                totalXP = data.TotalXP;

                Debug.Log($"Level data loaded: Level {currentLevel}, XP: {currentXP}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load level data: {ex.Message}");
        }
    }

    // Getters
    public static LevelingSystem Instance => instance;
    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int TotalXP => totalXP;
}

// ==================== Data Classes ====================

[System.Serializable]
public class LevelConfig
{
    public int Level;
    public int XPRequired;
    public int RewardCoins;
    public int RewardGems;
    public int RewardPrestige;
}

[System.Serializable]
public class LevelReward
{
    public int Level;
    public string RewardType;  // special_item, costume, weapon
    public string RewardName;
    public int RewardValue;
}

[System.Serializable]
public class LevelInfo
{
    public int Level;
    public int CurrentXP;
    public int TotalXP;
    public int XPForNextLevel;
    public float LevelProgress;
    public int MaxLevel;
}

[System.Serializable]
public class LevelData
{
    public int CurrentLevel;
    public int CurrentXP;
    public int TotalXP;
}
