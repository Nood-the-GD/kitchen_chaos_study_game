using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Reflection;
using System;
using DG.Tweening;

public class PopupAnim{
    public static float screenWidth {
        get{
            var div = Screen.width/2;
            return div;
        }
    }
    public static float screenHeight{
        get{
            var div = Screen.height/2;
            return div - div * 1/4;
        }
    }

    public static void MoveLeft(Transform trans,float timeMove = 0.6f,bool doKill=false, Action onComplete = null){

        if(doKill){
            trans.DOKill();
        }
        //Debug.Log("screen width: "+ Screen.width +  " /2: "+ Screen.width/2);
        //trans.localPosition = new Vector3()
        var w = PopupController.sizeDelta.x;
        trans.gameObject.SetActive(true);
        trans.DOLocalMoveX(-w,timeMove).OnComplete(()=>{
            onComplete?.Invoke();
        });
    }   

    public static void MoveRightToMiddle(Transform trans,float timeMove = 0.6f, bool doKill = false,Action onComplete = null){
        if(doKill){
            trans.DOKill();
        }

        //start
        var w = PopupController.sizeDelta.x;
        trans.localPosition = new Vector3(w,0,0);
        MoveToMiddle(trans, timeMove, onComplete);
    }

    public static void MoveLeftToMiddle(Transform trans,float timeMove = 0.6f, bool doKill = false, Action onComplete = null){
        if(doKill){
            trans.DOKill();
        }
        var w = PopupController.sizeDelta.x;
        trans.localPosition = new Vector3(-w,0,0);
        MoveToMiddle(trans, timeMove, onComplete);
    }

    public static void MoveToMiddle(Transform trans,float timeMove = 0.6f, Action onComplete = null){
        trans.gameObject.SetActive(true);
        trans.DOLocalMoveX(0,timeMove).OnComplete(()=>{
            onComplete?.Invoke();
        });
    }

    //move right
    public static void MoveRight(Transform trans, float timeMove = 0.6f, bool doKill = false,Action onComplete = null){
        if(doKill){
            trans.DOKill();
        }
        trans.gameObject.SetActive(true);
        var w = PopupController.sizeDelta.x;
        trans.DOLocalMoveX(w,timeMove).OnComplete(()=>{
            onComplete?.Invoke();
        });
        
    }

    //public static void

    //move up
    public static void MoveBotToMiddle(Transform trans, float timeMove, Action onComplete = null){
        
    }


}


[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public class BasePopup<T> : MonoBehaviour{
    public static T Instance => PopupController.s.GetComponentInChildren<T>();

    [FoldoutGroup("BasePopup")]
    [SerializeField] protected Button[] _closeButton = new Button[0];

    [Header("Show Popup")]
    [FoldoutGroup("BasePopup")] private UnityEvent onCallShowPopup;

    [Header("Hide Popup")]
    [FoldoutGroup("BasePopup")]  public float hidePopupTime;
    [FoldoutGroup("BasePopup")]  public UnityEvent onCallHidePopup;

    [FoldoutGroup("BasePopup")] public bool isOnlyOne = true;
    [FoldoutGroup("BasePopup")] [SerializeField] private bool addOnEnable = true;
    private CanvasGroup _canvasGroup;

    #region editor
    [Button]
    void SetName()
    {
        gameObject.name = typeof(T).ToString();
    }
    #endregion

    public void AddOnCloseAction(Action action){
        foreach(var i in _closeButton){
            i.onClick.AddListener(()=> action?.Invoke());
        }
    }

    public void AddOnCloseAction(int index, Action action){
        Debug.Log("added close action: "+index);
        _closeButton[index].onClick.AddListener(()=>action?.Invoke());
    }

    protected virtual void OnEnable()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.interactable = true;
        foreach (var i in _closeButton)
        {
            i.onClick.AddListener(HidePopup);
        }

        

        //PopupController.s.allPopup.Add(gameObject);
        if (addOnEnable)
            PopupController.s.SetupPopup(transform);
        onCallShowPopup?.Invoke();
    }

    protected virtual void OnDisable()
    {
        foreach(var i in _closeButton)
        {
            i.onClick.RemoveAllListeners();
        }

        //PopupController.s.allPopup.Remove(gameObject);
    }
    public static T ShowPopup()
    {
        return PopupController.ForceGetInstance().ShowPopup<T>(null, null).GetComponent<T>();
    }

    public static T ShowMoveFromLeft(float time){
        var popup = PopupController.s.ShowPopup<T>(null, null);
        popup.transform.localPosition = new Vector3(-Screen.width,0,0);
        popup.transform.DOLocalMoveX(0,time);
        return popup.GetComponent<T>();
    }

    public static T ShowMoveFromRight(float time){
        var popup = PopupController.s.ShowPopup<T>(null, null);
        popup.transform.localPosition = new Vector3(Screen.width,0,0);
        popup.transform.DOLocalMoveX(0,time);
        return popup.GetComponent<T>();
    }


    public static BasePopup<T> ShowPopup(params object[] data)
    {
        return PopupController.s.ShowPopup<T>(null,data);
    }
    
    public static void HidePopup()
    {
        PopupController.s.HidePopup<T>();
    }

    public static void SetActivePopup(bool active){
        PopupController.s.SetAvtivePopup<T>(active);
    }


    
    public void HideMoveLeft(float timeMove = 0.6f, bool hideOncomplete = true){
        _canvasGroup.interactable = false;
        PopupAnim.MoveLeft(transform,timeMove,true,()=>{
            if(hideOncomplete)
                HidePopup();
        });
    }

    public void HideMoveRight(float timeMove = 0.6f, bool hideOncomplete = true){
        _canvasGroup.interactable = false;
        PopupAnim.MoveRight(transform,timeMove,true,()=>{
            if(hideOncomplete)
                HidePopup();
        });
    }

    public void FadeOut(float alphaTo = 0,float scaleTo = 0.7f,float time = 0.5f, bool hideOncomplete = true){
        _canvasGroup.interactable = false;
        //_canvasGroup.DOFade(alphaTo,time);
        transform.DOScale(scaleTo,time).SetEase(Ease.InBack).OnComplete(()=>{
            if(hideOncomplete)
                HidePopup();
        });
    }



    
    public virtual void InitDataPopup(params object[] data)
    {

    }

}

