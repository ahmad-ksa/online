using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// مدير واجهة تسجيل الدخول - أونلاين فقط
/// </summary>
public class LoginUIManager : MonoBehaviour
{
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Text statusText;
    [SerializeField] private Text connectionStatusText;
    [SerializeField] private CanvasGroup loadingPanel;

    private GameStarter gameStarter;
    private GameManager gameManager;
    private bool isLoading = false;

    private void Start()
    {
        gameStarter = GetComponent<GameStarter>();

        // ربط الأزرار
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginClicked);
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClicked);

        UpdateConnectionStatus();
        InvokeRepeating("UpdateConnectionStatus", 0, 2f); // تحديث كل ثانيتين

        Logger.Log("LoginUIManager initialized", "LoginUIManager");
    }

    /// <summary>
    /// تحديث حالة الاتصال
    /// </summary>
    private void UpdateConnectionStatus()
    {
        if (connectionStatusText == null)
            return;

        if (gameStarter.IsOnline)
        {
            connectionStatusText.text = "<color=green>✓ متصل أونلاين</color>";
            connectionStatusText.color = Color.green;
        }
        else
        {
            connectionStatusText.text = "<color=red>✗ غير متصل</color>";
            connectionStatusText.color = Color.red;
        }
    }

    /// <summary>
    /// زر تسجيل الدخول
    /// </summary>
    private async void OnLoginClicked()
    {
        if (!gameStarter.IsOnline)
        {
            ShowError(GameConstants.ERROR_NO_INTERNET);
            return;
        }

        if (!ValidateInputs())
            return;

        await Login();
    }

    /// <summary>
    /// زر التسجيل الجديد
    /// </summary>
    private async void OnRegisterClicked()
    {
        if (!gameStarter.IsOnline)
        {
            ShowError(GameConstants.ERROR_NO_INTERNET);
            return;
        }

        if (!ValidateInputs())
            return;

        await Register();
    }

    /// <summary>
    /// تسجيل الدخول
    /// </summary>
    private async Task Login()
    {
        isLoading = true;
        ShowLoading(true);
        SetButtonsEnabled(false);

        try
        {
            string email = emailInput.text;
            string password = passwordInput.text;

            ShowStatus("جاري تسجيل الدخول...", Color.yellow);

            bool success = await gameStarter.Login(email, password);

            if (success)
            {
                ShowStatus("✓ تم تسجيل الدخول بنجاح!", Color.green);
                Logger.Log("Login successful", "LoginUIManager");

                // انتظر قليلاً ثم انتقل للعبة
                await Task.Delay(1000);
                LoadGameScene();
            }
            else
            {
                ShowError(GameConstants.ERROR_INVALID_CREDENTIALS);
            }
        }
        catch (System.Exception ex)
        {
            ShowError($"خطأ: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            ShowLoading(false);
            SetButtonsEnabled(true);
        }
    }

    /// <summary>
    /// تسجيل لاعب جديد
    /// </summary>
    private async Task Register()
    {
        isLoading = true;
        ShowLoading(true);
        SetButtonsEnabled(false);

        try
        {
            string email = emailInput.text;
            string password = passwordInput.text;
            string username = email.Split('@')[0]; // استخدم جزء البريد قبل @

            ShowStatus("جاري التسجيل...", Color.yellow);

            bool success = await gameStarter.Register(email, password, username);

            if (success)
            {
                ShowStatus("✓ تم التسجيل بنجاح! جاري دخول اللعبة...", Color.green);
                Logger.Log("Registration and login successful", "LoginUIManager");

                await Task.Delay(1000);
                LoadGameScene();
            }
            else
            {
                ShowError("فشل التسجيل. حاول مرة أخرى.");
            }
        }
        catch (System.Exception ex)
        {
            ShowError($"خطأ: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            ShowLoading(false);
            SetButtonsEnabled(true);
        }
    }

    /// <summary>
    /// التحقق من صحة المدخلات
    /// </summary>
    private bool ValidateInputs()
    {
        if (emailInput == null || string.IsNullOrEmpty(emailInput.text))
        {
            ShowError("أدخل البريد الإلكتروني");
            return false;
        }

        if (passwordInput == null || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowError("أدخل كلمة المرور");
            return false;
        }

        if (passwordInput.text.Length < 6)
        {
            ShowError("كلمة المرور يجب أن تكون 6 أحرف على الأقل");
            return false;
        }

        return true;
    }

    /// <summary>
    /// عرض رسالة الحالة
    /// </summary>
    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }

    /// <summary>
    /// عرض رسالة خطأ
    /// </summary>
    private void ShowError(string message)
    {
        ShowStatus($"❌ {message}", Color.red);
        Logger.LogWarning(message, "LoginUIManager");
    }

    /// <summary>
    /// عرض/إخفاء شاشة التحميل
    /// </summary>
    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.alpha = show ? 1f : 0f;
            loadingPanel.blocksRaycasts = show;
        }
    }

    /// <summary>
    /// تفعيل/تعطيل الأزرار
    /// </summary>
    private void SetButtonsEnabled(bool enabled)
    {
        if (loginButton != null)
            loginButton.interactable = enabled;
        if (registerButton != null)
            registerButton.interactable = enabled;
        if (emailInput != null)
            emailInput.interactable = enabled;
        if (passwordInput != null)
            passwordInput.interactable = enabled;
    }

    /// <summary>
    /// تحميل مشهد اللعبة
    /// </summary>
    private void LoadGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
