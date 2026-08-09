using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير المطابقة - يتعامل مع البحث عن لاعبين والمطابقة بينهم
/// </summary>
public class MatchmakingManager : MonoBehaviour
{
    private static MatchmakingManager instance;

    public enum MatchmakingMode
    {
        QuickMatch,      // بحث سريع
        RankedMatch,     // بحث حسب الترتيب
        FriendMatch,     // لعب مع الأصدقاء
        CustomMatch      // لعبة مخصصة
    }

    public enum QueueStatus
    {
        Idle,
        Searching,
        Found,
        Joining,
        Cancelled,
        Timeout
    }

    private MatchmakingMode currentMode = MatchmakingMode.QuickMatch;
    private QueueStatus queueStatus = QueueStatus.Idle;
    private float searchStartTime = 0f;
    private float searchTimeout = 120f; // دقيقتين

    // Current Search Parameters
    private MatchmakingFilter searchFilter = new MatchmakingFilter();
    private List<PlayerInQueue> playersInQueue = new List<PlayerInQueue>();
    private MatchResult foundMatch = null;

    // Events
    public static event Action<MatchmakingMode> OnSearchStarted;
    public static event Action<QueueStatus> OnQueueStatusChanged;
    public static event Action<MatchResult> OnMatchFound;
    public static event Action OnSearchCancelled;
    public static event Action OnSearchTimeout;

