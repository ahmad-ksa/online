using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير الحفظ المتعدد المنصات - يتعامل مع حفظ البيانات والتزامنها
/// </summary>
public class CrossSaveManager : MonoBehaviour
{
    private static CrossSaveManager instance;

    public enum SyncStatus
    {
        NotSynced,
        Syncing,
        Synced,
        Error
    }

    private Dictionary<string, SaveData> localSaves = new Dictionary<string, SaveData>();
    private Dictionary<string, SaveData> cloudSaves = new Dictionary<string, SaveData>();
    private SyncStatus currentSyncStatus = SyncStatus.NotSynced;

    // Events
    public static event Action<SyncStatus> OnSyncStatusChanged;
    public static event Action<SaveData> OnSaveSynced;
    public static event Action<string> OnSyncFailed;

    // Storage Keys
    private const string SAVE_DATA_KEY = "SaveData";
    private const string LAST_SYNC_KEY = "LastSyncTime";

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
    /// تهيئة مدير الحفظ
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadLocalSaves();

        isInitialized = true;

        Debug.Log("CrossSaveManager Initialized");
    }

    /// <summary>
    /// حفظ البيانات محلياً
    /// </summary>
    public async Task<bool> SaveLocal(string slotName, bool autoSync = true)
    {
        try
        {
            Debug.Log($"Saving locally: {slotName}");

            await Task.Delay(500);

            var saveData = new SaveData
            {
                SlotName = slotName,
                PlayerId = PlayerManager.Instance?.CurrentProfile.playerId ?? "unknown",
                SaveTime = DateTime.Now,
                GameMode = "default",
                Level = PlayerManager.Instance?.CurrentStats.currentLevel ?? 1,
                Coins = PlayerManager.Instance?.CurrentStats.coins ?? 0,
                Gems = PlayerManager.Instance?.CurrentStats.gems ?? 0,
                IsSynced = false
            };

            localSaves[slotName] = saveData;
            SaveToPlayerPrefs();

            if (autoSync)
            {
                await SyncToCloud();
            }

            Debug.Log($"Local save created: {slotName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save locally: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تحميل البيانات محلياً
    /// </summary>
    public async Task<SaveData> LoadLocal(string slotName)
    {
        try
        {
            Debug.Log($"Loading locally: {slotName}");

            await Task.Delay(300);

            if (localSaves.ContainsKey(slotName))
            {
                Debug.Log($"Local save loaded: {slotName}");
                return localSaves[slotName];
            }

            Debug.LogWarning($"Save slot not found: {slotName}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load locally: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// مزامنة إلى السحابة
    /// </summary>
    public async Task<bool> SyncToCloud()
    {
        try
        {
            UpdateSyncStatus(SyncStatus.Syncing);

            Debug.Log("Syncing to cloud...");

            await Task.Delay(1000);

            // في بيئة حقيقية، ترسل البيانات إلى السيرفر
            foreach (var save in localSaves.Values)
            {
                save.IsSynced = true;
                cloudSaves[save.SlotName] = save;

                OnSaveSynced?.Invoke(save);
            }

            UpdateLastSyncTime();
            UpdateSyncStatus(SyncStatus.Synced);

            Debug.Log("Cloud sync completed");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to sync to cloud: {ex.Message}");
            OnSyncFailed?.Invoke(ex.Message);
            UpdateSyncStatus(SyncStatus.Error);
            return false;
        }
    }

    /// <summary>
    /// مزامنة من السحابة
    /// </summary>
    public async Task<bool> SyncFromCloud()
    {
        try
        {
            UpdateSyncStatus(SyncStatus.Syncing);

            Debug.Log("Syncing from cloud...");

            await Task.Delay(1000);

            // في بيئة حقيقية، تحميل البيانات من السيرفر
            foreach (var save in cloudSaves.Values)
            {
                localSaves[save.SlotName] = save;
                OnSaveSynced?.Invoke(save);
            }

            SaveToPlayerPrefs();
            UpdateLastSyncTime();
            UpdateSyncStatus(SyncStatus.Synced);

            Debug.Log("Cloud sync from completed");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to sync from cloud: {ex.Message}");
            OnSyncFailed?.Invoke(ex.Message);
            UpdateSyncStatus(SyncStatus.Error);
            return false;
        }
    }

    /// <summary>
    /// حل النزاعات
    /// </summary>
    public async Task<bool> ResolveConflict(string slotName, bool preferLocal)
    {
        try
        {
            Debug.Log($"Resolving conflict: {slotName} (Prefer Local: {preferLocal})");

            await Task.Delay(400);

            SaveData chosenSave = preferLocal ? localSaves[slotName] : cloudSaves[slotName];

            // التحديث الآخر
            if (preferLocal)
            {
                cloudSaves[slotName] = chosenSave;
            }
            else
            {
                localSaves[slotName] = chosenSave;
            }

            chosenSave.IsSynced = true;
            SaveToPlayerPrefs();

            Debug.Log("Conflict resolved");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to resolve conflict: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// حذف حفظة
    /// </summary>
    public async Task<bool> DeleteSave(string slotName)
    {
        try
        {
            Debug.Log($"Deleting save: {slotName}");

            await Task.Delay(300);

            localSaves.Remove(slotName);
            cloudSaves.Remove(slotName);

            SaveToPlayerPrefs();

            Debug.Log("Save deleted");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete save: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// الحصول على جميع الحفظات
    /// </summary>
    public List<SaveData> GetAllSaves()
    {
        return new List<SaveData>(localSaves.Values);
    }

    /// <summary>
    /// الحصول على معلومات المزامنة
    /// </summary>
    public SyncInfo GetSyncInfo()
    {
        return new SyncInfo
        {
            CurrentStatus = currentSyncStatus,
            LocalSaveCount = localSaves.Count,
            CloudSaveCount = cloudSaves.Count,
            LastSyncTime = GetLastSyncTime(),
            IsSynced = localSaves.All(s => s.Value.IsSynced)
        };
    }

    /// <summary>
    /// تحديث حالة المزامنة
    /// </summary>
    private void UpdateSyncStatus(SyncStatus status)
    {
        currentSyncStatus = status;
        OnSyncStatusChanged?.Invoke(status);
    }

    /// <summary>
    /// تحديث وقت آخر مزامنة
    /// </summary>
    private void UpdateLastSyncTime()
    {
        PlayerPrefs.SetString(LAST_SYNC_KEY, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// الحصول على وقت آخر مزامنة
    /// </summary>
    private DateTime GetLastSyncTime()
    {
        string timeStr = PlayerPrefs.GetString(LAST_SYNC_KEY, "");
        if (DateTime.TryParse(timeStr, out var time))
            return time;

        return DateTime.MinValue;
    }

    /// <summary>
    /// حفظ البيانات في PlayerPrefs
    /// </summary>
    private void SaveToPlayerPrefs()
    {
        try
        {
            var saveList = new SaveDataList { saves = localSaves.Values.ToList() };
            string json = JsonUtility.ToJson(saveList);
            PlayerPrefs.SetString(SAVE_DATA_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save to PlayerPrefs: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل البيانات من PlayerPrefs
    /// </summary>
    private void LoadLocalSaves()
    {
        try
        {
            string json = PlayerPrefs.GetString(SAVE_DATA_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var saveList = JsonUtility.FromJson<SaveDataList>(json);
                localSaves.Clear();

                foreach (var save in saveList.saves)
                {
                    localSaves[save.SlotName] = save;
                }

                Debug.Log($"Loaded {localSaves.Count} local saves");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load local saves: {ex.Message}");
        }
    }

    // Getters
    public static CrossSaveManager Instance => instance;
    public SyncStatus CurrentSyncStatus => currentSyncStatus;
}

// ==================== Data Classes ====================

[System.Serializable]
public class SaveData
{
    public string SlotName;
    public string PlayerId;
    public DateTime SaveTime;
    public string GameMode;
    public int Level;
    public int Coins;
    public int Gems;
    public bool IsSynced;
}

[System.Serializable]
public class SyncInfo
{
    public CrossSaveManager.SyncStatus CurrentStatus;
    public int LocalSaveCount;
    public int CloudSaveCount;
    public DateTime LastSyncTime;
    public bool IsSynced;
}

[System.Serializable]
public class SaveDataList
{
    public List<SaveData> saves = new List<SaveData>();
}
