using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// مدير اللعبة الرئيسي - نقطة الدخول للنظام
/// يتحكم في حالة اللعبة والتنقل بين الـ Scenes
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private static GameManager instance;
    
    // الحالات الممكنة للعبة
    public enum GameState
    {
        Initializing,
        MainMenu,
        Login,
        Home,
        Lobby,
        InGame,
        Results,
        Settings,
        Paused
    }

    private GameState currentState = GameState.Initializing;
    private GameState previousState = GameState.MainMenu;

    // Events
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<GameState, GameState> OnGameStateTransition;
    public static event Action OnGameInitialized;

    // Platform
    private RuntimePlatform currentPlatform;
    private bool isMobile = false;

    // Configuration
    [SerializeField] private GameConfiguration gameConfig;
    private bool isInitialized = false;

    private void Awake()
    {
        // Singleton Pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // تحديد المنصة
        DetectPlatform();
    }

    private void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// تحديد نوع المنصة (Mobile/PC/Steam)
    /// </summary>
    private void DetectPlatform()
    {
        currentPlatform = Application.platform;

        isMobile = (currentPlatform == RuntimePlatform.Android || 
                    currentPlatform == RuntimePlatform.IPhonePlayer);

        Debug.Log($"Platform Detected: {currentPlatform} (Mobile: {isMobile})");
    }

    /// <summary>
    /// تهيئة اللعبة
    /// </summary>
    private void InitializeGame()
    {
        if (isInitialized)
            return;

        Debug.Log("=== Game Initialization Started ===");

        try
        {
            // تحميل الإعدادات
            LoadGameConfiguration();

            // تهيئة Managers
            InitializeManagers();

            // تعيين الحالة الأولية
            SetGameState(GameState.MainMenu);

            isInitialized = true;
            OnGameInitialized?.Invoke();

            Debug.Log("=== Game Initialization Completed ===");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize game: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل إعدادات اللعبة
    /// </summary>
    private void LoadGameConfiguration()
    {
        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfiguration>("GameConfig");
            
            if (gameConfig == null)
            {
                Debug.LogWarning("GameConfiguration not found! Creating default...");
                gameConfig = ScriptableObject.CreateInstance<GameConfiguration>();
            }
        }

        Debug.Log($"Game Config Loaded: {gameConfig.GameTitle} v{gameConfig.GameVersion}");
    }

    /// <summary>
    /// تهيئة جميع الـ Managers
    /// </summary>
    private void InitializeManagers()
    {
        // سيتم تفعيل الـ Managers من خلال Singleton Pattern
        NetworkManager.Initialize();
        PlayerManager.Initialize();
        ConfigurationManager.Initialize(gameConfig);

        Debug.Log("All Managers Initialized");
    }

    /// <summary>
    /// تغيير حالة اللعبة
    /// </summary>
    public void SetGameState(GameState newState)
    {
        if (newState == currentState)
            return;

        previousState = currentState;
        currentState = newState;

        Debug.Log($"Game State Changed: {previousState} -> {currentState}");

        OnGameStateChanged?.Invoke(currentState);
        OnGameStateTransition?.Invoke(previousState, currentState);

        HandleStateTransition(previousState, currentState);
    }

    /// <summary>
    /// معالجة انتقال الحالات
    /// </summary>
    private void HandleStateTransition(GameState from, GameState to)
    {
        switch (to)
        {
            case GameState.MainMenu:
                LoadScene("MainMenu");
                break;

            case GameState.Login:
                LoadScene("Login");
                break;

            case GameState.Home:
                LoadScene("Home");
                break;

            case GameState.Lobby:
                // سيتم التحكم به من Matchmaking System
                break;

            case GameState.InGame:
                // سيتم التحكم به من Game System
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.Results:
                // سيتم التحكم به من Results System
                break;
        }
    }

    /// <summary>
    /// تحميل Scene
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading Scene: {sceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// تحميل Scene بشكل Async
    /// </summary>
    public void LoadSceneAsync(string sceneName, System.Action onComplete = null)
    {
        StartCoroutine(LoadSceneAsyncRoutine(sceneName, onComplete));
    }

    private System.Collections.IEnumerator LoadSceneAsyncRoutine(string sceneName, System.Action onComplete)
    {
        var asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }

    // Getters
    public static GameManager Instance => instance;
    public GameState CurrentState => currentState;
    public GameState PreviousState => previousState;
    public bool IsMobile => isMobile;
    public RuntimePlatform Platform => currentPlatform;
    public GameConfiguration Config => gameConfig;
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// إيقاف اللعبة
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.InGame)
            return;

        SetGameState(GameState.Paused);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// استئناف اللعبة
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused)
            return;

        SetGameState(GameState.InGame);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// الخروج من اللعبة
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
