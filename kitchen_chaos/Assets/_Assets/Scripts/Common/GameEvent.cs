using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameEvent : Singleton<GameEvent>
{
    public Action OnStartDay;

    [Button]
    public void StartDay() => OnStartDay?.Invoke();
}
