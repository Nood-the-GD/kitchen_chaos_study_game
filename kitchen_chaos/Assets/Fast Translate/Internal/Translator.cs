using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

//using Gridly.Example;
namespace FastTranslate
{
    public interface ITranslareText
    {
        void Refesh();
    }
    public class Translator : MonoBehaviour, ITranslareText
    {
        public TextMeshProUGUI textMeshPro;
        public Text text;
        public bool autoApplyFont;
        [HideInInspector]
        public string grid;

        [HideInInspector]
        public string key;

        [HideInInspector]
        public string fontKey;

        

        void OnEnable()
        {
            Refesh();

        }

        public void Refesh()
        {
            if (textMeshPro != null)
            {
                textMeshPro.text = FTranslate.GetStringData(grid, key);
                if(autoApplyFont){
                    var fontAsset = Project.singleton.GetFontAsset(fontKey);
                    if(fontAsset.tmFont != null){
                        textMeshPro.font = fontAsset.tmFont;
                    }
                }

                //refesh the tmPro
                textMeshPro.enabled = false;
                textMeshPro.enabled = true;
            }

            if (text != null)
            {
                text.text = FTranslate.GetStringData(grid, key);
                
                if(autoApplyFont){
                    var fontAsset = Project.singleton.GetFontAsset(fontKey);
                    if(fontAsset.font != null){
                        text.font = fontAsset.font;
                    }
                }

                //refesh the text
                text.enabled = false;
                text.enabled = true;
            }

            if (text == null && textMeshPro == null)
                Debug.LogWarning("text,textMeshPro fields is empty " + gameObject.name);

        }

        public void AutoSetup(){
            textMeshPro = GetComponent<TextMeshProUGUI>();
            text = GetComponent<Text>();
            Refesh();
        }
    }
}