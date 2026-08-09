using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير الأصدقاء - يتعامل مع إضافة/حذف الأصدقاء والطلبات
/// </summary>
public class FriendsManager : MonoBehaviour
{
    private static FriendsManager instance;

    // Friend Lists
    private List<FriendProfile> friendsList = new List<FriendProfile>();
    private List<FriendRequest> pendingRequests = new List<FriendRequest>();
    private List<string> blockedPlayers = new List<string>();

    // Events
    public static event Action<FriendProfile> OnFriendAdded;
    public static event Action<string> OnFriendRemoved;
    public static event Action<FriendRequest> OnFriendRequestReceived;
    public static event Action<string> OnFriendRequestAccepted;
    public static event Action<string> OnFriendRequestRejected;
    public static event Action<string> OnPlayerBlocked;
    public static event Action<string> OnPlayerUnblocked;
    public static event Action<FriendProfile> OnFriendStatusChanged;

    // Storage Keys
    private const string FRIENDS_LIST_KEY = "FriendsList";
    private const string PENDING_REQUESTS_KEY = "PendingRequests";
    private const string BLOCKED_PLAYERS_KEY = "BlockedPlayers";

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
    /// تهيئة مدير الأصدقاء
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadFriendsLocal();
        LoadBlockedPlayersLocal();
        LoadPendingRequestsLocal();

