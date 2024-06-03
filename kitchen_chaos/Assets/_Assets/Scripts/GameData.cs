using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class InGameProduct{
        public string id;
        public int amount;

        public int starRMAmount;
        public enum ItemShopType{
            buyOneTime,
            upgradeable
        }

        public ItemShopType itemShopType;
        [ShowIf("itemShopType", ItemShopType.upgradeable)]
        public int maxUpdate;
        string key => "Shop_"+id;
        public int curUpdate{
            get{
                return PlayerPrefs.GetInt(key, 0);
            }
            set => PlayerPrefs.SetInt(key, value);
        }

        public bool isRealMoney;
        public bool autoSetPrice = true;
}
[System.Serializable]
public class ColorSkin{
    public Color color;
    public string colorCode;
    public Material material;
    public Sprite icon;
}

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData", order = 1)]
public class GameData : SerializedScriptableObject
{
    static GameData _s;
    public static GameData s
    {
        get
        {
            if (_s == null)
            {
                _s = Resources.Load("GameData") as GameData;
                if (_s == null)
                    Debug.LogError("Cant find GameData");
            }

            return _s;
        }
    }

    public List<StageData> stages;
    public List<ColorSkin> colorElements;
  
    #region editor
    bool showEdit;
    [ShowIf(nameof(showEdit))][Button]
    public void Save()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Saved");
#endif
    }

    [HideIf(nameof(showEdit))]
    [Button]
    void ShowEdit()
    {
        showEdit = true;
    }

    [ShowIf(nameof(showEdit))]
    [Button]
    void HideEdit()
    {
        showEdit = false;
    }

#if UNITY_EDITOR
    [ShowIf(nameof(showEdit))]
    [Button]
    void UpdatePath()
    {
        var photonViews = Resources.FindObjectsOfTypeAll<PhotonView>();
        prefabPaths.Clear();
        foreach(var i in photonViews)
        {
            if (i.gameObject.GetComponent<ObjectTypeView>() == null)
                continue;
            var id = i.gameObject.GetComponent<ObjectTypeView>().objectType;
            if(string.IsNullOrEmpty(id)){
                Debug.LogError(i.gameObject.name+ " is empty id");
                continue;
            }

            var path = AssetDatabase.GetAssetPath(i.gameObject);
            path = FormatPath(path);
            //Debug.Log("update path: "+ path + " "+ i.gameObject.name);
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("path is empty " + i.gameObject.name);
                continue;
            }
            if (prefabPaths.ContainsKey(id.ToString()))
            {
                prefabPaths[id.ToString()] = path;
            }
            else
            {
                prefabPaths.Add(id.ToString(), path);
            }
        }

        var kitchenObjectSOs = Resources.FindObjectsOfTypeAll<KitchenObjectSO>();
        foreach(var kitchenObjectSO in kitchenObjectSOs)
        {
            if(kitchenObjectSODic.ContainsKey(kitchenObjectSO.objectName))
            {
                kitchenObjectSODic[kitchenObjectSO.objectName] = kitchenObjectSO;
            }
            else
            {
                kitchenObjectSODic.Add(kitchenObjectSO.objectName, kitchenObjectSO);
            }
        }
        Save();
    }
    [ShowIf(nameof(showEdit))]
    [Button]
    private void UpdateCustomer()
    {
        _customerList.Clear();
        var customers = Resources.FindObjectsOfTypeAll<Customer>();
        foreach(var i in customers)
        {
            _customerList.Add(i);
        }
        Save();
    }

    [ShowIf(nameof(showEdit))]
    [Button]
    void GenerateConstantFile(){
        var names = new List<string>();
        foreach(var i in objectTypeViews)
        {
            names.Add(i.Key.ToString());
        }
        AutoGenerateFile.GenerateEnum("ObjectEnum", names);

        names.Clear();
        // foreach(var i in animSequences){
        //     names.Add(i.Key.ToString());
        // }
        // AutoGenerateFile.GenerateEnum("AnimSequenceType", names);


        names.Clear();
        foreach(var i in icons){
            names.Add(i.Key.ToString());
        }
        AutoGenerateFile.GenerateEnum("IconType", names);
        AssetDatabase.Refresh();
    }

    string FormatPath(string input)
    {
        var split = input.Split('/');
        int startIndex = -1;
        for (int i = 0; i < split.Length; i++)
        {
            if (split[i] == "Resources")
                startIndex = i;
        }

        string res = "";
        for (int i = startIndex+1; i < split.Length; i++)
        {
            if (!string.IsNullOrEmpty(res))
                res += "/";

            res += split[i];
        }

        return res.Replace(".prefab","");
    }
    
    
    public void AddNewObjectTypeViews(ObjectTypeView objectTypeView)
    {
        //check if objectTypeView is active in the current scene
        if (objectTypeView.gameObject.scene.name != null)
        {
            Debug.LogError("objectTypeView is active in the" + objectTypeView.gameObject.scene.name + " scene");
            return;
        }


        //check if any item in objectTypeViews is null then delete it
        var removeKeys = new List<string>();
        foreach (var i in objectTypeViews)
        {
            if (i.Value == null)
            {
                removeKeys.Add(i.Key);
            }
        }
        foreach (var i in removeKeys)
        {
            objectTypeViews.Remove(i);
            Debug.Log("Remove " + i);
        }

        objectTypeViews.Add(objectTypeView.objectType, objectTypeView);
        
        //save
        Save();
    }

    [ShowIf(nameof(showEdit))] [Button]
    void MergeFrom(GameData other){
        //merge objectTypeViews
        foreach(var i in other.objectTypeViews){
            if(objectTypeViews.ContainsKey(i.Key)){
                objectTypeViews[i.Key] = i.Value;
            }else{
                objectTypeViews.Add(i.Key, i.Value);
            }
        }
        
        //merge inGameProducts
        foreach(var i in other.inGameProducts){
            if(inGameProducts.Find(x=> x.id == i.id) == null){
                inGameProducts.Add(i);
            }
        }

        //merge icons
        foreach(var i in other.icons){
            if(icons.ContainsKey(i.Key)){
                icons[i.Key] = i.Value;
            }else{
                icons.Add(i.Key, i.Value);
            }
        }



        Save();
    }
    

    [ShowIf(nameof(showEdit))] [Button(Icon = SdfIconType.ArrowCounterclockwise, IconAlignment = IconAlignment.LeftOfText)]
    public void RefeshData(){
        //refesh objectTypeViews
        var list = Resources.FindObjectsOfTypeAll<ObjectTypeView>();


        objectTypeViews = new Dictionary<string, ObjectTypeView>();
        for (int i = 0; i < list.Length; i++)
        {
            //if object is in the scene then skip 
            if(list[i].gameObject.scene.name != null){
                //Debug.LogError("ObjectTypeView is active in the "+ list[i].gameObject.scene.name + " scene");
                continue;
            }

            if(!list[i].gameObject.name.IsValidNameForEnum()){
                Debug.LogError("ObjectTypeView name is not valid: "+ list[i].name);
            }

            if(list[i].objectType != list[i].gameObject.name){
                list[i].objectType = list[i].gameObject.name;
                //list[i].SetDirty();
            }

            if(objectTypeViews.ContainsKey(list[i].objectType)){
                Debug.LogError("Duplicate objectTypeViews: "+ list[i].gameObject.name);
                continue;
            }
            objectTypeViews.Add(list[i].objectType, list[i]);
        }

        UpdateCustomer();
        UpdatePath();
        Save();
    }
