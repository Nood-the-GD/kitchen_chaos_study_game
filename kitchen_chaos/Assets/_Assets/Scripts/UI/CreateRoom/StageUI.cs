using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class StageData{
    
    public int levelId;
    public int star{
        get{
            return PlayerPrefs.GetInt("level."+levelId, 0);
        }
        set{
            PlayerPrefs.SetInt("level."+levelId, value);
        }
    }

    public bool isUnlocked{
        get{
            return PlayerPrefs.GetInt("level."+levelId+".unlocked", 0) == 1;
        }
        set{
            PlayerPrefs.SetInt("level."+levelId+".unlocked", value?1:0);
        }
    }

    public Sprite previewImage;


}

public class StageUI : MonoBehaviour
{
    public Transform[] star;
    public Sprite lockStageSprite;
    public Sprite selectStageSprite;
    public Sprite unSelectStageSprite;

    public Image stageUI;
    TextMeshProUGUI stageText;
    Action<StageData> switchStage;
    [HideInInspector] public StageData stageData;
    public void SetData(StageData data, Action<StageData> onSwitchStage){
        stageData = data;
        switchStage = onSwitchStage;
        //Set data to UI
        for (int i = 0; i < star.Length; i++)
        {
            star[i].gameObject.SetActive(i<data.star);
        }
        stageUI.sprite = data.isUnlocked?unSelectStageSprite:lockStageSprite;

        stageText.text = data.levelId.ToString();
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
