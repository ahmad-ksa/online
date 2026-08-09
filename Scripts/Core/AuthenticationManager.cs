using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// مدير المصادقة - يتعامل مع تسجيل الدخول والتسجيل والمصادقة
/// </summary>
public class AuthenticationManager : MonoBehaviour
{
    private static AuthenticationManager instance;

    public enum AuthState
    {
        Unauthenticated,
        Authenticating,
        Authenticated,
        RegistrationPending,
        PasswordReset,
        MFARequired,
        Error
    }

    private AuthState currentState = AuthState.Unauthenticated;
    private string currentUserId = "";
    private string currentEmail = "";
    private string currentUsername = "";

    // Events
    public static event Action<AuthState> OnAuthStateChanged;
    public static event Action<string> OnAuthError;
    public static event Action OnAuthSuccess;
    public static event Action OnRegistrationSuccess;
    public static event Action OnPasswordResetRequested;

    // Validation
    private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    private const int MIN_PASSWORD_LENGTH = 6;
    private const int MIN_USERNAME_LENGTH = 3;

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
    /// تهيئة مدير المصادقة
    /// </summary>
    public static void CreateInstance()
    {
        if (instance != null)
        {
            Debug.Log("AuthenticationManager already exists");
            return;
        }

        GameObject authObject = new GameObject("AuthenticationManager");
        instance = authObject.AddComponent<AuthenticationManager>();
        DontDestroyOnLoad(authObject);
    }

    private void Initialize()
    {
        if (isInitialized)
            return;

        // تحميل session محفوظة إذا كانت موجودة
        LoadSavedSession();

        isInitialized = true;
        Debug.Log("AuthenticationManager Initialized");
    }

    /// <summary>
    /// تحميل session محفوظة
    /// </summary>
    private void LoadSavedSession()
    {
        string savedUserId = PlayerPrefs.GetString("SavedUserId", "");
        string savedEmail = PlayerPrefs.GetString("SavedEmail", "");
        string savedUsername = PlayerPrefs.GetString("SavedUsername", "");
        bool rememberMe = PlayerPrefs.GetInt("RememberMe", 0) == 1;

        if (!string.IsNullOrEmpty(savedUserId) && rememberMe)
        {
            currentUserId = savedUserId;
            currentEmail = savedEmail;
            currentUsername = savedUsername;

            SetAuthState(AuthState.Authenticated);
            Debug.Log("Restored session from local storage");
        }
    }

