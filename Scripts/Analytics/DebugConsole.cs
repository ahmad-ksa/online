using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// وحدة تصحيح - توفر واجهة لتصحيح وتشخيص المشاكل أثناء اللعب
/// </summary>
public class DebugConsole : MonoBehaviour
{
    private static DebugConsole instance;

    private bool isOpen = false;
    private Vector2 scrollPosition = Vector2.zero;
    private string commandInput = "";

    private List<string> consoleLogs = new List<string>();
    private Dictionary<string, DebugCommand> commands = new Dictionary<string, DebugCommand>();

    // UI
    private GUIStyle consoleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle textStyle;

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
        // فتح/إغلاق القائمة بـ ~
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            isOpen = !isOpen;
        }

        // تنفيذ الأمر بـ Enter
        if (Input.GetKeyDown(KeyCode.Return) && isOpen && !string.IsNullOrEmpty(commandInput))
        {
            ExecuteCommand(commandInput);
            commandInput = "";
        }
    }

    private void OnGUI()
    {
        if (!isOpen)
            return;

        // رسم خلفية شفافة
        GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height * 0.4f), "");

        // رسم السجلات
        DrawConsoleLogs();

        // رسم حقل الإدخال
        DrawCommandInput();

        // رسم الأوامر المتاحة
        DrawAvailableCommands();
    }

    /// <summary>
    /// تهيئة وحدة التصحيح
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        RegisterDefaultCommands();

        isInitialized = true;

        Log("Debug Console Initialized");
        Log("Press ~ to toggle console");
        Log("Type 'help' for available commands");
    }

    /// <summary>
    /// تسجيل رسالة
    /// </summary>
    public void Log(string message)
    {
        string logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
        consoleLogs.Add(logMessage);

        // احفظ آخر 100 رسالة فقط
        if (consoleLogs.Count > 100)
            consoleLogs.RemoveAt(0);

        Debug.Log(logMessage);
    }

    /// <summary>
    /// رسم السجلات
    /// </summary>
    private void DrawConsoleLogs()
    {
        Rect logArea = new Rect(10, 10, Screen.width - 20, Screen.height * 0.3f - 60);

        GUI.Box(logArea, "");
        scrollPosition = GUI.BeginScrollView(logArea, scrollPosition, 
            new Rect(0, 0, Screen.width - 40, consoleLogs.Count * 20));

        float yPosition = 0;
        foreach (var log in consoleLogs)
        {
            GUI.Label(new Rect(0, yPosition, Screen.width - 40, 20), log);
            yPosition += 20;
        }

        GUI.EndScrollView();
    }

    /// <summary>
    /// رسم حقل الإدخال
    /// </summary>
    private void DrawCommandInput()
    {
        float inputY = Screen.height * 0.3f - 35;
        GUI.Label(new Rect(10, inputY, 50, 25), "> ");

        commandInput = GUI.TextField(new Rect(60, inputY, Screen.width - 80, 25), commandInput);

        if (GUI.Button(new Rect(Screen.width - 90, inputY, 80, 25), "Execute"))
        {
            ExecuteCommand(commandInput);
            commandInput = "";
        }
    }

    /// <summary>
    /// رسم الأوامر المتاحة
    /// </summary>
    private void DrawAvailableCommands()
    {
        float commandsY = Screen.height * 0.3f - 5;
        GUI.Label(new Rect(10, commandsY, 200, 20), $"Available Commands: {commands.Count}");
    }

    /// <summary>
    /// تنفيذ أمر
    /// </summary>
    private void ExecuteCommand(string input)
    {
        if (string.IsNullOrEmpty(input))
            return;

        string[] parts = input.Split(' ');
        string commandName = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        Log($"$ {input}");

        if (commands.ContainsKey(commandName))
        {
            try
            {
                commands[commandName].Execute(args);
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }
        else
        {
            Log($"Unknown command: {commandName}");
        }
    }

    /// <summary>
    /// تسجيل أمر جديد
    /// </summary>
    public void RegisterCommand(string name, DebugCommand command)
    {
        commands[name.ToLower()] = command;
    }

    /// <summary>
    /// تسجيل الأوامر الافتراضية
    /// </summary>
    private void RegisterDefaultCommands()
    {
        // أمر Help
        RegisterCommand("help", new DebugCommand(() =>
        {
            Log("Available commands:");
            foreach (var cmd in commands.Keys)
            {
                Log($"  - {cmd}");
            }
        }));

        // أمر Clear
        RegisterCommand("clear", new DebugCommand(() =>
        {
            consoleLogs.Clear();
            Log("Console cleared");
        }));

        // أمر Stats
        RegisterCommand("stats", new DebugCommand(() =>
        {
            Log("=== Game Stats ===");
            if (PlayerManager.Instance != null)
            {
                Log($"Player: {PlayerManager.Instance.CurrentProfile.playerName}");
                Log($"Level: {PlayerManager.Instance.CurrentStats.currentLevel}");
                Log($"Coins: {PlayerManager.Instance.CurrentStats.coins}");
            }

            if (AnalyticsManager.Instance != null)
            {
                var stats = AnalyticsManager.Instance.GetSessionStats();
                Log($"Sessions: {stats.TotalSessions}");
                Log($"Total Play Time: {stats.TotalPlayTime}s");
            }

            Log($"FPS: {(int)(1f / Time.deltaTime)}");
            Log($"Memory: {SystemInfo.systemMemorySize}MB");
        }));

        // أمر Health (اختبار)
        RegisterCommand("health", new DebugCommand((args) =>
        {
            int health = int.Parse(args.Length > 0 ? args[0] : "100");
            Log($"Health set to: {health}");
        }));

        // أمر Level
        RegisterCommand("level", new DebugCommand((args) =>
        {
            if (args.Length > 0 && int.TryParse(args[0], out int level))
            {
                if (LevelingSystem.Instance != null)
                {
                    LevelingSystem.Instance.SetLevel(level);
                    Log($"Level set to: {level}");
                }
            }
            else
            {
                Log("Usage: level <number>");
            }
        }));

        // أمر XP
        RegisterCommand("xp", new DebugCommand((args) =>
        {
            if (args.Length > 0 && int.TryParse(args[0], out int xp))
            {
                if (LevelingSystem.Instance != null)
                {
                    LevelingSystem.Instance.AddXP(xp);
                    Log($"Added {xp} XP");
                }
            }
            else
            {
                Log("Usage: xp <amount>");
            }
        }));

        // أمر Coins
        RegisterCommand("coins", new DebugCommand((args) =>
        {
            if (args.Length > 0 && int.TryParse(args[0], out int coins))
            {
                if (PlayerManager.Instance != null)
                {
                    PlayerManager.Instance.AddCurrency(coins, CurrencyType.Coins);
                    Log($"Added {coins} coins");
                }
            }
            else
            {
                Log("Usage: coins <amount>");
            }
        }));

        // أمر Errors
        RegisterCommand("errors", new DebugCommand(() =>
        {
            if (CrashReporter.Instance != null)
            {
                var stats = CrashReporter.Instance.GetErrorStats();
                Log($"=== Error Stats ===");
                Log($"Total Errors: {stats.TotalErrors}");
                Log($"Warnings: {stats.TotalWarnings}");
                Log($"Critical: {stats.CriticalErrors}");
                Log($"Last Hour: {stats.ErrorsLastHour}");
                Log($"Most Common: {stats.MostCommonError}");
            }
        }));

        // أمر Quit
        RegisterCommand("quit", new DebugCommand(() =>
        {
            Log("Quitting game...");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }));

        Log("Default commands registered");
    }

    // Getters
    public static DebugConsole Instance => instance;
    public bool IsOpen => isOpen;
}

// ==================== Data Classes ====================

public class DebugCommand
{
    private Action<string[]> executeWithArgs;
    private Action executeNoArgs;

    public DebugCommand(Action execute)
    {
        executeNoArgs = execute;
    }

    public DebugCommand(Action<string[]> execute)
    {
        executeWithArgs = execute;
    }

    public void Execute(string[] args)
    {
        if (executeWithArgs != null)
        {
            executeWithArgs.Invoke(args);
        }
        else if (executeNoArgs != null)
        {
            executeNoArgs.Invoke();
        }
    }
}
