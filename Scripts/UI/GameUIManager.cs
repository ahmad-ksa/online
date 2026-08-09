using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// مدير واجهة مستخدم اللعبة
/// </summary>
public class GameUIManager : MonoBehaviour
{
    private static GameUIManager instance;

    [SerializeField] private CanvasGroup hudCanvasGroup;
    [SerializeField] private CanvasGroup chatCanvasGroup;
    [SerializeField] private CanvasGroup friendsCanvasGroup;
    [SerializeField] private CanvasGroup notificationsCanvasGroup;

    [SerializeField] private Text playerNameText;
    [SerializeField] private Text playerLevelText;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text gemsText;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private InputField chatInputField;
    [SerializeField] private Button sendMessageButton;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private Transform chatContentArea;

    [SerializeField] private Transform friendsListArea;
    [SerializeField] private Transform requestsArea;
    [SerializeField] private Text onlineFriendsCountText;

    [SerializeField] private Transform notificationsArea;
    [SerializeField] private GameObject notificationPrefab;

    private PlayerManager playerManager;
    private ChatManager chatManager;
    private FriendsManager friendsManager;
    private VoiceChatManager voiceChatManager;

    private bool isChatOpen = false;
    private bool isFriendsOpen = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Logger.Log("GameUIManager initialized", "GameUIManager");
    }

    private void Start()
    {
        playerManager = PlayerManager.Instance;
        chatManager = ChatManager.Instance;
        friendsManager = FriendsManager.Instance;
        voiceChatManager = VoiceChatManager.Instance;

        InitializeUI();
        SubscribeToEvents();
    }

    private void InitializeUI()
    {
        // تحديث معلومات اللاعب
        UpdatePlayerInfo();

        // الأزرار
        if (sendMessageButton != null)
        {
            sendMessageButton.onClick.AddListener(OnSendMessageClicked);
        }

        // إغلاق/فتح النوافذ
        if (chatCanvasGroup != null)
            chatCanvasGroup.alpha = 0.9f;
        if (friendsCanvasGroup != null)
            friendsCanvasGroup.alpha = 0.9f;
    }

    private void SubscribeToEvents()
    {
        if (chatManager != null)
        {
            chatManager.OnMessageReceived += OnMessageReceived;
            chatManager.OnMessageSent += OnMessageSent;
        }

        if (friendsManager != null)
        {
            friendsManager.OnFriendAdded += OnFriendAdded;
            friendsManager.OnFriendRemoved += OnFriendRemoved;
            friendsManager.OnFriendStatusChanged += OnFriendStatusChanged;
            friendsManager.OnFriendRequestReceived += OnFriendRequestReceived;
        }

        if (voiceChatManager != null)
        {
            voiceChatManager.OnPlayerVoiceStarted += OnPlayerVoiceStarted;
            voiceChatManager.OnPlayerVoiceEnded += OnPlayerVoiceEnded;
        }
    }

    private void Update()
    {
        // الاختصارات
        if (Input.GetKeyDown(KeyCode.C))
            ToggleChat();
        if (Input.GetKeyDown(KeyCode.F))
            ToggleFriends();
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseAllPanels();
    }

    /// <summary>
    /// تحديث معلومات اللاعب في الواجهة
    /// </summary>
    public void UpdatePlayerInfo()
    {
        if (playerManager.CurrentProfile == null)
            return;

        var profile = playerManager.CurrentProfile;

        if (playerNameText != null)
            playerNameText.text = profile.username;
        if (playerLevelText != null)
            playerLevelText.text = $"Level {profile.level}";
        if (coinsText != null)
            coinsText.text = profile.coins.ToString();
        if (gemsText != null)
            gemsText.text = profile.gems.ToString();
        if (healthSlider != null)
            healthSlider.value = 1f; // 100% صحة
    }

    /// <summary>
    /// إرسال رسالة من الدردشة
    /// </summary>
    private async void OnSendMessageClicked()
    {
        if (chatInputField == null || string.IsNullOrEmpty(chatInputField.text))
            return;

        string message = chatInputField.text;
        chatInputField.text = "";

        await chatManager.SendGlobalMessage(message);
    }

    /// <summary>
    /// استقبال رسالة جديدة
    /// </summary>
    private void OnMessageReceived(ChatManager.ChatMessage message)
    {
        // إضافة الرسالة لنافذة الدردشة
        AddChatMessage(message.senderName, message.content, false);
    }

    private void OnMessageSent(ChatManager.ChatMessage message)
    {
        AddChatMessage(message.senderName, message.content, true);
    }

    /// <summary>
    /// إضافة رسالة لنافذة الدردشة
    /// </summary>
    private void AddChatMessage(string senderName, string content, bool isLocal)
    {
        if (chatContentArea == null)
            return;

        // TODO: إنشاء UI element للرسالة
        Logger.LogDebug($"{senderName}: {content}", "GameUIManager");
    }

    /// <summary>
    /// فتح/إغلاق نافذة الدردشة
    /// </summary>
    public void ToggleChat()
    {
        isChatOpen = !isChatOpen;
        if (chatCanvasGroup != null)
        {
            chatCanvasGroup.alpha = isChatOpen ? 1f : 0.3f;
        }
    }

    /// <summary>
    /// فتح/إغلاق نافذة الأصدقاء
    /// </summary>
    public void ToggleFriends()
    {
        isFriendsOpen = !isFriendsOpen;
        if (friendsCanvasGroup != null)
        {
            friendsCanvasGroup.alpha = isFriendsOpen ? 1f : 0.3f;
        }
        if (isFriendsOpen)
            RefreshFriendsList();
    }

    /// <summary>
    /// إغلاق جميع النوافذ
    /// </summary>
    public void CloseAllPanels()
    {
        isChatOpen = false;
        isFriendsOpen = false;
        if (chatCanvasGroup != null) chatCanvasGroup.alpha = 0.3f;
        if (friendsCanvasGroup != null) friendsCanvasGroup.alpha = 0.3f;
    }

    private void OnFriendAdded(FriendsManager.Friend friend)
    {
        RefreshFriendsList();
        ShowNotification($"{friend.friendName} added as friend!");
    }

    private void OnFriendRemoved(FriendsManager.Friend friend)
    {
        RefreshFriendsList();
    }

    private void OnFriendStatusChanged(FriendsManager.Friend friend)
    {
        RefreshFriendsList();
        ShowNotification($"{friend.friendName} is now {(friend.isOnline ? "online" : "offline")}");
    }

    private void OnFriendRequestReceived(FriendsManager.FriendRequest request)
    {
        ShowNotification($"{request.fromPlayerName} sent you a friend request!");
    }

    private void OnPlayerVoiceStarted(string playerName)
    {
        ShowNotification($"{playerName} is speaking...");
    }

    private void OnPlayerVoiceEnded(string playerName)
    {
        // إزالة الإشعار
    }

    /// <summary>
    /// تحديث قائمة الأصدقاء
    /// </summary>
    private void RefreshFriendsList()
    {
        if (friendsListArea != null && friendsManager != null)
        {
            // حذف الأصدقاء السابقين
            foreach (Transform child in friendsListArea)
            {
                Destroy(child.gameObject);
            }

            // إضافة الأصدقاء الحاليين
            foreach (var friend in friendsManager.FriendsList)
            {
                // TODO: إنشاء UI element لكل صديق
            }
        }

        if (onlineFriendsCountText != null)
        {
            onlineFriendsCountText.text = $"Online: {friendsManager.OnlineFriendsCount}";
        }
    }

    /// <summary>
    /// عرض إشعار
    /// </summary>
    public void ShowNotification(string message)
    {
        Logger.Log($"Notification: {message}", "GameUIManager");
        // TODO: إنشاء UI notification
    }

    public static GameUIManager Instance => instance;
}
