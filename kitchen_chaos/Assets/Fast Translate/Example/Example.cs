using System.Collections;
using System.Collections.Generic;
using FastTranslate;
using UnityEngine;

public class Example : MonoBehaviour
{
    public void NextLanguage()
    {
        var index = Project.singleton.getIndexChosenLang;
        index += 1;
        if(index >= Project.singleton.langSupports.Count){
            index = 0;
        }
        FTranslate.SetLanguage(Project.singleton.langSupports[index].languagesSuport);
    }

    public void PreviousLanguage()
    {
        var index = Project.singleton.getIndexChosenLang;
        index -= 1;
        if(index < 0){
            index = Project.singleton.langSupports.Count - 1;
        }
        FTranslate.SetLanguage(Project.singleton.langSupports[index].languagesSuport);
    }



}
