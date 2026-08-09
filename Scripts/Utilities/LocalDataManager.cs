using UnityEngine;
using System;
using System.Threading.Tasks;
using System.IO;

/// <summary>
/// نظام حفظ البيانات المحلية
/// </summary>
public class LocalDataManager : MonoBehaviour
{
    private static LocalDataManager instance;
    private string savePath;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "GameData");
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        Logger.Log($"LocalDataManager initialized at: {savePath}", "LocalDataManager");
    }

    /// <summary>
    /// حفظ البيانات في ملف محلي
    /// </summary>
    public async Task<bool> SaveData<T>(string fileName, T data)
    {
        try
        {
            string filePath = Path.Combine(savePath, fileName + ".json");
            string json = JsonUtility.ToJson(data, true);

            await Task.Run(() => File.WriteAllText(filePath, json));

            Logger.Log($"Data saved: {fileName}", "LocalDataManager");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to save data: {ex.Message}", "LocalDataManager");
            return false;
        }
    }

    /// <summary>
    /// تحميل البيانات من ملف محلي
    /// </summary>
    public async Task<T> LoadData<T>(string fileName)
    {
        try
        {
            string filePath = Path.Combine(savePath, fileName + ".json");

            if (!File.Exists(filePath))
            {
                Logger.LogWarning($"File not found: {fileName}", "LocalDataManager");
                return default;
            }

            string json = "";
            await Task.Run(() => json = File.ReadAllText(filePath));

            T data = JsonUtility.FromJson<T>(json);

            Logger.Log($"Data loaded: {fileName}", "LocalDataManager");
            return data;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load data: {ex.Message}", "LocalDataManager");
            return default;
        }
    }

    /// <summary>
    /// حذف ملف بيانات
    /// </summary>
    public bool DeleteData(string fileName)
    {
        try
        {
            string filePath = Path.Combine(savePath, fileName + ".json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Logger.Log($"Data deleted: {fileName}", "LocalDataManager");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to delete data: {ex.Message}", "LocalDataManager");
            return false;
        }
    }

    /// <summary>
    /// التحقق من وجود ملف البيانات
    /// </summary>
    public bool HasData(string fileName)
    {
        string filePath = Path.Combine(savePath, fileName + ".json");
        return File.Exists(filePath);
    }

    public static LocalDataManager Instance => instance;
}