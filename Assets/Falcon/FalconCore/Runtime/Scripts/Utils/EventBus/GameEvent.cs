/*
 * Author: minhddv
 * Email: minhddv@falcongames.com
 * Company: Falcon Games
 * Date: 2025-06-05
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public static class GameEvent<T>
{
    private static readonly Dictionary<string, HashSet<Action<T>>> kListenerDict = new();
    private static readonly Dictionary<string, List<ListenerWrapper>> kWrapperDict = new();

    /// <summary>
    /// Register an event.
    /// </summary>
    /// <param name="eventName">Event name. Better to using a constant string.</param>
    /// <param name="action">The action is called when event should be raised</param>
    /// <param name="listener">The listener component. This parameter should be null if the listener is not a component. </param>
    public static void Register(string eventName, Action<T> action, Component listener)
    {
        if (kListenerDict.ContainsKey(eventName) && kListenerDict[eventName].Contains(action))
        {
            Debug.LogWarning("This action has already registered for this event");
            return;
        }

        if (kListenerDict.TryGetValue(eventName, out var value))
        {
            value.Add(action);
        }
        else
        {
            value = new HashSet<Action<T>> { action };
            kListenerDict.Add(eventName, value);
        }

        if (!kWrapperDict.TryGetValue(eventName, out var wrapperList))
        {
            wrapperList = new List<ListenerWrapper>();
            kWrapperDict[eventName] = wrapperList;
        }

        wrapperList.Add(new ListenerWrapper(action, listener));

#if UNITY_EDITOR
        if (listener != null)
            GameEventViewer.OnRegisterEvent(eventName, listener);
#endif
    }

    /// <summary>
    /// This action cannot be null.
    /// <param name="listener">.The listener component. This parameter should be null if the listener is not a component </param>
    /// </summary>
    public static void Unregister(string eventName, Action<T> action, Component listener)
    {
        if (!kListenerDict.TryGetValue(eventName, out var value) || !value.Contains(action))
        {
            Debug.LogWarning($"This event: {eventName} doesn't contain this action!");
            return;
        }

        value.Remove(action);

        if (kWrapperDict.TryGetValue(eventName, out var wrapperList))
        {
            wrapperList.RemoveAll(w => w.action.Equals(action));
            if (wrapperList.Count == 0)
            {
                kWrapperDict.Remove(eventName);
            }
        }

#if UNITY_EDITOR
        if (listener != null)
            GameEventViewer.OnUnregisterEvent(eventName, listener);
#endif
    }

    public static void UnregisterAll(string eventName)
    {
        if (!kListenerDict.ContainsKey(eventName))
        {
            Debug.LogWarning($"This event: {eventName} has no register yet!");
            return;
        }

#if UNITY_EDITOR
        GameEventViewer.OnUnregisterAll(eventName);
#endif

        kListenerDict.Remove(eventName);
        kWrapperDict.Remove(eventName);
    }

    public static void Emit(string eventName, T data = default)
    {
        if (!kListenerDict.TryGetValue(eventName, out var value))
        {
            return;
        }

        var listeners = new List<Action<T>>(value);
        foreach (var listener in listeners)
        {
            try
            {
                listener.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception thrown while invoking event '{eventName}': {e}");
            }
        }
    }

    private class ListenerWrapper
    {
        internal readonly Delegate action;
        internal readonly Component listener;

        internal ListenerWrapper(Delegate action, Component listener)
        {
            this.action = action;
            this.listener = listener;
        }
    }
}

#if UNITY_EDITOR
internal class EventViewer : MonoBehaviour
{
    [SerializeField] private List<Component> _listeners = new();

    internal int ListenerCount => _listeners.Count;

    internal void AddListener(Component component)
    {
        _listeners.Add(component);
    }

    internal void RemoveListener(Component component)
    {
        _listeners.Remove(component);
    }

    internal void Destroy()
    {
        if (gameObject != null) Destroy(gameObject);
    }
}

internal static class GameEventViewer
{
    private static GameObject _gameObject;
    private static readonly Dictionary<string, EventViewer> kViewers = new();

    internal static void OnRegisterEvent(string eventName, Component listener)
    {
        if (_gameObject == null)
        {
            _gameObject = new GameObject("GameEventViewer");
            Object.DontDestroyOnLoad(_gameObject);
        }

        if (kViewers.TryGetValue(eventName, out var viewer1))
        {
            viewer1.AddListener(listener);
        }
        else
        {
            var go = new GameObject(eventName);
            go.transform.SetParent(_gameObject.transform);
            var viewer = go.AddComponent<EventViewer>();
            viewer.AddListener(listener);
            kViewers.Add(eventName, viewer);
        }
    }

    internal static void OnUnregisterEvent(string eventName, Component listener)
    {
        if (!kViewers.TryGetValue(eventName, out var viewer)) return;

        viewer.RemoveListener(listener);

        if (kViewers[eventName].ListenerCount > 0 || kViewers[eventName] == null) return;
        kViewers[eventName].Destroy();
        kViewers.Remove(eventName);
    }

    internal static void OnUnregisterAll(string eventName)
    {
        if (!kViewers.Remove(eventName, out var viewer)) return;
        viewer.Destroy();
    }
}
#endif

/// <summary>
/// Event bus for events with no parameters.
/// </summary>
public static class GameEvent
{
    private static readonly Dictionary<string, HashSet<Action>> kListenerDict = new();
    private static readonly Dictionary<string, List<ListenerWrapper>> kWrapperDict = new();

    public static void Register(string eventName, Action action, Component listener)
    {
        if (kListenerDict.ContainsKey(eventName) && kListenerDict[eventName].Contains(action))
        {
            Debug.LogWarning("This action has already registered for this event");
            return;
        }

        if (kListenerDict.TryGetValue(eventName, out var value))
        {
            value.Add(action);
        }
        else
        {
            value = new HashSet<Action> { action };
            kListenerDict.Add(eventName, value);
        }

        if (!kWrapperDict.TryGetValue(eventName, out var wrapperList))
        {
            wrapperList = new List<ListenerWrapper>();
            kWrapperDict[eventName] = wrapperList;
        }

        wrapperList.Add(new ListenerWrapper(action, listener));

#if UNITY_EDITOR
        if (listener != null)
            GameEventViewer.OnRegisterEvent(eventName, listener);
#endif
    }

    public static void Unregister(string eventName, Action action, Component listener)
    {
        if (!kListenerDict.TryGetValue(eventName, out var value) || !value.Contains(action))
        {
            Debug.LogWarning($"This event: {eventName} doesn't contain this action!");
            return;
        }

        value.Remove(action);

        if (kWrapperDict.TryGetValue(eventName, out var wrapperList))
        {
            wrapperList.RemoveAll(w => w.action.Equals(action));
            if (wrapperList.Count == 0)
            {
                kWrapperDict.Remove(eventName);
            }
        }

#if UNITY_EDITOR
        if (listener != null)
            GameEventViewer.OnUnregisterEvent(eventName, listener);
#endif
    }

    public static void Emit(string eventName)
    {
        if (!kListenerDict.TryGetValue(eventName, out var value))
        {
            return;
        }

        var listeners = new List<Action>(value);
        foreach (var listener in listeners)
        {
            try
            {
                listener.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception thrown while invoking event '{eventName}': {e}");
            }
        }
    }

    public static void UnregisterAll(string eventName)
    {
        if (!kListenerDict.ContainsKey(eventName))
        {
            Debug.LogWarning($"This event: {eventName} has no register yet!");
            return;
        }

#if UNITY_EDITOR
        if (kWrapperDict.TryGetValue(eventName, out var wrapperList))
        {
            foreach (var wrapper in wrapperList)
            {
                if (wrapper.listener != null)
                    GameEventViewer.OnUnregisterEvent(eventName, wrapper.listener);
            }
        }
#endif

        kListenerDict.Remove(eventName);
        kWrapperDict.Remove(eventName);
    }

    private class ListenerWrapper
    {
        internal readonly Delegate action;
        internal readonly Component listener;

        internal ListenerWrapper(Delegate action, Component listener)
        {
            this.action = action;
            this.listener = listener;
        }
    }
}