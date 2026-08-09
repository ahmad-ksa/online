using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// نظام logging متقدم مع حفظ في ملف
/// </summary>
public class Logger : MonoBehaviour
{
    private static Logger instance;
    private List<LogEntry> logHistory = new List<LogEntry>();
    private string logFilePath;
    private int maxLogEntries = 1000;

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    [System.Serializable]
    private class LogEntry
    {
        public DateTime timestamp;
        public LogLevel level;
        public string message;
        public string stackTrace;

        public override string ToString()
        {
            return $"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // إعداد مسار ملف الـ log
        string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        logFilePath = Path.Combine(logDirectory, $"game_log_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
    }

    /// <summary>
    /// تسجيل رسالة معلومات
    /// </summary>
    public static void Log(string message, string context = "")
    {
        LogMessage(LogLevel.Info, message, context);
    }

    /// <summary>
    /// تسجيل رسالة تنبيه
    /// </summary>
    public static void LogWarning(string message, string context = "")
    {
        LogMessage(LogLevel.Warning, message, context);
    }

    /// <summary>
    /// تسجيل رسالة خطأ
    /// </summary>
    public static void LogError(string message, string context = "")
    {
        LogMessage(LogLevel.Error, message, context);
    }

    /// <summary>
    /// تسجيل رسالة خطأ حرج
    /// </summary>
    public static void LogCritical(string message, Exception ex = null, string context = "")
    {
        string fullMessage = ex != null 
            ? $"{message}\n{ex.Message}\n{ex.StackTrace}" 
            : message;
        
        LogMessage(LogLevel.Critical, fullMessage, context);
    }

    /// <summary>
    /// تسجيل رسالة debug
    /// </summary>
    public static void LogDebug(string message, string context = "")
    {
        #if UNITY_EDITOR || DEBUG
        LogMessage(LogLevel.Debug, message, context);
        #endif
    }

    /// <summary>
    /// تسجيل رسالة داخلية
    /// </summary>
    private static void LogMessage(LogLevel level, string message, string context)
    {
        if (instance == null) return;

        // إضافة السياق
        if (!string.IsNullOrEmpty(context))
        {
            message = $"[{context}] {message}";
        }

        LogEntry entry = new LogEntry
        {
            timestamp = DateTime.Now,
            level = level,
            message = message,
            stackTrace = level >= LogLevel.Warning ? StackTraceUtility.ExtractStackTrace() : ""
        };

        instance.logHistory.Add(entry);

        // حذف الرسائل القديمة
        if (instance.logHistory.Count > instance.maxLogEntries)
        {
            instance.logHistory.RemoveAt(0);
        }

        // طباعة في Console
        PrintToConsole(level, entry.ToString());

        // كتابة في الملف
        instance.WriteToFile(entry.ToString());
    }

    /// <summary>
    /// طباعة في Unity Console
    /// </summary>
    private static void PrintToConsole(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Debug:
                Debug.Log($"<color=cyan>{message}</color>");
                break;
            case LogLevel.Info:
                Debug.Log($"<color=white>{message}</color>");
                break;
            case LogLevel.Warning:
                Debug.LogWarning($"<color=yellow>{message}</color>");
                break;
            case LogLevel.Error:
                Debug.LogError($"<color=red>{message}</color>");
                break;
            case LogLevel.Critical:
                Debug.LogError($"<color=magenta>{message}</color>");
                break;
        }
    }

    /// <summary>
    /// كتابة الرسالة في الملف
    /// </summary>
    private void WriteToFile(string message)
    {
        try
        {
            File.AppendAllText(logFilePath, message + "\n");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"فشل كتابة الـ log في الملف: {ex.Message}");
        }
    }

    /// <summary>
    /// الحصول على سجل الـ logs
    /// </summary>
    public static List<string> GetLogHistory(int count = 100)
    {
        if (instance == null) return new List<string>();

        List<string> result = new List<string>();
        int startIndex = Mathf.Max(0, instance.logHistory.Count - count);

        for (int i = startIndex; i < instance.logHistory.Count; i++)
        {
            result.Add(instance.logHistory[i].ToString());
        }

        return result;
    }

    /// <summary>
    /// مسح السجل
    /// </summary>
    public static void ClearHistory()
    {
        if (instance != null)
        {
            instance.logHistory.Clear();
        }
    }

    /// <summary>
    /// الحصول على مسار ملف الـ log
    /// </summary>
    public static string GetLogFilePath()
    {
        return instance != null ? instance.logFilePath : "";
    }
}
