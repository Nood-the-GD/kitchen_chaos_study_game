using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Sirenix.OdinInspector;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;

public class KitchenObject : MonoBehaviourPunCallbacks
{
    [SerializeField] public KitchenObjectSO kitchenObjectSO;

    private IKitchenContainable _containerParent;
    private PhotonView photonView;
    private Transform visualTransform;
    [ReadOnly] public Transform platePoint;
    public Action<List<KitchenObjectSO>> onAddIngredient;
    [SerializeField] private SerializedDictionary<KitchenObjectSO, GameObject> _ingredientMapping = new SerializedDictionary<KitchenObjectSO, GameObject>();
    [ReadOnly] public List<KitchenObjectSO> ingredients = new List<KitchenObjectSO>();

    // Track last synced parent's PhotonView ID to avoid redundant updates.
    private int lastSyncedParentId = -1;

    // Timestamp for tracking interaction order
    [HideInInspector] public long interactionTimestamp =-1;

    public bool IsHavingPlate => platePoint != null;
    public bool IsPlate => kitchenObjectSO.name == "Plate";

    public bool TryAddPlate()
    {
        if (IsHavingPlate || IsPlate)
            return false;

        if (SectionData.s.isSinglePlay)
        {
            AddPlateLocal(-1);
        }
        else
        {
            photonView.RPC(nameof(RpcTryAddPlate), RpcTarget.All);
        }
        return true;
    }

    [PunRPC]
    private void RpcTryAddPlate()
    {
        AddPlateLocal(-1);
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
        ingredients.Add(ingredient);
        SetActiveIngredient(ingredient);
        if (addPlate)
        {
            TryAddPlate();
        }
        onAddIngredient?.Invoke(ingredients);
    }

    public void AddIngredientIndexes(int[] ingredientIndex)
    {
        Debug.Log("AddIngredientIndexes: " + ingredientIndex.Length);
        var rep = CookingBookSO.s.FindRecipeByOutput(kitchenObjectSO);
        if (rep == null)
            return;
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
        if (ingredients.Count == 0)
            return false;

        var recipe = CookingBookSO.s.FindRecipeByOutput(kitchenObjectSO);
        if (recipe == null)
            return false;

        var isSame = recipe.IsSameIngredients(ingredients);
        Debug.Log("IsHaveEnoughIngredient: " + isSame);
        if (!isSame)
        {
            foreach (var i in ingredients)
                Debug.Log("Ingredient: " + i.name);
            foreach (var i in recipe.ingredients)
                Debug.Log("Recipe ingredient: " + i.name);
        }
        return isSame;
    }

    public static void SpawnKitchenObject(ObjectEnum objectEnum, IKitchenContainable kitchenObjectParent)
    {
        SpawnKitchenObject(
            GameData.s.GetObject(objectEnum).GetComponent<KitchenObject>().GetKitchenObjectSO(),
            kitchenObjectParent, new List<int> { 0 }
        );
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenContainable kitchenObjectParent)
    {
        SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent, new List<int> { 0 });
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenContainable kitchenObjectParent, List<int> ingredient, bool isHavePlate = false)
    {
        var kitchenObjectParentGameObject = kitchenObjectParent as MonoBehaviour;
        if (SectionData.s.isSinglePlay)
        {
            var obj = Instantiate(kitchenObjectSO.prefab, kitchenObjectParentGameObject.transform);
            var p = obj.GetComponent<KitchenObject>();
            p.kitchenObjectSO = kitchenObjectSO;
            p.CmdSetContainerParent(kitchenObjectParent);
            p.AddIngredientIndexes(ingredient.ToArray());
            if (isHavePlate)
                p.TryAddPlate();
        }
        else
        {
            var player = kitchenObjectParent as Player;
            if (player != null && !player.photonView.IsMine)
                return;

            int parentId = -1;
            if (kitchenObjectParentGameObject != null)
                parentId = kitchenObjectParentGameObject.GetComponent<PhotonView>().ViewID;
            int koId = CookingBookSO.s.GetKitchenObjectSoId(kitchenObjectSO);
            PhotonManager.s.CmdSpawnFoodObject(kitchenObjectSO.prefab.GetComponent<ObjectTypeView>().objectType, koId, parentId, ingredient, isHavePlate);
        }
    }

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        visualTransform = transform.GetChild(0);
        if (_ingredientMapping != null)
        {
            foreach (var item in _ingredientMapping)
            {
                item.Value.SetActive(false);
            }
        }
    }

    // Removed the OnSync coroutine
    // Instead, rely on immediate RPC calls when a change happens

    // async func reset interaction timestamp after 50ms using thread delay
    public async void ResetInteractionTimestamp()
    {
        await UniTask.Delay(50);
        interactionTimestamp = -1;
    }

    [PunRPC]
    public void RpcSetParentWithPhotonId(int photonId, long timestamp = 0)
    {

        if(timestamp < interactionTimestamp && interactionTimestamp != -1){
            ResetInteractionTimestamp();
            return;
        }

        interactionTimestamp = timestamp;
 
        PhotonView parentPhotonView = PhotonNetwork.GetPhotonView(photonId);
        if (parentPhotonView == null)
        {
            Debug.LogError("Can't find PhotonView with ID: " + photonId);
            return;
        }

        IKitchenContainable kitchenObjectParent = parentPhotonView.GetComponent<IKitchenContainable>();
        if (kitchenObjectParent == null)
        {
            Debug.LogError("KitchenObjectParent is null for PhotonView ID: " + photonId);
            return;
        }

        // If the current parent is already set to this, no need to update.
        if (_containerParent != null)
        {
            MonoBehaviour currentParentMB = _containerParent as MonoBehaviour;
            if (currentParentMB != null && currentParentMB.GetComponent<PhotonView>().ViewID == photonId)
                return;
        }

        SetContainerParentLocal(kitchenObjectParent);
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    private void SetContainerParentLocal(IKitchenContainable newContainer)
    {
        // Clear the object's current parent if it exists.
        if (_containerParent != null)
        {
            _containerParent.ClearKitchenObject(false);
        }

        _containerParent = newContainer;
        if (newContainer != null)
        {
            newContainer.SetKitchenObject(this);
            newContainer.kitchenObject = this;
            this.transform.parent = newContainer.GetKitchenObjectFollowTransform();
            this.transform.localPosition = Vector3.zero;
            lastSyncedParentId = (newContainer as MonoBehaviour).GetComponent<PhotonView>().ViewID;
        }
        else
        {
            transform.position = Vector3.zero;
            transform.parent = null;
            lastSyncedParentId = -1;
        }
    }

    /// <summary>
    /// Command the update of the container parent.
    /// In single play, update locally.
    /// In multiplayer, only send an update if the parent has really changed.
    /// </summary>
    public void CmdSetContainerParent(IKitchenContainable newContainer)
    {
        // Update the interaction timestamp
        interactionTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (SectionData.s.isSinglePlay)
        {
            SetContainerParentLocal(newContainer);
        }
        else
        {
            int newParentId = (newContainer as MonoBehaviour).GetComponent<PhotonView>().ViewID;
            if (newParentId != lastSyncedParentId)
            {
                photonView.RPC(nameof(RpcSetParentWithPhotonId), RpcTarget.All, newParentId, interactionTimestamp);
                lastSyncedParentId = newParentId;
            }
        }
    }

    public IKitchenContainable GetKitchenObjectParent()
    {
        return _containerParent;
    }

    public void DestroySelf()
    {
        if (SectionData.s.isSinglePlay)
        {
            Destroy(this.gameObject);
        }
        else
        {
            CmdDestroy();
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