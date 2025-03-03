using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialDoneUI : MonoBehaviour
{
    [SerializeField] private Button _doneButton;

    void Awake()
    {
        _doneButton.onClick.AddListener(OnDoneButtonClick);
    }
    private void OnDoneButtonClick()
    {
        PhotonManager.s.EndSession();
        SceneManager.LoadScene(SceneType.MainMenuScene.ToString());
    }
}
