using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير غرفة اللعبة - يتعامل مع التزامن وإدارة اللعبة أثناء اللعب
/// </summary>
public class GameRoomManager : MonoBehaviour
{
    private static GameRoomManager instance;

    private GameRoom currentRoom = null;
    private Dictionary<string, PlayerGameState> playerStates = new Dictionary<string, PlayerGameState>();
    private float syncInterval = 0.1f; // مزامنة كل 100ms
    private float lastSyncTime = 0f;

    // Game Stats
    private GameStats gameStats = new GameStats();

    // Events
    public static event Action<GameRoom> OnRoomCreated;
    public static event Action<string, PlayerGameState> OnPlayerStateUpdated;
    public static event Action<PlayerGameState> OnPlayerDied;
    public static event Action<int> OnScoreUpdated;
    public static event Action OnGameEnded;

    // Storage Keys
    private const string ROOM_STATS_KEY = "RoomStats";

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

    private void Update()
    {
        SyncPlayerStates();
        UpdateGameStats();
    }

    /// <summary>
    /// تهيئة مدير الغرفة
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadRoomStatsLocal();
        isInitialized = true;

        Debug.Log("GameRoomManager Initialized");
    }

    /// <summary>
    /// إنشاء غرفة لعبة جديدة
    /// </summary>
    public async Task<bool> CreateRoom(string roomName, List<LobbyMember> members)
    {
        if (currentRoom != null)
        {
            Debug.LogWarning("Room already exists!");
            return false;
        }

        try
        {
            Debug.Log($"Creating game room: {roomName}");

            await Task.Delay(1000);

            // إنشاء الغرفة
            currentRoom = new GameRoom
            {
                RoomId = System.Guid.NewGuid().ToString(),
                Name = roomName,
                CreatedAt = DateTime.Now,
                Status = GameRoomStatus.Active,
                PlayerCount = members.Count,
                MaxPlayers = 8
            };

            gameStats = new GameStats
            {
                StartTime = DateTime.Now,
                RoomId = currentRoom.RoomId
            };

            // إضافة اللاعبين
            foreach (var member in members)
            {
                var playerState = new PlayerGameState
                {
                    PlayerId = member.PlayerId,
                    Username = member.Username,
                    Health = 100,
                    Score = 0,
                    Kills = 0,
                    Deaths = 0,
                    JoinTime = DateTime.Now,
                    IsAlive = true
                };

                playerStates[member.PlayerId] = playerState;
            }

            OnRoomCreated?.Invoke(currentRoom);

            Debug.Log($"Game room created: {currentRoom.RoomId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create room: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تحديث حالة اللاعب
    /// </summary>
    public void UpdatePlayerState(string playerId, PlayerGameState newState)
    {
        if (!playerStates.ContainsKey(playerId))
        {
            Debug.LogWarning($"Player not found: {playerId}");
            return;
        }

        playerStates[playerId] = newState;
        OnPlayerStateUpdated?.Invoke(playerId, newState);
    }

    /// <summary>
    /// تحديث موقع اللاعب
    /// </summary>
    public void UpdatePlayerPosition(string playerId, Vector3 position, Vector3 rotation)
    {
        if (!playerStates.ContainsKey(playerId))
            return;

        playerStates[playerId].Position = position;
        playerStates[playerId].Rotation = rotation;
    }

    /// <summary>
    /// تسجيل إصابة
    /// </summary>
    public void RegisterHit(string shooterId, string targetId, int damage)
    {
        if (!playerStates.ContainsKey(shooterId) || !playerStates.ContainsKey(targetId))
            return;

        try
        {
            Debug.Log($"{playerStates[shooterId].Username} hit {playerStates[targetId].Username} ({damage} damage)");

            // تقليل الصحة
            playerStates[targetId].Health -= damage;

            // التحقق من الوفاة
            if (playerStates[targetId].Health <= 0)
            {
                RegisterKill(shooterId, targetId);
            }

            OnPlayerStateUpdated?.Invoke(targetId, playerStates[targetId]);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to register hit: {ex.Message}");
        }
    }

    /// <summary>
    /// تسجيل قتل
    /// </summary>
    public void RegisterKill(string killerId, string victimId)
    {
        if (!playerStates.ContainsKey(killerId) || !playerStates.ContainsKey(victimId))
            return;

        try
        {
            Debug.Log($"{playerStates[killerId].Username} killed {playerStates[victimId].Username}");

            // تحديث الإحصائيات
            playerStates[killerId].Kills++;
            playerStates[killerId].Score += 100;

            playerStates[victimId].Deaths++;
            playerStates[victimId].IsAlive = false;

            OnPlayerDied?.Invoke(playerStates[victimId]);
            OnScoreUpdated?.Invoke(playerStates[killerId].Score);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to register kill: {ex.Message}");
        }
    }

    /// <summary>
    /// إحياء اللاعب
    /// </summary>
    public void RespawnPlayer(string playerId)
    {
        if (!playerStates.ContainsKey(playerId))
            return;

        try
        {
            Debug.Log($"Respawning player: {playerStates[playerId].Username}");

            playerStates[playerId].IsAlive = true;
            playerStates[playerId].Health = 100;
            playerStates[playerId].Position = GetRandomSpawnPoint();

            OnPlayerStateUpdated?.Invoke(playerId, playerStates[playerId]);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to respawn player: {ex.Message}");
        }
    }

    /// <summary>
    /// إنهاء اللعبة
    /// </summary>
    public async Task<bool> EndGame()
    {
        if (currentRoom == null)
            return false;

        try
        {
            Debug.Log("Ending game...");

            currentRoom.Status = GameRoomStatus.Finished;
            gameStats.EndTime = DateTime.Now;

            await Task.Delay(1000);

            SaveRoomStatsLocal();
            OnGameEnded?.Invoke();

            Debug.Log("Game ended");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to end game: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// الحصول على الفائزين
    /// </summary>
    public List<PlayerGameState> GetWinners()
    {
        if (playerStates.Count == 0)
            return new List<PlayerGameState>();

        var sorted = playerStates.Values.OrderByDescending(p => p.Score).ToList();
        return sorted;
    }

    /// <summary>
    /// مزامنة حالات اللاعبين
    /// </summary>
    private void SyncPlayerStates()
    {
        if (currentRoom == null || Time.time - lastSyncTime < syncInterval)
            return;

        lastSyncTime = Time.time;

        // في الواقع، سيتم إرسال التحديثات عبر الشبكة
        foreach (var kvp in playerStates)
        {
            // تزامن حالة اللاعب
        }
    }

    /// <summary>
    /// تحديث إحصائيات اللعبة
    /// </summary>
    private void UpdateGameStats()
    {
        if (currentRoom == null)
            return;

        gameStats.PlayersOnline = playerStates.Count(p => p.Value.IsAlive);
        gameStats.TotalKills = playerStates.Values.Sum(p => p.Kills);
    }

    /// <summary>
    /// الحصول على نقطة إحياء عشوائية
    /// </summary>
    private Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(
            UnityEngine.Random.Range(-50, 50),
            1,
            UnityEngine.Random.Range(-50, 50)
        );
    }

    /// <summary>
    /// حفظ إحصائيات الغرفة
    /// </summary>
    private void SaveRoomStatsLocal()
    {
        try
        {
            string json = JsonUtility.ToJson(gameStats);
            PlayerPrefs.SetString(ROOM_STATS_KEY, json);
            PlayerPrefs.Save();

            Debug.Log("Room stats saved");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save room stats: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل إحصائيات الغرفة
    /// </summary>
    private void LoadRoomStatsLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(ROOM_STATS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                gameStats = JsonUtility.FromJson<GameStats>(json);
                Debug.Log("Room stats loaded");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load room stats: {ex.Message}");
        }
    }

    // Getters
    public static GameRoomManager Instance => instance;
    public GameRoom CurrentRoom => currentRoom;
    public Dictionary<string, PlayerGameState> PlayerStates => playerStates;
    public GameStats GameStats => gameStats;
}

// ==================== Data Classes ====================

public enum GameRoomStatus
{
    Loading,
    Active,
    Paused,
    Finished,
    Closed
}

[System.Serializable]
public class GameRoom
{
    public string RoomId;
    public string Name;
    public DateTime CreatedAt;
    public GameRoomStatus Status;
    public int PlayerCount;
    public int MaxPlayers;
    public string Map = "Forest";
    public string GameMode = "deathmatch";
}

[System.Serializable]
public class PlayerGameState
{
    public string PlayerId;
    public string Username;
    public int Health = 100;
    public int Score = 0;
    public int Kills = 0;
    public int Deaths = 0;
    public bool IsAlive = true;
    public DateTime JoinTime;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public string CurrentWeapon = "pistol";
    public int Ammo = 30;
}

[System.Serializable]
public class GameStats
{
    public string RoomId;
    public DateTime StartTime;
    public DateTime EndTime;
    public int PlayersOnline = 0;
    public int TotalKills = 0;
    public int TotalDeaths = 0;
    public float AverageFPS = 60;
    public float HighestPing = 0;
}
