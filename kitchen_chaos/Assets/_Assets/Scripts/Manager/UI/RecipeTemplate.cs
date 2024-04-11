using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeTemplate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconHolder;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private Slider waitingSlider;
    private RecipeSO recipeSO;

    private void Start()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    public void SetRecipeSO(RecipeSO recipeSO)
    {
        this.recipeSO = recipeSO;
        recipeNameText.text = recipeSO.recipeName;
        waitingSlider.maxValue = recipeSO.waitingTime;

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

    public void UpdateTimer()
    {
        waitingSlider.value = recipeSO.waitingTime;
    }
}
