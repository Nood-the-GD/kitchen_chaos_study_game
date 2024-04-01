using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CanvasGroup))]
public class EnhancePopup : MonoBehaviour
{
    public float scaleFrom = 0.7f;
    public float scaleTo = 1f;
    public float alphaFrom = 0f;
    public float alphaTo = 1f;
    public float time = 0.5f;
    public bool enhanceFadeOut = false;
    [ShowIf("enhanceFadeOut")]
    public float delayFadeOut = 3;
    public GameObject parent;

    [Button]
    void AutoSetUpScale(){
        scaleFrom = transform.localScale.x * 0.7f;
        scaleTo = transform.localScale.x;
    }

    void OnEnable()
    {
        transform.localScale = Vector3.one * scaleFrom;
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = alphaFrom;
        canvasGroup.DOFade(alphaTo,time);
        transform.DOScale(scaleTo,time).SetEase(Ease.OutBack);
        if(enhanceFadeOut){
            Invoke("FadeOut",delayFadeOut);
        }    
    }

    void FadeOut(){
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.DOFade(alphaFrom,time);
        transform.DOScale(scaleFrom,time).SetEase(Ease.InBack).OnComplete(()=>{
            Destroy(parent);
        });
    }

    

    
}


public static class EnhancePopupExtension{
    public static void Enhance(this GameObject obj){
        var enhancePopup = obj.GetComponent<EnhancePopup>();
        if(enhancePopup == null){
            Debug.LogError("EnhancePopup not found");
            return;
        }
        obj.SetActive(true);
    }

    public static void DOFade(this CanvasGroup canvasGroup, float alphaTo, float time){
        DOTween.To(() => canvasGroup != null ? canvasGroup.alpha : 0, 
                x => { if (canvasGroup != null) canvasGroup.alpha = x; }, 
                alphaTo, 
                time);
    }

}
