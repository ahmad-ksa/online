using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// مدير النماذج (Prefabs) - تحميل وإنشاء الكائنات
/// </summary>
public class PrefabManager : MonoBehaviour
{
    private static PrefabManager instance;

    [SerializeField]
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    private const string PREFABS_PATH = "Prefabs/";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Logger.Log("PrefabManager initialized", "PrefabManager");
    }

    /// <summary>
    /// تحميل Prefab من Resources
    /// </summary>
    public GameObject LoadPrefab(string prefabName)
    {
        if (prefabCache.ContainsKey(prefabName))
        {
            return prefabCache[prefabName];
        }

        try
        {
            GameObject prefab = Resources.Load<GameObject>(PREFABS_PATH + prefabName);

            if (prefab == null)
            {
                Logger.LogError($"Prefab not found: {prefabName}", "PrefabManager");
                return null;
            }

            prefabCache[prefabName] = prefab;
            Logger.LogDebug($"Prefab loaded: {prefabName}", "PrefabManager");
            return prefab;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to load prefab {prefabName}: {ex.Message}", "PrefabManager");
            return null;
        }
    }

    /// <summary>
    /// إنشاء كائن من Prefab
    /// </summary>
    public GameObject InstantiatePrefab(string prefabName, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = LoadPrefab(prefabName);

        if (prefab == null)
            return null;

        try
        {
            GameObject instance = Instantiate(prefab, position, rotation);
            instance.name = prefabName;
            Logger.LogDebug($"Prefab instantiated: {prefabName}", "PrefabManager");
            return instance;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to instantiate prefab {prefabName}: {ex.Message}", "PrefabManager");
            return null;
        }
    }

    /// <summary>
    /// إنشاء كائن بدون موضع محدد
    /// </summary>
    public GameObject InstantiatePrefab(string prefabName)
    {
        return InstantiatePrefab(prefabName, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// إنشاء كائن مع parent
    /// </summary>
    public GameObject InstantiatePrefab(string prefabName, Transform parent)
    {
        GameObject prefab = LoadPrefab(prefabName);

        if (prefab == null)
            return null;

        try
        {
            GameObject instance = Instantiate(prefab, parent);
            instance.name = prefabName;
            Logger.LogDebug($"Prefab instantiated with parent: {prefabName}", "PrefabManager");
            return instance;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to instantiate prefab with parent {prefabName}: {ex.Message}", "PrefabManager");
            return null;
        }
    }

    /// <summary>
    /// حذف كائن
    /// </summary>
    public void DestroyGameObject(GameObject obj)
    {
        if (obj != null)
        {
            Destroy(obj);
            Logger.LogDebug($"GameObject destroyed: {obj.name}", "PrefabManager");
        }
    }

    /// <summary>
    /// تفريغ الـ Cache
    /// </summary>
    public void ClearCache()
    {
        prefabCache.Clear();
        Logger.Log("Prefab cache cleared", "PrefabManager");
    }

    /// <summary>
    /// الحصول على عدد الـ Prefabs المخزنة
    /// </summary>
    public int GetCacheCount()
    {
        return prefabCache.Count;
    }

    public static PrefabManager Instance => instance;
}
