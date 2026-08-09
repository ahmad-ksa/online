using System;
using System.Threading.Tasks;

/// <summary>
/// واجهة مدير الشبكة - لتحسين القابلية للاختبار والمرونة
/// </summary>
public interface INetworkManager
{
    /// <summary>
    /// الاتصال بالسيرفر
    /// </summary>
    Task<bool> ConnectToServer(string customHost = null, int customPort = -1);

    /// <summary>
    /// مصادقة اللاعب
    /// </summary>
    Task<bool> AuthenticatePlayer(string email, string password);

    /// <summary>
    /// تسجيل لاعب جديد
    /// </summary>
    Task<bool> RegisterPlayer(string email, string password, string username);

    /// <summary>
    /// قطع الاتصال
    /// </summary>
    void Disconnect();

    /// <summary>
    /// إرسال رسالة إلى السيرفر
    /// </summary>
    Task<bool> SendMessage(NetworkMessage message);

    /// <summary>
    /// الحصول على حالة الاتصال
    /// </summary>
    NetworkManager.NetworkState CurrentState { get; }

    /// <summary>
    /// التحقق من الاتصال
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// التحقق من المصادقة
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// الحصول على session token
    /// </summary>
    string SessionToken { get; }

    /// <summary>
    /// حدث تغيير حالة الشبكة
    /// </summary>
    event Action<NetworkManager.NetworkState> OnNetworkStateChanged;

    /// <summary>
    /// حدث خطأ في الاتصال
    /// </summary>
    event Action<string> OnConnectionError;

    /// <summary>
    /// حدث المصادقة
    /// </summary>
    event Action OnAuthenticated;

    /// <summary>
    /// حدث قطع الاتصال
    /// </summary>
    event Action OnDisconnected;
}
