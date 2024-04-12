using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoseColorPopup : BasePopup<ChoseColorPopup>
{
    //Transform child;
    public ChoseColorElement choseColorElementPref;

    void Start(){
        Init();
    }
    void Init(){
        var colorList = GameData.s.colorElements;
        foreach (var color in colorList)
        {
            var choseColorElement = Instantiate(choseColorElementPref, choseColorElementPref.transform.parent);
            //choseColorElement.transform.SetParent();
            choseColorElement.Init(color, (colorSkin) => {
                UserSetting.colorSkin = colorSkin;
                HidePopup();
            });
        }

        Destroy(choseColorElementPref.gameObject);
    }
}
