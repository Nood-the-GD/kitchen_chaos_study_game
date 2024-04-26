using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class TimeupPopup : BasePopup<TimeupPopup>
{
    [SerializeField] private StarController starController;
    public TextMeshProUGUI[] scores;
    public TextMeshProUGUI userScoreUI;


    public void SetData(StageData data, int userScore){
        for(int i = 0; i < scores.Length; i++){
            scores[i].text = data.pointTarget[i].ToString();
        }
        
        userScoreUI.text = userScore.ToString();

        data.ApplyNewScore(userScore);
        starController.ShowStar(data.star);
    }

    protected override void OnDisable() {
        base.OnDisable();
        // Error when return to main screen
        //PhotonNetwork.JoinLobby();
        PhotonManager.s.EndSesstion();
        SceneManager.LoadScene("MainMenuScene");        

    }
}
