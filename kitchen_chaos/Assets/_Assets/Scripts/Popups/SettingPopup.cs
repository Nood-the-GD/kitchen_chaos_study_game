using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SettingPopup : BasePopup<SettingPopup>
{
    public Sprite onVolumeSprite;
    public Sprite offVolumeSprite;
    public Image volumeImage;

    public void OnClickChangeVolume(){
        UserSetting.volume = !UserSetting.volume;
        volumeImage.sprite = UserSetting.volume ? onVolumeSprite : offVolumeSprite;
    }

    public void OnClickResetName(){
        SetUserNamePopup.ShowPopup(false);
    }

    
}