#endif
    #endregion

    public StageData GetStage(int levelId){
        var find = stages.Find(x=>x.levelId == levelId);
        if(find == null){
            Debug.LogError("Cant find stage with levelId: "+ levelId);
        }
        return find;
    }

    public Dictionary<string, string> prefabPaths = new Dictionary<string, string>();
    public Dictionary<string, KitchenObjectSO> kitchenObjectSODic = new Dictionary<string, KitchenObjectSO>();
    [Searchable]
    [ListDrawerSettings(ShowIndexLabels = true, NumberOfItemsPerPage = 10)]
    public Dictionary<string, ObjectTypeView> objectTypeViews = new Dictionary<string, ObjectTypeView>();
    public List<Customer> _customerList = new List<Customer>();
    
    public ColorSkin GetColorSkin(string id){
        return colorElements.Find(x=>x.colorCode == id);
    }


    public List<InGameProduct> inGameProducts;
    public int GetCurUpdateItem(string product){
        return inGameProducts.Find(x=> x.id == product).curUpdate;
    }

    public Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();



    //get object
    public GameObject GetObject(ObjectEnum id)
    {
        if (objectTypeViews.ContainsKey(id.ToString()))
        {
            return objectTypeViews[id.ToString()].gameObject;
        }
        else
        {
            Debug.LogError("Cant find object with id: " + id);
            return null;
        }
    }

    public GameObject GetObject(string id){
        if (objectTypeViews.ContainsKey(id))
        {
            return objectTypeViews[id].gameObject;
        }
        else
        {
            Debug.LogError("Cant find object with id: " + id);
            return null;
        }
    }

    public string GetPath(ObjectEnum id)
    {
        if (prefabPaths.ContainsKey(id.ToString()))
        {
            return prefabPaths[id.ToString()];
        }
        else
        {
            Debug.LogError("Cant find path with id: " + id);
            return null;
        }
    }

    public string GetPath(string id)
    {
        if (prefabPaths.ContainsKey(id))
        {
            return prefabPaths[id];
        }
        else
        {
            Debug.LogError("Cant find path with id: " + id);
            return null;
        }
    }

    public KitchenObjectSO GetKitchenObjectSO(string objectName)
    {
        if (kitchenObjectSODic.ContainsKey(objectName))
        {
            return kitchenObjectSODic[objectName];
        }
        else
        {
            Debug.LogError("Cant find object with name: " + objectName);
            return null;
        }
    }

    
    public InGameProduct GetInGameProduct(string id){
        var inGameProduct = inGameProducts.Find(x=>x.id == id);
        if(inGameProduct == null){
            Debug.LogError("cant find: "+ id);
            return null;
        }

        return inGameProduct;
    }

    public Customer GetCustomer(int id)
    {
        var customer = _customerList[id];
        if (customer == null)
        {
            Debug.LogError("cant find customer at: " + id);
            return null;
        }
        return customer;
    }
}
