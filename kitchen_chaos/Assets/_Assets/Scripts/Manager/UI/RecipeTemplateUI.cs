using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static DeliveryManager;

public class RecipeTemplateUI : MonoBehaviour
{
    #region Variables
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconHolder;
    [SerializeField] private IconTemplate iconTemplate;
    [SerializeField] private Slider waitingSlider;
    private List<IconTemplate> iconTemplateList= new List<IconTemplate>();
    #endregion

    #region Unity functions
    private void Start()
    {
        iconTemplate.gameObject.SetActive(false);
    }
    #endregion

    #region Public functions
    public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.nameRec;
        for(int i = 0; i < recipeSO.ingredients.Count; i++)
        {
            if(i == iconTemplateList.Count)
            {
                // i over the final index
                IconTemplate iconTemplateClone = Instantiate(iconTemplate, iconHolder);
                iconTemplateClone.gameObject.SetActive(true);
                iconTemplateClone.SetKitchenObjectSO(recipeSO.ingredients[i]);
                iconTemplateList.Add(iconTemplateClone);
                continue;
            }
            // Update new KitchenObjectSO
            iconTemplateList[i].SetKitchenObjectSO(recipeSO.ingredients[i]);
            if(i == recipeSO.ingredients.Count - 1 && i < iconTemplateList.Count - 1)
            {
                // reach the final index of kitchenObjectSOList but not reach the final index of iconTemplates
                for(int j = i; j < iconTemplateList.Count; j++)
                {
                    Destroy(iconTemplateList[j].gameObject);
                    iconTemplateList.RemoveAt(j);
                }
            }
        }
    }
    public void UpdateTimer(TimerClass timerClass)
    {
        waitingSlider.maxValue = timerClass.maxTimer;
        waitingSlider.value = timerClass.timer;
    }
    #endregion
}
