using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus
{
    static readonly Dictionary<string, Action<object>> events = new Dictionary<string, Action<object>>();

    public static void Subscribe(string key, Action<object> handler)
    {
        if (!events.ContainsKey(key)) events[key] = null;
        events[key] += handler;
    }

    public static void Unsubscribe(string key, Action<object> handler)
    {
        if (events.ContainsKey(key)) events[key] -= handler;
    }

    public static void Publish(string key, object payload = null)
    {
        if (events.ContainsKey(key)) events[key]?.Invoke(payload);
    }
}
