using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeupPopup : BasePopup<TimeupPopup>
{
    [SerializeField] private StarController starController;
    public TextMeshProUGUI userScoreUI;

    public void SetData(StageData data, int score){
        userScoreUI.text = score.ToString();

        int tempStar = 0;

        for(int i = 0; i < data.pointTarget.Length; i++){
            if(score >= data.pointTarget[i]){
                tempStar = i + 1;
            }
        }

        starController.ShowStar(tempStar);
        starController.ShowPoint(data.pointTarget);
        if(score > data.score)
            data.ApplyNewScore(score);
    }

    protected override void OnDisable() {
        base.OnDisable();
        // Error when return to main screen
        //PhotonNetwork.JoinLobby();
        PhotonManager.s.EndSession();
        SceneManager.LoadScene("MainMenuScene");        

    }
}
