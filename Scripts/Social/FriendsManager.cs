using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// نظام إدارة قائمة الأصدقاء
/// </summary>
public class FriendsManager : MonoBehaviour
{
    private static FriendsManager instance;

    [System.Serializable]
    public class Friend
    {
        public string friendId;
        public string friendName;
        public bool isOnline;
        public int level;
        public string lastSeen;
        public DateTime addedDate;
    }

    [System.Serializable]
    public class FriendRequest
    {
        public string requestId;
        public string fromPlayerId;
        public string fromPlayerName;
        public DateTime sentDate;
        public bool isAccepted;
    }

    private List<Friend> friendsList = new List<Friend>();
    private List<FriendRequest> pendingRequests = new List<FriendRequest>();
    private LocalDataManager localDataManager;
    private NetworkManager networkManager;

    // Events
    public event Action<Friend> OnFriendAdded;
    public event Action<Friend> OnFriendRemoved;
    public event Action<Friend> OnFriendStatusChanged; // تصل/يغادر
    public event Action<FriendRequest> OnFriendRequestReceived;
    public event Action<FriendRequest> OnFriendRequestAccepted;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        localDataManager = LocalDataManager.Instance;
        networkManager = NetworkManager.Instance;

        Logger.Log("FriendsManager initialized", "FriendsManager");
    }

    private async void Start()
    {
        await LoadFriends();
    }

    /// <summary>
    /// تحميل قائمة الأصدقاء من البيانات المحلية
    /// </summary>
    public async Task<bool> LoadFriends()
    {
        try
        {
            Logger.Log("Loading friends list...", "FriendsManager");

            // محاكاة تحميل البيانات
            await Task.Delay(500);

            friendsList.Clear();
            // TODO: تحميل من قاعدة البيانات السحابية

            Logger.Log($"Loaded {friendsList.Count} friends", "FriendsManager");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load friends: {ex.Message}", "FriendsManager");
            return false;
        }
    }

    /// <summary>
    /// إضافة لاعب كصديق (إرسال طلب)
    /// </summary>
    public async Task<bool> AddFriend(string playerName)
    {
        try
        {
            if (string.IsNullOrEmpty(playerName))
            {
                Logger.LogError("Player name cannot be empty!", "FriendsManager");
                return false;
            }

            Logger.Log($"Sending friend request to {playerName}", "FriendsManager");

            var requestMsg = new NetworkMessage
            {
                messageType = "FRIEND_REQUEST_SEND",
                data = new Dictionary<string, object>
                {
                    { "targetPlayerName", playerName },
                    { "message", "Let's be friends!" }
                }
            };

            bool sent = await networkManager.SendMessage(requestMsg);

            if (sent)
            {
                Logger.Log($"Friend request sent to {playerName}", "FriendsManager");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to add friend: {ex.Message}", "FriendsManager");
            return false;
        }
    }

    /// <summary>
    /// قبول طلب الصداقة
    /// </summary>
    public async Task<bool> AcceptFriendRequest(string requestId)
    {
        try
        {
            var request = pendingRequests.Find(r => r.requestId == requestId);
            if (request == null)
            {
                Logger.LogError("Request not found!", "FriendsManager");
                return false;
            }

            Logger.Log($"Accepting friend request from {request.fromPlayerName}", "FriendsManager");

            var acceptMsg = new NetworkMessage
            {
                messageType = "FRIEND_REQUEST_ACCEPT",
                data = new Dictionary<string, object>
                {
                    { "requestId", requestId },
                    { "fromPlayerId", request.fromPlayerId }
                }
            };

            bool sent = await networkManager.SendMessage(acceptMsg);

            if (sent)
            {
                // إضافة للقائمة
                var newFriend = new Friend
                {
                    friendId = request.fromPlayerId,
                    friendName = request.fromPlayerName,
                    isOnline = true,
                    level = 1,
                    addedDate = DateTime.Now
                };

                friendsList.Add(newFriend);
                pendingRequests.Remove(request);

                OnFriendAdded?.Invoke(newFriend);
                Logger.Log($"Friend added: {request.fromPlayerName}", "FriendsManager");

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to accept friend request: {ex.Message}", "FriendsManager");
            return false;
        }
    }

    /// <summary>
    /// رفض طلب الصداقة
    /// </summary>
    public async Task<bool> DeclineFriendRequest(string requestId)
    {
        try
        {
            var request = pendingRequests.Find(r => r.requestId == requestId);
            if (request == null)
                return false;

            var declineMsg = new NetworkMessage
            {
                messageType = "FRIEND_REQUEST_DECLINE",
                data = new Dictionary<string, object> { { "requestId", requestId } }
            };

            bool sent = await networkManager.SendMessage(declineMsg);

            if (sent)
            {
                pendingRequests.Remove(request);
                Logger.Log($"Friend request declined", "FriendsManager");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to decline friend request: {ex.Message}", "FriendsManager");
            return false;
        }
    }

    /// <summary>
    /// حذف صديق
    /// </summary>
    public async Task<bool> RemoveFriend(string friendId)
    {
        try
        {
            var friend = friendsList.Find(f => f.friendId == friendId);
            if (friend == null)
            {
                Logger.LogError("Friend not found!", "FriendsManager");
                return false;
            }

            Logger.Log($"Removing friend: {friend.friendName}", "FriendsManager");

            var removeMsg = new NetworkMessage
            {
                messageType = "FRIEND_REMOVE",
                data = new Dictionary<string, object> { { "friendId", friendId } }
            };

            bool sent = await networkManager.SendMessage(removeMsg);

            if (sent)
            {
                friendsList.Remove(friend);
                OnFriendRemoved?.Invoke(friend);
                Logger.Log($"Friend removed: {friend.friendName}", "FriendsManager");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to remove friend: {ex.Message}", "FriendsManager");
            return false;
        }
    }

    /// <summary>
    /// استقبال طلب صداقة جديد
    /// </summary>
    public void ReceiveFriendRequest(string requestId, string fromPlayerId, string fromPlayerName)
    {
        var request = new FriendRequest
        {
            requestId = requestId,
            fromPlayerId = fromPlayerId,
            fromPlayerName = fromPlayerName,
            sentDate = DateTime.Now,
            isAccepted = false
        };

        pendingRequests.Add(request);
        OnFriendRequestReceived?.Invoke(request);

        Logger.Log($"Friend request received from {fromPlayerName}", "FriendsManager");
    }

    /// <summary>
    /// تحديث حالة الصديق (تصل/يغادر)
    /// </summary>
    public void UpdateFriendStatus(string friendId, bool isOnline)
    {
        var friend = friendsList.Find(f => f.friendId == friendId);
        if (friend != null)
        {
            friend.isOnline = isOnline;
            friend.lastSeen = isOnline ? "Now" : DateTime.Now.ToString();
            OnFriendStatusChanged?.Invoke(friend);
            Logger.Log($"Friend {friend.friendName} is now {(isOnline ? "online" : "offline")}", "FriendsManager");
        }
    }

    // Getters
    public List<Friend> FriendsList => friendsList;
    public List<FriendRequest> PendingRequests => pendingRequests;
    public int OnlineFriendsCount => friendsList.FindAll(f => f.isOnline).Count;
    public static FriendsManager Instance => instance;
}
