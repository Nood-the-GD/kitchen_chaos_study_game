using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DeliveryManager : MonoBehaviour
{
    #region TimerClass
    public class TimerClass
    {
        public float maxTimer;
        public float timer;
    }
    #endregion

    #region Variables
    public event EventHandler OnRecipeAdded;
    public event EventHandler OnRecipeRemove;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance{get; private set;}

    [SerializeField] private RecipeListSO recipeListSO;
    [SerializeField] private float waitingTimeForEachRecipe;

    private List<RecipeSO> waitingRecipeSOList = new List<RecipeSO>();
    private List<TimerClass> waitingTimerClassList = new List<TimerClass>();
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;
    public int recipeDeliveredPoint = 0;
    public PhotonView photonView;
    #endregion

    #region Unity functions
    private void Awake()
    {
        if(Instance == null) Instance = this;
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

        for(int i = 0; i < waitingTimerClassList.Count; i++)
        {
            waitingTimerClassList[i].timer -= Time.deltaTime;
            if(waitingTimerClassList[i].timer <= 0f)
            {
                // Debug.Log("Timer out");
                RemoveOrder(i);
            }
        }
    }
    #endregion

    #region Multiplay
    void CmdAddRecipe(int index){
        photonView.RPC("RPCAddRecipe", RpcTarget.All, index);
    }
    [PunRPC]
    void RPCAddRecipe(int index){
        AddOrder(index);
    }
    #endregion

    #region Deliver recipe
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
                    RemoveOrder(i);
                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                    recipeDeliveredPoint += waitingRecipeSO.Point;
                    PointUI.Instance.UpdateUI();
                    return true;
                }
            }
        }
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
        return false; 
    }
    #endregion

    #region Private
    private void AddOrder(int index)
    {
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[index];
        waitingRecipeSOList.Add(waitingRecipeSO);
        TimerClass timerClass = new TimerClass()
        {
            maxTimer = this.waitingTimeForEachRecipe,
            timer = this.waitingTimeForEachRecipe
        };
        waitingTimerClassList.Add(timerClass);
        OnRecipeAdded?.Invoke(this, EventArgs.Empty);
    }
    private void RemoveOrder(int index)
    {
        waitingRecipeSOList.RemoveAt(index);
        waitingTimerClassList.RemoveAt(index);
        OnRecipeRemove?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Get
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }
    public List<TimerClass> GetWaitingTimerClassList()
    {
        return waitingTimerClassList;
    }
    public int GetSuccessfulRecipePoint()
    {
        return recipeDeliveredPoint;
    }
    #endregion
}
