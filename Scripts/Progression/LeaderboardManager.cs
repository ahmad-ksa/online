using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير لوحة الصدارة - يتعامل مع الترتيبات والتصنيفات
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager instance;

    private List<LeaderboardEntry> globalLeaderboard = new List<LeaderboardEntry>();
    private List<LeaderboardEntry> friendsLeaderboard = new List<LeaderboardEntry>();
    private List<LeaderboardEntry> weeklyLeaderboard = new List<LeaderboardEntry>();
    private List<LeaderboardEntry> monthlyLeaderboard = new List<LeaderboardEntry>();

    private int playerRank = 0;
    private int playerFriendsRank = 0;

    // Events
    public static event Action<List<LeaderboardEntry>> OnLeaderboardUpdated;
    public static event Action<int> OnRankChanged;

    // Storage Keys
    private const string GLOBAL_LEADERBOARD_KEY = "GlobalLeaderboard";
    private const string PLAYER_RANK_KEY = "PlayerRank";

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
    /// تهيئة مدير لوحة الصدارة
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadLeaderboardsLocal();
        isInitialized = true;

        Debug.Log("LeaderboardManager Initialized");
    }

    /// <summary>
    /// تحديث لوحة الصدارة العامة
    /// </summary>
    public async Task<bool> UpdateGlobalLeaderboard()
    {
        try
        {
            Debug.Log("Updating global leaderboard...");

            await Task.Delay(1000);

            // محاكاة تحميل من السيرفر
            globalLeaderboard.Clear();

            // إنشاء بيانات وهمية
            for (int i = 0; i < 100; i++)
            {
                globalLeaderboard.Add(new LeaderboardEntry
                {
                    Rank = i + 1,
                    PlayerId = System.Guid.NewGuid().ToString(),
                    Username = $"Player_{i + 1}",
                    Score = 10000 - (i * 100),
                    Level = 50 - (i / 10),
                    UpdatedAt = DateTime.Now
                });
            }

            // تحديد رتبة اللاعب الحالي
            var playerEntry = globalLeaderboard.FirstOrDefault(
                e => e.PlayerId == PlayerManager.Instance.CurrentProfile.playerId
            );

            if (playerEntry == null)
            {
                // لم يكن موجوداً، أضفه
                int playerScore = (int)(PlayerManager.Instance.CurrentStats.totalWins * 500);
                globalLeaderboard.Add(new LeaderboardEntry
                {
                    PlayerId = PlayerManager.Instance.CurrentProfile.playerId,
                    Username = PlayerManager.Instance.CurrentProfile.playerName,
                    Score = playerScore,
                    Level = PlayerManager.Instance.CurrentStats.currentLevel,
                    UpdatedAt = DateTime.Now
                });

                // أعد الترتيب
                globalLeaderboard = globalLeaderboard
                    .OrderByDescending(e => e.Score)
                    .Select((e, i) => { e.Rank = i + 1; return e; })
                    .ToList();

                playerEntry = globalLeaderboard.FirstOrDefault(
                    e => e.PlayerId == PlayerManager.Instance.CurrentProfile.playerId
                );
            }

            playerRank = playerEntry?.Rank ?? 0;

            SaveLeaderboardsLocal();
            OnLeaderboardUpdated?.Invoke(globalLeaderboard);
            OnRankChanged?.Invoke(playerRank);

            Debug.Log($"Leaderboard updated. Player rank: {playerRank}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update leaderboard: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تحديث لوحة الصدارة الأسبوعية
    /// </summary>
    public async Task<bool> UpdateWeeklyLeaderboard()
    {
        try
        {
            Debug.Log("Updating weekly leaderboard...");

            await Task.Delay(800);

            weeklyLeaderboard.Clear();

            for (int i = 0; i < 50; i++)
            {
                weeklyLeaderboard.Add(new LeaderboardEntry
                {
                    Rank = i + 1,
                    PlayerId = System.Guid.NewGuid().ToString(),
                    Username = $"Weekly_Player_{i + 1}",
                    Score = UnityEngine.Random.Range(1000, 5000),
                    Level = UnityEngine.Random.Range(10, 50),
                    UpdatedAt = DateTime.Now
                });
            }

            OnLeaderboardUpdated?.Invoke(weeklyLeaderboard);

            Debug.Log("Weekly leaderboard updated");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update weekly leaderboard: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تحديث لوحة صدارة الأصدقاء
    /// </summary>
    public async Task<bool> UpdateFriendsLeaderboard()
    {
        try
        {
            Debug.Log("Updating friends leaderboard...");

            await Task.Delay(600);

            friendsLeaderboard.Clear();

            // الحصول على قائمة الأصدقاء
            var friends = FriendsManager.Instance.FriendsList;

            int rank = 1;
            foreach (var friend in friends.OrderByDescending(f => f.Level))
            {
                friendsLeaderboard.Add(new LeaderboardEntry
                {
                    Rank = rank,
                    PlayerId = friend.PlayerId,
                    Username = friend.Username,
                    Score = UnityEngine.Random.Range(1000, 5000),
                    Level = friend.Level,
                    UpdatedAt = DateTime.Now
                });

                rank++;
            }

            // إضافة اللاعب الحالي
            friendsLeaderboard.Add(new LeaderboardEntry
            {
                Rank = friendsLeaderboard.Count + 1,
                PlayerId = PlayerManager.Instance.CurrentProfile.playerId,
                Username = PlayerManager.Instance.CurrentProfile.playerName,
                Score = PlayerManager.Instance.CurrentStats.totalWins * 500,
                Level = PlayerManager.Instance.CurrentStats.currentLevel,
                UpdatedAt = DateTime.Now
            });

            playerFriendsRank = friendsLeaderboard.Count;

            OnLeaderboardUpdated?.Invoke(friendsLeaderboard);

            Debug.Log("Friends leaderboard updated");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update friends leaderboard: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// الحصول على لوحة الصدارة العامة
    /// </summary>
    public List<LeaderboardEntry> GetGlobalLeaderboard(int limit = 100)
    {
        return globalLeaderboard.Take(limit).ToList();
    }

    /// <summary>
    /// الحصول على لوحة صدارة الأصدقاء
    /// </summary>
    public List<LeaderboardEntry> GetFriendsLeaderboard()
    {
        return new List<LeaderboardEntry>(friendsLeaderboard);
    }

    /// <summary>
    /// الحصول على لوحة الصدارة الأسبوعية
    /// </summary>
    public List<LeaderboardEntry> GetWeeklyLeaderboard(int limit = 50)
    {
        return weeklyLeaderboard.Take(limit).ToList();
    }

    /// <summary>
    /// الحصول على المقدمات (Top 10)
    /// </summary>
    public List<LeaderboardEntry> GetTopPlayers(int count = 10)
    {
        return globalLeaderboard.Take(count).ToList();
    }

    /// <summary>
    /// الحصول على رتبة اللاعب
    /// </summary>
    public int GetPlayerRank()
    {
        return playerRank;
    }

    /// <summary>
    /// الحصول على اللاعبين حول الحالي
    /// </summary>
    public List<LeaderboardEntry> GetNearbyPlayers(int range = 5)
    {
        if (playerRank == 0)
            return new List<LeaderboardEntry>();

        int startIndex = Mathf.Max(0, playerRank - range - 1);
        int endIndex = Mathf.Min(globalLeaderboard.Count, playerRank + range);

        return globalLeaderboard.GetRange(startIndex, endIndex - startIndex);
    }

    /// <summary>
    /// حفظ لوحات الصدارة محلياً
    /// </summary>
    private void SaveLeaderboardsLocal()
    {
        try
        {
            string json = JsonUtility.ToJson(new LeaderboardList { entries = globalLeaderboard });
            PlayerPrefs.SetString(GLOBAL_LEADERBOARD_KEY, json);
            PlayerPrefs.SetInt(PLAYER_RANK_KEY, playerRank);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save leaderboards: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل لوحات الصدارة محلياً
    /// </summary>
    private void LoadLeaderboardsLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(GLOBAL_LEADERBOARD_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var leaderboardList = JsonUtility.FromJson<LeaderboardList>(json);
                globalLeaderboard = leaderboardList.entries ?? new List<LeaderboardEntry>();

                playerRank = PlayerPrefs.GetInt(PLAYER_RANK_KEY, 0);

                Debug.Log($"Leaderboards loaded. Player rank: {playerRank}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load leaderboards: {ex.Message}");
        }
    }

    // Getters
    public static LeaderboardManager Instance => instance;
    public int PlayerRank => playerRank;
    public int PlayerFriendsRank => playerFriendsRank;
}

// ==================== Data Classes ====================

[System.Serializable]
public class LeaderboardEntry
{
    public int Rank;
    public string PlayerId;
    public string Username;
    public int Score;
    public int Level;
    public DateTime UpdatedAt;
}

[System.Serializable]
public class LeaderboardList
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}
