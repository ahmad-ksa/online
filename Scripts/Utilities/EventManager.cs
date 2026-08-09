using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// نظام الأحداث المركزي
/// </summary>
public class EventManager : MonoBehaviour
{
    private static EventManager instance;
    private Dictionary<string, Action<object>> events = new Dictionary<string, Action<object>>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Logger.Log("EventManager initialized", "EventManager");
    }

    /// <summary>
    /// الاستماع إلى حدث
    /// </summary>
    public void Subscribe(string eventName, Action<object> callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Logger.LogError("Event name cannot be empty!", "EventManager");
            return;
        }

        if (!events.ContainsKey(eventName))
        {
            events[eventName] = null;
        }

        events[eventName] += callback;
        Logger.LogDebug($"Subscribed to event: {eventName}", "EventManager");
    }

    /// <summary>
    /// إيقاف الاستماع إلى حدث
    /// </summary>
    public void Unsubscribe(string eventName, Action<object> callback)
    {
        if (string.IsNullOrEmpty(eventName) || !events.ContainsKey(eventName))
        {
            return;
        }

        events[eventName] -= callback;
        Logger.LogDebug($"Unsubscribed from event: {eventName}", "EventManager");
    }

    /// <summary>
    /// إطلاق حدث
    /// </summary>
    public void Publish(string eventName, object data = null)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Logger.LogError("Event name cannot be empty!", "EventManager");
            return;
        }

        if (!events.ContainsKey(eventName) || events[eventName] == null)
        {
            Logger.LogDebug($"No subscribers for event: {eventName}", "EventManager");
            return;
        }

        try
        {
            events[eventName]?.Invoke(data);
            Logger.LogDebug($"Event published: {eventName}", "EventManager");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error publishing event {eventName}: {ex.Message}", "EventManager");
        }
    }

    /// <summary>
    /// حذف جميع المستمعين لحدث معين
    /// </summary>
    public void Clear(string eventName)
    {
        if (events.ContainsKey(eventName))
        {
            events[eventName] = null;
            Logger.LogDebug($"Cleared all subscribers for event: {eventName}", "EventManager");
        }
    }

    /// <summary>
    /// حذف جميع الأحداث
    /// </summary>
    public void ClearAll()
    {
        events.Clear();
        Logger.Log("All events cleared", "EventManager");
    }

    public static EventManager Instance => instance;
}
