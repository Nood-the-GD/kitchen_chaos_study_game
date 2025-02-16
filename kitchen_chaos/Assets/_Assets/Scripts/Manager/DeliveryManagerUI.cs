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
        DeliveryManager.Instance.OnRecipeAdded += DeliveryManager_OnRecipeAdded;
        DeliveryManager.Instance.OnRecipeRemove += DeliveryManager_OnRecipeRemoved;
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
    private void DeliveryManager_OnRecipeAdded(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }
    private void DeliveryManager_OnRecipeRemoved(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }
    #endregion

    #region Update visual
    private void UpdateVisual()
    {
        List<Recipe> waitingRecipeSOList = DeliveryManager.Instance.GetWaitingRecipeSOList();
        int numWaiting = waitingRecipeSOList.Count;
        for (int i = 0; i < numWaiting; i++)
        {
            if (i == recipeTemplateList.Count)
            {
                CreateRecipeTemplate(waitingRecipeSOList[i]);
            }
            else
            {
                recipeTemplateList[i].SetRecipeSO(waitingRecipeSOList[i]);
            }
        }

        // Remove extra templates
        while (recipeTemplateList.Count > numWaiting)
        {
            RemoveRecipeTemplate(recipeTemplateList.Count - 1);
        }
    }

    #endregion

    #region Private
    private void RemoveRecipeTemplate(int index)
    {
        Destroy(recipeTemplateList[index].gameObject);
        recipeTemplateList.RemoveAt(index);
    }
    private void CreateRecipeTemplate(Recipe recipe)
    {
        Transform template = Instantiate(recipeTemplate, container);
        template.gameObject.SetActive(true);
        template.GetComponent<RecipeTemplateUI>().SetRecipeSO(recipe);
        recipeTemplateList.Add(template.GetComponent<RecipeTemplateUI>());
    }
    #endregion
}
