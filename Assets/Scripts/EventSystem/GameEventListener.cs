using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent _gameEvent;

    [SerializeField] private UnityEvent<int> _actionsToDo;

    [SerializeField] private UnityEvent<int, Vector2> _specialActions;  // For some map changes

    private void OnEnable()
    {
        _gameEvent.AddListener(this);
    }

    private void OnDisable()
    {
        _gameEvent.RemoveListener(this);
    }

    public void OnGameEvent(int value)
    {
        _actionsToDo.Invoke(value);
    }

    public void OnGameEvent(int id, Vector2 value)
    {
        _specialActions.Invoke(id, value);
    }
}
