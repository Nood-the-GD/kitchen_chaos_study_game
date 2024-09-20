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

    #endregion

    #region Variables
    public event EventHandler OnRecipeAdded;
    public event EventHandler OnRecipeRemove;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;



    public static DeliveryManager Instance { get; private set; }

    private float waitingTimeForEachRecipe = 5;
    private List<Order> waitingCompleteDish = new List<Order>();
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
    private void FixedUpdate()
    {

        foreach (var i in waitingCompleteDish)
        {
            i.timer -= Time.deltaTime;
        }

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (GameManager.Instance.IsGamePlaying() == false || GameManager.Instance.isTesting) return;

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingCompleteDish.Count < waitingRecipeMax)
            {
                var index = UnityEngine.Random.Range(0, GameData.s.completeDishSOs.Count);
                CmdAddOrder(index);
            }
        }

        for (int i = 0; i < waitingCompleteDish.Count; i++)
        {
            if (waitingCompleteDish[i].timer <= 0f)
            {
                CmdRemoveOrder(i);
            }
        }
    }
    #endregion

    #region Multiplay

    #endregion


    #region Deliver recipe

    /// <summary>
    /// Check if the given plate contains a valid recipe
    /// </summary>
    /// <param name="completeDishKitchenObject">The plate that the player try to deliver</param>
    /// <returns>True if the plate contain a valid recipe, false otherwise</returns>
    public bool DeliverRecipe(KitchenObject completeDishKitchenObject)
    {
        var find = waitingCompleteDish.Find(x => x.completeDish.name == completeDishKitchenObject.name);
        if (find != null)
        {
            // Remove the recipe from the waiting list
            CmdRemoveOrder(waitingCompleteDish.IndexOf(find));
            // Invoke the OnRecipeSuccess event
            OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
            // Add the point of the recipe to the delivered point
            recipeDeliveredPoint += find.completeDish.point;
            // Update the UI
            PointUI.Instance.UpdateUI();
            // Return true
            return true;
        }

        // None of the recipe match
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
        // Return false
        return false;
    }


    #endregion

    #region Private
    private void CmdAddOrder(int index)
    {
        photonView.RPC(nameof(AddOrder), RpcTarget.All, index);
    }

    [PunRPC]
    private void AddOrder(int index)
    {
        // Check that index is valid
        if (index < 0 || index >= GameData.s.completeDishSOs.Count)
        {
            Debug.LogError($"AddOrder: index {index} is out of range");
            return;
        }


        // Add the recipe to the waiting list
        waitingCompleteDish.Add(GameData.s.completeDishSOs[index].ConvertToOrder());
        // Invoke the OnRecipeAdded event
        OnRecipeAdded?.Invoke(this, EventArgs.Empty);
    }

    private void CmdRemoveOrder(int index)
    {
        photonView.RPC(nameof(RemoveOrder), RpcTarget.All, index);
    }

    [PunRPC]
    private void RemoveOrder(int recipeIndex)
    {
        // Check that recipeIndex is valid
        if (recipeIndex < 0 || recipeIndex >= waitingCompleteDish.Count)
        {
            Debug.LogError($"RemoveOrder: recipeIndex {recipeIndex} is out of range");
            return;
        }

        waitingCompleteDish.RemoveAt(recipeIndex);

        OnRecipeRemove?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Get
    public List<Order> GetWaitingRecipeSOList()
    {
        return waitingCompleteDish;
    }
    public int GetSuccessfulRecipePoint()
    {
        return recipeDeliveredPoint;
    }
    #endregion
}
