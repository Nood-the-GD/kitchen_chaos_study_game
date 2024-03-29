using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FastTranslate.Internal;
using TMPro;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace FastTranslate
{

    public static class FTHelper{
        public static void setDirty(this Object i)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(i);
            #endif
        }
    }

    [System.Serializable]
    public class LangSupport
    {
        public Languages languagesSuport;
        public string name;
    }

    [System.Serializable]
    public class LocalizeFont{
        public string fontName;
        public List<FontAsset> fontAssets = new List<FontAsset>();

        [System.Serializable]
        public class FontAsset{
            public Languages languagesSuport;
            public Font font;
            public TMP_FontAsset tmFont;
            
        }

    }

    //[CreateAssetMenu(fileName = "Project", menuName = "Gridly/Project", order = 1)]
    public class Project : ScriptableObject
    {

        static Project _singleton;
        public string ProjectID;
        UnityEngine.TextAsset langDefineTextAsset
        {
            get{
                return (UnityEngine.TextAsset)Resources.Load("LanguageDefine");
            }
        }
        private string chosenLangCodeName{
            get{
                return PlayerPrefs.GetString("FT.chosenLangCodeName",langSupports[0].languagesSuport.ToString());
            }
            set{
                PlayerPrefs.SetString("FT.chosenLangCodeName",value);
            }
        }


        public LangSupport targetLanguage
        {
            set { chosenLangCodeName = value.languagesSuport.ToString(); }
            get
            {
                LangSupport _ = langSupports.Find(x => x.languagesSuport.ToString() == chosenLangCodeName);
                try
                {

                    if (_ == null)
                    {
                        //Debug.Log("Cant find the language: " + chosenLangCodeName);
                        return langSupports[0];
                    }
                }
                catch
                {
                    Debug.LogError("no supported language");
                    return null;
                }
                return _;
            }
        }



        public List<Internal.Grid> grids = new List<Internal.Grid>();
        //[HideInInspector]
        public List<LangSupport> langSupports = new List<LangSupport>();
        public List<LocalizeFont> localizeFonts = new List<LocalizeFont>();

        public LocalizeFont.FontAsset GetFontAsset(string fontName, Languages languages){
            var find = localizeFonts.Find(x=>x.fontName == fontName);
            if(find == null){
                Debug.LogError("Cant find the font name: " + fontName);
                return null;
            }

            var find2 = find.fontAssets.Find(x=>x.languagesSuport == languages);
            if(find2 == null){
                Debug.LogError("Cant find the font name: " + fontName + " with language: " + languages);
                return null;
            }

            return find2;
        }

        public LocalizeFont.FontAsset GetFontAsset(string fontName){
            var find = localizeFonts.Find(x=>x.fontName == fontName);
            if(find == null){
                Debug.LogError("Cant find the font name: " + fontName);
                return null;
            }

            var find2 = find.fontAssets.Find(x=>x.languagesSuport.ToString() == chosenLangCodeName);
            if(find2 == null){
                Debug.LogError("Cant find the font name: " + fontName + " with language: " + chosenLangCodeName);
                return null;
            }

            return find2;

        }

        public void AddNewLocalizeFont(string fontName){
            var find = localizeFonts.Find(x=>x.fontName == fontName);
            if(find == null){
                var newLocalizeFont = new LocalizeFont();
                newLocalizeFont.fontName = fontName;
                localizeFonts.Add(newLocalizeFont);
            }

            CheckLanguageInFontCheck();
        }

        public void CheckLanguageInFontCheck(){
            foreach(var i in localizeFonts){
                foreach(var j in langSupports){
                    var find = i.fontAssets.Find(x=>x.languagesSuport == j.languagesSuport);
                    if(find == null){
                        var newFontAsset = new LocalizeFont.FontAsset();
                        newFontAsset.languagesSuport = j.languagesSuport;
                        i.fontAssets.Add(newFontAsset);
                    }
                }
            }

            //make the order look like the langSupports
            foreach(var i in localizeFonts){
                var newFontAssets = new List<LocalizeFont.FontAsset>();
                foreach(var j in langSupports){
                    var find = i.fontAssets.Find(x=>x.languagesSuport == j.languagesSuport);
                    if(find != null){
                        newFontAssets.Add(find);
                    }
                }
                i.fontAssets = newFontAssets;
            }
        }

        public Internal.Grid GetGrid(string name)
        {
            return grids.Find(x => x.nameGrid == name);
        }

        public static Project singleton
        {
            get
            {
                Init();
                return _singleton;
            }
            set
            {

                try
                {
                    _singleton = value;
                }
                catch
                {
                    Init();
                    _singleton = value;
                }
            }
        }
        static void Init()
        {
            if (_singleton == null)
            {
                _singleton = Resources.Load<Project>("GridlyData");
                if (_singleton == null)
                {
                    CreateAsset();
                    _singleton = Resources.Load<Project>("GridlyData");
                }
                
            }


        }

        private Dictionary<string, string> languageDefinitions = new Dictionary<string, string>();
        private Dictionary<string, string> languageDefinitionsRevert = new Dictionary<string, string>();
        
        
        private void LoadLanguageDefinitions()
        {
            
            var textAsset = langDefineTextAsset;

            if (textAsset != null)
            {
                string[] lines = textAsset.text.Split('\n');
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                       
                        string languageCode = parts[0].Trim();
                        //Debug.Log("Language definition loaded: " + line + " with code: "+ languageCode);
                        string languageDefinition = parts[1].Trim();
                        languageDefinitions[languageCode] = languageDefinition;
                        languageDefinitionsRevert[languageDefinition] = languageCode;
                    }
                    
                }
            }
            else
            {
                Debug.LogError("LanguageDefine.txt not found in Resources folder.");
            }
        }

        public string GetLanguageCode(string languageDefinition)
        {
            if (languageDefinitionsRevert.Count == 0)
                LoadLanguageDefinitions();
            if (languageDefinitionsRevert.ContainsKey(languageDefinition))
            {
                return languageDefinitionsRevert[languageDefinition];
            }
            else
            {
                return null;
            }
        }

        public List<string> GetAllLanguageDefinition()
        {
            if (languageDefinitions.Count == 0)
                LoadLanguageDefinitions();
            List<string> result = new List<string>();
            foreach (var i in languageDefinitions)
                result.Add(i.Value);
            return result;
        }

        public string GetLanguageDefinition(string languageCode)
        {
            
            //if(isLoadLangDefinition == false){
            if(languageDefinitions.Count == 0)
                LoadLanguageDefinitions();
            

            if (languageDefinitions.ContainsKey(languageCode))
            {
                return languageDefinitions[languageCode];
            }
            else
            {
                return null;
            }
        }

        static void CreateAsset()
        {
#if UNITY_EDITOR
            var sample = Resources.Load<Project>("SampleGridlyData");

            var p = ScriptableObject.CreateInstance<Project>();
            p.chosenLangCodeName = sample.chosenLangCodeName;
            p.grids = sample.grids;
            p.langSupports = sample.langSupports;
            p.ProjectID = sample.ProjectID;
            
            string path = "Assets/Resources/GridlyData.asset";
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            UnityEditor.AssetDatabase.CreateAsset(p, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("GRIDLY DATA CREATED");
#endif
        }

        public int getIndexChosenLang
        {
            get
            {
                int index = 0;
                foreach (var i in langSupports)
                {
                    if (i.languagesSuport.ToString() == chosenLangCodeName)
                        return index;
                    index += 1;
                }
                return 0;
            }
        }
        public void SetChosenLanguageCode(string langCode)
        {
            //check if langCode is supported
            var find = langSupports.Find(x => x.languagesSuport.ToString() == langCode);
            if (find == null)
            {
                Debug.LogError("Language code not supported");
                return;
            }

            chosenLangCodeName = langCode;
            ITranslareText[] translareTexts = FindObjectsOfType<Translator>();
            foreach (var i in translareTexts)
                i.Refesh();
        }
        public void SetChosenLanguageCode(Languages languages)
        {
            SetChosenLanguageCode(languages.ToString());
        }

        public string GetLangSupportName(Languages languages){
            var p = langSupports.Find(x=>x.languagesSuport == languages);
            return p.name;
        }

    }
}