using System.Collections;
using Photon.Pun;
using UnityEngine;
using System;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IContainable _containerParent;
    PhotonView photonView;

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IContainable kitchenObjectParent)
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
            obj.GetComponent<KitchenObject>().SetContainerParent(kitchenObjectParent);
        }
    }
    public static void SpawnCompleteDish(KitchenObjectSO completeDishSO, KitchenObjectSO[] ingredients, IContainable kitchenObjectParent)
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
            var obj = Instantiate(completeDishSO.prefab);
            CompleteDishKitchenObject completeDish = obj.GetComponent<CompleteDishKitchenObject>();
            completeDish.SetContainerParent(kitchenObjectParent);
            for (int i = 0; i < ingredients.Length; i++)
            {
                KitchenObjectSO ingredient = ingredients[i];
                completeDish.TryAddIngredient(ingredient);
            }
        }
    }

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (!SectionData.s.isSinglePlay && PhotonNetwork.IsMasterClient)
            StartCoroutine(OnSync());
    }

    [PunRPC]
    public void RpcSetParentWithPhotonId(int photonId)
    {
        var findId = PhotonNetwork.GetPhotonView(photonId);
        if (findId == null)
            Debug.LogError("cant find id: " + photonId);
        var kitchenObjectParent = findId.GetComponent<IContainable>();

        if (kitchenObjectParent == null)
            Debug.LogError("kitchenObjectParent is null cant find id: " + photonId);

        MonoBehaviour monoBehaviour = this._containerParent as MonoBehaviour;
        if (monoBehaviour.GetComponent<PhotonView>().ViewID != photonId)
            SetContainerParent(kitchenObjectParent);
    }



    IEnumerator OnSync()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);


            var kitchenObjectParentGameObject = _containerParent as MonoBehaviour;
            var parentId = kitchenObjectParentGameObject.GetComponent<PhotonView>().ViewID;
            // Debug.Log("sync object: "+name + " parent: " + kitchenObjectParentGameObject.name + " "+ parentId);

            photonView.RPC(nameof(RpcSetParentWithPhotonId), RpcTarget.All, parentId);
        }
    }



    public bool TryGetCompleteDishKitchenObject(out CompleteDishKitchenObject completeDish)
    {
        if (this is CompleteDishKitchenObject)
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

    public void SetContainerParent(IContainable otherContainer)
    {
        if (otherContainer == null)
        {
            this.transform.position = Vector3.zero;
            this.transform.parent = null;
            this._containerParent = otherContainer;
            return;
        }

        if (this._containerParent != null)
        {
            this._containerParent.ClearKitchenObject();
        }
        this._containerParent = otherContainer;

        otherContainer.SetKitchenObject(this);
        this.transform.parent = otherContainer.GetKitchenObjectFollowTransform();
        this.transform.localPosition = Vector3.zero;
    }

    public IContainable GetKitchenObjectParent()
    {
        return _containerParent;
    }

    public void DestroySelf()
    {
        if (!SectionData.s.isSinglePlay)
            CmdDestroy();
        else
        {
            if (_containerParent != null)
                _containerParent.ClearKitchenObject();
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
        if (_containerParent != null)
            _containerParent.ClearKitchenObject();
        Destroy(this.gameObject);
    }
}
