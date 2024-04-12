using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class ChoseColorElement : MonoBehaviour
{
    Action<ColorSkin> onSelect;
    [SerializeField] private Image colorImage;
    private ColorSkin colorSkin;
    public void Init(ColorSkin colorSkin, Action<ColorSkin> onSelect){
        this.colorSkin = colorSkin;
        colorImage.color = colorSkin.color;
        this.onSelect= (colorSkin) => {
            onSelect(colorSkin);
        };
    }

    public void OnClick(){
        onSelect(colorSkin);
    }


    


}
