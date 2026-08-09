using UnityEngine;

/// <summary>
/// مجموعة من المتغيرات العامة - أونلاين فقط
/// </summary>
public static class GameConstants
{
    // حالات الاتصال
    public const string STATUS_CONNECTING = "جاري الاتصال...";
    public const string STATUS_CONNECTED = "متصل ✓";
    public const string STATUS_DISCONNECTED = "غير متصل ✗";
    public const string STATUS_ERROR = "خطأ في الاتصال!";

    // رسائل الأخطاء
    public const string ERROR_NO_INTERNET = "لا يوجد اتصال إنترنت!";
    public const string ERROR_INVALID_CREDENTIALS = "بيانات دخول غير صحيحة";
    public const string ERROR_USER_NOT_FOUND = "المستخدم غير موجود";
    public const string ERROR_SERVER_OFFLINE = "السيرفر غير متاح حالياً";
    public const string ERROR_GAME_FULL = "اللعبة ممتلئة";

    // إعدادات الشبكة
    public const float NETWORK_TICK_RATE = 0.1f; // 100ms
    public const float POSITION_SYNC_DISTANCE = 0.5f; // متر
    public const float ROTATION_SYNC_ANGLE = 2f; // درجة

    // إعدادات الصوت
    public const float PROXIMITY_RANGE_MIN = 5f;
    public const float PROXIMITY_RANGE_DEFAULT = 20f;
    public const float PROXIMITY_RANGE_MAX = 100f;

    // الحد الأقصى للرسائل
    public const int MAX_MESSAGE_LENGTH = 255;
    public const int MAX_USERNAME_LENGTH = 32;
    public const int MAX_PLAYERS = 100;
}
