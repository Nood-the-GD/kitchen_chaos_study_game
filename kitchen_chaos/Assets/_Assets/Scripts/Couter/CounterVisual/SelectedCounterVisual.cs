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
    void OnEnable()
    {
    }
    private void Start()
    {
        GameManager.Instance.OnPlayerSpawn += GameManager_OnPlayerSpawn;
        HideVisual();
    }
    #endregion

    #region Events functions
    private void GameManager_OnPlayerSpawn(Player e)
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
            Debug.Log("ShowVisual");
            visualGameObject.SetActive(true);
        }
    }
    private void HideVisual()
    {
        foreach(GameObject visualGameObject in visualGameObjectArray)
        {
            Debug.Log("Hide Visual");
            visualGameObject.SetActive(false);
        }
    }
    #endregion
}
