using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TimeupPopup : BasePopup<TimeupPopup>
{
    public TextMeshProUGUI[] scores;
    public TextMeshProUGUI userScoreUI;
    public void SetData(StageData data, int userScore){
        for(int i = 0; i < scores.Length; i++){
            scores[i].text = data.pointTarget[i].ToString();
        }
        
        userScoreUI.text = userScore.ToString();

        data.ApplyNewScore(userScore);
    }

    protected override void OnDisable() {
        base.OnDisable();
        SceneManager.LoadScene("MainMenuScene");        
    }
    

}
