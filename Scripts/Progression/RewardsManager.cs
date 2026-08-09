using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير الجوائز - يتعامل مع المكافآت والجوائز اليومية
/// </summary>
public class RewardsManager : MonoBehaviour
{
    private static RewardsManager instance;

    // Daily Rewards
    private List<DailyReward> dailyRewards = new List<DailyReward>();
    private DateTime lastClaimDate = DateTime.MinValue;
    private int currentStreak = 0;

    // Season Rewards
    private List<SeasonReward> seasonRewards = new List<SeasonReward>();
    private int currentSeason = 1;

    // Events
    public static event Action<DailyReward> OnDailyRewardClaimed;
    public static event Action<int> OnStreakUpdated;
    public static event Action<SeasonReward> OnSeasonRewardEarned;
    public static event Action OnRewardAvailable;

    // Storage Keys
    private const string DAILY_REWARD_KEY = "DailyReward";
    private const string STREAK_KEY = "RewardStreak";
    private const string SEASON_REWARD_KEY = "SeasonReward";

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

    private void Update()
    {
        // التحقق من الجوائز اليومية كل دقيقة
        CheckDailyRewardAvailability();
    }

    /// <summary>
    /// تهيئة مدير الجوائز
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        CreateDailyRewards();
        CreateSeasonRewards();
        LoadRewardsLocal();

        isInitialized = true;

