using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstTutorialController : MonoBehaviour
{
    [SerializeField] private List<BaseCounter> _navigationDestinationList = new List<BaseCounter>();
    [SerializeField] private TutorialDialogue _tutorialDialogue;
    [SerializeField] private TutorialDoneUI _tutorialDoneUI;
    private int _currentDestinationIndex = 0;

    #region Unity Functions
    void Start()
    {
        BaseCounter.OnInteract += OnInteract;
        DeliveryManager.Instance.OnRecipeSuccess += OnRecipeSuccess;
        _tutorialDialogue.OnDialogDone += OnDialogDone;
        _tutorialDialogue.StartDialogue();
    }
    void OnDestroy()
    {
        BaseCounter.OnInteract -= OnInteract;
        BaseCounter.OnAlternativeInteract -= OnAlternativeInteract;
    }
    #endregion

    #region Event Functions
    private void OnDialogDone(int index)
    {
        switch (index)
        {
            case 5:
                // Start making salad
                DeliveryManager.Instance.AddTutorialOrder();
                _currentDestinationIndex = -1;
                NextDestination();
                break;
        }
    }
    private void OnInteract(object sender, EventArgs e)
    {
        var currentDestination = _navigationDestinationList[_currentDestinationIndex];
        if ((sender as BaseCounter) != currentDestination) return;
        if (sender is CuttingCounter)
        {
            BaseCounter.OnAlternativeInteract += OnAlternativeInteract;
            return;
        }
        NextDestination();
    }
    void OnAlternativeInteract(object sender, EventArgs e)
    {
        var currentDestination = _navigationDestinationList[_currentDestinationIndex];
        if ((sender as BaseCounter) != currentDestination) return;
        if (sender is CuttingCounter)
        {
            var cuttingCounter = sender as CuttingCounter;
            if (cuttingCounter.isComplete)
            {
                NextDestination();
                BaseCounter.OnAlternativeInteract -= OnAlternativeInteract;
                return;
            }
        }
    }
    private void OnRecipeSuccess(object sender, EventArgs e)
    {
        UserData.IsFirstTutorialDone = true;
        _tutorialDoneUI.gameObject.SetActive(true);
    }
    #endregion

    private void NextDestination()
    {
        _currentDestinationIndex++;
        _currentDestinationIndex = Mathf.Clamp(_currentDestinationIndex, 0, _navigationDestinationList.Count - 1);
        TutorialNavigate.s.ShowNavigationArrow(_navigationDestinationList[_currentDestinationIndex].transform);
    }
}
