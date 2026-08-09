using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// مدير اللعبة الرئيسي - متعدد لاعبين أونلاين فقط
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int maxPlayers = 100;

    private NetworkManager networkManager;
    private PlayerManager playerManager;
    private FriendsManager friendsManager;
    private ChatManager chatManager;
    private VoiceChatManager voiceChatManager;

    private Dictionary<string, GameObject> activePlayers = new Dictionary<string, GameObject>();
    private bool isGameStarted = false;
    private bool isOnline = false;

    // Events
    public event System.Action OnGameStarted;
    public event System.Action<string> OnPlayerJoined;
    public event System.Action<string> OnPlayerLeft;
    public event System.Action OnConnectionLost;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Logger.Log("GameManager initialized", "GameManager");
    }

    private async void Start()
    {
        networkManager = NetworkManager.Instance;
        playerManager = PlayerManager.Instance;
        friendsManager = FriendsManager.Instance;
        chatManager = ChatManager.Instance;
        voiceChatManager = VoiceChatManager.Instance;

        // الاتصال الإلزامي بالسيرفر
        await InitializeOnlineGame();
    }

    /// <summary>
    /// تهيئة اللعبة الأونلاين (إلزامي)
    /// </summary>
    private async Task<bool> InitializeOnlineGame()
    {
        try
        {
            Logger.Log("Initializing online game...", "GameManager");

            // 1. الاتصال بالسيرفر
            bool connected = await networkManager.ConnectToServer();
            if (!connected)
            {
                Logger.LogError("Failed to connect to server! Game cannot start.", "GameManager");
                ShowConnectionErrorUI("فشل الاتصال بالسيرفر");
                return false;
            }

            Logger.Log("✓ Connected to server", "GameManager");

            // 2. مصادقة اللاعب
            var playerProfile = playerManager.CurrentProfile;
            if (playerProfile == null)
            {
                Logger.LogError("Player profile not loaded!", "GameManager");
                return false;
            }

            bool authenticated = await networkManager.AuthenticatePlayer(
                playerProfile.email,
                "password" // في الحقيقة يجب تخزين كلمة المرور بأمان
            );

            if (!authenticated)
            {
                Logger.LogError("Authentication failed!", "GameManager");
                ShowConnectionErrorUI("فشلت المصادقة");
                return false;
            }

            Logger.Log("✓ Player authenticated", "GameManager");

            // 3. تحميل بيانات اللاعب
            await playerManager.LoadPlayerData();
            Logger.Log("✓ Player data loaded", "GameManager");

            // 4. تحميل الأصدقاء
            await friendsManager.LoadFriends();
            Logger.Log("✓ Friends list loaded", "GameManager");

            // 5. بدء اللعبة
            isOnline = true;
            isGameStarted = true;
            OnGameStarted?.Invoke();

            // 6. إنشاء اللاعب الحالي
            SpawnLocalPlayer();

            Logger.Log("✅ Online game started successfully!", "GameManager");
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogCritical("Failed to initialize online game", ex, "GameManager");
            ShowConnectionErrorUI("خطأ في تهيئة اللعبة");
            return false;
        }
    }

    /// <summary>
    /// إنشاء اللاعب الحالي في الماب
    /// </summary>
    private void SpawnLocalPlayer()
    {
        try
        {
            if (playerPrefab == null)
            {
                Logger.LogError("Player prefab not assigned!", "GameManager");
                return;
            }

            Vector3 spawnPos = playerSpawnPoint != null 
                ? playerSpawnPoint.position 
                : Vector3.zero;

            GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            playerObj.name = "LocalPlayer";
            playerObj.tag = "LocalPlayer";

            var controller = playerObj.GetComponent<PlayerController>();
            var networkSync = playerObj.GetComponent<PlayerNetworkSync>();

            if (controller != null && networkSync != null)
            {
                var profile = playerManager.CurrentProfile;
                controller.SetPlayerData(profile.playerId, profile.username);
                networkSync.SetRemotePlayerData(profile.playerId, profile.username);

                activePlayers[profile.playerId] = playerObj;
                OnPlayerJoined?.Invoke(profile.playerId);

                Logger.Log($"Local player spawned: {profile.username}", "GameManager");
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to spawn player: {ex.Message}", "GameManager");
        }
    }

    /// <summary>
    /// استقبال لاعب جديد من السيرفر
    /// </summary>
    public void SpawnRemotePlayer(string playerId, string playerName, Vector3 position)
    {
        try
        {
            if (activePlayers.ContainsKey(playerId))
                return; // اللاعب موجود بالفعل

            if (playerPrefab == null)
            {
                Logger.LogError("Player prefab not assigned!", "GameManager");
                return;
            }

            GameObject playerObj = Instantiate(playerPrefab, position, Quaternion.identity);
            playerObj.name = $"RemotePlayer_{playerName}";
            playerObj.tag = "RemotePlayer";

            var networkSync = playerObj.GetComponent<PlayerNetworkSync>();
            if (networkSync != null)
            {
                networkSync.SetRemotePlayerData(playerId, playerName);
                activePlayers[playerId] = playerObj;
                OnPlayerJoined?.Invoke(playerId);

                Logger.Log($"Remote player spawned: {playerName}", "GameManager");
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to spawn remote player: {ex.Message}", "GameManager");
        }
    }

    /// <summary>
    /// إزالة لاعب من اللعبة
    /// </summary>
    public void RemovePlayer(string playerId)
    {
        if (activePlayers.ContainsKey(playerId))
        {
            Destroy(activePlayers[playerId]);
            activePlayers.Remove(playerId);
            OnPlayerLeft?.Invoke(playerId);

            Logger.Log($"Player removed: {playerId}", "GameManager");
        }
    }

    /// <summary>
    /// عرض رسالة خطأ الاتصال
    /// </summary>
    private void ShowConnectionErrorUI(string message)
    {
        // TODO: عرض UI قائمة على الخطأ
        Logger.LogError(message, "GameManager");
    }

    /// <summary>
    /// التحقق من الاتصال بالسيرفر
    /// </summary>
    public bool IsOnline => isOnline && networkManager.IsConnected;

    /// <summary>
    /// التحقق من بدء اللعبة
    /// </summary>
    public bool IsGameStarted => isGameStarted && IsOnline;

    // Getters
    public static GameManager Instance => instance;
    public Dictionary<string, GameObject> ActivePlayers => activePlayers;
    public int PlayerCount => activePlayers.Count;
}
