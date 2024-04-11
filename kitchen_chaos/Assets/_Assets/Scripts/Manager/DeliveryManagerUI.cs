using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform recipeTemplate;
    private List<RecipeTemplate> recipeTemplates= new List<RecipeTemplate>();

    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }
    void Update()
    {

    }
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeCompleted += DeliveryManager_OnRecipeCompleted;
        UpdateVisual();
    }

    private void DeliveryManager_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void DeliveryManager_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        List<RecipeSO> waitingList = DeliveryManager.Instance.GetWaitingRecipeSOList();
        if(recipeTemplates.Count > waitingList.Count)
        {
            for(int i = waitingList.Count - 1;i < recipeTemplates.Count; i++)
            {
                RemoveRecipeTemplate(i);
            }
        }
        if(recipeTemplates.Count < waitingList.Count)
        {
            for(int i = recipeTemplates.Count - 1; i < waitingList.Count; i++)
            {
                AddRecipeTemplate();
            }
        }
        // Update all RecipeTemplates
        for (int i = 0; i < recipeTemplates.Count; i++)
        {
            
        }
    }

    private void RemoveRecipeTemplate(int index)
    {
        Destroy(recipeTemplates[index].gameObject);
        recipeTemplates.RemoveAt(index);
    }
    private void AddRecipeTemplate()
    {
        Transform recipeTemplateTrans = Instantiate(this.recipeTemplate, container);
        RecipeTemplate recipeTemplate = recipeTemplateTrans.GetComponent<RecipeTemplate>();
        recipeTemplates.Add(recipeTemplate);
    }
}