    /// <summary>
    /// تسجيل دخول بـ Email و Password
    /// </summary>
    public async Task<bool> LoginWithEmail(string email, string password, bool rememberMe = false)
    {
        // التحقق من صحة البيانات
        if (!ValidateEmail(email))
        {
            HandleError("Invalid email format");
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            HandleError("Password cannot be empty");
            return false;
        }

        SetAuthState(AuthState.Authenticating);

        try
        {
            // إرسال طلب تسجيل الدخول للسيرفر
            var result = await SendLoginRequest(email, password);

            if (result.success)
            {
                // حفظ بيانات المستخدم
                currentUserId = result.userId;
                currentEmail = email;
                currentUsername = result.username;

                // حفظ محلياً إذا اختار Remember Me
                if (rememberMe)
                {
                    PlayerPrefs.SetString("SavedUserId", currentUserId);
                    PlayerPrefs.SetString("SavedEmail", currentEmail);
                    PlayerPrefs.SetString("SavedUsername", currentUsername);
                    PlayerPrefs.SetInt("RememberMe", 1);
                    PlayerPrefs.Save();
                }

                SetAuthState(AuthState.Authenticated);
                OnAuthSuccess?.Invoke();

                Debug.Log($"Login successful: {currentUsername}");
                return true;
            }
            else
            {
                HandleError(result.errorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            HandleError($"Login failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تسجيل حساب جديد
    /// </summary>
    public async Task<bool> Register(string email, string password, string confirmPassword, string username)
    {
        // التحقق من صحة البيانات
        var validationError = ValidateRegistration(email, password, confirmPassword, username);
        if (!string.IsNullOrEmpty(validationError))
        {
            HandleError(validationError);
            return false;
        }

        SetAuthState(AuthState.Authenticating);

        try
        {
            // إرسال طلب التسجيل للسيرفر
            var result = await SendRegistrationRequest(email, password, username);

            if (result.success)
            {
                SetAuthState(AuthState.RegistrationPending);
                OnRegistrationSuccess?.Invoke();

                Debug.Log("Registration successful! Please verify your email.");
                return true;
            }
            else
            {
                HandleError(result.errorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            HandleError($"Registration failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// طلب إعادة تعيين كلمة المرور
    /// </summary>
    public async Task<bool> RequestPasswordReset(string email)
    {
        if (!ValidateEmail(email))
        {
            HandleError("Invalid email format");
            return false;
        }

        try
        {
            var result = await SendPasswordResetRequest(email);

            if (result.success)
            {
                SetAuthState(AuthState.PasswordReset);
                OnPasswordResetRequested?.Invoke();

                Debug.Log("Password reset email sent");
                return true;
            }
            else
            {
                HandleError(result.errorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            HandleError($"Password reset failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تأكيد البريد الإلكتروني
    /// </summary>
    public async Task<bool> VerifyEmail(string email, string verificationCode)
    {
        try
        {
            var result = await SendEmailVerification(email, verificationCode);

            if (result.success)
            {
                Debug.Log("Email verified successfully");
                return true;
            }
            else
            {
                HandleError(result.errorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            HandleError($"Email verification failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تسجيل الخروج
    /// </summary>
    public void Logout()
    {
        Debug.Log("Logging out...");

        currentUserId = "";
        currentEmail = "";
        currentUsername = "";

        // حذف البيانات المحفوظة
        PlayerPrefs.DeleteKey("SavedUserId");
        PlayerPrefs.DeleteKey("SavedEmail");
        PlayerPrefs.DeleteKey("SavedUsername");
        PlayerPrefs.DeleteKey("RememberMe");
        PlayerPrefs.Save();

        SetAuthState(AuthState.Unauthenticated);
    }

    // ==================== Validation Methods ====================

    /// <summary>
    /// التحقق من صيغة البريد الإلكتروني
    /// </summary>
    private bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        return Regex.IsMatch(email, EMAIL_PATTERN);
    }

    /// <summary>
    /// التحقق من قوة كلمة المرور
    /// </summary>
    private bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        if (password.Length < MIN_PASSWORD_LENGTH)
            return false;

        // تحقق من وجود حروف وأرقام
        bool hasLetters = Regex.IsMatch(password, @"[a-zA-Z]");
        bool hasNumbers = Regex.IsMatch(password, @"[0-9]");

        return hasLetters && hasNumbers;
    }

    /// <summary>
    /// التحقق من اسم المستخدم
    /// </summary>
    private bool ValidateUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;

        if (username.Length < MIN_USERNAME_LENGTH)
            return false;

        // تحقق من أن الاسم لا يحتوي على أحرف خاصة
        return Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$");
    }

    /// <summary>
    /// التحقق من بيانات التسجيل
    /// </summary>
    private string ValidateRegistration(string email, string password, string confirmPassword, string username)
    {
        if (!ValidateEmail(email))
            return "Invalid email format";

        if (!ValidatePassword(password))
            return $"Password must be at least {MIN_PASSWORD_LENGTH} characters with letters and numbers";

        if (password != confirmPassword)
            return "Passwords do not match";

        if (!ValidateUsername(username))
            return $"Username must be at least {MIN_USERNAME_LENGTH} characters (alphanumeric, dash, underscore only)";

        return "";
    }

    // ==================== Server Communication ====================

    /// <summary>
    /// محاكاة طلب تسجيل الدخول
    /// </summary>
    private async Task<LoginResult> SendLoginRequest(string email, string password)
    {
        await Task.Delay(1000); // محاكاة تأخير الشبكة

        // في الواقع، سيتم إرسال الطلب إلى Nakama
        // هنا نحاكي النتيجة
        return new LoginResult
        {
            success = true,
            userId = System.Guid.NewGuid().ToString(),
            username = email.Split('@')[0],
            errorMessage = ""
        };
    }

    /// <summary>
    /// محاكاة طلب التسجيل
    /// </summary>
    private async Task<RegistrationResult> SendRegistrationRequest(string email, string password, string username)
    {
        await Task.Delay(1000); // محاكاة تأخير الشبكة

        return new RegistrationResult
        {
            success = true,
            userId = System.Guid.NewGuid().ToString(),
            errorMessage = ""
        };
    }

    /// <summary>
    /// محاكاة طلب إعادة تعيين كلمة المرور
    /// </summary>
    private async Task<GenericResult> SendPasswordResetRequest(string email)
    {
        await Task.Delay(1000);

        return new GenericResult
        {
            success = true,
            errorMessage = ""
        };
    }

    /// <summary>
    /// محاكاة تحقق البريد الإلكتروني
    /// </summary>
    private async Task<GenericResult> SendEmailVerification(string email, string code)
    {
        await Task.Delay(1000);

        return new GenericResult
        {
            success = true,
            errorMessage = ""
        };
    }

    // ==================== State Management ====================

    /// <summary>
    /// تغيير حالة المصادقة
    /// </summary>
    private void SetAuthState(AuthState newState)
    {
        if (newState == currentState)
            return;

        currentState = newState;
        OnAuthStateChanged?.Invoke(currentState);

        Debug.Log($"Auth State Changed: {currentState}");
    }

    /// <summary>
    /// معالجة الأخطاء
    /// </summary>
    private void HandleError(string errorMessage)
    {
        SetAuthState(AuthState.Error);
        OnAuthError?.Invoke(errorMessage);

        Debug.LogError($"Authentication Error: {errorMessage}");
    }

    // Getters
    public static AuthenticationManager Instance => instance;
    public AuthState CurrentState => currentState;
    public bool IsAuthenticated => currentState == AuthState.Authenticated;
    public string CurrentUserId => currentUserId;
    public string CurrentEmail => currentEmail;
    public string CurrentUsername => currentUsername;
}

// ==================== Result Classes ====================

[System.Serializable]
public class LoginResult
{
    public bool success;
    public string userId;
    public string username;
    public string errorMessage;
}

[System.Serializable]
public class RegistrationResult
{
    public bool success;
    public string userId;
    public string errorMessage;
}

[System.Serializable]
public class GenericResult
{
    public bool success;
    public string errorMessage;
}
