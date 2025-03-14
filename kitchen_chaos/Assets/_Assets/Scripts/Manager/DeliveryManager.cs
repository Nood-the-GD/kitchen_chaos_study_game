using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;
using System.Linq;

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
    public static DeliveryManager Instance { get; private set; }
    public List<KitchenObjectSO> OrderList => orderList.recipes.Select(recipe => recipe.output).ToList();
    [SerializeField]
    [InlineEditor]
    private CookingBookSO orderList;
    [SerializeField] private float waitingTimeForEachRecipe = 20f;
    private List<RecipeSO> waitingRecipeSOList = new List<RecipeSO>();
    private List<TimerClass> waitingTimerClassList = new List<TimerClass>();
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 6f;
    private int waitingRecipeMax = 3;
    public static int recipeDeliveredPoint = 0;
    public PhotonView photonView;
    #endregion

    #region Unity functions
    private void Awake()
    {

        if (Instance == null) Instance = this;
        recipeDeliveredPoint = 0;
    }
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CR_UpdateTimerClass());
            StartCoroutine(UpdateOrder());
        }
    }
    private void Update()
    {
        if (UserData.IsFirstTutorialDone == false)
        {
            return;
        }

        for (int i = 0; i < waitingTimerClassList.Count; i++)
        {
            waitingTimerClassList[i].timer -= Time.deltaTime;
        }

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (GameManager.s.IsGamePlaying() == false || GameManager.s.isTesting) return;

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingRecipeSOList.Count < waitingRecipeMax)
            {
                var index = UnityEngine.Random.Range(0, orderList.recipes.Count);
                AddOrder(index);
            }
        }

        for (int i = 0; i < waitingTimerClassList.Count; i++)
        {
            if (waitingTimerClassList[i].timer <= 0f)
            {
                RemoveOrder(i);
            }
        }
    }
    #endregion

    #region Multiplay
    void CmdUpdateList(string[] orders)
    {
        photonView.RPC(nameof(RpcUpdateList), RpcTarget.Others, orders);
    }
    [PunRPC]
    void RpcUpdateList(params string[] orders)
    {
        if (orders.Contains("None"))
        {
            var index = orderList.recipes.FindIndex(x => x.nameRec == orders[0]);
            UpdateOrder(0, index, 1);
            return;
        }
        for (int i = 0; i < orders.Length; i++)
        {
            string order = orders[i];
            if (order == "None")
                continue;
            var index = orderList.recipes.FindIndex(x => x.nameRec == order);
            UpdateOrder(i, index, orders.Length);
        }
    }

    void CmdUpdateTimerClass(int index, float value)
    {
        photonView.RPC(nameof(RPCUpdateTimerClass), RpcTarget.All, new object[] { index, value });
    }

    [PunRPC]
    void RPCUpdateTimerClass(int index, float timer)
    {
        if (index < waitingTimerClassList.Count - 1)
            waitingTimerClassList[index].timer = timer;
    }
    #endregion

    #region Delay functions
    IEnumerator UpdateOrder()
    {
        while (GameManager.s.IsGameOver() == false)
        {
            yield return new WaitForSeconds(1f);

            var listOfName = new List<string>();
            foreach (var recipe in waitingRecipeSOList)
            {
                listOfName.Add(recipe.nameRec);
            }

            if (listOfName.Count == 0)
            {
                continue;
            }
            if (listOfName.Count == 1)
            {
                listOfName.Add("None");
            }

            string[] arr = new string[listOfName.Count];
            for (int i = 0; i < listOfName.Count; i++)
            {
                arr[i] = listOfName[i];
            }
            CmdUpdateList(arr);

        }
    }
    private IEnumerator CR_UpdateTimerClass()
    {
        while (GameManager.s.IsGameOver() == false)
        {
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < waitingTimerClassList.Count; i++)
            {
                CmdUpdateTimerClass(i, waitingTimerClassList[i].timer);
            }
        }
    }
    #endregion

    #region Deliver recipe
    /// <summary>
    /// Check if the given plate contains a valid recipe
    /// </summary>
    /// <param name="kitchenObject">The plate that the player try to deliver</param>
    /// <returns>True if the plate contain a valid recipe, false otherwise</returns>
    public bool DeliverFood(KitchenObject kitchenObject)
    {
        // Go through all the waiting recipes
        foreach (RecipeSO recipe in waitingRecipeSOList)
        {
            // Check if the number of ingredient in the recipe is equal to the number of ingredient in the plate
            if (recipe.output == kitchenObject.GetKitchenObjectSO() && kitchenObject.IsHaveEnoughIngredient() && kitchenObject.IsHavingPlate)
            {
                // Remove the recipe from the waiting list
                RemoveOrder(waitingRecipeSOList.IndexOf(recipe));
                CmdOnDeliverySuccess();
                // Add the point of the recipe to the delivered point
                recipeDeliveredPoint += recipe.point;
                // Update the UI
                PointUI.Instance.UpdateUI();
                // Return true
                return true;
            }
        }
        Debug.Log("Failed: " + kitchenObject.GetKitchenObjectSO().name);
        // None of the recipe match
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
        // Return false
        return false;
    }
    public void CmdOnDeliverySuccess()
    {
        photonView.RPC(nameof(RpcOnDeliverySuccess), RpcTarget.All);
    }
    [PunRPC]
    public void RpcOnDeliverySuccess()
    {
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Private
    private void AddOrder(int index)
    {
        // Check that index is valid
        if (index < 0 || index >= orderList.recipes.Count)
        {
            Debug.LogError($"AddOrder: index {index} is out of range");
            return;
        }

        Debug.Log("AddOrder: " + orderList.recipes[index].nameRec);
        // Add the recipe to the waiting list
        waitingRecipeSOList.Add(orderList.recipes[index]);
        // Add a new timer to the list
        waitingTimerClassList.Add(new TimerClass
        {
            maxTimer = waitingTimeForEachRecipe,
            timer = waitingTimeForEachRecipe
        });

        // Invoke the OnRecipeAdded event
        OnRecipeAdded?.Invoke(this, EventArgs.Empty);
    }
    private void UpdateOrder(int indexOfOrder, int indexOfRecipe, int orderCount)
    {
        while (waitingRecipeSOList.Count != orderCount)
        {
            if (waitingRecipeSOList.Count > orderCount)
            {
                RemoveOrder(waitingRecipeSOList.Count - 1);
            }
            if (waitingRecipeSOList.Count < orderCount)
            {
                AddOrder(indexOfRecipe);
            }
        }
        waitingRecipeSOList[indexOfOrder] = orderList.recipes[indexOfRecipe];
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

    #region Tutorial
    public void AddTutorialOrder(string name = "Salad")
    {
        var index = orderList.recipes.FindIndex(x => x.nameRec == name);
        AddOrder(index);
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
