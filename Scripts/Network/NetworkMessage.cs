using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// رسالة الشبكة - تنقل البيانات بين اللاعبين
/// </summary>
[System.Serializable]
public class NetworkMessage
{
    [SerializeField]
    public string messageType; // نوع الرسالة (LOGIN, MOVE, CHAT, إلخ)

    [SerializeField]
    public Dictionary<string, object> data; // البيانات

    [SerializeField]
    public string playerId; // معرف اللاعب المرسل

    [SerializeField]
    public long timestamp; // الوقت

    /// <summary>
    /// Constructor
    /// </summary>
    public NetworkMessage()
    {
        messageType = "";
        data = new Dictionary<string, object>();
        playerId = "";
        timestamp = System.DateTime.UtcNow.Ticks;
    }

    /// <summary>
    /// Constructor مع البيانات
    /// </summary>
    public NetworkMessage(string type, string id, Dictionary<string, object> messageData = null)
    {
        messageType = type;
        playerId = id;
        data = messageData ?? new Dictionary<string, object>();
        timestamp = System.DateTime.UtcNow.Ticks;
    }

    /// <summary>
    /// تحويل إلى JSON
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    /// <summary>
    /// من JSON
    /// </summary>
    public static NetworkMessage FromJson(string json)
    {
        try
        {
            return JsonUtility.FromJson<NetworkMessage>(json);
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to deserialize NetworkMessage: {ex.Message}", "NetworkMessage");
            return new NetworkMessage();
        }
    }

    /// <summary>
    /// طباعة الرسالة (للـ Debug)
    /// </summary>
    public override string ToString()
    {
        return $"NetworkMessage[Type={messageType}, PlayerId={playerId}, DataCount={data.Count}]";
    }

    /// <summary>
    /// الحصول على قيمة من البيانات
    /// </summary>
    public T GetData<T>(string key, T defaultValue = default)
    {
        if (data.ContainsKey(key))
        {
            try
            {
                return (T)data[key];
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// تعيين قيمة في البيانات
    /// </summary>
    public void SetData<T>(string key, T value)
    {
        data[key] = value;
    }

    /// <summary>
    /// التحقق من وجود مفتاح
    /// </summary>
    public bool HasData(string key)
    {
        return data.ContainsKey(key);
    }
}

/// <summary>
/// أنواع الرسائل المختلفة
/// </summary>
public static class NetworkMessageType
{
    public const string LOGIN = "LOGIN";
    public const string LOGOUT = "LOGOUT";
    public const string PLAYER_JOIN = "PLAYER_JOIN";
    public const string PLAYER_LEAVE = "PLAYER_LEAVE";
    public const string PLAYER_MOVE = "PLAYER_MOVE";
    public const string PLAYER_ACTION = "PLAYER_ACTION";
    public const string CHAT_MESSAGE = "CHAT_MESSAGE";
    public const string MATCH_STATE_SYNC = "MATCH_STATE_SYNC";
    public const string MATCH_START = "MATCH_START";
    public const string MATCH_END = "MATCH_END";
    public const string VOICE_DATA = "VOICE_DATA";
    public const string FRIEND_REQUEST = "FRIEND_REQUEST";
    public const string FRIEND_RESPONSE = "FRIEND_RESPONSE";
    public const string GAME_EVENT = "GAME_EVENT";
    public const string HEALTH_UPDATE = "HEALTH_UPDATE";
    public const string SPAWN = "SPAWN";
    public const string KILL = "KILL";
}
