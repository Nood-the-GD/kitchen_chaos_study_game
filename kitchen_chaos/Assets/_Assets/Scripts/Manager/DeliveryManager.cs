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
                RemoveOrder(i);
            }
        }
    }
    #endregion

    #region Multiplay
    void CmdAddRecipe(int index)
    {
        photonView.RPC("RPCAddRecipe", RpcTarget.All, index);
    }
    [PunRPC]
    void RPCAddRecipe(int recipeIndex)
    {
        AddOrder(recipeIndex);
    }
    #endregion

    #region Deliver recipe
    /// <summary>
    /// Check if the given plate contains a valid recipe
    /// </summary>
    /// <param name="plate">The plate that the player try to deliver</param>
    /// <returns>True if the plate contain a valid recipe, false otherwise</returns>
    public bool DeliverRecipe(PlateKitchenObject plate)
    {
        // Go through all the waiting recipes
        foreach (RecipeSO recipe in waitingRecipeSOList)
        {
            // Check if the number of ingredient in the recipe is equal to the number of ingredient in the plate
            if (recipe.kitchenObjectSOList.Count == plate.GetKitchenObjectSOList().Count)
            {
                // Set to true until we find a mismatch
                bool isMatch = true;
                // Go through all the ingredient in the plate
                foreach (KitchenObjectSO ingredient in plate.GetKitchenObjectSOList())
                {
                    // Check if the recipe contains the ingredient
                    if (!recipe.kitchenObjectSOList.Contains(ingredient))
                    {
                        // If not, set isMatch to false and break the loop
                        isMatch = false;
                        break;
                    }
                }
                // If all the ingredient match
                if (isMatch)
                {
                    // Remove the recipe from the waiting list
                    RemoveOrder(waitingRecipeSOList.IndexOf(recipe));
                    // Invoke the OnRecipeSuccess event
                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                    // Add the point of the recipe to the delivered point
                    recipeDeliveredPoint += recipe.Point;
                    // Update the UI
                    PointUI.Instance.UpdateUI();
                    // Return true
                    return true;
                }
            }
        }
        // None of the recipe match
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
        // Return false
        return false;
    }


    #endregion

    #region Private
    private void AddOrder(int index)
    {
        // Check that index is valid
        if (index < 0 || index >= recipeListSO.recipeSOList.Count)
        {
            Debug.LogError($"AddOrder: index {index} is out of range");
            return;
        }

        // Add the recipe to the waiting list
        waitingRecipeSOList.Add(recipeListSO.recipeSOList[index]);
        // Add a new timer to the list
        waitingTimerClassList.Add(new TimerClass 
        { 
            maxTimer = waitingTimeForEachRecipe, 
            timer = waitingTimeForEachRecipe 
        });

        // Invoke the OnRecipeAdded event
        OnRecipeAdded?.Invoke(this, EventArgs.Empty);
    }


    private void RemoveOrder(int recipeIndex)
    {
        // Check that recipeIndex is valid
        if (recipeIndex < 0 || recipeIndex >= waitingRecipeSOList.Count)
        {
            Debug.LogError($"RemoveOrder: recipeIndex {recipeIndex} is out of range");
            return;
        }
        // Check that both lists are the same size
        if (waitingRecipeSOList.Count != waitingTimerClassList.Count)
        {
            Debug.LogError($"RemoveOrder: waitingRecipeSOList and waitingTimerClassList are not the same size");
            return;
        }

        waitingRecipeSOList.RemoveAt(recipeIndex);
        waitingTimerClassList.RemoveAt(recipeIndex);
        // Invoke the OnRecipeRemove event
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
