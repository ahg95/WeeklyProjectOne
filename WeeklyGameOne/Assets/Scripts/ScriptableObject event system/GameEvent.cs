using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newGameEvent", menuName = "GameEvent/GameEvent")]
public class GameEvent : ScriptableObject
{
    List<GameEventListener> listeners;

    public void AddListener(GameEventListener gameEventListener)
    {
        if (listeners == null)
            listeners = new();
            
        listeners.Add(gameEventListener);
    }

    public void RemoveListener(GameEventListener gameEventListener)
    {
        if (listeners!= null)
            listeners.Remove(gameEventListener);
    }

    public void Raise()
    {
        for (int i = listeners.Count - 1; 0 <= i; i--)
            listeners[i].OnEventRaised();
    }
}
