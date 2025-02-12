using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;

    private void Start()
    {
        GameManager.s.OnStateChanged += GameManager_OnStateChanged;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        countdownText.text = Mathf.Ceil(GameManager.s.GetCountdownToStartTimer()).ToString();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.s.IsCountdownToStartActive())
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


}
