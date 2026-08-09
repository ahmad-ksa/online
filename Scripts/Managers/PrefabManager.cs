using UnityEngine;

/// <summary>
/// Prefab Manager - إنشاء واستخدام Prefabs بسهولة
/// </summary>
public class PrefabManager : MonoBehaviour
{
    private static PrefabManager instance;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject chatMessagePrefab;
    [SerializeField] private GameObject friendItemPrefab;
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private GameObject inviteNotificationPrefab;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPrefabs();
        Logger.Log("PrefabManager initialized", "PrefabManager");
    }

    /// <summary>
    /// تحميل جميع الـ Prefabs من Resources
    /// </summary>
    private void LoadPrefabs()
    {
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        chatMessagePrefab = Resources.Load<GameObject>("Prefabs/ChatMessage");
        friendItemPrefab = Resources.Load<GameObject>("Prefabs/FriendItem");
        notificationPrefab = Resources.Load<GameObject>("Prefabs/Notification");
        inviteNotificationPrefab = Resources.Load<GameObject>("Prefabs/InviteNotification");

        if (playerPrefab == null)
            Logger.LogWarning("Player prefab not found in Resources/Prefabs/", "PrefabManager");
        if (chatMessagePrefab == null)
            Logger.LogWarning("ChatMessage prefab not found", "PrefabManager");
        if (friendItemPrefab == null)
            Logger.LogWarning("FriendItem prefab not found", "PrefabManager");
    }

    /// <summary>
    /// إنشاء لاعب
    /// </summary>
    public GameObject CreatePlayer(Vector3 position, Quaternion rotation)
    {
        if (playerPrefab == null)
        {
            Logger.LogError("Player prefab not loaded!", "PrefabManager");
            return null;
        }

        return Instantiate(playerPrefab, position, rotation);
    }

    /// <summary>
    /// إنشاء رسالة دردشة
    /// </summary>
    public GameObject CreateChatMessage(string senderName, string content, Transform parent)
    {
        if (chatMessagePrefab == null)
        {
            Logger.LogError("ChatMessage prefab not loaded!", "PrefabManager");
            return null;
        }

        GameObject msg = Instantiate(chatMessagePrefab, parent);
        
        // تعيين البيانات
        var chatMsgUI = msg.GetComponent<ChatMessageUI>();
        if (chatMsgUI != null)
        {
            chatMsgUI.SetMessage(senderName, content);
        }

        return msg;
    }

    /// <summary>
    /// إنشاء عنصر صديق
    /// </summary>
    public GameObject CreateFriendItem(string friendName, bool isOnline, Transform parent)
    {
        if (friendItemPrefab == null)
        {
            Logger.LogError("FriendItem prefab not loaded!", "PrefabManager");
            return null;
        }

        GameObject item = Instantiate(friendItemPrefab, parent);
        
        var friendUI = item.GetComponent<FriendItemUI>();
        if (friendUI != null)
        {
            friendUI.SetFriend(friendName, isOnline);
        }

        return item;
    }

    /// <summary>
    /// إنشاء إشعار عام
    /// </summary>
    public GameObject CreateNotification(string message, Transform parent)
    {
        if (notificationPrefab == null)
        {
            Logger.LogError("Notification prefab not loaded!", "PrefabManager");
            return null;
        }

        GameObject notif = Instantiate(notificationPrefab, parent);
        
        var notifUI = notif.GetComponent<NotificationUI>();
        if (notifUI != null)
        {
            notifUI.SetMessage(message);
        }

        return notif;
    }

    /// <summary>
    /// إنشاء إشعار دعوة
    /// </summary>
    public GameObject CreateInviteNotification(string fromPlayerName, Transform parent)
    {
        if (inviteNotificationPrefab == null)
        {
            Logger.LogError("InviteNotification prefab not loaded!", "PrefabManager");
            return null;
        }

        GameObject invite = Instantiate(inviteNotificationPrefab, parent);
        
        var inviteUI = invite.GetComponent<InviteNotificationUI>();
        if (inviteUI != null)
        {
            inviteUI.SetInvite(fromPlayerName);
        }

        return invite;
    }

    public static PrefabManager Instance => instance;
}
