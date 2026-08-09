using System;
using System.Threading.Tasks;

/// <summary>
/// واجهة مدير اللاعب
/// </summary>
public interface IPlayerManager
{
    /// <summary>
    /// الحصول على ملف اللاعب الحالي
    /// </summary>
    PlayerProfile CurrentProfile { get; }

    /// <summary>
    /// الحصول على إحصائيات اللاعب
    /// </summary>
    PlayerStats CurrentStats { get; }

    /// <summary>
    /// إضافة عملة للاعب
    /// </summary>
    Task<bool> AddCurrency(int amount, string currencyType);

    /// <summary>
    /// إضافة XP للاعب
    /// </summary>
    Task<bool> AddXP(int amount);

    /// <summary>
    /// التحقق من توفر المال
    /// </summary>
    bool HasCurrency(int amount, string currencyType);

    /// <summary>
    /// تحميل بيانات اللاعب
    /// </summary>
    Task<bool> LoadPlayerData();

    /// <summary>
    /// حفظ بيانات اللاعب
    /// </summary>
    Task<bool> SavePlayerData();

    /// <summary>
    /// التحقق من التوثيق
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// حدث تحديث ملف اللاعب
    /// </summary>
    event Action<PlayerProfile> OnProfileUpdated;

    /// <summary>
    /// حدث تحديث الإحصائيات
    /// </summary>
    event Action<PlayerStats> OnStatsUpdated;
}

/// <summary>
/// ملف اللاعب
/// </summary>
[System.Serializable]
public class PlayerProfile
{
    public string playerId;
    public string username;
    public string email;
    public int level;
    public long xp;
    public int coins;
    public int gems;
    public DateTime createdAt;
    public DateTime lastLogin;
    public string displayName;
    public string avatar;
}

/// <summary>
/// إحصائيات اللاعب
/// </summary>
[System.Serializable]
public class PlayerStats
{
    public int gamesPlayed;
    public int gamesWon;
    public int totalKills;
    public int totalDeaths;
    public float winRate;
    public int playtime; // بالدقائق
    public DateTime lastUpdated;
}
