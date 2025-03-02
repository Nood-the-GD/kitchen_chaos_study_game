using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstTutorialController : MonoBehaviour
{
    [SerializeField] private List<BaseCounter> _navigationDestinationList = new List<BaseCounter>();
    private int _currentDestinationIndex = 0;

    void Awake()
    {
        TutorialDialogue.s.OnDialogDone += OnDialogDone;
        BaseCounter.OnInteract += OnInteractDone;
    }

    void Start()
    {
        TutorialDialogue.s.StartDialogue();
    }

    private void OnDialogDone(int index)
    {
        switch (index)
        {
            case 5:
                // Start making salad
                DeliveryManager.s.AddTutorialOrder();
                _currentDestinationIndex = -1;
                NextDestination();
                break;
        }
    }

    private void NextDestination()
    {
        _currentDestinationIndex++;
        _currentDestinationIndex = Mathf.Clamp(_currentDestinationIndex, 0, _navigationDestinationList.Count - 1);
        TutorialNavigate.s.ShowNavigationArrow(_navigationDestinationList[_currentDestinationIndex].transform);
    }

    private void OnInteractDone(object sender, EventArgs e)
    {
        if (sender is CuttingCounter)
        {
            var cuttingCounter = sender as CuttingCounter;
            if (cuttingCounter.isComplete)
            {
                NextDestination();
            }
        }
    }
}
