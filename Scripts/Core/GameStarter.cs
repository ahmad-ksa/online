using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// مدير بدء اللعبة والمصادقة - أونلاين فقط
/// </summary>
public class GameStarter : MonoBehaviour
{
    [SerializeField] private GameObject loginPanelPrefab;
    [SerializeField] private GameObject gameSceneName = "GameScene";

    private NetworkManager networkManager;
    private PlayerManager playerManager;
    private bool isInitialized = false;

    private async void Start()
    {
        networkManager = NetworkManager.Instance;
        playerManager = PlayerManager.Instance;

        Logger.Log("GameStarter initialized", "GameStarter");

        // التحقق من الاتصال الإلزامي
        await VerifyOnlineConnection();
    }

    /// <summary>
    /// التحقق من الاتصال الأونلاين (إلزامي)
    /// </summary>
    private async Task<bool> VerifyOnlineConnection()
    {
        try
        {
            Logger.Log("Verifying online connection...", "GameStarter");

            // محاولة الاتصال بالسيرفر
            bool connected = await networkManager.ConnectToServer();

            if (!connected)
            {
                Logger.LogError("No internet connection! Game requires online.", "GameStarter");
                ShowOfflineError();
                return false;
            }

            Logger.Log("✓ Online connection verified", "GameStarter");
            isInitialized = true;
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogCritical("Connection verification failed", ex, "GameStarter");
            ShowOfflineError();
            return false;
        }
    }

    /// <summary>
    /// عرض رسالة عدم الاتصال بالإنترنت
    /// </summary>
    private void ShowOfflineError()
    {
        // TODO: عرض UI خطأ الاتصال
        Debug.LogError("❌ This game requires internet connection!");
    }

    /// <summary>
    /// تسجيل الدخول
    /// </summary>
    public async Task<bool> Login(string email, string password)
    {
        if (!isInitialized)
        {
            Logger.LogError("Not connected to server!", "GameStarter");
            return false;
        }

        try
        {
            Logger.Log($"Logging in as {email}...", "GameStarter");

            bool authenticated = await networkManager.AuthenticatePlayer(email, password);

            if (authenticated)
            {
                Logger.Log("✓ Login successful", "GameStarter");
                await playerManager.LoadPlayerData();
                return true;
            }
            else
            {
                Logger.LogError("Invalid credentials", "GameStarter");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Login failed: {ex.Message}", "GameStarter");
            return false;
        }
    }

    /// <summary>
    /// تسجيل لاعب جديد
    /// </summary>
    public async Task<bool> Register(string email, string password, string username)
    {
        if (!isInitialized)
        {
            Logger.LogError("Not connected to server!", "GameStarter");
            return false;
        }

        try
        {
            Logger.Log($"Registering new player: {username}...", "GameStarter");

            bool registered = await networkManager.RegisterPlayer(email, password, username);

            if (registered)
            {
                Logger.Log("✓ Registration successful", "GameStarter");
                // الآن حاول تسجيل الدخول
                return await Login(email, password);
            }
            else
            {
                Logger.LogError("Registration failed", "GameStarter");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Registration failed: {ex.Message}", "GameStarter");
            return false;
        }
    }

    /// <summary>
    /// التحقق من الاتصال
    /// </summary>
    public bool IsOnline => networkManager != null && networkManager.IsConnected;
}
