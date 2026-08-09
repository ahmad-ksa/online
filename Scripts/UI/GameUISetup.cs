using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// إعداد واجهة اللعبة - بعد دخول اللاعب
/// </summary>
public class GameUISetup : MonoBehaviour
{
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text gemsText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private ScrollRect friendsScrollRect;
    [SerializeField] private Transform chatContentArea;
    [SerializeField] private Transform friendsListArea;
    [SerializeField] private Transform notificationsArea;
    [SerializeField] private InputField chatInputField;
    [SerializeField] private Button sendMessageButton;
    [SerializeField] private Toggle chatToggle;
    [SerializeField] private Toggle friendsToggle;
    [SerializeField] private Toggle micToggle;

    private GameUIManager gameUIManager;
    private PlayerManager playerManager;
    private ChatManager chatManager;
    private FriendsManager friendsManager;
    private VoiceChatManager voiceChatManager;

    private void Start()
    {
        gameUIManager = GameUIManager.Instance;
        playerManager = PlayerManager.Instance;
        chatManager = ChatManager.Instance;
        friendsManager = FriendsManager.Instance;
        voiceChatManager = VoiceChatManager.Instance;

        if (gameUIManager != null)
        {
            gameUIManager.UpdatePlayerInfo();
        }

        SetupUIReferences();
        SubscribeToEvents();

        Logger.Log("GameUISetup completed", "GameUISetup");
    }

    /// <summary>
    /// ربط مراجع الـ UI
    /// </summary>
    private void SetupUIReferences()
    {
        // ربط أزرار الدردشة
        if (sendMessageButton != null)
            sendMessageButton.onClick.AddListener(OnSendMessageClicked);

        // ربط Toggles
        if (chatToggle != null)
            chatToggle.onValueChanged.AddListener(OnChatToggled);
        if (friendsToggle != null)
            friendsToggle.onValueChanged.AddListener(OnFriendsToggled);
        if (micToggle != null)
            micToggle.onValueChanged.AddListener(OnMicToggled);
    }

    /// <summary>
    /// الاشتراك في الأحداث
    /// </summary>
    private void SubscribeToEvents()
    {
        if (chatManager != null)
        {
            chatManager.OnMessageReceived += OnChatMessageReceived;
            chatManager.OnMessageSent += OnChatMessageSent;
        }

        if (friendsManager != null)
        {
            friendsManager.OnFriendAdded += OnFriendAdded;
            friendsManager.OnFriendRequestReceived += OnFriendRequestReceived;
        }
    }

    /// <summary>
    /// إرسال رسالة دردشة
    /// </summary>
    private async void OnSendMessageClicked()
    {
        if (chatInputField == null || chatManager == null)
            return;

        string message = chatInputField.text;
        if (string.IsNullOrEmpty(message))
            return;

        chatInputField.text = "";
        await chatManager.SendGlobalMessage(message);
    }

    /// <summary>
    /// استقبال رسالة دردشة
    /// </summary>
    private void OnChatMessageReceived(ChatManager.ChatMessage message)
    {
        AddChatMessageToUI(message.senderName, message.content, false);
    }

    private void OnChatMessageSent(ChatManager.ChatMessage message)
    {
        AddChatMessageToUI(message.senderName, message.content, true);
    }

    /// <summary>
    /// إضافة رسالة إلى الواجهة
    /// </summary>
    private void AddChatMessageToUI(string senderName, string content, bool isLocal)
    {
        if (chatContentArea == null)
            return;

        // TODO: استخدام PrefabManager لإنشاء الرسالة
        GameObject msgObj = new GameObject($"Message_{senderName}");
        msgObj.transform.SetParent(chatContentArea);

        var text = msgObj.AddComponent<Text>();
        text.text = $"{senderName}: {content}";
        text.font = Resources.Load<Font>("Arial");
        text.fontSize = 14;
        text.color = isLocal ? Color.cyan : Color.white;

        Logger.LogDebug($"Message added: {senderName}: {content}", "GameUISetup");
    }

    /// <summary>
    /// تفعيل/تعطيل الدردشة
    /// </summary>
    private void OnChatToggled(bool isOn)
    {
        if (gameUIManager != null)
        {
            if (isOn)
                gameUIManager.ToggleChat();
        }
    }

    /// <summary>
    /// تفعيل/تعطيل الأصدقاء
    /// </summary>
    private void OnFriendsToggled(bool isOn)
    {
        if (gameUIManager != null)
        {
            if (isOn)
                gameUIManager.ToggleFriends();
        }
    }

    /// <summary>
    /// تفعيل/تعطيل الميكروفون
    /// </summary>
    private void OnMicToggled(bool isOn)
    {
        if (voiceChatManager != null)
        {
            voiceChatManager.SetMicrophoneEnabled(isOn);
        }
    }

    /// <summary>
    /// إضافة صديق جديد
    /// </summary>
    private void OnFriendAdded(FriendsManager.Friend friend)
    {
        if (friendsListArea == null || friendsManager == null)
            return;

        // TODO: استخدام PrefabManager لإنشاء FriendItem
        GameObject friendObj = new GameObject($"Friend_{friend.friendName}");
        friendObj.transform.SetParent(friendsListArea);

        var text = friendObj.AddComponent<Text>();
        text.text = $"{friend.friendName} ({(friend.isOnline ? "Online" : "Offline")})";
        text.font = Resources.Load<Font>("Arial");
        text.fontSize = 14;
        text.color = friend.isOnline ? Color.green : Color.gray;

        Logger.Log($"Friend added to UI: {friend.friendName}", "GameUISetup");
    }

    /// <summary>
    /// استقبال طلب صداقة
    /// </summary>
    private void OnFriendRequestReceived(FriendsManager.FriendRequest request)
    {
        if (notificationsArea == null)
            return;

        // TODO: استخدام PrefabManager لإنشاء InviteNotification
        GameObject inviteObj = new GameObject($"Invite_{request.fromPlayerName}");
        inviteObj.transform.SetParent(notificationsArea);

        var text = inviteObj.AddComponent<Text>();
        text.text = $"{request.fromPlayerName} sent a friend request!";
        text.font = Resources.Load<Font>("Arial");
        text.fontSize = 14;
        text.color = Color.yellow;

        Logger.Log($"Friend request received from {request.fromPlayerName}", "GameUISetup");
    }
}
