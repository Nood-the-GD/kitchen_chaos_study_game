using System.Collections;
using Photon.Pun;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using AYellowpaper.SerializedCollections;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] public KitchenObjectSO kitchenObjectSO;

    private IKitchenContainable _containerParent;
    PhotonView photonView;
    public Transform visualTransform;
    [ReadOnly] public Transform platePoint;
    public Action<List<KitchenObjectSO>> onAddIngredient;
    [SerializeField] private SerializedDictionary<KitchenObjectSO, GameObject> _ingredientMapping = new SerializedDictionary<KitchenObjectSO, GameObject>();
    [ReadOnly] public List<KitchenObjectSO> ingredients = new List<KitchenObjectSO>();

    public bool IsHavingPlate => platePoint != null;
    public bool IsPlate => kitchenObjectSO.name == "Plate";

    public bool TryAddPlate()
    {
        if (IsHavingPlate)
            return false;
        if (IsPlate)
            return false;
        if (SectionData.s.isSinglePlay)
        {
            AddPlateLocal(-1);
        }
        else
        {
            PhotonManager.s.CmdAddPlate(photonView.ViewID);
        }
        return true;
    }

    public void AddPlateLocal(int viewId)
    {
        var plate = Instantiate(GameData.s.GetObject(ObjectEnum.Plate), Vector3.zero, Quaternion.identity).transform;
        if (viewId != -1)
            plate.GetComponent<PhotonView>().ViewID = viewId;
        plate.SetParent(visualTransform);
        platePoint = plate;
        plate.transform.localPosition = Vector3.zero;
    }

    public void AddIngredient(KitchenObjectSO ingredient, bool addPlate = false)
    {
        this.ingredients.Add(ingredient);
        SetActiveIngredient(ingredient);
        if (addPlate)
        {
            TryAddPlate();
        }
    }

    public void AddIngredientIndexes(int[] ingredientIndex)
    {
        var rep = CookingBookSO.s.FindRecipeByOutput(kitchenObjectSO);
        if (rep == null)
        {
            return;
        }
        foreach (var i in ingredientIndex)
        {
            AddIngredient(rep.ingredients[i]);
        }
    }

    public void SetActiveIngredient(KitchenObjectSO ingredient)
    {
        if (_ingredientMapping.TryGetValue(ingredient, out var visual))
        {
            visual.SetActive(true);
        }
    }

    public bool IsHaveEnoughIngredient()
    {
        var recipe = CookingBookSO.s.FindRecipeByOutput(kitchenObjectSO);
        return recipe.IsSameIngredients(ingredients);
    }


    public static void SpawnKitchenObject(ObjectEnum objectEnum, IKitchenContainable kitchenObjectParent)
    {
        SpawnKitchenObject(
            GameData.s.GetObject(objectEnum).GetComponent<KitchenObject>().GetKitchenObjectSO()
            , kitchenObjectParent, new List<int> { 0 }
        );
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenContainable kitchenObjectParent)
    {
        SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent, new List<int> { 0 });
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenContainable kitchenObjectParent, List<int> ingredient, bool isHavePlate = false)
    {
        //convert interface to gameObject
        var kitchenObjectParentGameObject = kitchenObjectParent as MonoBehaviour;
        if (SectionData.s.isSinglePlay)
        {
            var obj = Instantiate(kitchenObjectSO.prefab, kitchenObjectParentGameObject.transform);
            var p = obj.GetComponent<KitchenObject>();
            p.kitchenObjectSO = kitchenObjectSO;
            p.SetContainerParent(kitchenObjectParent);
            p.AddIngredientIndexes(ingredient.ToArray());
            if (isHavePlate)
            {
                p.TryAddPlate();
            }
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
            PhotonManager.s.CmdSpawnFoodObject(kitchenObjectSO.prefab.GetComponent<ObjectTypeView>().objectType, parentId, ingredient, isHavePlate);
        }
    }
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (visualTransform != null)
        {
            var count = visualTransform.childCount;
            for (var i = 0; i < count; i++)
            {
                visualTransform.GetChild(i).gameObject.SetActive(false);
            }
        }
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
        if (_containerParent != null)
        {
            _containerParent.ClearKitchenObject(false);
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