        isInitialized = true;
        Debug.Log("FriendsManager Initialized");
    }

    /// <summary>
    /// إضافة صديق (طلب صداقة)
    /// </summary>
    public async Task<bool> SendFriendRequest(string targetPlayerId, string targetUsername)
    {
        // تحقق من عدم الطلب مسبقاً
        if (friendsList.Any(f => f.PlayerId == targetPlayerId))
        {
            Debug.LogWarning("Already friends with this player");
            return false;
        }

        if (pendingRequests.Any(r => r.TargetPlayerId == targetPlayerId))
        {
            Debug.LogWarning("Friend request already sent");
            return false;
        }

        if (blockedPlayers.Contains(targetPlayerId))
        {
            Debug.LogWarning("Cannot add blocked player");
            return false;
        }

        try
        {
            Debug.Log($"Sending friend request to {targetUsername}");

            // محاكاة إرسال الطلب
            await Task.Delay(500);

            // إضافة للطلبات المعلقة محلياً
            var request = new FriendRequest
            {
                RequestId = System.Guid.NewGuid().ToString(),
                SenderId = PlayerManager.Instance.CurrentProfile.playerId,
                TargetPlayerId = targetPlayerId,
                TargetUsername = targetUsername,
                SentAt = DateTime.Now,
                Status = FriendRequestStatus.Pending
            };

            pendingRequests.Add(request);
            SavePendingRequestsLocal();

            Debug.Log($"Friend request sent to {targetUsername}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send friend request: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// قبول طلب صداقة
    /// </summary>
    public async Task<bool> AcceptFriendRequest(string requestId)
    {
        var request = pendingRequests.FirstOrDefault(r => r.RequestId == requestId);
        if (request == null)
        {
            Debug.LogWarning("Friend request not found");
            return false;
        }

        try
        {
            Debug.Log($"Accepting friend request from {request.TargetUsername}");

            await Task.Delay(500);

            // إنشاء صداقة جديدة
            var friend = new FriendProfile
            {
                PlayerId = request.TargetPlayerId,
                Username = request.TargetUsername,
                AddedAt = DateTime.Now,
                IsOnline = true,
                LastSeen = DateTime.Now
            };

            friendsList.Add(friend);
            pendingRequests.Remove(request);

            SaveFriendsLocal();
            SavePendingRequestsLocal();

            OnFriendRequestAccepted?.Invoke(request.TargetPlayerId);
            OnFriendAdded?.Invoke(friend);

            Debug.Log($"Friend request accepted: {request.TargetUsername}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to accept friend request: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// رفض طلب صداقة
    /// </summary>
    public async Task<bool> RejectFriendRequest(string requestId)
    {
        var request = pendingRequests.FirstOrDefault(r => r.RequestId == requestId);
        if (request == null)
        {
            Debug.LogWarning("Friend request not found");
            return false;
        }

        try
        {
            Debug.Log($"Rejecting friend request from {request.TargetUsername}");

            await Task.Delay(300);

            pendingRequests.Remove(request);
            SavePendingRequestsLocal();

            OnFriendRequestRejected?.Invoke(request.TargetPlayerId);

            Debug.Log($"Friend request rejected: {request.TargetUsername}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to reject friend request: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// حذف صديق
    /// </summary>
    public async Task<bool> RemoveFriend(string friendId)
    {
        var friend = friendsList.FirstOrDefault(f => f.PlayerId == friendId);
        if (friend == null)
        {
            Debug.LogWarning("Friend not found");
            return false;
        }

        try
        {
            Debug.Log($"Removing friend: {friend.Username}");

            await Task.Delay(300);

            friendsList.Remove(friend);
            SaveFriendsLocal();

            OnFriendRemoved?.Invoke(friendId);

            Debug.Log($"Friend removed: {friend.Username}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to remove friend: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// حظر لاعب
    /// </summary>
    public async Task<bool> BlockPlayer(string playerId)
    {
        if (blockedPlayers.Contains(playerId))
        {
            Debug.LogWarning("Player already blocked");
            return false;
        }

        try
        {
            Debug.Log($"Blocking player: {playerId}");

            await Task.Delay(300);

            blockedPlayers.Add(playerId);
            
            // إزالة من الأصدقاء إذا كان موجود
            var friend = friendsList.FirstOrDefault(f => f.PlayerId == playerId);
            if (friend != null)
            {
                friendsList.Remove(friend);
                SaveFriendsLocal();
            }

            SaveBlockedPlayersLocal();
            OnPlayerBlocked?.Invoke(playerId);

            Debug.Log($"Player blocked: {playerId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to block player: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// إلغاء حظر لاعب
    /// </summary>
    public async Task<bool> UnblockPlayer(string playerId)
    {
        if (!blockedPlayers.Contains(playerId))
        {
            Debug.LogWarning("Player is not blocked");
            return false;
        }

        try
        {
            Debug.Log($"Unblocking player: {playerId}");

            await Task.Delay(300);

            blockedPlayers.Remove(playerId);
            SaveBlockedPlayersLocal();

            OnPlayerUnblocked?.Invoke(playerId);

            Debug.Log($"Player unblocked: {playerId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to unblock player: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تحديث حالة الصديق (أون لاين/أوف لاين)
    /// </summary>
    public void UpdateFriendStatus(string friendId, bool isOnline)
    {
        var friend = friendsList.FirstOrDefault(f => f.PlayerId == friendId);
        if (friend != null)
        {
            friend.IsOnline = isOnline;
            friend.LastSeen = DateTime.Now;
            SaveFriendsLocal();

            OnFriendStatusChanged?.Invoke(friend);
            Debug.Log($"Friend status updated: {friend.Username} - {(isOnline ? "Online" : "Offline")}");
        }
    }

    /// <summary>
    /// البحث عن لاعب
    /// </summary>
    public async Task<List<PlayerSearchResult>> SearchPlayers(string searchQuery, int limit = 10)
    {
        List<PlayerSearchResult> results = new List<PlayerSearchResult>();

        try
        {
            Debug.Log($"Searching for players: {searchQuery}");

            // محاكاة البحث
            await Task.Delay(800);

            // في الواقع، سيتم البحث في قاعدة بيانات Nakama
            // هنا نحاكي النتائج
            for (int i = 0; i < limit; i++)
            {
                results.Add(new PlayerSearchResult
                {
                    PlayerId = System.Guid.NewGuid().ToString(),
                    Username = $"{searchQuery}_{i}",
                    Level = UnityEngine.Random.Range(1, 50),
                    IsOnline = UnityEngine.Random.value > 0.5f,
                    IsBlocked = false,
                    IsFriend = false
                });
            }

            Debug.Log($"Found {results.Count} players");
            return results;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Search failed: {ex.Message}");
            return results;
        }
    }

    /// <summary>
    /// الحصول على قائمة الأصدقاء المتصلين
    /// </summary>
    public List<FriendProfile> GetOnlineFriends()
    {
        return friendsList.Where(f => f.IsOnline).ToList();
    }

    /// <summary>
    /// الحصول على قائمة الأصدقاء غير المتصلين
    /// </summary>
    public List<FriendProfile> GetOfflineFriends()
    {
        return friendsList.Where(f => !f.IsOnline).ToList();
    }

    /// <summary>
    /// حفظ قائمة الأصدقاء محلياً
    /// </summary>
    private void SaveFriendsLocal()
    {
        try
        {
            string json = JsonUtility.ToJson(new FriendsListWrapper { friends = friendsList });
            PlayerPrefs.SetString(FRIENDS_LIST_KEY, json);
            PlayerPrefs.Save();

            Debug.Log($"Friends list saved ({friendsList.Count} friends)");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save friends: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل قائمة الأصدقاء محلياً
    /// </summary>
    private void LoadFriendsLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(FRIENDS_LIST_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<FriendsListWrapper>(json);
                friendsList = wrapper.friends ?? new List<FriendProfile>();

                Debug.Log($"Friends list loaded ({friendsList.Count} friends)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load friends: {ex.Message}");
        }
    }

    /// <summary>
    /// حفظ الطلبات المعلقة
    /// </summary>
    private void SavePendingRequestsLocal()
    {
        try
        {
            string json = JsonUtility.ToJson(new PendingRequestsWrapper { requests = pendingRequests });
            PlayerPrefs.SetString(PENDING_REQUESTS_KEY, json);
            PlayerPrefs.Save();

            Debug.Log($"Pending requests saved ({pendingRequests.Count} requests)");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save pending requests: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل الطلبات المعلقة
    /// </summary>
    private void LoadPendingRequestsLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(PENDING_REQUESTS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<PendingRequestsWrapper>(json);
                pendingRequests = wrapper.requests ?? new List<FriendRequest>();

                Debug.Log($"Pending requests loaded ({pendingRequests.Count} requests)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load pending requests: {ex.Message}");
        }
    }

    /// <summary>
    /// حفظ قائمة اللاعبين المحظورين
    /// </summary>
    private void SaveBlockedPlayersLocal()
    {
        try
        {
            string json = JsonUtility.ToJson(new BlockedPlayersWrapper { blockedPlayers = blockedPlayers });
            PlayerPrefs.SetString(BLOCKED_PLAYERS_KEY, json);
            PlayerPrefs.Save();

            Debug.Log($"Blocked players list saved ({blockedPlayers.Count} players)");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save blocked players: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل قائمة اللاعبين المحظورين
    /// </summary>
    private void LoadBlockedPlayersLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(BLOCKED_PLAYERS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<BlockedPlayersWrapper>(json);
                blockedPlayers = wrapper.blockedPlayers ?? new List<string>();

                Debug.Log($"Blocked players list loaded ({blockedPlayers.Count} players)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load blocked players: {ex.Message}");
        }
    }

    // Getters
    public static FriendsManager Instance => instance;
    public List<FriendProfile> FriendsList => friendsList;
    public List<FriendRequest> PendingRequests => pendingRequests;
    public List<string> BlockedPlayers => blockedPlayers;
    public int FriendCount => friendsList.Count;
    public int PendingRequestCount => pendingRequests.Count;
}

// ==================== Data Classes ====================

[System.Serializable]
public class FriendProfile
{
    public string PlayerId;
    public string Username;
    public string ProfilePictureUrl = "";
    public int Level = 1;
    public bool IsOnline = false;
    public DateTime AddedAt;
    public DateTime LastSeen;
    public string StatusMessage = "";
    public bool IsFavorite = false;
}

[System.Serializable]
public class FriendRequest
{
    public string RequestId;
    public string SenderId;
    public string TargetPlayerId;
    public string TargetUsername;
    public DateTime SentAt;
    public FriendRequestStatus Status;
}

public enum FriendRequestStatus
{
    Pending,
    Accepted,
    Rejected,
    Expired
}

[System.Serializable]
public class PlayerSearchResult
{
    public string PlayerId;
    public string Username;
    public int Level;
    public bool IsOnline;
    public bool IsBlocked;
    public bool IsFriend;
}

// ==================== Wrapper Classes ====================

[System.Serializable]
public class FriendsListWrapper
{
    public List<FriendProfile> friends = new List<FriendProfile>();
}

[System.Serializable]
public class PendingRequestsWrapper
{
    public List<FriendRequest> requests = new List<FriendRequest>();
}

[System.Serializable]
public class BlockedPlayersWrapper
{
    public List<string> blockedPlayers = new List<string>();
}
