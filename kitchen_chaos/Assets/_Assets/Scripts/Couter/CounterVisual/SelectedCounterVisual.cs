using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{

    #region Variables
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;
    #endregion

    #region Unity functions
    private void Start()
    {
        GameManager.Instance.OnPlayerSpawn += GameManager_OnPlayerSpawn;
        HideVisual();
    }
    #endregion

    #region Events functions
    private void GameManager_OnPlayerSpawn(object sender, Player e)
    {
        e.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
    }
    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if(baseCounter == e.selectedCounter)
        {
            ShowVisual();
        }
        else
        {
            HideVisual();  
        }
    }
    #endregion

    #region Support
    private void ShowVisual()
    {
        foreach(GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(true);
        }
    }
    private void HideVisual()
    {
        foreach(GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(false);
        }
    }
    #endregion
}
