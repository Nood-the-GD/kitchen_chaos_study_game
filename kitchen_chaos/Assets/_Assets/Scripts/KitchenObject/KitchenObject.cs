using System.Collections;
using Photon.Pun;
using UnityEngine;
using System;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent _kitchenObjectParent;
    PhotonView photonView;
    bool isCompleteDish => kitchenObjectSO is CompleteDishSO;
    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        //convert interface to gameObject
        var kitchenObjectParentGameObject = kitchenObjectParent as MonoBehaviour;

        if (SectionData.s.isSinglePlay == false)
        {
            //--Player Clone are not allowed to spawn object
            var player = kitchenObjectParent as Player;
            if (player != null && !player.photonView.IsMine)
                return;

            var parentId = -1;
            if (kitchenObjectParentGameObject != null)
                parentId = kitchenObjectParentGameObject.GetComponent<PhotonView>().ViewID;

            PhotonManager.s.CmdSpawnKitchenObject(kitchenObjectSO.prefab.GetComponent<ObjectTypeView>().objectType, parentId);
        }
        else
        {
            var obj = Instantiate(kitchenObjectSO.prefab, kitchenObjectParentGameObject.transform);
            obj.GetComponent<KitchenObject>().SetKitchenObjectParent(kitchenObjectParent);
        }
    }
    public static void SpawnCompleteDish(KitchenObjectSO completeDishSO, KitchenObjectSO[] ingredients, IKitchenObjectParent kitchenObjectParent)
    {
        //convert interface to gameObject
        var kitchenObjectParentGameObject = kitchenObjectParent as MonoBehaviour;

        //--Player Clone are not allowed to spawn object
        var player = kitchenObjectParent as Player;
        if (player != null && !player.photonView.IsMine)
            return;

        var parentId = -1;
        if (kitchenObjectParentGameObject != null)
            parentId = kitchenObjectParentGameObject.GetComponent<PhotonView>().ViewID;

        string[] ingredientString = new string[ingredients.Length];
        for (int i = 0; i < ingredients.Length; i++)
        {
            ingredientString[i] = ingredients[i].objectName;
        }

        if (!SectionData.s.isSinglePlay)
        {
            PhotonManager.s.CmdSpawnCompleteDish(completeDishSO.prefab.GetComponent<ObjectTypeView>().objectType, ingredientString, parentId);
        }
        else
        {

        }
    }

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (!SectionData.s.isSinglePlay)
            StartCoroutine(OnSync());
    }

    [PunRPC]
    public void RpcSetParentWithPhotonId(int photonId)
    {
        var findId = PhotonNetwork.GetPhotonView(photonId);
        if (findId == null)
            Debug.LogError("cant find id: " + photonId);
        var kitchenObjectParent = findId.GetComponent<IKitchenObjectParent>();

        if (kitchenObjectParent == null)
            Debug.LogError("kitchenObjectParent is null cant find id: " + photonId);

        MonoBehaviour monoBehaviour = this._kitchenObjectParent as MonoBehaviour;
        if (monoBehaviour.GetComponent<PhotonView>().ViewID != photonId)
            SetKitchenObjectParent(kitchenObjectParent);
    }



    IEnumerator OnSync()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        while (true)
        {
            yield return new WaitForSeconds(1f);


            var kitchenObjectParentGameObject = _kitchenObjectParent as MonoBehaviour;
            var parentId = kitchenObjectParentGameObject.GetComponent<PhotonView>().ViewID;
            // Debug.Log("sync object: "+name + " parent: " + kitchenObjectParentGameObject.name + " "+ parentId);

            photonView.RPC(nameof(RpcSetParentWithPhotonId), RpcTarget.All, parentId);
        }
    }



    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        if (kitchenObjectParent == null)
        {
            this.transform.position = Vector3.zero;
            this.transform.parent = null;
            this._kitchenObjectParent = kitchenObjectParent;
            return;
        }

        if (this._kitchenObjectParent != null)
        {
            this._kitchenObjectParent.ClearKitchenObject();
        }
        this._kitchenObjectParent = kitchenObjectParent;

        kitchenObjectParent.SetKitchenObject(this);
        this.transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        this.transform.localPosition = Vector3.zero;
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return _kitchenObjectParent;
    }

    public void DestroySelf()
    {
        if (!SectionData.s.isSinglePlay)
            CmdDestroy();
        else
        {
            if (_kitchenObjectParent != null)
                _kitchenObjectParent.ClearKitchenObject();
            Destroy(this.gameObject);
        }
    }

    public void CmdDestroy()
    {
        photonView.RPC("RpcDestroy", RpcTarget.All);
    }

    [PunRPC]
    public void RpcDestroy()
    {
        if (_kitchenObjectParent != null)
            _kitchenObjectParent.ClearKitchenObject();
        Destroy(this.gameObject);
    }
}
