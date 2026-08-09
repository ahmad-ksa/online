using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير الدردشة - يتعامل مع الرسائل الخاصة والمجموعات
/// </summary>
public class ChatManager : MonoBehaviour
{
    private static ChatManager instance;

    // Chat Conversations
    private Dictionary<string, ChatConversation> conversations = new Dictionary<string, ChatConversation>();
    private List<ChatMessage> globalMessages = new List<ChatMessage>();
    private List<ChatMessage> lobbyMessages = new List<ChatMessage>();

    // Current Conversation
    private string currentConversationId = "";

    // Events
    public static event Action<ChatMessage> OnMessageReceived;
    public static event Action<ChatMessage> OnMessageSent;
    public static event Action<string> OnConversationCreated;
    public static event Action<string, List<ChatMessage>> OnConversationLoaded;
    public static event Action<string> OnTypingStatusChanged;

    // Storage Keys
    private const string CONVERSATIONS_KEY = "ChatConversations";
    private const string GLOBAL_MESSAGES_KEY = "GlobalMessages";
    private const string LOBBY_MESSAGES_KEY = "LobbyMessages";

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
    /// تهيئة مدير الدردشة
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadConversationsLocal();
        LoadGlobalMessagesLocal();
        LoadLobbyMessagesLocal();

