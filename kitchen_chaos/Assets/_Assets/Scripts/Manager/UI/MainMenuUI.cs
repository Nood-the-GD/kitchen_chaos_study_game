using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playBtn; 
    [SerializeField] private Button exitBtn; 


    private void Awake()
    {
        playBtn.onClick.AddListener(() => {
            //Loader.Load(Loader.Scene.GameScene);
            SelectModePopup.ShowPopup();
        });
        exitBtn.onClick.AddListener(() => {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }
}
