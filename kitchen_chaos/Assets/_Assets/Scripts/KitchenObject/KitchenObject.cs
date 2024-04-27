using System.Collections;
using Photon.Pun;
using UnityEngine;
using System;

public class KitchenObject : MonoBehaviour
{
    public static Action<KitchenObject> OnAnyKitchenObjectSpawned;

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent kitchenObjectParent;
    PhotonView photonView;

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        //convert interface to gameObject
        var kitchenObjectParentGameObject = kitchenObjectParent as MonoBehaviour;

        //--Player Clone are not allowed to spawn object
        var player = kitchenObjectParent as Player;
        if(player != null && !player.photonView.IsMine)
            return;

        var parentId = -1;
        if(kitchenObjectParentGameObject != null)
            parentId = kitchenObjectParentGameObject.GetComponent<PhotonView>().ViewID;

        PhotonManager.s.CmdSpawnKitchenObject(kitchenObjectSO.prefab.GetComponent<ObjectTypeView>().objectType, parentId);
    }
    
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start(){
        StartCoroutine(OnSync());
    }

    [PunRPC]
    public void RpcSetParentWithPhotonId(int photonId){
        var findId = PhotonNetwork.GetPhotonView(photonId);
        if(findId == null)
            Debug.LogError("cant find id: " + photonId);
        var kitchenObjectParent = findId.GetComponent<IKitchenObjectParent>();
        
        if(kitchenObjectParent == null)
            Debug.LogError("kitchenObjectParent is null cant find id: " + photonId);

        MonoBehaviour monoBehaviour = this.kitchenObjectParent as MonoBehaviour;
        if(monoBehaviour.GetComponent<PhotonView>().ViewID != photonId)
            SetKitchenObjectParent(kitchenObjectParent);
    }



    IEnumerator OnSync(){
        if(!PhotonNetwork.IsMasterClient)
            yield break;

        while(true){
        yield return new WaitForSeconds(1f);


            var kitchenObjectParentGameObject = kitchenObjectParent as MonoBehaviour;
            var parentId = kitchenObjectParentGameObject.GetComponent<PhotonView>().ViewID;
            // Debug.Log("sync object: "+name + " parent: " + kitchenObjectParentGameObject.name + " "+ parentId);
            
            photonView.RPC(nameof(RpcSetParentWithPhotonId), RpcTarget.All, parentId);
        }
    }



    public bool TryGetCompleteDishKitchenObject(out CompleteDishKitchenObject completeDish)
    {
        if(this is CompleteDishKitchenObject)
        {
            completeDish = this as CompleteDishKitchenObject;
            return true;
        }
        else
        {
            completeDish = null;
            return false;
        }
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        if(kitchenObjectParent == null)
        {
            this.transform.position = Vector3.zero;
            this.transform.parent = null;
            this.kitchenObjectParent = kitchenObjectParent;
            return;
        }

        if(this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }
        this.kitchenObjectParent = kitchenObjectParent;

        kitchenObjectParent.SetKitchenObject(this);
        this.transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        this.transform.localPosition = Vector3.zero;
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {
        CmdDestroy();
    }

    public void CmdDestroy(){
        photonView.RPC("RpcDestroy", RpcTarget.All);
    }

    [PunRPC]
    public void RpcDestroy(){
        if(kitchenObjectParent != null)
            kitchenObjectParent.ClearKitchenObject();
        Destroy(this.gameObject);
    }
}