        isInitialized = true;
        Debug.Log("ChatManager Initialized");
    }

    /// <summary>
    /// إنشاء محادثة جديدة (رسالة خاصة)
    /// </summary>
    public async Task<string> CreatePrivateConversation(string recipientId, string recipientUsername)
    {
        try
        {
            Debug.Log($"Creating private conversation with {recipientUsername}");

            // تحقق من وجود محادثة سابقة
            string existingConversationId = FindConversation(recipientId);
            if (!string.IsNullOrEmpty(existingConversationId))
            {
                Debug.Log("Conversation already exists");
                return existingConversationId;
            }

            await Task.Delay(300);

            // إنشاء محادثة جديدة
            string conversationId = System.Guid.NewGuid().ToString();
            var conversation = new ChatConversation
            {
                ConversationId = conversationId,
                ConversationType = ChatConversationType.Private,
                ParticipantIds = new List<string> 
                { 
                    PlayerManager.Instance.CurrentProfile.playerId, 
                    recipientId 
                },
                CreatedAt = DateTime.Now,
                LastMessageAt = DateTime.Now
            };

            conversations[conversationId] = conversation;
            SaveConversationsLocal();

            OnConversationCreated?.Invoke(conversationId);

            Debug.Log($"Private conversation created: {conversationId}");
            return conversationId;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create conversation: {ex.Message}");
            return "";
        }
    }

    /// <summary>
    /// إرسال رسالة
    /// </summary>
    public async Task<bool> SendMessage(string conversationId, string messageText)
    {
        if (string.IsNullOrEmpty(messageText))
        {
            Debug.LogWarning("Message cannot be empty");
            return false;
        }

        try
        {
            Debug.Log($"Sending message to {conversationId}");

            await Task.Delay(200);

            // إنشاء الرسالة
            var message = new ChatMessage
            {
                MessageId = System.Guid.NewGuid().ToString(),
                ConversationId = conversationId,
                SenderId = PlayerManager.Instance.CurrentProfile.playerId,
                SenderUsername = PlayerManager.Instance.CurrentProfile.playerName,
                MessageText = messageText,
                SentAt = DateTime.Now,
                IsRead = true
            };

            // إضافة للمحادثة
            if (conversations.ContainsKey(conversationId))
            {
                conversations[conversationId].Messages.Add(message);
                conversations[conversationId].LastMessageAt = DateTime.Now;
                SaveConversationsLocal();
            }

            OnMessageSent?.Invoke(message);

            Debug.Log("Message sent successfully");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send message: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تحميل محادثة
    /// </summary>
    public async Task<List<ChatMessage>> LoadConversation(string conversationId)
    {
        try
        {
            Debug.Log($"Loading conversation: {conversationId}");

            currentConversationId = conversationId;
            await Task.Delay(300);

            if (conversations.ContainsKey(conversationId))
            {
                var messages = conversations[conversationId].Messages.OrderBy(m => m.SentAt).ToList();
                OnConversationLoaded?.Invoke(conversationId, messages);

                Debug.Log($"Conversation loaded ({messages.Count} messages)");
                return messages;
            }

            return new List<ChatMessage>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load conversation: {ex.Message}");
            return new List<ChatMessage>();
        }
    }

    /// <summary>
    /// إرسال رسالة عامة (Global Chat)
    /// </summary>
    public async Task<bool> SendGlobalMessage(string messageText)
    {
        if (string.IsNullOrEmpty(messageText))
        {
            Debug.LogWarning("Message cannot be empty");
            return false;
        }

        try
        {
            Debug.Log("Sending global message");

            await Task.Delay(200);

            var message = new ChatMessage
            {
                MessageId = System.Guid.NewGuid().ToString(),
                ConversationId = "GLOBAL",
                SenderId = PlayerManager.Instance.CurrentProfile.playerId,
                SenderUsername = PlayerManager.Instance.CurrentProfile.playerName,
                MessageText = messageText,
                SentAt = DateTime.Now,
                IsRead = true
            };

            globalMessages.Add(message);
            SaveGlobalMessagesLocal();

            OnMessageSent?.Invoke(message);

            Debug.Log("Global message sent");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send global message: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// إرسال رسالة لوبي
    /// </summary>
    public async Task<bool> SendLobbyMessage(string messageText)
    {
        if (string.IsNullOrEmpty(messageText))
        {
            Debug.LogWarning("Message cannot be empty");
            return false;
        }

        try
        {
            Debug.Log("Sending lobby message");

            await Task.Delay(200);

            var message = new ChatMessage
            {
                MessageId = System.Guid.NewGuid().ToString(),
                ConversationId = "LOBBY",
                SenderId = PlayerManager.Instance.CurrentProfile.playerId,
                SenderUsername = PlayerManager.Instance.CurrentProfile.playerName,
                MessageText = messageText,
                SentAt = DateTime.Now,
                IsRead = true
            };

            lobbyMessages.Add(message);
            SaveLobbyMessagesLocal();

            OnMessageSent?.Invoke(message);

            Debug.Log("Lobby message sent");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send lobby message: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// البحث عن محادثة
    /// </summary>
    private string FindConversation(string recipientId)
    {
        foreach (var kvp in conversations)
        {
            var conversation = kvp.Value;
            if (conversation.ConversationType == ChatConversationType.Private &&
                conversation.ParticipantIds.Contains(recipientId))
            {
                return conversation.ConversationId;
            }
        }

        return "";
    }

    /// <summary>
    /// الحصول على قائمة المحادثات
    /// </summary>
    public List<ChatConversation> GetAllConversations()
    {
        return conversations.Values.OrderByDescending(c => c.LastMessageAt).ToList();
    }

    /// <summary>
    /// الحصول على آخر رسالة في محادثة
    /// </summary>
    public ChatMessage GetLastMessage(string conversationId)
    {
        if (conversations.ContainsKey(conversationId))
        {
            var messages = conversations[conversationId].Messages;
            return messages.Count > 0 ? messages[messages.Count - 1] : null;
        }

        return null;
    }

    /// <summary>
    /// حذف رسالة
    /// </summary>
    public async Task<bool> DeleteMessage(string conversationId, string messageId)
    {
        try
        {
            Debug.Log($"Deleting message: {messageId}");

            await Task.Delay(200);

            if (conversations.ContainsKey(conversationId))
            {
                var message = conversations[conversationId].Messages
                    .FirstOrDefault(m => m.MessageId == messageId);

                if (message != null)
                {
                    conversations[conversationId].Messages.Remove(message);
                    SaveConversationsLocal();

                    Debug.Log("Message deleted");
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete message: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// حفظ المحادثات محلياً
    /// </summary>
    private void SaveConversationsLocal()
    {
        try
        {
            // حفظ فقط آخر 50 رسالة لكل محادثة للحفاظ على الأداء
            var limitedConversations = new Dictionary<string, ChatConversation>(conversations);
            foreach (var kvp in limitedConversations)
            {
                if (kvp.Value.Messages.Count > 50)
                {
                    kvp.Value.Messages = kvp.Value.Messages
                        .Skip(kvp.Value.Messages.Count - 50)
                        .ToList();
                }
            }

            string json = JsonUtility.ToJson(new ConversationsWrapper 
            { 
                conversations = limitedConversations.Values.ToList() 
            });
            PlayerPrefs.SetString(CONVERSATIONS_KEY, json);
            PlayerPrefs.Save();

            Debug.Log($"Conversations saved ({conversations.Count} conversations)");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save conversations: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل المحادثات محلياً
    /// </summary>
    private void LoadConversationsLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(CONVERSATIONS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<ConversationsWrapper>(json);
                conversations.Clear();

                foreach (var conv in wrapper.conversations)
                {
                    conversations[conv.ConversationId] = conv;
                }

                Debug.Log($"Conversations loaded ({conversations.Count} conversations)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load conversations: {ex.Message}");
        }
    }

    /// <summary>
    /// حفظ الرسائل العامة
    /// </summary>
    private void SaveGlobalMessagesLocal()
    {
        try
        {
            // احتفظ بـ آخر 100 رسالة
            if (globalMessages.Count > 100)
            {
                globalMessages = globalMessages.Skip(globalMessages.Count - 100).ToList();
            }

            string json = JsonUtility.ToJson(new GlobalMessagesWrapper { messages = globalMessages });
            PlayerPrefs.SetString(GLOBAL_MESSAGES_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save global messages: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل الرسائل العامة
    /// </summary>
    private void LoadGlobalMessagesLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(GLOBAL_MESSAGES_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<GlobalMessagesWrapper>(json);
                globalMessages = wrapper.messages ?? new List<ChatMessage>();

                Debug.Log($"Global messages loaded ({globalMessages.Count} messages)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load global messages: {ex.Message}");
        }
    }

    /// <summary>
    /// حفظ رسائل اللوبي
    /// </summary>
    private void SaveLobbyMessagesLocal()
    {
        try
        {
            // احتفظ بـ آخر 50 رسالة
            if (lobbyMessages.Count > 50)
            {
                lobbyMessages = lobbyMessages.Skip(lobbyMessages.Count - 50).ToList();
            }

            string json = JsonUtility.ToJson(new LobbyMessagesWrapper { messages = lobbyMessages });
            PlayerPrefs.SetString(LOBBY_MESSAGES_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save lobby messages: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل رسائل اللوبي
    /// </summary>
    private void LoadLobbyMessagesLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(LOBBY_MESSAGES_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<LobbyMessagesWrapper>(json);
                lobbyMessages = wrapper.messages ?? new List<ChatMessage>();

                Debug.Log($"Lobby messages loaded ({lobbyMessages.Count} messages)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load lobby messages: {ex.Message}");
        }
    }

    // Getters
    public static ChatManager Instance => instance;
    public Dictionary<string, ChatConversation> Conversations => conversations;
    public List<ChatMessage> GlobalMessages => globalMessages;
    public List<ChatMessage> LobbyMessages => lobbyMessages;
    public string CurrentConversationId => currentConversationId;
}

// ==================== Data Classes ====================

[System.Serializable]
public class ChatConversation
{
    public string ConversationId;
    public ChatConversationType ConversationType;
    public List<string> ParticipantIds = new List<string>();
    public List<ChatMessage> Messages = new List<ChatMessage>();
    public DateTime CreatedAt;
    public DateTime LastMessageAt;
    public string ConversationName = "";
    public bool IsMuted = false;
}

[System.Serializable]
public class ChatMessage
{
    public string MessageId;
    public string ConversationId;
    public string SenderId;
    public string SenderUsername;
    public string MessageText;
    public DateTime SentAt;
    public bool IsRead;
    public string MessageType = "text"; // text, image, system
    public List<string> Reactions = new List<string>(); // emoji reactions
}

public enum ChatConversationType
{
    Private,
    Group,
    Global,
    Lobby,
    System
}

// ==================== Wrapper Classes ====================

[System.Serializable]
public class ConversationsWrapper
{
    public List<ChatConversation> conversations = new List<ChatConversation>();
}

[System.Serializable]
public class GlobalMessagesWrapper
{
    public List<ChatMessage> messages = new List<ChatMessage>();
}

[System.Serializable]
public class LobbyMessagesWrapper
{
    public List<ChatMessage> messages = new List<ChatMessage>();
}
