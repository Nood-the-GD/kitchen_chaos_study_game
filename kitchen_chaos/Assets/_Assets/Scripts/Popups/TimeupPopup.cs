using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeupPopup : BasePopup<TimeupPopup>
{
    [SerializeField] private StarController starController;
    public TextMeshProUGUI userScoreUI;


    public void SetData(StageData data, int userScore){
        userScoreUI.text = userScore.ToString();

        data.ApplyNewScore(userScore);
        starController.SetData(data);
    }

    protected override void OnDisable() {
        base.OnDisable();
        // Error when return to main screen
        //PhotonNetwork.JoinLobby();
        PhotonManager.s.EndSesstion();
        SceneManager.LoadScene("MainMenuScene");        

    }
}
