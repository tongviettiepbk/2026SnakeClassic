using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEventManager : MonoBehaviour
{
    public static SimpleEventManager Instance { get; private set; }

    public static string ChangeLanguageID = "changeLanguage";

    private Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PostEvent(string eventId, object param = null)
    {
        try
        {
            if (eventDictionary.TryGetValue(eventId, out var callback))
            {
                callback?.Invoke(param);
            }
            else
            {
                Debug.LogWarning($"[SimpleEventManager] No listener for event: {eventId}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
       
    }

    public void Register(string eventId, Action<object> listener)
    {
        if (eventDictionary.ContainsKey(eventId))
            eventDictionary[eventId] += listener;
        else
            eventDictionary[eventId] = listener;
    }

    public void Unregister(string eventId, Action<object> listener)
    {
        if (eventDictionary.TryGetValue(eventId, out var callback))
        {
            callback -= listener;

            if (callback == null)
                eventDictionary.Remove(eventId);
            else
                eventDictionary[eventId] = callback;
        }
    }

    public void ClearAll()
    {
        eventDictionary.Clear();
    }
}
