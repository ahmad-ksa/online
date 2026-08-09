using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// مدير المباريات الفعلية - تزامن اللاعبين في الوقت الفعلي
/// </summary>
public class RealTimeMatchManager : MonoBehaviour
{
    private static RealTimeMatchManager instance;

    private NakamaClient nakamaClient;
    private string currentMatchId = "";
    private Dictionary<string, PlayerData> playersInMatch = new Dictionary<string, PlayerData>();
    private float syncInterval = 0.1f; // تحديث كل 100ms
    private float timeSinceLastSync = 0f;

    [System.Serializable]
    public class PlayerData
    {
        public string playerId;
        public string playerName;
        public Vector3 position;
        public Quaternion rotation;
        public float health;
    }

    // Events
    public event System.Action<string, PlayerData> OnPlayerJoined;
    public event System.Action<string> OnPlayerLeft;
    public event System.Action<string, PlayerData> OnPlayerPositionUpdated;
    public event System.Action<string> OnMatchStarted;
    public event System.Action OnMatchEnded;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Logger.Log("RealTimeMatchManager initialized", "RealTimeMatchManager");
    }

    private void Start()
    {
        nakamaClient = NakamaClient.Instance;
    }

    private void Update()
    {
        if (!nakamaClient.IsAuthenticated)
            return;

        timeSinceLastSync += Time.deltaTime;
        if (timeSinceLastSync >= syncInterval)
        {
            SyncPlayersPositions();
            timeSinceLastSync = 0f;
        }
    }

    /// <summary>
    /// إنشاء مباراة جديدة
    /// </summary>
    public async Task<bool> CreateMatch(string matchName, int minPlayers, int maxPlayers)
    {
        try
        {
            Logger.Log($"Creating match: {matchName}...", "RealTimeMatchManager");

            // محاكاة إنشاء المباراة
            await Task.Delay(500);

            currentMatchId = System.Guid.NewGuid().ToString();

            // TODO: استبدل بـ Nakama SDK الحقيقي
            // var result = await nakamaClient.RpcAsync("create_match", new { name = matchName, minPlayers, maxPlayers });

            OnMatchStarted?.Invoke(currentMatchId);
            Logger.Log($"✓ Match created: {currentMatchId}", "RealTimeMatchManager");
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to create match: {ex.Message}", "RealTimeMatchManager");
            return false;
        }
    }

    /// <summary>
    /// الانضمام إلى مباراة
    /// </summary>
    public async Task<bool> JoinMatch(string matchId)
    {
        try
        {
            Logger.Log($"Joining match: {matchId}...", "RealTimeMatchManager");

            // محاكاة الانضمام
            await Task.Delay(500);

            currentMatchId = matchId;

            // TODO: استبدل بـ Nakama SDK الحقيقي
            // await socket.JoinMatchAsync(matchId);

            Logger.Log($"✓ Joined match: {matchId}", "RealTimeMatchManager");
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to join match: {ex.Message}", "RealTimeMatchManager");
            return false;
        }
    }

    /// <summary>
    /// إضافة لاعب إلى المباراة
    /// </summary>
    public void AddPlayer(string playerId, string playerName, Vector3 position)
    {
        if (playersInMatch.ContainsKey(playerId))
            return;

        var playerData = new PlayerData
        {
            playerId = playerId,
            playerName = playerName,
            position = position,
            rotation = Quaternion.identity,
            health = 100f
        };

        playersInMatch[playerId] = playerData;
        OnPlayerJoined?.Invoke(playerId, playerData);

        Logger.Log($"Player added to match: {playerName}", "RealTimeMatchManager");
    }

    /// <summary>
    /// إزالة لاعب من المباراة
    /// </summary>
    public void RemovePlayer(string playerId)
    {
        if (playersInMatch.ContainsKey(playerId))
        {
            var playerData = playersInMatch[playerId];
            playersInMatch.Remove(playerId);
            OnPlayerLeft?.Invoke(playerId);

            Logger.Log($"Player removed from match: {playerData.playerName}", "RealTimeMatchManager");
        }
    }

    /// <summary>
    /// تحديث موضع لاعب
    /// </summary>
    public void UpdatePlayerPosition(string playerId, Vector3 position, Quaternion rotation)
    {
        if (playersInMatch.ContainsKey(playerId))
        {
            playersInMatch[playerId].position = position;
            playersInMatch[playerId].rotation = rotation;
        }
    }

    /// <summary>
    /// تزامن مواضع جميع اللاعبين
    /// </summary>
    private async void SyncPlayersPositions()
    {
        if (string.IsNullOrEmpty(currentMatchId) || playersInMatch.Count == 0)
            return;

        try
        {
            var message = new NetworkMessage
            {
                messageType = "MATCH_STATE_SYNC",
                data = new Dictionary<string, object>
                {
                    { "matchId", currentMatchId },
                    { "playersCount", playersInMatch.Count }
                }
            };

            await nakamaClient.SendMessage(message);

            // إخطار بتحديثات المواضع
            foreach (var kvp in playersInMatch)
            {
                OnPlayerPositionUpdated?.Invoke(kvp.Key, kvp.Value);
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogDebug($"Sync error: {ex.Message}", "RealTimeMatchManager");
        }
    }

    /// <summary>
    /// إنهاء المباراة
    /// </summary>
    public async Task<bool> EndMatch()
    {
        try
        {
            Logger.Log($"Ending match: {currentMatchId}...", "RealTimeMatchManager");

            // محاكاة إنهاء المباراة
            await Task.Delay(500);

            playersInMatch.Clear();
            currentMatchId = "";
            OnMatchEnded?.Invoke();

            Logger.Log("✓ Match ended", "RealTimeMatchManager");
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to end match: {ex.Message}", "RealTimeMatchManager");
            return false;
        }
    }

    // Getters
    public string CurrentMatchId => currentMatchId;
    public int PlayersCount => playersInMatch.Count;
    public Dictionary<string, PlayerData> PlayersInMatch => playersInMatch;
    public static RealTimeMatchManager Instance => instance;
}