        Debug.Log("RewardsManager Initialized");
    }

    /// <summary>
    /// إنشاء جوائز يومية
    /// </summary>
    private void CreateDailyRewards()
    {
        dailyRewards = new List<DailyReward>
        {
            new DailyReward
            {
                Day = 1,
                RewardType = "coins",
                RewardAmount = 500,
                BonusMultiplier = 1.0f,
                Description = "يوم 1: 500 عملة"
            },

            new DailyReward
            {
                Day = 2,
                RewardType = "coins",
                RewardAmount = 600,
                BonusMultiplier = 1.0f,
                Description = "يوم 2: 600 عملة"
            },

            new DailyReward
            {
                Day = 3,
                RewardType = "gems",
                RewardAmount = 50,
                BonusMultiplier = 1.0f,
                Description = "يوم 3: 50 جوهرة"
            },

            new DailyReward
            {
                Day = 4,
                RewardType = "coins",
                RewardAmount = 700,
                BonusMultiplier = 1.0f,
                Description = "يوم 4: 700 عملة"
            },

            new DailyReward
            {
                Day = 5,
                RewardType = "gems",
                RewardAmount = 75,
                BonusMultiplier = 1.0f,
                Description = "يوم 5: 75 جوهرة"
            },

            new DailyReward
            {
                Day = 6,
                RewardType = "coins",
                RewardAmount = 800,
                BonusMultiplier = 1.0f,
                Description = "يوم 6: 800 عملة"
            },

            new DailyReward
            {
                Day = 7,
                RewardType = "coins",
                RewardAmount = 1000,
                BonusMultiplier = 2.0f, // مضاعف في اليوم السابع
                Description = "يوم 7 (الأخير): 1000 عملة + 100 بونص!"
            }
        };

        Debug.Log($"Created {dailyRewards.Count} daily rewards");
    }

    /// <summary>
    /// إنشاء جوائز الموسم
    /// </summary>
    private void CreateSeasonRewards()
    {
        seasonRewards = new List<SeasonReward>
        {
            new SeasonReward
            {
                Season = 1,
                Tier = 1,
                RequiredRank = 100,
                RewardCoins = 5000,
                RewardGems = 500,
                RewardSkin = "season1_tier1_skin",
                Description = "الموسم 1 - المستوى الأول"
            },

            new SeasonReward
            {
                Season = 1,
                Tier = 2,
                RequiredRank = 50,
                RewardCoins = 3000,
                RewardGems = 300,
                RewardSkin = "season1_tier2_skin",
                Description = "الموسم 1 - المستوى الثاني"
            },

            new SeasonReward
            {
                Season = 1,
                Tier = 3,
                RequiredRank = 1,
                RewardCoins = 1000,
                RewardGems = 100,
                RewardSkin = "",
                Description = "الموسم 1 - المشاركة"
            }
        };

        Debug.Log($"Created {seasonRewards.Count} season rewards");
    }

    /// <summary>
    /// الحصول على الجائزة اليومية
    /// </summary>
    public async Task<bool> ClaimDailyReward()
    {
        if (!IsDailyRewardAvailable())
        {
            Debug.LogWarning("Daily reward not available yet!");
            return false;
        }

        try
        {
            Debug.Log("Claiming daily reward...");

            await Task.Delay(500);

            // تحديد اليوم الحالي
            int currentDay = (currentStreak % 7) + 1;
            var reward = dailyRewards.FirstOrDefault(r => r.Day == currentDay);

            if (reward == null)
            {
                Debug.LogWarning("Reward not found for day!");
                return false;
            }

            // تطبيق الجائزة
            if (reward.RewardType == "coins")
            {
                int amount = (int)(reward.RewardAmount * reward.BonusMultiplier);
                PlayerManager.Instance.AddCurrency(amount, CurrencyType.Coins);
            }
            else if (reward.RewardType == "gems")
            {
                int amount = (int)(reward.RewardAmount * reward.BonusMultiplier);
                PlayerManager.Instance.AddCurrency(amount, CurrencyType.Gems);
            }

            // تحديث الـ Streak
            currentStreak++;
            lastClaimDate = DateTime.Now;

            OnDailyRewardClaimed?.Invoke(reward);
            OnStreakUpdated?.Invoke(currentStreak);

            SaveRewardsLocal();

            Debug.Log($"Daily reward claimed! Streak: {currentStreak}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to claim daily reward: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// التحقق من توفر الجائزة اليومية
    /// </summary>
    public bool IsDailyRewardAvailable()
    {
        if (lastClaimDate == DateTime.MinValue)
            return true; // لم يتم المطالبة بعد

        var timeSinceLastClaim = DateTime.Now - lastClaimDate;
        return timeSinceLastClaim.TotalHours >= 24;
    }

    /// <summary>
    /// الحصول على وقت انتظار الجائزة التالية
    /// </summary>
    public TimeSpan GetTimeUntilNextReward()
    {
        if (!IsDailyRewardAvailable())
        {
            var timeSinceLastClaim = DateTime.Now - lastClaimDate;
            var timeRemaining = TimeSpan.FromHours(24) - timeSinceLastClaim;
            return timeRemaining;
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// الحصول على جوائز الموسم
    /// </summary>
    public List<SeasonReward> GetSeasonRewards()
    {
        return seasonRewards.Where(r => r.Season == currentSeason).ToList();
    }

    /// <summary>
    /// الحصول على جوائز موسم مكتسبة
    /// </summary>
    public async Task<List<SeasonReward>> ClaimSeasonRewards()
    {
        List<SeasonReward> claimedRewards = new List<SeasonReward>();

        try
        {
            Debug.Log("Claiming season rewards...");

            var playerRank = LeaderboardManager.Instance.PlayerRank;
            var seasonRewardsForPlayer = seasonRewards
                .Where(r => r.Season == currentSeason && playerRank <= r.RequiredRank)
                .ToList();

            foreach (var reward in seasonRewardsForPlayer)
            {
                await Task.Delay(300);

                // تطبيق الجوائز
                PlayerManager.Instance.AddCurrency(reward.RewardCoins, CurrencyType.Coins);
                PlayerManager.Instance.AddCurrency(reward.RewardGems, CurrencyType.Gems);

                OnSeasonRewardEarned?.Invoke(reward);
                claimedRewards.Add(reward);

                Debug.Log($"Season reward claimed: {reward.Description}");
            }

            SaveRewardsLocal();
            return claimedRewards;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to claim season rewards: {ex.Message}");
            return claimedRewards;
        }
    }

    /// <summary>
    /// التحقق من توفر جوائز يومية
    /// </summary>
    private void CheckDailyRewardAvailability()
    {
        if (IsDailyRewardAvailable() && lastClaimDate != DateTime.MinValue)
        {
            OnRewardAvailable?.Invoke();
        }
    }

    /// <summary>
    /// إعادة تعيين الـ Streak (في حالة تخطي يوم)
    /// </summary>
    private void CheckStreakReset()
    {
        if (lastClaimDate == DateTime.MinValue)
            return;

        var timeSinceLastClaim = DateTime.Now - lastClaimDate;

        if (timeSinceLastClaim.TotalHours > 48)
        {
            Debug.Log("Streak reset!");
            currentStreak = 0;
            SaveRewardsLocal();
        }
    }

    /// <summary>
    /// حفظ الجوائز محلياً
    /// </summary>
    private void SaveRewardsLocal()
    {
        try
        {
            PlayerPrefs.SetString(DAILY_REWARD_KEY, lastClaimDate.ToString());
            PlayerPrefs.SetInt(STREAK_KEY, currentStreak);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save rewards: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل الجوائز محلياً
    /// </summary>
    private void LoadRewardsLocal()
    {
        try
        {
            string dateStr = PlayerPrefs.GetString(DAILY_REWARD_KEY, "");
            if (!string.IsNullOrEmpty(dateStr))
            {
                if (DateTime.TryParse(dateStr, out var date))
                {
                    lastClaimDate = date;
                }
            }

            currentStreak = PlayerPrefs.GetInt(STREAK_KEY, 0);

            Debug.Log($"Rewards loaded. Streak: {currentStreak}");

            CheckStreakReset();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load rewards: {ex.Message}");
        }
    }

    // Getters
    public static RewardsManager Instance => instance;
    public int CurrentStreak => currentStreak;
    public int CurrentSeason => currentSeason;
    public List<DailyReward> DailyRewards => dailyRewards;
}

// ==================== Data Classes ====================

[System.Serializable]
public class DailyReward
{
    public int Day;
    public string RewardType;  // coins, gems, item
    public int RewardAmount;
    public float BonusMultiplier;
    public string Description;
}

[System.Serializable]
public class SeasonReward
{
    public int Season;
    public int Tier;
    public int RequiredRank;
    public int RewardCoins;
    public int RewardGems;
    public string RewardSkin;
    public string Description;
}