    // Storage Keys
    private const string QUEUE_STATS_KEY = "QueueStats";

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
        UpdateQueueTimeout();
    }

    /// <summary>
    /// تهيئة مدير المطابقة
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadQueueStatsLocal();
        isInitialized = true;

        Debug.Log("MatchmakingManager Initialized");
    }

    /// <summary>
    /// بدء البحث عن خصم
    /// </summary>
    public async Task<bool> StartSearch(MatchmakingMode mode, MatchmakingFilter filter = null)
    {
        if (queueStatus == QueueStatus.Searching)
        {
            Debug.LogWarning("Already searching!");
            return false;
        }

        try
        {
            Debug.Log($"Starting {mode} search...");

            currentMode = mode;
            searchFilter = filter ?? new MatchmakingFilter();
            queueStatus = QueueStatus.Searching;
            searchStartTime = Time.time;

            OnSearchStarted?.Invoke(mode);
            SetQueueStatus(QueueStatus.Searching);

            // محاكاة البحث
            await Task.Delay(2000);

            // البحث عن لاعبين
            playersInQueue = await SearchForPlayers(mode, filter);

            if (playersInQueue.Count > 0)
            {
                // عثرنا على مطابقة
                foundMatch = CreateMatch(playersInQueue);
                SetQueueStatus(QueueStatus.Found);
                OnMatchFound?.Invoke(foundMatch);

                Debug.Log($"Match found! Players: {foundMatch.PlayerCount}");
                return true;
            }

            Debug.Log("No players found, waiting...");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Search failed: {ex.Message}");
            CancelSearch();
            return false;
        }
    }

    /// <summary>
    /// البحث السريع (Quick Match)
    /// </summary>
    public async Task<bool> QuickMatch()
    {
        var filter = new MatchmakingFilter
        {
            MaxLevel = PlayerManager.Instance.CurrentStats.currentLevel + 5,
            MinLevel = PlayerManager.Instance.CurrentStats.currentLevel - 5,
            GameMode = "all",
            MaxPing = 150
        };

        return await StartSearch(MatchmakingMode.QuickMatch, filter);
    }

    /// <summary>
    /// البحث المرتب (Ranked Match)
    /// </summary>
    public async Task<bool> RankedMatch()
    {
        var filter = new MatchmakingFilter
        {
            MaxLevel = PlayerManager.Instance.CurrentStats.currentLevel + 3,
            MinLevel = PlayerManager.Instance.CurrentStats.currentLevel - 3,
            GameMode = "ranked",
            MaxPing = 100
        };

        return await StartSearch(MatchmakingMode.RankedMatch, filter);
    }

    /// <summary>
    /// اللعب مع أصدقاء
    /// </summary>
    public async Task<bool> PlayWithFriends(List<string> friendIds)
    {
        if (friendIds.Count == 0)
        {
            Debug.LogWarning("No friends selected");
            return false;
        }

        try
        {
            Debug.Log($"Creating match with {friendIds.Count} friends...");

            SetQueueStatus(QueueStatus.Searching);

            await Task.Delay(1000);

            // إنشاء لعبة مع الأصدقاء
            var players = new List<PlayerInQueue>();
            
            // إضافة اللاعب الحالي
            players.Add(new PlayerInQueue
            {
                PlayerId = PlayerManager.Instance.CurrentProfile.playerId,
                Username = PlayerManager.Instance.CurrentProfile.playerName,
                Level = PlayerManager.Instance.CurrentStats.currentLevel
            });

            // إضافة الأصدقاء
            foreach (var friendId in friendIds)
            {
                players.Add(new PlayerInQueue
                {
                    PlayerId = friendId,
                    Username = $"Friend_{friendId}",
                    Level = UnityEngine.Random.Range(1, 50)
                });
            }

            foundMatch = CreateMatch(players);
            SetQueueStatus(QueueStatus.Found);
            OnMatchFound?.Invoke(foundMatch);

            Debug.Log($"Match created with {foundMatch.PlayerCount} players");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create friend match: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// إلغاء البحث
    /// </summary>
    public void CancelSearch()
    {
        if (queueStatus != QueueStatus.Searching)
            return;

        Debug.Log("Search cancelled");

        playersInQueue.Clear();
        searchStartTime = 0f;
        SetQueueStatus(QueueStatus.Cancelled);

        OnSearchCancelled?.Invoke();
    }

    /// <summary>
    /// قبول المطابقة والدخول للعبة
    /// </summary>
    public async Task<bool> AcceptMatch()
    {
        if (foundMatch == null)
        {
            Debug.LogWarning("No match to accept");
            return false;
        }

        try
        {
            Debug.Log("Accepting match...");

            SetQueueStatus(QueueStatus.Joining);

            await Task.Delay(1000);

            // إدخال اللاعب للعبة
            GameManager.Instance.SetGameState(GameManager.GameState.InGame);

            SetQueueStatus(QueueStatus.Idle);

            Debug.Log("Joined match successfully");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to accept match: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// رفض المطابقة والبحث عن أخرى
    /// </summary>
    public async Task<bool> RejectMatch()
    {
        if (foundMatch == null)
            return false;

        Debug.Log("Match rejected, searching again...");

        foundMatch = null;
        return await StartSearch(currentMode, searchFilter);
    }

    /// <summary>
    /// البحث عن لاعبين (محاكاة)
    /// </summary>
    private async Task<List<PlayerInQueue>> SearchForPlayers(MatchmakingMode mode, MatchmakingFilter filter)
    {
        List<PlayerInQueue> players = new List<PlayerInQueue>();

        await Task.Delay(500);

        // محاكاة البحث
        int playerCount = UnityEngine.Random.Range(1, 9); // 1-8 لاعبين

        for (int i = 0; i < playerCount; i++)
        {
            players.Add(new PlayerInQueue
            {
                PlayerId = System.Guid.NewGuid().ToString(),
                Username = $"Player_{i}",
                Level = UnityEngine.Random.Range(filter.MinLevel, filter.MaxLevel),
                Ping = UnityEngine.Random.Range(20, filter.MaxPing)
            });
        }

        // إضافة اللاعب الحالي
        players.Add(new PlayerInQueue
        {
            PlayerId = PlayerManager.Instance.CurrentProfile.playerId,
            Username = PlayerManager.Instance.CurrentProfile.playerName,
            Level = PlayerManager.Instance.CurrentStats.currentLevel,
            Ping = UnityEngine.Random.Range(10, 50)
        });

        return players;
    }

    /// <summary>
    /// إنشاء مطابقة من اللاعبين
    /// </summary>
    private MatchResult CreateMatch(List<PlayerInQueue> players)
    {
        var match = new MatchResult
        {
            MatchId = System.Guid.NewGuid().ToString(),
            MatchMode = currentMode,
            GameMode = searchFilter.GameMode,
            PlayerCount = players.Count,
            Players = players,
            CreatedAt = DateTime.Now,
            Map = GetRandomMap()
        };

        return match;
    }

    /// <summary>
    /// الحصول على خريطة عشوائية
    /// </summary>
    private string GetRandomMap()
    {
        var maps = new[] { "Forest", "Desert", "City", "Mountain", "Snow" };
        return maps[UnityEngine.Random.Range(0, maps.Length)];
    }

    /// <summary>
    /// تحديث انتهاء صلاحية البحث
    /// </summary>
    private void UpdateQueueTimeout()
    {
        if (queueStatus != QueueStatus.Searching)
            return;

        float searchTime = Time.time - searchStartTime;

        if (searchTime > searchTimeout)
        {
            Debug.LogWarning("Search timeout!");
            SetQueueStatus(QueueStatus.Timeout);
            OnSearchTimeout?.Invoke();
            CancelSearch();
        }
    }

    /// <summary>
    /// تعيين حالة الطابور
    /// </summary>
    private void SetQueueStatus(QueueStatus newStatus)
    {
        if (newStatus == queueStatus)
            return;

        queueStatus = newStatus;
        OnQueueStatusChanged?.Invoke(queueStatus);

        Debug.Log($"Queue Status: {queueStatus}");
    }

    /// <summary>
    /// حفظ إحصائيات الطابور
    /// </summary>
    private void SaveQueueStatsLocal()
    {
        try
        {
            var stats = new QueueStats
            {
                TotalSearches = UnityEngine.Random.Range(5, 50),
                AverageWaitTime = UnityEngine.Random.Range(10, 60),
                SuccessRate = UnityEngine.Random.Range(0.7f, 0.95f)
            };

            string json = JsonUtility.ToJson(stats);
            PlayerPrefs.SetString(QUEUE_STATS_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save queue stats: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل إحصائيات الطابور
    /// </summary>
    private void LoadQueueStatsLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(QUEUE_STATS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var stats = JsonUtility.FromJson<QueueStats>(json);
                Debug.Log($"Queue stats loaded: {stats.TotalSearches} searches");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load queue stats: {ex.Message}");
        }
    }

    // Getters
    public static MatchmakingManager Instance => instance;
    public QueueStatus CurrentStatus => queueStatus;
    public MatchmakingMode CurrentMode => currentMode;
    public MatchResult CurrentMatch => foundMatch;
    public List<PlayerInQueue> PlayersInQueue => playersInQueue;
}

// ==================== Data Classes ====================

[System.Serializable]
public class MatchmakingFilter
{
    public int MinLevel = 1;
    public int MaxLevel = 100;
    public string GameMode = "all";
    public int MaxPing = 200;
    public bool RankedOnly = false;
}

[System.Serializable]
public class PlayerInQueue
{
    public string PlayerId;
    public string Username;
    public int Level;
    public int Ping;
}

[System.Serializable]
public class MatchResult
{
    public string MatchId;
    public MatchmakingManager.MatchmakingMode MatchMode;
    public string GameMode;
    public int PlayerCount;
    public List<PlayerInQueue> Players = new List<PlayerInQueue>();
    public DateTime CreatedAt;
    public string Map;
}

[System.Serializable]
public class QueueStats
{
    public int TotalSearches;
    public float AverageWaitTime;
    public float SuccessRate;
}
