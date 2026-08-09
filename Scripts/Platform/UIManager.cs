using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// مدير الواجهات - يتعامل مع جميع الواجهات الرسومية
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    public enum UIPanel
    {
        MainMenu,
        Login,
        Home,
        Lobby,
        GameRoom,
        Shop,
        Leaderboard,
        Friends,
        Settings,
        Profile,
        Loading
    }

    private Dictionary<UIPanel, UIBase> activePanels = new Dictionary<UIPanel, UIBase>();
    private UIPanel currentPanel = UIPanel.MainMenu;

    // Events
    public static event Action<UIPanel> OnPanelOpened;
    public static event Action<UIPanel> OnPanelClosed;

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
    /// تهيئة مدير الواجهات
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        isInitialized = true;

        Debug.Log("UIManager Initialized");
    }

    /// <summary>
    /// فتح واجهة
    /// </summary>
    public void OpenPanel(UIPanel panel, bool closeOthers = true)
    {
        try
        {
            Debug.Log($"Opening panel: {panel}");

            if (closeOthers)
            {
                CloseAllPanels();
            }

            currentPanel = panel;
            OnPanelOpened?.Invoke(panel);

            Debug.Log($"Panel opened: {panel}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open panel: {ex.Message}");
        }
    }

    /// <summary>
    /// إغلاق واجهة
    /// </summary>
    public void ClosePanel(UIPanel panel)
    {
        try
        {
            Debug.Log($"Closing panel: {panel}");

            if (activePanels.ContainsKey(panel))
            {
                activePanels[panel].Close();
                activePanels.Remove(panel);
            }

            OnPanelClosed?.Invoke(panel);

            Debug.Log($"Panel closed: {panel}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to close panel: {ex.Message}");
        }
    }

    /// <summary>
    /// إغلاق جميع الواجهات
    /// </summary>
    public void CloseAllPanels()
    {
        foreach (var panel in activePanels.Values.ToList())
        {
            panel.Close();
        }

        activePanels.Clear();
    }

    /// <summary>
    /// عرض إشعار
    /// </summary>
    public void ShowNotification(string title, string message, float duration = 3f)
    {
        Debug.Log($"[NOTIFICATION] {title}: {message}");
    }

    /// <summary>
    /// عرض حوار تأكيد
    /// </summary>
    public void ShowConfirmDialog(string title, string message, Action onConfirm, Action onCancel)
    {
        Debug.Log($"[CONFIRM] {title}: {message}");
        onConfirm?.Invoke();
    }

    /// <summary>
    /// عرض شاشة تحميل
    /// </summary>
    public void ShowLoadingScreen(string message = "Loading...")
    {
        OpenPanel(UIPanel.Loading, true);
        Debug.Log($"Loading: {message}");
    }

    /// <summary>
    /// إغلاق شاشة التحميل
    /// </summary>
    public void HideLoadingScreen()
    {
        ClosePanel(UIPanel.Loading);
    }

    /// <summary>
    /// تحديث شريط الصحة
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float percentage = (currentHealth / maxHealth) * 100f;
        Debug.Log($"Health: {percentage}%");
    }

    /// <summary>
    /// تحديث شريط الخبرة
    /// </summary>
    public void UpdateXPBar(float currentXP, float maxXP)
    {
        float percentage = (currentXP / maxXP) * 100f;
        Debug.Log($"XP: {percentage}%");
    }

    /// <summary>
    /// تحديث عرض الاقتصاد
    /// </summary>
    public void UpdateCurrencyDisplay(int coins, int gems)
    {
        Debug.Log($"Currency - Coins: {coins}, Gems: {gems}");
    }

    /// <summary>
    /// تحديث عرض اللاعب
    /// </summary>
    public void UpdatePlayerDisplay(string playerName, int level, int rank)
    {
        Debug.Log($"Player - {playerName} Level {level} (Rank #{rank})");
    }

    /// <summary>
    /// عرض خطأ
    /// </summary>
    public void ShowError(string title, string message)
    {
        Debug.LogError($"[ERROR] {title}: {message}");
        ShowNotification(title, message, 5f);
    }

    /// <summary>
    /// عرض رسالة نجاح
    /// </summary>
    public void ShowSuccess(string title, string message)
    {
        Debug.Log($"[SUCCESS] {title}: {message}");
        ShowNotification(title, message, 3f);
    }

    /// <summary>
    /// الحصول على الواجهة الحالية
    /// </summary>
    public UIPanel GetCurrentPanel()
    {
        return currentPanel;
    }

    /// <summary>
    /// التحقق من فتح واجهة
    /// </summary>
    public bool IsPanelOpen(UIPanel panel)
    {
        return activePanels.ContainsKey(panel);
    }

    // Getters
    public static UIManager Instance => instance;
}

