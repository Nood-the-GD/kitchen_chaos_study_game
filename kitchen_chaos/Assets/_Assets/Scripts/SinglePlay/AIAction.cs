using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAction
{
    public Action action;
    public Action onComplete;

    public static AIAction CreateAction(Action action) => new AIAction { action = action };

    public void Perform()
    {
        ActionStart();
        action?.Invoke();
    }

    public void ActionComplete()
    {
        onComplete?.Invoke();
    }
    public void ActionStart()
    {
    }
}
