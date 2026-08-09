using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير اللوبي - يتعامل مع إدارة الروم والإعدادات قبل اللعبة
/// </summary>
public class LobbyManager : MonoBehaviour
{
    private static LobbyManager instance;

    private Lobby currentLobby = null;
    private bool isHost = false;
    private List<LobbyMember> lobbyMembers = new List<LobbyMember>();
    private LobbySettings lobbySettings = new LobbySettings();

    // Events
    public static event Action<Lobby> OnLobbyCreated;
    public static event Action<Lobby> OnLobbyJoined;
    public static event Action OnLobbyLeft;
    public static event Action<LobbyMember> OnMemberJoined;
    public static event Action<string> OnMemberLeft;
    public static event Action<LobbyMember> OnMemberStatusChanged;
    public static event Action<LobbySettings> OnSettingsChanged;
    public static event Action OnGameStarting;

    // Storage Keys
    private const string LAST_LOBBY_KEY = "LastLobby";

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
    /// تهيئة مدير اللوبي
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadLastLobbyLocal();
        isInitialized = true;

        Debug.Log("LobbyManager Initialized");
    }

    /// <summary>
    /// إنشاء لوبي جديد
    /// </summary>
    public async Task<bool> CreateLobby(string lobbyName, int maxPlayers = 8)
    {
        if (currentLobby != null)
        {
            Debug.LogWarning("Already in a lobby!");
            return false;
        }

        try
        {
            Debug.Log($"Creating lobby: {lobbyName}");

            await Task.Delay(500);

            // إنشاء اللوبي
            currentLobby = new Lobby
            {
                LobbyId = System.Guid.NewGuid().ToString(),
                Name = lobbyName,
                HostId = PlayerManager.Instance.CurrentProfile.playerId,
                HostName = PlayerManager.Instance.CurrentProfile.playerName,
                MaxPlayers = maxPlayers,
                CreatedAt = DateTime.Now,
                Status = LobbyStatus.Waiting
            };

            isHost = true;
            lobbySettings = new LobbySettings();

            // إضافة الـ Host للوبي
            var hostMember = new LobbyMember
            {
                PlayerId = currentLobby.HostId,
                Username = currentLobby.HostName,
                IsReady = false,
                JoinedAt = DateTime.Now,
                IsMemberReady = false
            };

            lobbyMembers.Add(hostMember);
            currentLobby.MemberCount = 1;

            SaveLastLobbyLocal();
            OnLobbyCreated?.Invoke(currentLobby);

            Debug.Log($"Lobby created: {currentLobby.LobbyId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// الدخول للوبي
    /// </summary>
    public async Task<bool> JoinLobby(string lobbyId)
    {
        if (currentLobby != null)
        {
            Debug.LogWarning("Already in a lobby!");
            return false;
        }

        try
        {
            Debug.Log($"Joining lobby: {lobbyId}");

            await Task.Delay(500);

            // محاكاة تحميل اللوبي
            currentLobby = new Lobby
            {
                LobbyId = lobbyId,
                Name = $"Lobby_{lobbyId.Substring(0, 8)}",
                HostId = System.Guid.NewGuid().ToString(),
                HostName = "Host",
                MaxPlayers = 8,
                CreatedAt = DateTime.Now,
                Status = LobbyStatus.Waiting,
                MemberCount = UnityEngine.Random.Range(2, 7)
            };

            isHost = false;

            // إضافة الدخول للوبي
            var member = new LobbyMember
            {
                PlayerId = PlayerManager.Instance.CurrentProfile.playerId,
                Username = PlayerManager.Instance.CurrentProfile.playerName,
                IsReady = false,
                JoinedAt = DateTime.Now
            };

            lobbyMembers.Add(member);
            currentLobby.MemberCount++;

            SaveLastLobbyLocal();
            OnLobbyJoined?.Invoke(currentLobby);

            Debug.Log($"Joined lobby: {currentLobby.LobbyId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join lobby: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// مغادرة اللوبي
    /// </summary>
    public async Task<bool> LeaveLobby()
    {
        if (currentLobby == null)
        {
            Debug.LogWarning("Not in a lobby!");
            return false;
        }

        try
        {
            Debug.Log("Leaving lobby...");

            await Task.Delay(300);

            currentLobby = null;
            isHost = false;
            lobbyMembers.Clear();

            OnLobbyLeft?.Invoke();

            Debug.Log("Left lobby");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to leave lobby: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// إضافة عضو للوبي
    /// </summary>
    public async Task<bool> AddMember(string playerId, string username)
    {
        if (currentLobby == null)
            return false;

        if (currentLobby.MemberCount >= currentLobby.MaxPlayers)
        {
            Debug.LogWarning("Lobby is full!");
            return false;
        }

        try
        {
            Debug.Log($"Adding member: {username}");

            await Task.Delay(200);

            var member = new LobbyMember
            {
                PlayerId = playerId,
                Username = username,
                IsReady = false,
                JoinedAt = DateTime.Now
            };

            lobbyMembers.Add(member);
            currentLobby.MemberCount++;

            OnMemberJoined?.Invoke(member);

            Debug.Log($"Member added: {username}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to add member: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// إزالة عضو من اللوبي
    /// </summary>
    public async Task<bool> RemoveMember(string playerId)
    {
        if (currentLobby == null)
            return false;

        try
        {
            Debug.Log($"Removing member: {playerId}");

            await Task.Delay(200);

            var member = lobbyMembers.FirstOrDefault(m => m.PlayerId == playerId);
            if (member != null)
            {
                lobbyMembers.Remove(member);
                currentLobby.MemberCount--;

                OnMemberLeft?.Invoke(playerId);

                Debug.Log($"Member removed: {member.Username}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to remove member: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تعيين حالة الجهوزية
    /// </summary>
    public async Task<bool> SetReadyStatus(bool isReady)
    {
        if (currentLobby == null)
            return false;

        try
        {
            Debug.Log($"Setting ready status: {isReady}");

            await Task.Delay(100);

            var member = lobbyMembers.FirstOrDefault(
                m => m.PlayerId == PlayerManager.Instance.CurrentProfile.playerId
            );

            if (member != null)
            {
                member.IsReady = isReady;
                member.IsMemberReady = isReady;

                OnMemberStatusChanged?.Invoke(member);

                Debug.Log($"Ready status set: {isReady}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to set ready status: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تعديل إعدادات اللوبي
    /// </summary>
    public async Task<bool> UpdateSettings(LobbySettings newSettings)
    {
        if (currentLobby == null || !isHost)
        {
            Debug.LogWarning("Not the host!");
            return false;
        }

        try
        {
            Debug.Log("Updating lobby settings...");

            await Task.Delay(200);

            lobbySettings = newSettings;
            OnSettingsChanged?.Invoke(lobbySettings);

            Debug.Log("Settings updated");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update settings: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// بدء اللعبة
    /// </summary>
    public async Task<bool> StartGame()
    {
        if (currentLobby == null || !isHost)
        {
            Debug.LogWarning("Not the host!");
            return false;
        }

        // التحقق من أن جميع الأعضاء جاهزين
        if (!lobbyMembers.All(m => m.IsReady))
        {
            Debug.LogWarning("Not all members are ready!");
            return false;
        }

        try
        {
            Debug.Log("Starting game...");

            currentLobby.Status = LobbyStatus.Starting;
            OnGameStarting?.Invoke();

            await Task.Delay(2000);

            // الانتقال لـ Game Scene
            GameManager.Instance.SetGameState(GameManager.GameState.InGame);

            Debug.Log("Game started!");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start game: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// طرد لاعب من اللوبي
    /// </summary>
    public async Task<bool> KickPlayer(string playerId)
    {
        if (!isHost)
        {
            Debug.LogWarning("Only host can kick players!");
            return false;
        }

        return await RemoveMember(playerId);
    }

    /// <summary>
    /// الحصول على عدد الأعضاء الجاهزين
    /// </summary>
    public int GetReadyMembersCount()
    {
        return lobbyMembers.Count(m => m.IsReady);
    }

    /// <summary>
    /// حفظ آخر لوبي
    /// </summary>
    private void SaveLastLobbyLocal()
    {
        if (currentLobby == null)
            return;

        try
        {
            string json = JsonUtility.ToJson(currentLobby);
            PlayerPrefs.SetString(LAST_LOBBY_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save lobby: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل آخر لوبي
    /// </summary>
    private void LoadLastLobbyLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(LAST_LOBBY_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                currentLobby = JsonUtility.FromJson<Lobby>(json);
                Debug.Log($"Last lobby loaded: {currentLobby.LobbyId}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load lobby: {ex.Message}");
        }
    }

    // Getters
    public static LobbyManager Instance => instance;
    public Lobby CurrentLobby => currentLobby;
    public bool IsHost => isHost;
    public List<LobbyMember> LobbyMembers => lobbyMembers;
    public LobbySettings Settings => lobbySettings;
    public int MemberCount => lobbyMembers.Count;
}

// ==================== Data Classes ====================

public enum LobbyStatus
{
    Waiting,
    Ready,
    Starting,
    InGame,
    Closed
}

[System.Serializable]
public class Lobby
{
    public string LobbyId;
    public string Name;
    public string HostId;
    public string HostName;
    public int MaxPlayers;
    public int MemberCount;
    public DateTime CreatedAt;
    public LobbyStatus Status;
    public string GameMode = "deathmatch";
    public string Map = "Forest";
}

[System.Serializable]
public class LobbyMember
{
    public string PlayerId;
    public string Username;
    public int Level = 1;
    public bool IsReady = false;
    public bool IsMemberReady = false;
    public DateTime JoinedAt;
    public string SelectedCharacter = "default";
}

[System.Serializable]
public class LobbySettings
{
    public string GameMode = "deathmatch";  // deathmatch, tdm, survival
    public string Map = "Forest";
    public int MaxPlayers = 8;
    public int TimeLimit = 600; // بالثواني
    public int TargetScore = 100;
    public bool FriendlyFire = false;
    public bool Voice = true;
    public string Difficulty = "normal"; // easy, normal, hard
}
