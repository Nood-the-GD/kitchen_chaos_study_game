using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    #region Variables
    [SerializeField] private Transform container;
    [SerializeField] private Transform recipeTemplate;
    private List<RecipeTemplateUI> recipeTemplateList= new List<RecipeTemplateUI>();
    #endregion

    #region Unity functions
    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeAdded += DeliveryManager_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeRemove += DeliveryManager_OnRecipeCompleted;
        UpdateVisual();
    }
    void Update()
    {
        for(int i = 0; i < recipeTemplateList.Count; i++)
        {
            recipeTemplateList[i].UpdateTimer(DeliveryManager.Instance.GetWaitingTimerClassList()[i]);
        }
    }
    #endregion

    #region Events
    private void DeliveryManager_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }
    private void DeliveryManager_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }
    #endregion

    #region Update visual
    private void UpdateVisual()
    {
        List<RecipeSO> waitingRecipeSOList = DeliveryManager.Instance.GetWaitingRecipeSOList();
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            if(i == recipeTemplateList.Count)
            {
                // i over the final index of recipeTemplateList
                AddRecipeTemplate(waitingRecipeSOList[i]);
                continue;
            }

            recipeTemplateList[i].SetRecipeSO(waitingRecipeSOList[i]);
            if(i == waitingRecipeSOList.Count - 1 && i < recipeTemplateList.Count - 1)
            {
                // reach the final index of waitingRecipeSOList but not reach the final index of recipeTemplateList
                for(int j = i; j < recipeTemplateList.Count; j++)
                {
                    RemoveRecipeTemplate(j);
                }
            }
        }
    }
    #endregion

    #region Private
    private void RemoveRecipeTemplate(int index)
    {
        Destroy(recipeTemplateList[index].gameObject);
        recipeTemplateList.RemoveAt(index);
    }
    private void AddRecipeTemplate(RecipeSO recipeSO)
    {
        Transform recipeTemplateTrans = Instantiate(this.recipeTemplate, container);
        RecipeTemplateUI recipeTemplate = recipeTemplateTrans.GetComponent<RecipeTemplateUI>();
        recipeTemplate.SetRecipeSO(recipeSO);
        recipeTemplate.gameObject.SetActive(true);
        recipeTemplateList.Add(recipeTemplate);
    }
    #endregion
}
