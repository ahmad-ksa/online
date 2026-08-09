using UnityEngine;
using System;
using System.Threading.Tasks;

/// <summary>
/// نظام إدارة بيانات اللاعب المحسّن
/// </summary>
public class PlayerManager : MonoBehaviour, IPlayerManager
{
    private static PlayerManager instance;

    private PlayerProfile currentProfile;
    private PlayerStats currentStats;
    private bool isAuthenticated = false;

    // Events
    public event Action<PlayerProfile> OnProfileUpdated;
    public event Action<PlayerStats> OnStatsUpdated;

    private GameConfig config;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        config = GameConfig.Instance;
        Logger.Log("PlayerManager initialized", "PlayerManager");
    }

    /// <summary>
    /// تحميل بيانات اللاعب
    /// </summary>
    public async Task<bool> LoadPlayerData()
    {
        try
        {
            Logger.Log("Loading player data...", "PlayerManager");

            // محاكاة تحميل البيانات
            await Task.Delay(500);

            currentProfile = new PlayerProfile
            {
                playerId = "player_123",
                username = "PlayerName",
                email = "player@example.com",
                level = 1,
                xp = 0,
                coins = 1000,
                gems = 50,
                createdAt = DateTime.Now,
                lastLogin = DateTime.Now,
                displayName = "Player",
                avatar = ""
            };

            currentStats = new PlayerStats
            {
                gamesPlayed = 0,
                gamesWon = 0,
                totalKills = 0,
                totalDeaths = 0,
                winRate = 0f,
                playtime = 0,
                lastUpdated = DateTime.Now
            };

            isAuthenticated = true;
            OnProfileUpdated?.Invoke(currentProfile);
            OnStatsUpdated?.Invoke(currentStats);

            Logger.Log("Player data loaded successfully!", "PlayerManager");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load player data: {ex.Message}", "PlayerManager");
            return false;
        }
    }

    /// <summary>
    /// حفظ بيانات اللاعب
    /// </summary>
    public async Task<bool> SavePlayerData()
    {
        try
        {
            if (currentProfile == null)
            {
                Logger.LogError("No player data to save!", "PlayerManager");
                return false;
            }

            Logger.Log("Saving player data...", "PlayerManager");

            // محاكاة حفظ البيانات
            await Task.Delay(500);

            // TODO: احفظ البيانات في قاعدة البيانات السحابية
            Logger.Log("Player data saved successfully!", "PlayerManager");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to save player data: {ex.Message}", "PlayerManager");
            return false;
        }
    }

    /// <summary>
    /// إضافة عملة للاعب
    /// </summary>
    public async Task<bool> AddCurrency(int amount, string currencyType)
    {
        try
        {
            if (currentProfile == null)
            {
                Logger.LogError("No player profile!", "PlayerManager");
                return false;
            }

            if (amount <= 0)
            {
                Logger.LogWarning("Invalid currency amount!", "PlayerManager");
                return false;
            }

            if (currencyType == "coins")
            {
                currentProfile.coins += amount;
            }
            else if (currencyType == "gems")
            {
                currentProfile.gems += amount;
            }
            else
            {
                Logger.LogError($"Unknown currency type: {currencyType}", "PlayerManager");
                return false;
            }

            Logger.Log($"Added {amount} {currencyType} to player", "PlayerManager");
            OnProfileUpdated?.Invoke(currentProfile);

            await SavePlayerData();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to add currency: {ex.Message}", "PlayerManager");
            return false;
        }
    }

    /// <summary>
    /// إضافة XP للاعب
    /// </summary>
    public async Task<bool> AddXP(int amount)
    {
        try
        {
            if (currentProfile == null)
            {
                Logger.LogError("No player profile!", "PlayerManager");
                return false;
            }

            if (amount <= 0)
            {
                Logger.LogWarning("Invalid XP amount!", "PlayerManager");
                return false;
            }

            currentProfile.xp += amount;

            // حساب Level بناءً على XP
            int newLevel = (int)(currentProfile.xp / 1000) + 1;
            if (newLevel != currentProfile.level)
            {
                currentProfile.level = newLevel;
                Logger.Log($"Player leveled up to {currentProfile.level}!", "PlayerManager");
            }

            Logger.Log($"Added {amount} XP to player", "PlayerManager");
            OnProfileUpdated?.Invoke(currentProfile);

            await SavePlayerData();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to add XP: {ex.Message}", "PlayerManager");
            return false;
        }
    }

    /// <summary>
    /// التحقق من توفر المال
    /// </summary>
    public bool HasCurrency(int amount, string currencyType)
    {
        if (currentProfile == null)
            return false;

        if (currencyType == "coins")
            return currentProfile.coins >= amount;
        else if (currencyType == "gems")
            return currentProfile.gems >= amount;

        return false;
    }

    // Properties
    public PlayerProfile CurrentProfile => currentProfile;
    public PlayerStats CurrentStats => currentStats;
    public bool IsAuthenticated => isAuthenticated;
    public static PlayerManager Instance => instance;
}
