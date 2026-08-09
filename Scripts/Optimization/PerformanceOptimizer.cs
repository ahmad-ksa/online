using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class PerformanceOptimizer : MonoBehaviour
{
    private static PerformanceOptimizer instance;
    public enum PerformanceLevel { VeryLow, Low, Medium, High, Ultra }
    
    private PerformanceLevel currentLevel = PerformanceLevel.Medium;
    private List<float> frameTimeHistory = new List<float>();

    public static event Action<PerformanceLevel> OnPerformanceLevelChanged;

    private void Awake()
    {
        if (instance && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        int ram = SystemInfo.systemMemorySize;
        currentLevel = ram >= 8000 ? PerformanceLevel.Ultra :
                       ram >= 6000 ? PerformanceLevel.High :
                       ram >= 3000 ? PerformanceLevel.Medium :
                       ram >= 1500 ? PerformanceLevel.Low :
                       PerformanceLevel.VeryLow;

        ApplySettings();
        Debug.Log($"✅ PerformanceOptimizer: {currentLevel}");
    }

    private void Update()
    {
        float fps = 1f / Time.deltaTime;
        frameTimeHistory.Add(Time.deltaTime * 1000f);
        if (frameTimeHistory.Count > 300) frameTimeHistory.RemoveAt(0);

        if (fps < 20 && currentLevel != PerformanceLevel.VeryLow)
            SetLevel(currentLevel + 1);
        else if (fps > 50 && currentLevel != PerformanceLevel.Ultra)
            SetLevel(currentLevel - 1);
    }

    private void ApplySettings()
    {
        switch (currentLevel)
        {
            case PerformanceLevel.Ultra:
                QualitySettings.SetQualityLevel(5);
                Application.targetFrameRate = 60;
                break;
            case PerformanceLevel.High:
                QualitySettings.SetQualityLevel(4);
                Application.targetFrameRate = 60;
                break;
            case PerformanceLevel.Medium:
                QualitySettings.SetQualityLevel(3);
                Application.targetFrameRate = 30;
                break;
            case PerformanceLevel.Low:
                QualitySettings.SetQualityLevel(2);
                Application.targetFrameRate = 30;
                break;
            case PerformanceLevel.VeryLow:
                QualitySettings.SetQualityLevel(0);
                Application.targetFrameRate = 20;
                break;
        }
    }

    public void SetLevel(PerformanceLevel level)
    {
        if (level == currentLevel) return;
        currentLevel = level;
        ApplySettings();
        OnPerformanceLevelChanged?.Invoke(level);
        Debug.Log($"🔄 Performance: {level}");
    }

    public PerformanceLevel GetLevel() => currentLevel;
    public static PerformanceOptimizer Instance => instance;
}
