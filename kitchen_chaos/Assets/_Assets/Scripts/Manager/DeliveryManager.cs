using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance{get; private set;}

    [SerializeField] private RecipeListSO recipeListSO;
    [SerializeField] private float waitingTimeForEachRecipe;

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;
    public int recipeDeliveredPoint = 0;
    public PhotonView photonView;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

        //feature only for mobile, because in editor sometime need to test of line
        if(PhotonNetwork.PlayerList.Length <2 && !Application.isEditor){
            return;
        }



        spawnRecipeTimer -= Time.deltaTime;
        if(spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if(waitingRecipeSOList.Count < waitingRecipeMax)
            {
                var index = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
                CmdAddRecipe(index);
            }
        }

        foreach(var recipe in waitingRecipeSOList)
        {
            recipe.waitingTime -= Time.deltaTime;
            if(recipe.waitingTime <= 0f)
            {

            }
        }
    }

    void CmdAddRecipe(int index){
        photonView.RPC("RPCAddRecipe", RpcTarget.All, index);
    }

    [PunRPC]
    void RPCAddRecipe(int index){
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[index];
        waitingRecipeSO.waitingTime = this.waitingTimeForEachRecipe;
        waitingRecipeSOList.Add(waitingRecipeSO);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public bool DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for(int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if(waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool isTrue = false;
                //Has the same ingredient number
                foreach(KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                {
                    if(waitingRecipeSO.kitchenObjectSOList.Contains(plateKitchenObjectSO))
                    {
                        //Ingredient on plate is in this recipe
                        isTrue = true;
                        continue;
                    }
                    else
                    {
                        //Ingredient on plate is not in this recipe
                        isTrue = false;
                        break;
                    }
                }
                if(isTrue == true) 
                {
                    // Delivery Success
                    waitingRecipeSOList.RemoveAt(i);
                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    recipeDeliveredPoint += waitingRecipeSO.Point;
                    PointUI.Instance.UpdateUI();
                    return true;
                }
            }
        }
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
        return false; 
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    private void TimeOutRecipe(RecipeSO recipeSO)
    {
        // Minus point
        waitingRecipeSOList.Remove(recipeSO);
    }

    public int GetSuccessfulRecipePoint()
    {
        return recipeDeliveredPoint;
    }
}
