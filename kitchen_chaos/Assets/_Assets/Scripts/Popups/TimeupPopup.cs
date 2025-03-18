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

        if(tempStar > data.star){
            data.SaveNewScore(score);
            Debug.Log("New score saved");
        }
        else{
            Debug.Log("No new score");
        }
    }

    protected override void OnDisable() {
        base.OnDisable();
        
        // Apply player data changes to persistence
        if (GameManager.getStageData != null)
        {
            // Save player preferences to ensure data persistence
            PlayerPrefs.Save();
        }
        
        // End the photon session and return to main menu
        PhotonManager.s.EndSession();
        SceneManager.LoadScene("MainMenuScene");        
    }
}