// ==================== Base UI Class ====================

public abstract class UIBase : MonoBehaviour
{
    public virtual void Open() { }
    public virtual void Close() { }
    public virtual void Refresh() { }
}

// ==================== Specific UI Panels ====================

public class MainMenuUI : UIBase
{
    public override void Open()
    {
        Debug.Log("MainMenuUI opened");
    }

    public override void Close()
    {
        Debug.Log("MainMenuUI closed");
    }
}

public class LoginUI : UIBase
{
    public override void Open()
    {
        Debug.Log("LoginUI opened");
    }

    public override void Close()
    {
        Debug.Log("LoginUI closed");
    }
}

public class HomeUI : UIBase
{
    public override void Open()
    {
        Debug.Log("HomeUI opened");
    }

    public override void Close()
    {
        Debug.Log("HomeUI closed");
    }

    public override void Refresh()
    {
        Debug.Log("HomeUI refreshed");
    }
}

public class LobbyUI : UIBase
{
    public override void Open()
    {
        Debug.Log("LobbyUI opened");
    }

    public override void Close()
    {
        Debug.Log("LobbyUI closed");
    }
}

public class GameRoomUI : UIBase
{
    public override void Open()
    {
        Debug.Log("GameRoomUI opened");
    }

    public override void Close()
    {
        Debug.Log("GameRoomUI closed");
    }
}

public class ShopUI : UIBase
{
    public override void Open()
    {
        Debug.Log("ShopUI opened");
    }

    public override void Close()
    {
        Debug.Log("ShopUI closed");
    }

    public override void Refresh()
    {
        Debug.Log("ShopUI refreshed");
    }
}

public class LeaderboardUI : UIBase
{
    public override void Open()
    {
        Debug.Log("LeaderboardUI opened");
    }

    public override void Close()
    {
        Debug.Log("LeaderboardUI closed");
    }

    public override void Refresh()
    {
        Debug.Log("LeaderboardUI refreshed");
    }
}

public class FriendsUI : UIBase
{
    public override void Open()
    {
        Debug.Log("FriendsUI opened");
    }

    public override void Close()
    {
        Debug.Log("FriendsUI closed");
    }

    public override void Refresh()
    {
        Debug.Log("FriendsUI refreshed");
    }
}

public class SettingsUI : UIBase
{
    public override void Open()
    {
        Debug.Log("SettingsUI opened");
    }

    public override void Close()
    {
        Debug.Log("SettingsUI closed");
    }
}

public class ProfileUI : UIBase
{
    public override void Open()
    {
        Debug.Log("ProfileUI opened");
    }

    public override void Close()
    {
        Debug.Log("ProfileUI closed");
    }

    public override void Refresh()
    {
        Debug.Log("ProfileUI refreshed");
    }
}

public class LoadingScreenUI : UIBase
{
    public override void Open()
    {
        Debug.Log("LoadingScreenUI opened");
    }

    public override void Close()
    {
        Debug.Log("LoadingScreenUI closed");
    }
}