public class PopupController : Singleton<PopupController>
{
    public static Vector2 sizeDelta => s.GetComponent<RectTransform>().rect.size;
    public static int width => (int)sizeDelta.x;
    public static int height => (int)sizeDelta.y;


    public void ShowPopup(string namePopup)
    {
        Debug.Log("Show popup: "+ namePopup);
        Type t = Type.GetType(namePopup);
        t.BaseType.GetMethod("ShowPopup").Invoke(null, new object[1]);
        //t.GetMethod("ShowPopup").Invoke(null, null);
        //ShowPopup<t.>
    }
    public BasePopup<T> ShowPopup<T>(string namePopup = null,object[] data = null)
    {
        string nameType;
        if(namePopup == null)
            nameType = typeof(T).ToString();
        else nameType = namePopup;


        var p = Resources.Load<BasePopup<T>>("Popups/" + nameType);

        if (p.isOnlyOne && IsActivePopup(nameType)){
            Debug.Log("cant init: "+nameType + " cause");
            if(p.isOnlyOne){
                Debug.Log("is only one");
            }
            if(IsActivePopup(nameType)){
                Debug.Log("is active popup");
            }
            

            return null;
        }

        var popup = Instantiate(p, transform, true);
        popup.name = nameType;
        var tran = popup.transform;
        SetupPopup(tran);

        if (data != null)
            popup.InitDataPopup(data);
        return popup;
    }

    public void SetupPopup(Transform tran)
    {
        tran.SetParent(transform);
        tran.localPosition = Vector3.zero;
        tran.localScale = Vector3.one;
        var rectTrans = tran as RectTransform;
        if (rectTrans != null)
        {
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.one;

            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
        }

        tran.SetAsLastSibling();
    }


    public void HidePopup<T>()
    {
        var p = GetComponentInChildren<T>() as BasePopup<T>;
        if (p == null)
            return;
        p.onCallHidePopup?.Invoke();
        StartCoroutine(DestroyDelay(p.gameObject,p.hidePopupTime));
        
    }

    public void SetAvtivePopup<T>(bool active){
        var p = GetComponentInChildren<T>(true) as BasePopup<T>;
        if (p == null)
            return;
        p.gameObject.SetActive(active);
    }

    public T GetActivePopup<T>(){
        return GetComponentInChildren<T>();
    }

    IEnumerator DestroyDelay( GameObject go,float time){
        yield return new WaitForSecondsRealtime(time);
        Destroy(go);        
    }



    public bool IsActivePopup(string find)
    {
        
        foreach(Transform i in transform){
           
            if(i.name == find)
                return true;
            
        }
        return false;
    }
}
