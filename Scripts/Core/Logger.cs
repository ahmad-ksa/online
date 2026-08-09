using UnityEngine;
using System;

/// <summary>
/// نظام تسجيل الأخطاء والرسائل (Logging System)
/// </summary>
public static class Logger
{
    private static bool debugMode = true;
    private const string LOG_PREFIX = "[GAME]";

    /// <summary>
    /// رسالة عادية
    /// </summary>
    public static void Log(string message, string context = "")
    {
        if (!debugMode) return;
        string log = FormatMessage(message, context);
        Debug.Log(log);
    }

    /// <summary>
    /// تحذير
    /// </summary>
    public static void LogWarning(string message, string context = "")
    {
        string log = FormatMessage(message, context);
        Debug.LogWarning(log);
    }

    /// <summary>
    /// خطأ
    /// </summary>
    public static void LogError(string message, string context = "")
    {
        string log = FormatMessage(message, context);
        Debug.LogError(log);
    }

    /// <summary>
    /// خطأ حرج (Critical)
    /// </summary>
    public static void LogCritical(string message, Exception ex, string context = "")
    {
        string log = FormatMessage($"{message}: {ex.Message}\n{ex.StackTrace}", context);
        Debug.LogError(log);
    }

    /// <summary>
    /// رسالة Debug فقط (تختفي في Build)
    /// </summary>
    public static void LogDebug(string message, string context = "")
    {
#if UNITY_EDITOR
        if (!debugMode) return;
        string log = FormatMessage($"[DEBUG] {message}", context);
        Debug.Log(log);
#endif
    }

    /// <summary>
    /// تنسيق الرسالة
    /// </summary>
    private static string FormatMessage(string message, string context)
    {
        string contextStr = string.IsNullOrEmpty(context) ? "" : $" [{context}]";
        return $"{LOG_PREFIX}{contextStr} {message}";
    }

    /// <summary>
    /// تفعيل/تعطيل Debug Mode
    /// </summary>
    public static void SetDebugMode(bool enabled)
    {
        debugMode = enabled;
    }

    /// <summary>
    /// الحصول على حالة Debug Mode
    /// </summary>
    public static bool GetDebugMode() => debugMode;
}
