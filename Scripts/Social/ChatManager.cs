using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// نظام إدارة الدردشة (عام وخاص)
/// </summary>
public class ChatManager : MonoBehaviour
{
    private static ChatManager instance;

    [System.Serializable]
    public class ChatMessage
    {
        public string messageId;
        public string senderId;
        public string senderName;
        public string content;
        public DateTime timestamp;
        public string channelId; // "global" أو friendId
        public bool isLocal; // رسالتي أم رسالة شخص آخر
    }

    private Dictionary<string, List<ChatMessage>> chatChannels = new Dictionary<string, List<ChatMessage>>();
    private NetworkManager networkManager;
    private PlayerManager playerManager;

    // Events
    public event Action<ChatMessage> OnMessageReceived;
    public event Action<ChatMessage> OnMessageSent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        networkManager = NetworkManager.Instance;
        playerManager = PlayerManager.Instance;

        // إنشاء قنوات الدردشة الأساسية
        chatChannels["global"] = new List<ChatMessage>();

        Logger.Log("ChatManager initialized", "ChatManager");
    }

    /// <summary>
    /// إرسال رسالة في الدردشة العامة
    /// </summary>
    public async Task<bool> SendGlobalMessage(string content)
    {
        return await SendMessage("global", content);
    }

    /// <summary>
    /// إرسال رسالة خاصة لصديق
    /// </summary>
    public async Task<bool> SendPrivateMessage(string friendId, string friendName, string content)
    {
        // إنشاء قناة خاصة إذا لم تكن موجودة
        if (!chatChannels.ContainsKey(friendId))
        {
            chatChannels[friendId] = new List<ChatMessage>();
        }

        return await SendMessage(friendId, content);
    }

    /// <summary>
    /// إرسال الرسالة الفعلية
    /// </summary>
    private async Task<bool> SendMessage(string channelId, string content)
    {
        try
        {
            if (string.IsNullOrEmpty(content))
            {
                Logger.LogWarning("Message content is empty!", "ChatManager");
                return false;
            }

            if (content.Length > 255)
            {
                Logger.LogWarning("Message too long! Max 255 characters", "ChatManager");
                return false;
            }

            var playerProfile = playerManager.CurrentProfile;
            if (playerProfile == null)
            {
                Logger.LogError("Player profile not loaded!", "ChatManager");
                return false;
            }

            var message = new ChatMessage
            {
                messageId = System.Guid.NewGuid().ToString(),
                senderId = playerProfile.playerId,
                senderName = playerProfile.username,
                content = content,
                timestamp = DateTime.Now,
                channelId = channelId,
                isLocal = true
            };

            // إضافة الرسالة محلياً
            if (!chatChannels.ContainsKey(channelId))
            {
                chatChannels[channelId] = new List<ChatMessage>();
            }

            chatChannels[channelId].Add(message);

            // إرسال للسيرفر
            var msgData = new NetworkMessage
            {
                messageType = "CHAT_MESSAGE",
                data = new Dictionary<string, object>
                {
                    { "messageId", message.messageId },
                    { "senderId", message.senderId },
                    { "senderName", message.senderName },
                    { "content", message.content },
                    { "channelId", message.channelId }
                }
            };

            bool sent = await networkManager.SendMessage(msgData);

            if (sent)
            {
                OnMessageSent?.Invoke(message);
                Logger.LogDebug($"Message sent to {channelId}", "ChatManager");
            }

            return sent;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to send message: {ex.Message}", "ChatManager");
            return false;
        }
    }

    /// <summary>
    /// استقبال رسالة جديدة
    /// </summary>
    public void ReceiveMessage(string messageId, string senderId, string senderName, string content, string channelId)
    {
        try
        {
            if (!chatChannels.ContainsKey(channelId))
            {
                chatChannels[channelId] = new List<ChatMessage>();
            }

            var message = new ChatMessage
            {
                messageId = messageId,
                senderId = senderId,
                senderName = senderName,
                content = content,
                timestamp = DateTime.Now,
                channelId = channelId,
                isLocal = false
            };

            chatChannels[channelId].Add(message);
            OnMessageReceived?.Invoke(message);

            Logger.Log($"Message received from {senderName} in {channelId}", "ChatManager");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to receive message: {ex.Message}", "ChatManager");
        }
    }

    /// <summary>
    /// الحصول على رسائل قناة معينة
    /// </summary>
    public List<ChatMessage> GetChannelMessages(string channelId, int limit = 50)
    {
        if (!chatChannels.ContainsKey(channelId))
            return new List<ChatMessage>();

        var messages = chatChannels[channelId];
        int startIndex = Mathf.Max(0, messages.Count - limit);
        return messages.GetRange(startIndex, messages.Count - startIndex);
    }

    /// <summary>
    /// حذف قنوات الدردشة (حذف صديق)
    /// </summary>
    public void DeleteChannel(string channelId)
    {
        if (chatChannels.ContainsKey(channelId))
        {
            chatChannels.Remove(channelId);
            Logger.Log($"Chat channel deleted: {channelId}", "ChatManager");
        }
    }

    // Getters
    public static ChatManager Instance => instance;
}
