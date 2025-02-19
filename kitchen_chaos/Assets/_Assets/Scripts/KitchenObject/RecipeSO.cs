using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "KitchenObject")]
public class RecipeSO : ScriptableObject
{
    public string nameRec;
    public List<KitchenObjectSO> ingredients;

    [EnumToggleButtons]
    public KitchenObjectType actionType;

    public KitchenObjectSO output;

    public float step;
    public int point = 5;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (output != null && output.name != this.name)
        {
            nameRec = output.name;
            this.name = nameRec;
            var path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(path, nameRec);
            AssetDatabase.SaveAssets();
        }
    }
#endif

    public override string ToString()
    {
        return nameRec;
    }

    public bool IsSameIngredients(List<KitchenObjectSO> kitchenObjectSOs)
    {
        // Ensure both lists are not null and then use SequenceEqual for a one-line comparison
        return kitchenObjectSOs != null && ingredients != null &&
            ingredients.All(item => kitchenObjectSOs.Contains(item));
    }

    public bool IsSubsetIngredients(List<KitchenObjectSO> kitchenObjectSOs)
    {
        // Check that the provided list is not null and all its items exist in the ingredients list
        return kitchenObjectSOs != null && ingredients != null &&
            kitchenObjectSOs.All(item => ingredients.Contains(item));
    }
}
