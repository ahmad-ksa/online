using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مراقب الأعطال - يتعامل مع التقاط والإبلاغ عن الأخطاء والأعطال
/// </summary>
public class CrashReporter : MonoBehaviour
{
    private static CrashReporter instance;

    public enum ErrorSeverity
    {
        Info,       // معلومات
        Warning,    // تحذير
        Error,      // خطأ
        Critical    // حرج جداً
    }

    private List<CrashLog> crashLogs = new List<CrashLog>();
    private List<WarningLog> warningLogs = new List<WarningLog>();
    private int errorCount = 0;
    private int warningCount = 0;

    // Events
    public static event Action<CrashLog> OnCrashLogged;
    public static event Action<WarningLog> OnWarningLogged;
    public static event Action<int> OnErrorCountChanged;

    // Storage Keys
    private const string CRASH_LOGS_KEY = "CrashLogs";
    private const string WARNING_LOGS_KEY = "WarningLogs";

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

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void OnApplicationCrash()
    {
        Debug.LogError("Application crashed!");
        SaveCrashLogsLocal();
    }

    /// <summary>
    /// تهيئة مراقب الأعطال
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadCrashLogsLocal();
        LoadWarningLogsLocal();

        isInitialized = true;

        Debug.Log("CrashReporter Initialized");
    }

    /// <summary>
    /// معالج السجلات التلقائي
    /// </summary>
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
                LogError(logString, stackTrace);
                break;

            case LogType.Warning:
                LogWarning(logString, stackTrace);
                break;

            case LogType.Exception:
                LogCrash(logString, stackTrace, ErrorSeverity.Critical);
                break;

            case LogType.Log:
                LogInfo(logString);
                break;
        }
    }

    /// <summary>
    /// تسجيل خطأ
    /// </summary>
    public void LogError(string message, string stackTrace = "")
    {
        var crashLog = new CrashLog
        {
            LogId = System.Guid.NewGuid().ToString(),
            Message = message,
            StackTrace = stackTrace,
            Severity = ErrorSeverity.Error,
            Timestamp = DateTime.Now,
            PlayerId = PlayerManager.Instance?.CurrentProfile.playerId ?? "unknown",
            DeviceInfo = GetDeviceInfo()
        };

        crashLogs.Add(crashLog);
        errorCount++;

        OnCrashLogged?.Invoke(crashLog);
        OnErrorCountChanged?.Invoke(errorCount);

        SaveCrashLogsLocal();

        Debug.LogError($"Error logged: {message}");
    }

    /// <summary>
    /// تسجيل تحذير
    /// </summary>
    public void LogWarning(string message, string stackTrace = "")
    {
        var warningLog = new WarningLog
        {
            LogId = System.Guid.NewGuid().ToString(),
            Message = message,
            StackTrace = stackTrace,
            Timestamp = DateTime.Now,
            PlayerId = PlayerManager.Instance?.CurrentProfile.playerId ?? "unknown"
        };

        warningLogs.Add(warningLog);
        warningCount++;

        OnWarningLogged?.Invoke(warningLog);

        SaveWarningLogsLocal();

        Debug.LogWarning($"Warning logged: {message}");
    }

    /// <summary>
    /// تسجيل معلومات
    /// </summary>
    public void LogInfo(string message)
    {
        Debug.Log($"Info: {message}");
    }

    /// <summary>
    /// تسجيل عطل حرج
    /// </summary>
    public void LogCrash(string message, string stackTrace, ErrorSeverity severity)
    {
        var crashLog = new CrashLog
        {
            LogId = System.Guid.NewGuid().ToString(),
            Message = message,
            StackTrace = stackTrace,
            Severity = severity,
            Timestamp = DateTime.Now,
            PlayerId = PlayerManager.Instance?.CurrentProfile.playerId ?? "unknown",
            DeviceInfo = GetDeviceInfo()
        };

        crashLogs.Add(crashLog);

        if (severity == ErrorSeverity.Critical)
        {
            errorCount++;
            OnErrorCountChanged?.Invoke(errorCount);
        }

        OnCrashLogged?.Invoke(crashLog);

        SaveCrashLogsLocal();

        Debug.LogError($"CRASH LOGGED [{severity}]: {message}\n{stackTrace}");
    }

    /// <summary>
    /// الحصول على معلومات الجهاز
    /// </summary>
    private string GetDeviceInfo()
    {
        return $"Device: {SystemInfo.deviceName} | OS: {SystemInfo.operatingSystem} | " +
               $"RAM: {SystemInfo.systemMemorySize}MB | GPU: {SystemInfo.graphicsDeviceName}";
    }

    /// <summary>
    /// الحصول على جميع سجلات الأعطال
    /// </summary>
    public List<CrashLog> GetCrashLogs()
    {
        return new List<CrashLog>(crashLogs);
    }

    /// <summary>
    /// الحصول على سجلات التحذيرات
    /// </summary>
    public List<WarningLog> GetWarningLogs()
    {
        return new List<WarningLog>(warningLogs);
    }

    /// <summary>
    /// الحصول على آخر الأخطاء
    /// </summary>
    public List<CrashLog> GetRecentCrashes(int count = 10)
    {
        return crashLogs.OrderByDescending(c => c.Timestamp).Take(count).ToList();
    }

    /// <summary>
    /// الحصول على إحصائيات الأخطاء
    /// </summary>
    public ErrorStats GetErrorStats()
    {
        return new ErrorStats
        {
            TotalErrors = errorCount,
            TotalWarnings = warningCount,
            CriticalErrors = crashLogs.Count(c => c.Severity == ErrorSeverity.Critical),
            ErrorsLastHour = crashLogs.Count(c => (DateTime.Now - c.Timestamp).TotalHours < 1),
            MostCommonError = GetMostCommonError()
        };
    }

    /// <summary>
    /// الحصول على أكثر خطأ شيوعاً
    /// </summary>
    private string GetMostCommonError()
    {
        if (crashLogs.Count == 0)
            return "None";

        var grouped = crashLogs.GroupBy(c => c.Message)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return grouped?.Key ?? "None";
    }

    /// <summary>
    /// مسح السجلات
    /// </summary>
    public void ClearLogs()
    {
        crashLogs.Clear();
        warningLogs.Clear();
        errorCount = 0;
        warningCount = 0;

        PlayerPrefs.DeleteKey(CRASH_LOGS_KEY);
        PlayerPrefs.DeleteKey(WARNING_LOGS_KEY);
        PlayerPrefs.Save();

        Debug.Log("All logs cleared");
    }

    /// <summary>
    /// حفظ سجلات الأعطال محلياً
    /// </summary>
    private void SaveCrashLogsLocal()
    {
        try
        {
            string json = JsonUtility.ToJson(new CrashLogList { logs = crashLogs });
            PlayerPrefs.SetString(CRASH_LOGS_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save crash logs: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل سجلات الأعطال محلياً
    /// </summary>
    private void LoadCrashLogsLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(CRASH_LOGS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var logList = JsonUtility.FromJson<CrashLogList>(json);
                crashLogs = logList.logs ?? new List<CrashLog>();

                Debug.Log($"Crash logs loaded ({crashLogs.Count} logs)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load crash logs: {ex.Message}");
        }
    }

    /// <summary>
    /// حفظ سجلات التحذيرات محلياً
    /// </summary>
    private void SaveWarningLogsLocal()
    {
        try
        {
            string json = JsonUtility.ToJson(new WarningLogList { logs = warningLogs });
            PlayerPrefs.SetString(WARNING_LOGS_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save warning logs: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل سجلات التحذيرات محلياً
    /// </summary>
    private void LoadWarningLogsLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(WARNING_LOGS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var logList = JsonUtility.FromJson<WarningLogList>(json);
                warningLogs = logList.logs ?? new List<WarningLog>();

                Debug.Log($"Warning logs loaded ({warningLogs.Count} logs)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load warning logs: {ex.Message}");
        }
    }

    // Getters
    public static CrashReporter Instance => instance;
    public int ErrorCount => errorCount;
    public int WarningCount => warningCount;
}

// ==================== Data Classes ====================

[System.Serializable]
public class CrashLog
{
    public string LogId;
    public string Message;
    public string StackTrace;
    public CrashReporter.ErrorSeverity Severity;
    public DateTime Timestamp;
    public string PlayerId;
    public string DeviceInfo;
}

[System.Serializable]
public class WarningLog
{
    public string LogId;
    public string Message;
    public string StackTrace;
    public DateTime Timestamp;
    public string PlayerId;
}

[System.Serializable]
public class ErrorStats
{
    public int TotalErrors;
    public int TotalWarnings;
    public int CriticalErrors;
    public int ErrorsLastHour;
    public string MostCommonError;
}

[System.Serializable]
public class CrashLogList
{
    public List<CrashLog> logs = new List<CrashLog>();
}

[System.Serializable]
public class WarningLogList
{
    public List<WarningLog> logs = new List<WarningLog>();
}
