using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير الشراء - يتعامل مع عمليات الشراء داخل التطبيق
/// </summary>
public class InAppPurchaseManager : MonoBehaviour
{
    private static InAppPurchaseManager instance;

    public enum PlatformType
    {
        None,
        GooglePlay,
        AppleStore,
        Steam
    }

    private PlatformType currentPlatform = PlatformType.None;
    private List<IAProduct> products = new List<IAProduct>();
    private List<PurchaseTransaction> purchaseHistory = new List<PurchaseTransaction>();

    // Events
    public static event Action<IAProduct> OnProductPurchased;
    public static event Action<IAProduct> OnPurchaseFailed;
    public static event Action<List<IAProduct>> OnProductsLoaded;

    // Storage Keys
    private const string PURCHASE_HISTORY_KEY = "PurchaseHistory";

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
    /// تهيئة مدير الشراء
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        DetectPlatform();
        CreateProducts();
        LoadPurchaseHistory();

        isInitialized = true;

        Debug.Log($"InAppPurchaseManager Initialized - Platform: {currentPlatform}");
    }

    /// <summary>
    /// الكشف عن المنصة الحالية
    /// </summary>
    private void DetectPlatform()
    {
        #if UNITY_ANDROID
            currentPlatform = PlatformType.GooglePlay;
        #elif UNITY_IOS
            currentPlatform = PlatformType.AppleStore;
        #elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            currentPlatform = PlatformType.Steam;
        #else
            currentPlatform = PlatformType.None;
        #endif

        Debug.Log($"Platform detected: {currentPlatform}");
    }

    /// <summary>
    /// إنشاء المنتجات المتاحة
    /// </summary>
    private void CreateProducts()
    {
        products = new List<IAProduct>
        {
            new IAProduct
            {
                ProductId = "com.game.coins_500",
                Name = "500 Coins",
                Description = "500 game coins",
                Price = 0.99f,
                Currency = "USD",
                Coins = 500,
                Gems = 0
            },

            new IAProduct
            {
                ProductId = "com.game.coins_2500",
                Name = "2500 Coins",
                Description = "2500 game coins + bonus",
                Price = 4.99f,
                Currency = "USD",
                Coins = 2500,
                Gems = 0
            },

            new IAProduct
            {
                ProductId = "com.game.gems_100",
                Name = "100 Gems",
                Description = "100 premium gems",
                Price = 1.99f,
                Currency = "USD",
                Coins = 0,
                Gems = 100
            },

            new IAProduct
            {
                ProductId = "com.game.gems_600",
                Name = "600 Gems",
                Description = "600 premium gems + bonus",
                Price = 9.99f,
                Currency = "USD",
                Coins = 0,
                Gems = 600
            },

            new IAProduct
            {
                ProductId = "com.game.battle_pass",
                Name = "Battle Pass",
                Description = "Premium battle pass for this season",
                Price = 9.99f,
                Currency = "USD",
                Coins = 0,
                Gems = 0,
                IsBattlePass = true
            }
        };

        OnProductsLoaded?.Invoke(products);

        Debug.Log($"Created {products.Count} products");
    }

    /// <summary>
    /// شراء منتج
    /// </summary>
    public async Task<bool> PurchaseProduct(string productId)
    {
        var product = products.FirstOrDefault(p => p.ProductId == productId);

        if (product == null)
        {
            Debug.LogWarning($"Product not found: {productId}");
            return false;
        }

        try
        {
            Debug.Log($"Purchasing: {product.Name} ({product.Price}{product.Currency})");

            await Task.Delay(1000);

            // محاكاة العملية - في الحقيقة ستتصل بـ Platform SDK
            var transaction = new PurchaseTransaction
            {
                TransactionId = System.Guid.NewGuid().ToString(),
                ProductId = productId,
                PlayerId = PlayerManager.Instance?.CurrentProfile.playerId ?? "unknown",
                Amount = product.Price,
                Currency = product.Currency,
                PurchasedAt = DateTime.Now,
                IsSuccessful = true,
                Platform = currentPlatform.ToString()
            };

            purchaseHistory.Add(transaction);

            // إضافة المنتجات للاعب
            if (product.Coins > 0)
            {
                PlayerManager.Instance.AddCurrency(product.Coins, CurrencyType.Coins);
            }

            if (product.Gems > 0)
            {
                PlayerManager.Instance.AddCurrency(product.Gems, CurrencyType.Gems);
            }

            // تسجيل الشراء
            AnalyticsManager.Instance?.LogPurchase(
                transaction.PlayerId,
                productId,
                (int)product.Price,
                product.Currency
            );

            SavePurchaseHistory();
            OnProductPurchased?.Invoke(product);

            Debug.Log($"Purchase successful: {product.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Purchase failed: {ex.Message}");
            OnPurchaseFailed?.Invoke(product);
            return false;
        }
    }

    /// <summary>
    /// الحصول على المنتجات
    /// </summary>
    public List<IAProduct> GetProducts()
    {
        return new List<IAProduct>(products);
    }

    /// <summary>
    /// الحصول على منتج محدد
    /// </summary>
    public IAProduct GetProduct(string productId)
    {
        return products.FirstOrDefault(p => p.ProductId == productId);
    }

    /// <summary>
    /// الحصول على سجل الشراء
    /// </summary>
    public List<PurchaseTransaction> GetPurchaseHistory()
    {
        return new List<PurchaseTransaction>(purchaseHistory);
    }

    /// <summary>
    /// إرجاع منتج
    /// </summary>
    public async Task<bool> RefundProduct(string transactionId)
    {
        var transaction = purchaseHistory.FirstOrDefault(t => t.TransactionId == transactionId);

        if (transaction == null)
        {
            Debug.LogWarning("Transaction not found!");
            return false;
        }

        try
        {
            Debug.Log($"Refunding transaction: {transactionId}");

            await Task.Delay(800);

            transaction.IsSuccessful = false;

            SavePurchaseHistory();

            Debug.Log("Refund processed");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Refund failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// استعادة الشراءات السابقة
    /// </summary>
    public async Task<bool> RestorePurchases()
    {
        try
        {
            Debug.Log("Restoring purchases...");

            await Task.Delay(1000);

            // محاكاة استرجاع الشراءات
            Debug.Log($"Restored {purchaseHistory.Count} purchases");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Restore failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// حفظ سجل الشراء
    /// </summary>
    private void SavePurchaseHistory()
    {
        try
        {
            string json = JsonUtility.ToJson(new PurchaseTransactionList { transactions = purchaseHistory });
            PlayerPrefs.SetString(PURCHASE_HISTORY_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save purchase history: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل سجل الشراء
    /// </summary>
    private void LoadPurchaseHistory()
    {
        try
        {
            string json = PlayerPrefs.GetString(PURCHASE_HISTORY_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var list = JsonUtility.FromJson<PurchaseTransactionList>(json);
                purchaseHistory = list.transactions ?? new List<PurchaseTransaction>();

                Debug.Log($"Purchase history loaded ({purchaseHistory.Count} transactions)");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load purchase history: {ex.Message}");
        }
    }

    // Getters
    public static InAppPurchaseManager Instance => instance;
    public PlatformType CurrentPlatform => currentPlatform;
}

// ==================== Data Classes ====================

[System.Serializable]
public class IAProduct
{
    public string ProductId;
    public string Name;
    public string Description;
    public float Price;
    public string Currency;
    public int Coins;
    public int Gems;
    public bool IsBattlePass;
}

[System.Serializable]
public class PurchaseTransaction
{
    public string TransactionId;
    public string ProductId;
    public string PlayerId;
    public float Amount;
    public string Currency;
    public DateTime PurchasedAt;
    public bool IsSuccessful;
    public string Platform;
}

[System.Serializable]
public class PurchaseTransactionList
{
    public List<PurchaseTransaction> transactions = new List<PurchaseTransaction>();
}
