using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
public class ObjectTypeView : MonoBehaviour
{   
    public string objectType;
    [HideInInspector] public bool isMultiplayObject;
    public bool destroyDelay;
    [ShowIf("destroyDelay")]
    public float destroyDelayTime;
    protected virtual void Awake() {
        isMultiplayObject = GetComponent<PhotonView>() != null;
    }
    protected virtual void Start(){
        
    }
    protected virtual void OnEnable(){
        DestroyDelay();
    }
    protected async void DestroyDelay(){
        if(destroyDelay){
            await System.Threading.Tasks.Task.Delay((int)(destroyDelayTime*1000));
            if(isMultiplayObject)
                PhotonManager.s.DestroyPhotonView(GetComponent<PhotonView>());
            else{
                //check if object have been destroy       
                //PoolSystem.s.PoolDespawn(this);
                Destroy(gameObject);
            }
        }
    }
    

#if UNITY_EDITOR

    GameData data => GameData.s;
    //bool refesh_editor = false;

    [Button(Icon = SdfIconType.Plus, IconAlignment = IconAlignment.LeftOfText)]
    void AddToGameData(){
        //refesh_editor = true;
        data.AddNewObjectTypeViews(this);
    }

    // [ShowIf(nameof(refesh_editor))][Button(Icon = SdfIconType.ArrowCounterclockwise, IconAlignment = IconAlignment.LeftOfText)]
    // void Refesh(){

    //     data.RefeshData();
    //     refesh_editor = false;
    // }

#endif
}
