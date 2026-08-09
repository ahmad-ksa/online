using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// مدير التحليلات - يتعامل مع جمع وتحليل بيانات اللعب
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    private static AnalyticsManager instance;

    // Game Events Tracking
    private List<GameEvent> gameEvents = new List<GameEvent>();
    private List<SessionData> sessions = new List<SessionData>();

    // Current Session
    private SessionData currentSession = null;
    private DateTime sessionStartTime = DateTime.UtcNow;

    // Performance Metrics
    private PerformanceMetrics performanceMetrics = new PerformanceMetrics();

    // User Behavior
    private Dictionary<string, PlayerBehavior> playerBehavior = new Dictionary<string, PlayerBehavior>();

    // Events
    public static event Action<GameEvent> OnEventLogged;
    public static event Action<SessionData> OnSessionEnded;
    public static event Action<PerformanceMetrics> OnMetricsCollected;

    // Storage Keys
    private const string EVENTS_KEY = "AnalyticsEvents";
    private const string SESSIONS_KEY = "AnalyticsSessions";
    private const string BEHAVIOR_KEY = "PlayerBehavior";

    private bool isInitialized = false;

    // FPS smoothing
    private const float fpsSmoothing = 0.1f;
    private float averageFps = 0f;

    // Dropped frames calculation
    [SerializeField]
    private int targetFPS = 60;
    private int droppedCount = 0;
    private float droppedAccumulator = 0f;

    // Network latency smoothing
    private float lastNetworkLatency = 0f;
    private float latencyAvg = 0f;
    private const float latencySmoothing = 0.1f;

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
        CollectPerformanceMetrics();
    }

    private void OnApplicationQuit()
    {
        EndSession();
    }

    /// <summary>
    /// تهيئة مدير التحليلات
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadAnalyticsLocal();
        StartSession();

        isInitialized = true;

        Debug.Log("AnalyticsManager Initialized");
    }

    /// <summary>
    /// بدء جلسة لعبة جديدة
    /// </summary>
    private void StartSession()
    {
        currentSession = new SessionData
        {
            SessionId = System.Guid.NewGuid().ToString(),
            PlayerId = PlayerManager.Instance?.CurrentProfile?.playerId ?? "unknown",
            StartTime = DateTime.UtcNow.ToString("o"),
            EndTime = "", // empty until ended
            Duration = 0,
            GameMode = "unknown",
            Map = "unknown"
        };

        sessionStartTime = DateTime.UtcNow;

        Debug.Log($"Session started: {currentSession.SessionId}");
    }

    /// <summary>
    /// إنهاء الجلسة الحالية
    /// </summary>
    public void EndSession()
    {
        if (currentSession == null)
            return;

        DateTime endDt = DateTime.UtcNow;
        currentSession.EndTime = endDt.ToString("o");

        DateTime startDt;
        if (!DateTime.TryParse(currentSession.StartTime, out startDt))
        {
            startDt = sessionStartTime;
        }

        currentSession.Duration = (endDt - startDt).TotalSeconds;

        sessions.Add(currentSession);

        // Update player behavior totals for this session
        try
        {
            var pid = currentSession.PlayerId ?? "unknown";
            if (!playerBehavior.ContainsKey(pid))
            {
                playerBehavior[pid] = new PlayerBehavior
                {
                    PlayerId = pid,
                    SessionCount = 1,
                    TotalPlayTime = currentSession.Duration,
                    TotalKills = 0,
                    TotalDeaths = 0,
                    Achievements = 0,
                    Purchases = 0,
                    LevelUps = 0
                };
            }
            else
            {
                var beh = playerBehavior[pid];
                beh.SessionCount += 1;
                beh.TotalPlayTime += currentSession.Duration;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to update player behavior on session end: {ex.Message}");
        }

        SaveAnalyticsLocal();

        OnSessionEnded?.Invoke(currentSession);

        Debug.Log($"Session ended: Duration {currentSession.Duration}s");
    }

    /// <summary>
    /// تسجيل حدث
    /// </summary>
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        try
        {
            var gameEvent = new GameEvent
            {
                EventId = System.Guid.NewGuid().ToString(),
                EventName = eventName,
                PlayerId = PlayerManager.Instance?.CurrentProfile?.playerId ?? "unknown",
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            if (parameters != null)
            {
                gameEvent.SetParameters(parameters);
            }
            else
            {
                gameEvent.parameters = new List<SerializableParameter>();
            }

            gameEvents.Add(gameEvent);
            OnEventLogged?.Invoke(gameEvent);

            Debug.Log($"Event logged: {eventName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to log event: {ex.Message}");
        }
    }

    /// <summary>
    /// تسجيل قتل
    /// </summary>
    public void LogKill(string killerId, string victimId, string weapon)
    {
        var parameters = new Dictionary<string, object>
        {
            { "killer_id", killerId },
            { "victim_id", victimId },
            { "weapon", weapon },
            { "timestamp", DateTime.UtcNow.ToString("o") }
        };

        LogEvent("player_kill", parameters);

        // تحديث سلوك اللاعب
        UpdatePlayerBehavior(killerId, "kills", 1);
    }

    /// <summary>
    /// تسجيل موت
    /// </summary>
    public void LogDeath(string playerId, string killerName)
    {
        var parameters = new Dictionary<string, object>
        {
            { "player_id", playerId },
            { "killer_name", killerName },
            { "timestamp", DateTime.UtcNow.ToString("o") }
        };

        LogEvent("player_death", parameters);

        // تحديث سلوك اللاعب
        UpdatePlayerBehavior(playerId, "deaths", 1);
    }

    /// <summary>
    /// تسجيل مشتريات
    /// </summary>
    public void LogPurchase(string playerId, string itemId, int cost, string currency)
    {
        var parameters = new Dictionary<string, object>
        {
            { "item_id", itemId },
            { "cost", cost },
            { "currency", currency },
            { "timestamp", DateTime.UtcNow.ToString("o") }
        };

        LogEvent("purchase", parameters);

        // تحديث السلوك
        UpdatePlayerBehavior(playerId, "purchases", 1);
    }

    /// <summary>
    /// تسجيل مستوى جديد
    /// </summary>
    public void LogLevelUp(string playerId, int newLevel)
    {
        var parameters = new Dictionary<string, object>
        {
            { "new_level", newLevel },
            { "timestamp", DateTime.UtcNow.ToString("o") }
        };

        LogEvent("level_up", parameters);

        UpdatePlayerBehavior(playerId, "level_ups", 1);
    }

    /// <summary>
    /// تسجيل إنجاز
    /// </summary>
    public void LogAchievementUnlocked(string playerId, string achievementId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "achievement_id", achievementId },
            { "timestamp", DateTime.UtcNow.ToString("o") }
        };

        LogEvent("achievement_unlocked", parameters);

        UpdatePlayerBehavior(playerId, "achievements", 1);
    }

    /// <summary>
    /// تحديث سلوك اللاعب
    /// </summary>
    private void UpdatePlayerBehavior(string playerId, string metric, int value)
    {
        if (string.IsNullOrEmpty(playerId))
            playerId = "unknown";

        if (!playerBehavior.ContainsKey(playerId))
        {
            playerBehavior[playerId] = new PlayerBehavior
            {
                PlayerId = playerId,
                SessionCount = 0,
                TotalPlayTime = 0,
                TotalKills = 0,
                TotalDeaths = 0,
                Achievements = 0,
                Purchases = 0,
                LevelUps = 0
            };
        }

        var behavior = playerBehavior[playerId];

        switch (metric)
        {
            case "kills":
                behavior.TotalKills += value;
                break;
            case "deaths":
                behavior.TotalDeaths += value;
                break;
            case "achievements":
                behavior.Achievements += value;
                break;
            case "purchases":
                behavior.Purchases += value;
                break;
            case "level_ups":
                behavior.LevelUps += value;
                break;
        }
    }

    /// <summary>
    /// جمع مقاييس الأداء
    /// </summary>
    private void CollectPerformanceMetrics()
    {
        // تجنب قسمة على صفر عند deltaTime صغير جداً
        float delta = Time.deltaTime;
        if (delta <= 0f)
            delta = 0.0001f;

        int currentFps = Mathf.Clamp((int)(1f / delta), 0, 10000);
        performanceMetrics.CurrentFPS = currentFps;

        // تنعيم للأوسط
        if (averageFps <= 0f)
            averageFps = currentFps;
        averageFps = Mathf.Lerp(averageFps, currentFps, fpsSmoothing);
        performanceMetrics.AverageFPS = Mathf.RoundToInt(averageFps);

        performanceMetrics.MemoryUsage = SystemInfo.systemMemorySize;
        performanceMetrics.GraphicsMemory = SystemInfo.graphicsMemorySize;

        // Dropped frames calculation: accumulator method
        float expected = 1f / Mathf.Max(1, targetFPS);
        if (delta > expected)
        {
            droppedAccumulator += (delta - expected);
            int droppedNow = Mathf.FloorToInt(droppedAccumulator / expected);
            if (droppedNow > 0)
            {
                droppedCount += droppedNow;
                droppedAccumulator -= droppedNow * expected;
            }
        }
        performanceMetrics.DroppedFrames = droppedCount;

        // Network latency: use last measured averaged value
        performanceMetrics.NetworkLatency = latencyAvg;

        OnMetricsCollected?.Invoke(performanceMetrics);
    }

    /// <summary>
    /// يتيح لطبقة الشبكة إعطاء قياس RTT/latency للـ AnalyticsManager
    /// استدعِ هذا من NetworkManager أو طبقة الشبكة عند حصول قياس جديد
    /// </summary>
    public void SetNetworkLatency(float latencyMs)
    {
        lastNetworkLatency = latencyMs;
        if (latencyAvg <= 0f)
            latencyAvg = latencyMs;
        latencyAvg = Mathf.Lerp(latencyAvg, latencyMs, latencySmoothing);
        performanceMetrics.NetworkLatency = latencyAvg;
    }

    /// <summary>
    /// تعيين هدف FPS (اختياري)
    /// </summary>
    public void SetTargetFPS(int fps)
    {
        if (fps <= 0) return;
        targetFPS = fps;
    }

    /// <summary>
    /// الحصول على إحصائيات الجلسة
    /// </summary>
    public SessionStats GetSessionStats()
    {
        return new SessionStats
        {
            TotalSessions = sessions.Count,
            AverageSessionDuration = sessions.Count > 0
                ? sessions.Average(s => s.Duration)
                : 0,
            TotalPlayTime = sessions.Sum(s => s.Duration),
            LongestSession = sessions.Count > 0
                ? sessions.Max(s => s.Duration)
                : 0
        };
    }

    /// <summary>
    /// الحصول على إحصائيات الأحداث
    /// </summary>
    public EventStats GetEventStats()
    {
        return new EventStats
        {
            TotalEvents = gameEvents.Count,
            KillEvents = gameEvents.Count(e => e.EventName == "player_kill"),
            DeathEvents = gameEvents.Count(e => e.EventName == "player_death"),
            PurchaseEvents = gameEvents.Count(e => e.EventName == "purchase"),
            AchievementEvents = gameEvents.Count(e => e.EventName == "achievement_unlocked")
        };
    }

    /// <summary>
    /// الحصول على سلوك اللاعب
    /// </summary>
    public PlayerBehavior GetPlayerBehavior(string playerId)
    {
        if (playerBehavior.ContainsKey(playerId))
            return playerBehavior[playerId];

        return null;
    }

    /// <summary>
    /// الحصول على مقاييس الأداء
    /// </summary>
    public PerformanceMetrics GetPerformanceMetrics()
    {
        return performanceMetrics;
    }

    /// <summary>
    /// حفظ البيانات محلياً
    /// </summary>
    private void SaveAnalyticsLocal()
    {
        try
        {
            string eventsJson = JsonUtility.ToJson(new GameEventList { events = gameEvents });
            PlayerPrefs.SetString(EVENTS_KEY, eventsJson);

            string sessionsJson = JsonUtility.ToJson(new SessionDataList { sessions = sessions });
            PlayerPrefs.SetString(SESSIONS_KEY, sessionsJson);

            string behaviorJson = JsonUtility.ToJson(new PlayerBehaviorList { behaviors = playerBehavior.Values.ToList() });
            PlayerPrefs.SetString(BEHAVIOR_KEY, behaviorJson);

            PlayerPrefs.Save();

            Debug.Log("Analytics data saved");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save analytics: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل البيانات محلياً
    /// </summary>
    private void LoadAnalyticsLocal()
    {
        try
        {
            string eventsJson = PlayerPrefs.GetString(EVENTS_KEY, "");
            if (!string.IsNullOrEmpty(eventsJson))
            {
                var eventList = JsonUtility.FromJson<GameEventList>(eventsJson);
                gameEvents = eventList?.events ?? new List<GameEvent>();
            }

            string sessionsJson = PlayerPrefs.GetString(SESSIONS_KEY, "");
            if (!string.IsNullOrEmpty(sessionsJson))
            {
                var sessionList = JsonUtility.FromJson<SessionDataList>(sessionsJson);
                sessions = sessionList?.sessions ?? new List<SessionData>();
            }

            string behaviorJson = PlayerPrefs.GetString(BEHAVIOR_KEY, "");
            if (!string.IsNullOrEmpty(behaviorJson))
            {
                var behaviorList = JsonUtility.FromJson<PlayerBehaviorList>(behaviorJson);
                playerBehavior.Clear();
                if (behaviorList?.behaviors != null)
                {
                    foreach (var behavior in behaviorList.behaviors)
                    {
                        playerBehavior[behavior.PlayerId] = behavior;
                    }
                }
            }

            Debug.Log("Analytics data loaded");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load analytics: {ex.Message}");
        }
    }

    // Getters
    public static AnalyticsManager Instance => instance;
    public List<GameEvent> GameEvents => gameEvents;
    public List<SessionData> Sessions => sessions;
}

// ==================== Data Classes ====================

[Serializable]
public enum ParamType
{
    String = 0,
    Int = 1,
    Float = 2,
    Bool = 3
}

[Serializable]
public class SerializableParameter
{
    public string Key;
    public ParamType Type = ParamType.String;

    // Values for each supported type (JsonUtility serializes public fields)
    public string StringValue;
    public int IntValue;
    public float FloatValue;
    public bool BoolValue;

    public void SetValue(object v)
    {
        if (v == null)
        {
            Type = ParamType.String;
            StringValue = "";
            return;
        }

        if (v is int i)
        {
            Type = ParamType.Int;
            IntValue = i;
        }
        else if (v is float f)
        {
            Type = ParamType.Float;
            FloatValue = f;
        }
        else if (v is double d) // convert to float (may lose precision)
        {
            Type = ParamType.Float;
            FloatValue = (float)d;
        }
        else if (v is long l) // store long as string to avoid overflow issues in int field
        {
            Type = ParamType.String;
            StringValue = l.ToString();
        }
        else if (v is bool b)
        {
            Type = ParamType.Bool;
            BoolValue = b;
        }
        else
        {
            // fallback to string representation
            Type = ParamType.String;
            StringValue = v.ToString();
        }
    }

    public object GetValue()
    {
        switch (Type)
        {
            case ParamType.Int:
                return IntValue;
            case ParamType.Float:
                return FloatValue;
            case ParamType.Bool:
                return BoolValue;
            case ParamType.String:
            default:
                return StringValue;
        }
    }
}

[Serializable]
public class GameEvent
{
    public string EventId;
    public string EventName;
    public string PlayerId;

    // Timestamp stored as ISO 8601 string
    public string Timestamp;

    // Serializable parameters (JsonUtility-friendly)
    public List<SerializableParameter> parameters = new List<SerializableParameter>();

    // Helper: get parameters as runtime dictionary with types preserved
    public Dictionary<string, object> Parameters
    {
        get
        {
            if (parameters == null)
                return new Dictionary<string, object>();
            return parameters.ToDictionary(p => p.Key, p => p.GetValue());
        }
    }

    // Set parameters from a runtime dictionary (retains types)
    public void SetParameters(Dictionary<string, object> dict)
    {
        if (dict == null)
        {
            parameters = new List<SerializableParameter>();
            return;
        }

        var list = new List<SerializableParameter>(dict.Count);
        foreach (var kv in dict)
        {
            var sp = new SerializableParameter { Key = kv.Key };
            sp.SetValue(kv.Value);
            list.Add(sp);
        }
        parameters = list;
    }
}

[Serializable]
public class SessionData
{
    public string SessionId;
    public string PlayerId;

    // Start/End times as ISO 8601 strings
    public string StartTime;
    public string EndTime;
    public double Duration;
    public string GameMode;
    public string Map;
}

[Serializable]
public class PlayerBehavior
{
    public string PlayerId;
    public int SessionCount;
    public double TotalPlayTime;
    public int TotalKills;
    public int TotalDeaths;
    public int Achievements;
    public int Purchases;
    public int LevelUps;
}

[Serializable]
public class PerformanceMetrics
{
    public int CurrentFPS;
    public int AverageFPS;
    public int MemoryUsage;
    public int GraphicsMemory;
    public float NetworkLatency;
    public int DroppedFrames;
}

[Serializable]
public class SessionStats
{
    public int TotalSessions;
    public double AverageSessionDuration;
    public double TotalPlayTime;
    public double LongestSession;
}

[Serializable]
public class EventStats
{
    public int TotalEvents;
    public int KillEvents;
    public int DeathEvents;
    public int PurchaseEvents;
    public int AchievementEvents;
}

[Serializable]
public class GameEventList
{
    public List<GameEvent> events = new List<GameEvent>();
}

[Serializable]
public class SessionDataList
{
    public List<SessionData> sessions = new List<SessionData>();
}

[Serializable]
public class PlayerBehaviorList
{
    public List<PlayerBehavior> behaviors = new List<PlayerBehavior>();
}