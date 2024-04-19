using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class StageData{
    
    public int levelId;
    public string sceneName;
    public int star{
        get{
            return PlayerPrefs.GetInt("level." + levelId, 0);
        }
        set{
            PlayerPrefs.SetInt("level." + levelId, value);
        }
    }

    public int[] pointTarget = new int[3];


    public bool isUnlocked{
        get{
            return PlayerPrefs.GetInt("level."+levelId+".unlocked", 0) == 1;
        }
        set{
            PlayerPrefs.SetInt("level."+levelId+".unlocked", value?1:0);
        }
    }

    public Sprite previewImage;

    public void ApplyNewScore(int score){
        for(int i = 0; i < pointTarget.Length; i++){
            if(score >= pointTarget[i]){
                star = i + 1;
            }
        }
    }

}

public class StageUI : MonoBehaviour
{
    public Transform[] star;
    public Sprite lockStageSprite;
    public Sprite selectStageSprite;
    public Sprite unSelectStageSprite;

    public Image stageUI;
    public TextMeshProUGUI stageText;
    Action<StageData> switchStage;
    public StageData stageData;
    [SerializeField] private StarController levelStarController;

    public void SetData(StageData data, Action<StageData> onSwitchStage){
        stageData = data;
        switchStage = onSwitchStage;
        //Set data to UI
        stageUI.sprite = data.isUnlocked?unSelectStageSprite:lockStageSprite;
        stageText.text = data.levelId.ToString();
        levelStarController.ShowStar(data.star);
    }

    public void Unselect(){
        stageUI.sprite = unSelectStageSprite;
    }

    public void SetSelect(){
        stageUI.sprite = selectStageSprite;
    }

    public void SelectStage(){
        switchStage?.Invoke(stageData);
    }
}
