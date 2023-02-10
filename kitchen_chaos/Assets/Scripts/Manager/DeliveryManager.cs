using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance{get; private set;}

    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        spawnRecipeTimer -= Time.deltaTime;
        if(spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if(waitingRecipeSOList.Count < waitingRecipeMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[Random.Range(0, recipeListSO.recipeSOList.Count)];
                Debug.Log(waitingRecipeSO.recipeName);
                waitingRecipeSOList.Add(waitingRecipeSO);
            }
        }
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
                    Debug.Log("Player is correct");
                    waitingRecipeSOList.RemoveAt(i);
                    return true;
                }
            }
        }
        Debug.Log("Player was wrong");
        return false; 
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }
}
