using System.Collections;
using Photon.Pun;
using UnityEngine;
using System;
using System.Collections.Generic;
using Codice.CM.Common;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] public KitchenObjectSO kitchenObjectSO;

    private IKitchenContainable _containerParent;
    PhotonView photonView;
    public Transform stackPoint;
    public Transform visualTransform;
    public Transform platePoint;

    [HideInInspector] Transform plate;

    [HideInInspector] public List<KitchenObjectSO> ingredient = new List<KitchenObjectSO>();
    public bool IsHavingPlate => plate != null;
    public bool IsPlate => kitchenObjectSO.name == "Plate";

    public bool TryAddPlate(){
        if(IsHavingPlate)
            return false;
        if(IsPlate)
            return false;
        PhotonManager.s.CmdAddPlate(photonView.ViewID);
        return true;
    }

    public void AddIngredient(KitchenObjectSO kitchenObjectSO){
        ingredient.Add(kitchenObjectSO);
        SetActiveIngredient(kitchenObjectSO);
    }

    public void AddIngredientIndexs(int[] ingredientIndex){
        var rep = CookingBookSO.s.FindRecipeByOutput(kitchenObjectSO);
        foreach(var i in ingredientIndex){
            AddIngredient(rep.ingredients[i]);
        }
    }

    public void SetActiveIngredient(KitchenObjectSO kitchenObjectSO){
        if(stackPoint == null)
            return;
        var recipe = CookingBookSO.s.FindRecipeByOutput(kitchenObjectSO);
        var index = recipe.ingredients.IndexOf(kitchenObjectSO);
        stackPoint.GetChild(index).gameObject.SetActive(true);
    }

    public bool IsHaveEngoughIngredient(){
        var recipe = CookingBookSO.s.FindRecipeByOutput(kitchenObjectSO);
        return recipe.IsSameIngredients(ingredient);
    }


    public static void SpawnKitchenObject(ObjectEnum objectEnum, IKitchenContainable kitchenObjectParent){
        SpawnKitchenObject(
            GameData.s.GetObject(objectEnum).GetComponent<KitchenObject>().GetKitchenObjectSO()
            , kitchenObjectParent, new List<int>{0}
        );
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenContainable kitchenObjectParent){
        SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent, new List<int>{0});
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenContainable kitchenObjectParent, List<int> ingredient)
    {
        //convert interface to gameObject
        var kitchenObjectParentGameObject = kitchenObjectParent as MonoBehaviour;
        if (SectionData.s.isSinglePlay)
        {
            var obj = Instantiate(kitchenObjectSO.prefab, kitchenObjectParentGameObject.transform);
            obj.GetComponent<KitchenObject>().SetContainerParent(kitchenObjectParent);
        }
        else
        {
             //--Player Clone are not allowed to spawn object
            var player = kitchenObjectParent as Player;
            if (player != null && !player.photonView.IsMine)
                return;

            var parentId = -1;
            if (kitchenObjectParentGameObject != null)
                parentId = kitchenObjectParentGameObject.GetComponent<PhotonView>().ViewID;
            PhotonManager.s.CmdSpawnFoodObject(kitchenObjectSO.prefab.GetComponent<ObjectTypeView>().objectType, parentId, ingredient);
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

        if(visualTransform == null){
            visualTransform = transform.GetChild(0);
        }
    }

    [PunRPC]
    public void RpcSetParentWithPhotonId(int photonId)
    {
        var findId = PhotonNetwork.GetPhotonView(photonId);
        if (findId == null)
            Debug.LogError("cant find id: " + photonId);
        var kitchenObjectParent = findId.GetComponent<IKitchenContainable>();

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



    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetContainerParent(IKitchenContainable otherContainer)
    {
        if (otherContainer == null)
        {
            this.transform.position = Vector3.zero;
            this.transform.parent = null;
            this._containerParent = otherContainer;
            return;
        }
        this._containerParent = otherContainer;
        otherContainer.SetKitchenObject(this);
        otherContainer.kitchenObject = this;
        this.transform.parent = otherContainer.GetKitchenObjectFollowTransform();
        this.transform.localPosition = Vector3.zero;
    }

    public IKitchenContainable GetKitchenObjectParent()
    {
        return _containerParent;
    }

    public void DestroySelf()
    {
        if (!SectionData.s.isSinglePlay)
            CmdDestroy();
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void CmdDestroy()
    {
        photonView.RPC(nameof(RpcDestroy), RpcTarget.All);
    }

    [PunRPC]
    public void RpcDestroy()
    {
        Destroy(this.gameObject);
    }
}
