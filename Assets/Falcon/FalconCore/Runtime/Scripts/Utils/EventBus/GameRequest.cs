/*
     * Author: minhddv
     * Email: minhddv@falcongames.com
     * Company: Falcon Games
     * Date: 2025-06-26
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Falcon.Helpers.EventBus
{
    public static class GameRequest<T>
    {
        private static readonly Dictionary<string, Func<T>> kRequestDict = new();
        private static readonly Dictionary<string, Component> kListenerDict = new();

        /// <summary>
        /// Register an Event for getting data.
        /// </summary>
        /// <param name="requestName"> Better to using a constant string.</param>
        /// <param name="responder">The function that return the request data</param>
        /// <param name="listener">The listener component. This parameter should be null if the listener is not a component.</param>
        public static void Register(string requestName, Func<T> responder, Component listener = null)
        {
            if (!kRequestDict.TryAdd(requestName, responder))
            {
                Debug.LogWarning($"Request [{requestName}] already has a responder registered.");
                return;
            }

            kListenerDict[requestName] = listener;

#if UNITY_EDITOR
            if (listener != null)
                GameRequestViewer.OnRegisterEvent(requestName, listener);
#endif
        }

        public static void Unregister(string requestName)
        {
            if (!kRequestDict.ContainsKey(requestName)) return;

#if UNITY_EDITOR
            if (kListenerDict.TryGetValue(requestName, out var listener) && listener != null)
                GameRequestViewer.OnUnregisterEvent(requestName, listener);
#endif

            kRequestDict.Remove(requestName);
            kListenerDict.Remove(requestName);
        }

        public static T Request(string requestName)
        {
            if (!kRequestDict.TryGetValue(requestName, out var responder))
            {
                Debug.LogWarning($"No responder registered for request [{requestName}].");
                return default;
            }

            try
            {
                return responder.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking responder for [{requestName}]: {ex.Message}");
                return default;
            }
        }
    }

    public static class GameRequest<TP, T>
    {
        private static readonly Dictionary<string, Func<TP, T>> kRequestDict = new();
        private static readonly Dictionary<string, Component> kListenerDict = new();

        public static void Register(string requestName, Func<TP, T> responder, Component listener = null)
        {
            if (!kRequestDict.TryAdd(requestName, responder))
            {
                Debug.LogWarning($"Request [{requestName}] already has a responder registered.");
                return;
            }

            kListenerDict[requestName] = listener;

#if UNITY_EDITOR
            if (listener != null)
                GameRequestViewer.OnRegisterEvent(requestName, listener);
#endif
        }

        public static void Unregister(string requestName)
        {
            if (!kRequestDict.ContainsKey(requestName)) return;

#if UNITY_EDITOR
            if (kListenerDict.TryGetValue(requestName, out var listener) && listener != null)
                GameRequestViewer.OnUnregisterEvent(requestName, listener);
#endif

            kRequestDict.Remove(requestName);
            kListenerDict.Remove(requestName);
        }

        public static T Request(string requestName, TP param)
        {
            if (!kRequestDict.TryGetValue(requestName, out var responder))
            {
                Debug.LogWarning($"No responder registered for request [{requestName}].");
                return default;
            }

            try
            {
                return responder.Invoke(param);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error invoking responder for [{requestName}]: {ex.Message}");
                return default;
            }
        }
    }
    
#if UNITY_EDITOR
    internal static class GameRequestViewer
    {
        private static GameObject _gameObject;
        private static readonly Dictionary<string, EventViewer> kViewers = new();

        internal static void OnRegisterEvent(string eventName, Component listener)
        {
            if (_gameObject == null)
            {
                _gameObject = new GameObject("GameRequestViewer");
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
            kViewers[eventName].RemoveListener(listener);

            if (kViewers[eventName].ListenerCount > 0 || kViewers[eventName] == null) return;
            kViewers[eventName].Destroy();
            kViewers.Remove(eventName);
        }
    }
#endif
}