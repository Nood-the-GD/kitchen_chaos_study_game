using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeTemplate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;

    [SerializeField] private Transform iconHolder;
    [SerializeField] private Transform iconTemplate;

    private void Start()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.recipeName;

        foreach(Transform child in iconHolder)
        {
            if(child != iconTemplate) Destroy(child.gameObject);
        }

        foreach(KitchenObjectSO kitchenObjectSO in recipeSO.kitchenObjectSOList)
        {
            Transform iconTemplateClone = Instantiate(iconTemplate, iconHolder);
            iconTemplateClone.gameObject.SetActive(true);
            iconTemplateClone.GetComponent<IconTemplate>().SetKitchenObjectSO(kitchenObjectSO);
        }
    }
}
