using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button mainMenuBtn;

    private void Awake()
    {
        resumeBtn.onClick.AddListener(() => {
            GameManager.Instance.PauseGame();
        });
        mainMenuBtn.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGamePause += GameManager_OnGamePause;
        GameManager.Instance.OnGameUnPause += GameManager_OnGameUnPause;

        gameObject.SetActive(false);
    }

    private void GameManager_OnGamePause(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);
    }

    private void GameManager_OnGameUnPause(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
    }
}
