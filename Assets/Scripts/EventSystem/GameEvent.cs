using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Scriptable Objects/Game Event")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> _listeners = new List<GameEventListener>();

    public void AddListener(GameEventListener listener)
    {
        _listeners.Add(listener);
    }

    public void RemoveListener(GameEventListener listener)
    {   
        _listeners.Remove(listener);
    }

    public void Init(int value)
    {
        foreach (var listener in _listeners)
            listener.OnGameEvent(value);
    }

    public void Init(int id, Vector2 value)
    {
        foreach (var listener in _listeners)
            listener.OnGameEvent(id, value);
    }
}
