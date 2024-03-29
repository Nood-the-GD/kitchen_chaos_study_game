using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UIElements;
using FastTranslate.Internal;
namespace FastTranslate.Editor
{
    public class GridlyInfo
    {
        public static string ver = "V1";
        public const int LanguagesLeng = 146;
    }
    public partial class GridlyEditor : EditorWindow
    {
        public static Texture2D m_logo;
        static GUIStyle Style_ToolBarButton_Big;
        public static GUIStyle TextStyle;
        //text asset field
        public static TextAsset m_textAsset;

        public static void OnEnableEditor()
        {

            try
            {
                //m_logo = (Texture2D)Resources.Load("Gridly_Icon", typeof(Texture2D));
                Style_ToolBarButton_Big = new GUIStyle(EditorStyles.toolbarButton);
                Style_ToolBarButton_Big.fixedHeight = Style_ToolBarButton_Big.fixedHeight * 1.5f;

                TextStyle = new GUIStyle(EditorStyles.toolbarTextField);
                TextStyle.fixedHeight = 0;

                SelectStyle = new GUIStyle(EditorStyles.toolbarTextField);
                SelectStyle.fixedHeight = 0;
                SelectStyle.normal.textColor = new Color(1, 1, 1, 0.3f);
            }
            catch
            {

            }
           

        }
        public Color darkLightColor = new Color(0, 0, 0, 0.15f);
        public Color darkColor = new Color(0, 0, 0, 0.25f);
        public static GUIStyle SelectStyle;

        public enum eViewMode
        {
            Setting,
            Languages,
            Font,
        }
        public static void OnGUI_ToggleEnumBig<Enum>(string text, ref Enum currentMode, Enum newMode, Texture texture, string tooltip)
        {
            OnGUI_ToggleEnum<Enum>(text, ref currentMode, newMode, texture, tooltip, Style_ToolBarButton_Big);
        }
        public static void OnGUI_ToggleEnum<Enum>(string text, ref Enum currentMode, Enum newMode, Texture texture, string tooltip, GUIStyle style)
        {
            GUI.changed = false;
            if (GUILayout.Toggle(currentMode.Equals(newMode), new GUIContent(text, texture, tooltip), style, GUILayout.ExpandWidth(true)))
            {
                currentMode = newMode;
                //if (GUI.changed)
                //  ClearErrors();
            }
        }


    }
    public partial class GridlySetting : GridlyEditor
    {
        #region Header
        public static eViewMode mCurrentViewMode = eViewMode.Setting;
        Vector3 m_Scroll = new Vector3();
        int selectLang;
        string search = "";
        [MenuItem("Tools/Fast Translate/Setup Setting", false, 0)]
        private static void InitWindow()
        {
            InitData();
            GridlySetting window = (GridlySetting)GetWindow(typeof(GridlySetting), true, "FastTranslate Setting Window - " + GridlyInfo.ver);
            Vector2 vector2 = new Vector2(400, 450);
            window.minSize = vector2;
            window.maxSize = vector2;
            window.Show();
            
        }

        //[MenuItem("Tools/Gridly/Export/Export All", false, 100)]
        private static void ExportAll()
        {
            if (EditorUtility.DisplayDialog("Confirm Export", "Are you sure you want to export everything to Gridly?. It will overwrite the old data including translations.", "Yes", "Cancel"))
            {
                foreach (var i in Project.singleton.grids)
                {
                    string viewID = i.choesenViewID;
                    GridlyFunctionEditor.editor.AddUpdateRecordAll(i.records, viewID, true, false);
                }
            }
        }

        static void InitData()
        {
            init = true;
            OnEnableEditor();
        }

        [MenuItem("Tools/Fast Translate/Join Discord", false, 10)]        
        private static void JoinDiscord(){
            Application.OpenURL("https://discord.gg/VdbYqPn3PP");
        }

        #endregion

        static bool init;
        private void OnGUI()
        {
            if (!init)
                InitData();
            GUI.changed = false;
            //GUILayout.Label(m_logo);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            OnGUI_ToggleEnumBig("Table Setup", ref mCurrentViewMode, eViewMode.Setting, null, null);
            OnGUI_ToggleEnumBig("Languages", ref mCurrentViewMode, eViewMode.Languages, null, null);
            OnGUI_ToggleEnumBig("Fonts", ref mCurrentViewMode, eViewMode.Font, null, null);            
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (mCurrentViewMode == eViewMode.Setting)
                SettingWin();
            else if (mCurrentViewMode == eViewMode.Languages)
            {
            
                LanguageWin();
                
            }
            else if (mCurrentViewMode == eViewMode.Font)
            {
                FontWin();
            }


            
        }



        void SettingWin()
        {
            GUILayout.Label("Enter your Translate API key here:", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            string _APIkey = EditorGUILayout.TextField(FastTranslate.Internal.UserData.singleton.keyAPI);
            if (EditorGUI.EndChangeCheck())
            {
                FastTranslate.Internal.UserData.singleton.keyAPI = _APIkey;
                FastTranslate.Internal.UserData.singleton.setDirty();
                
            }




            #region viewID
            GUILayout.Space(10);
            GUILayout.Label("Add Your Table Here:", EditorStyles.boldLabel);
            m_Scroll = GUILayout.BeginScrollView(m_Scroll, TextStyle, GUILayout.Height(300));

            Internal.Grid removeGrid = null;
            foreach(var i in Project.singleton.grids)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(50));
                i.nameGrid = GUILayout.TextField(i.nameGrid);


                //GUILayout.Label("Option", GUILayout.Width(50));


                //i.choesenViewID = GUILayout.TextField(i.choesenViewID, GUILayout.ExpandWidth(false), GUILayout.Width(140));
                //get the number of item in the table
                int count = 0;
                if (i.records != null)
                    count = i.records.Count;

                GUILayout.Label("Items: "+ count, GUILayout.Width(50));

                //export csv button
                if (GUILayout.Button(new GUIContent() { text = "Export CSV", tooltip = "Export all data to csv file" }, GUILayout.Width(80)))
                {
                    FTEditorHelper.ExportCSV(i);
                    //GridlyFunctionEditor.editor.ExportCSV(langSupport.languagesSuport);
                }

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    removeGrid = i;
                }
                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                    Project.singleton.setDirty();
            }

            if (removeGrid != null)
            {
                
                var temp = removeGrid;
                removeGrid = null;
                if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to delete this table", "Yes", "Cancel"))
                {
                    Project.singleton.grids.Remove(temp);
                }
                
            }


            #region add new grid "+"

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                Project.singleton.grids.Add(new Internal.Grid());
            }


            GUILayout.EndScrollView();
            #endregion


            #endregion

            //text asset field
            //horizone
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            m_textAsset = (TextAsset)EditorGUILayout.ObjectField("CSV Import", m_textAsset, typeof(TextAsset), false);
            if (GUILayout.Button("Import", GUILayout.Width(100)))
            {
                if (m_textAsset != null)
                {
                    FTEditorHelper.ImportCSV(m_textAsset);
                }
            }
            GUILayout.EndHorizontal();

            //GUILayout.Space(10);
            //EditorGUI.BeginChangeCheck();
            //UserData.singleton.showServerMess = GUILayout.Toggle(UserData.singleton.showServerMess, "Print server messages to the console");
            //if (EditorGUI.EndChangeCheck())
              //  UserData.singleton.setDirty();

            //GUILayout.Space(10);
            //if (GridlyFunctionEditor.isDowloading)
            //{
            //    GUILayout.BeginHorizontal();
            //    GUILayout.Label("Dowloading...");
            //    if(GUILayout.Button("Cancel", GUILayout.Width(50)))
            //    {
            //        GridlyFunctionEditor.editor.RefeshDowloadTotal();
            //        EditorApplication.update = null;
            //    }
            //    GUILayout.EndHorizontal();

            //}

            //setup
            //GUILayout.Space(10);
            //EditorGUILayout.HelpBox("Download and setup all data", MessageType.Info);
            //if (GUILayout.Button(new GUIContent() {text = "Import All", tooltip = "Dowload all data from Gridly" }))
            //{

            //    GridlyFunctionEditor.editor.doneOneProcess += TermEditor.Refesh;
            //    GridlyFunctionEditor.editor.doneOneProcess += TermEditor.RepaintThis;

            //    GridlyFunctionEditor.editor.SetupDatabases();
            //}

            //GUILayout.Space(10);
            //if (GUILayout.Button("Clear local data"))
            //{
            //    if (EditorUtility.DisplayDialog("Confirm delete", "Are you sure you want to delete the local data", "Yes", "Cancel"))
            //    {
            //        try
            //        {
            //            TermEditor.window.Close();
            //        }
            //        catch { }
            //        Project.singleton.grids = new List<Internal.Grid>();
            //        Project.singleton.setDirty();
            //    }
            //}

        }
        void LanguageWin()
        {
            int deleteIndex = -1;

            #region list Lang
            m_Scroll = GUILayout.BeginScrollView(m_Scroll, TextStyle, GUILayout.MinHeight(300), GUILayout.ExpandHeight(false));
            SerializedObject serializedObject = new SerializedObject(Project.singleton);
            SerializedProperty property = serializedObject.FindProperty("langSupports");

            for (int i = 0; i < property.arraySize; i++)
            {
                GUILayout.Space(2);
                LangSupport langSupport = Project.singleton.langSupports[i];
                GUILayout.BeginHorizontal();

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
               
                if (langSupport.languagesSuport == FastTranslate.Internal.UserData.singleton.mainLangEditor)
                {
                    buttonStyle.normal.textColor = Color.cyan;
                }
                else
                {
                    buttonStyle.normal.textColor = Color.white;
                }






                if (GUILayout.Button("X", "toolbarbutton"))
                {
                    deleteIndex = i;
                }

                EditorGUI.BeginChangeCheck();
                string name = langSupport.name;
                name = EditorGUILayout.TextField(langSupport.name);

                if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(name))
                {
                    langSupport.name = name;
                    Project.singleton.setDirty();
                }
                
               
                if (GUILayout.Button(new GUIContent() { text = "Main", tooltip = "Set this language as main language in editor" },buttonStyle))
                {
                    TermEditor.Refesh();
                    TermEditor.RepaintThis();
                    if (TermEditor.window != null)
                        TermEditor.window.OnGUI();
                    FastTranslate.Internal.UserData.singleton.mainLangEditor = langSupport.languagesSuport;
                    FastTranslate.Internal.UserData.singleton.setDirty();
                }

                GUILayout.Label("Code: "+  langSupport.languagesSuport.ToString(), GUILayout.Width(100));
                
                GUILayout.EndHorizontal();


                

                // //font
                // SerializedProperty font = property.GetArrayElementAtIndex(i).FindPropertyRelative("font");
                // EditorGUI.BeginChangeCheck();
                // GUIContent customLabel = new GUIContent("Default font");
                // EditorGUILayout.PropertyField(font, customLabel,true);
                // if (EditorGUI.EndChangeCheck())
                // {
                //     serializedObject.ApplyModifiedProperties();
                // }

                // SerializedProperty fontTM = property.GetArrayElementAtIndex(i).FindPropertyRelative("tmFont");
                // EditorGUI.BeginChangeCheck();
                // customLabel = new GUIContent("TMP font");
                // EditorGUILayout.PropertyField(fontTM,customLabel, true);
                // if (EditorGUI.EndChangeCheck())
                // {
                //     serializedObject.ApplyModifiedProperties();
                // }




                GUILayout.Space(15);
            }
            GUILayout.EndScrollView();

            if (deleteIndex != -1)
            {
                if (EditorUtility.DisplayDialog("Confirm delete", "Are you sure you want to delete the selected language", "Yes", "Cancel"))
                {
                    Languages langDelete = Project.singleton.langSupports[deleteIndex].languagesSuport;
                    foreach (var grid in Project.singleton.grids)
                    {
                        foreach (var i in grid.records)
                        {
                            i.columns.RemoveAll(x => x.columnID == langDelete.ToString());
                        }
                    }
                    Project.singleton.langSupports.RemoveAt(deleteIndex);
                    Project.singleton.setDirty();
                }
            }
            #endregion


            #region AddLang

            GUILayout.Space(3);
            //horizontal
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(50));
            search = GUILayout.TextField(search);
            //end
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            List<string> final = Project.singleton.GetAllLanguageDefinition().FindAll(x => x.Contains(search));
            selectLang = EditorGUILayout.Popup(selectLang, final.ToArray(), EditorStyles.toolbarDropDown);
            if (GUILayout.Button("Add"))
            {
                //Languages language = (Languages)System.Enum.Parse(typeof(Languages), final[selectLang]);
                var code =  Project.singleton.GetLanguageCode(final[selectLang]);
                Languages language =  (Languages)System.Enum.Parse(typeof(Languages), code);
                foreach (var i in Project.singleton.langSupports)
                    if (i.languagesSuport == language)
                        return;
                

                var name = Project.singleton.GetLanguageDefinition(language.ToString());
                if(name == null){
                    name = language.ToString();
                    Debug.Log("cant found language name: "+ language.ToString() + " in GridlyInfo.cs");
                    
                }
                
                Project.singleton.langSupports.Add(new LangSupport() { name =name, languagesSuport = language });
                //check if the language is already exist in all record
                foreach (var grid in Project.singleton.grids)
                {
                    foreach (var i in grid.records)
                    {
                        if (i.columns.Find(x => x.columnID == language.ToString()) == null)
                        {
                            i.columns.Add(new Internal.Column() { columnID = language.ToString()});
                        }
                    }
                }

                Project.singleton.setDirty();
            }
            GUILayout.EndHorizontal();
            #endregion
        }

        void FontWin(){
            int deleteIndex = -1;
            m_Scroll = GUILayout.BeginScrollView(m_Scroll, TextStyle, GUILayout.MinHeight(300), GUILayout.MaxWidth(450),GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
            SerializedObject serializedObject = new SerializedObject(Project.singleton);
            SerializedProperty property = serializedObject.FindProperty("localizeFonts");
            for (int i = 0; i < property.arraySize; i++)
            {
                GUILayout.Space(2);
                var localizeFont = Project.singleton.localizeFonts[i];

                GUILayout.BeginHorizontal(GUILayout.Width(300),GUILayout.ExpandWidth(false));
                GUILayout.Label("Font name: ", GUILayout.Width(100));
                localizeFont.fontName =  GUILayout.TextField(localizeFont.fontName);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    deleteIndex = i;
                }
                GUILayout.EndHorizontal();

                SerializedProperty fontAssets = property.GetArrayElementAtIndex(i).FindPropertyRelative("fontAssets");
                for(int j = 0; j < fontAssets.arraySize; j++){
                    GUILayout.BeginHorizontal(GUILayout.Width(300),GUILayout.ExpandWidth(false));
                    //GUILayout.Label("Language:
                    SerializedProperty font = fontAssets.GetArrayElementAtIndex(j).FindPropertyRelative("font");
                    SerializedProperty tmFont = fontAssets.GetArrayElementAtIndex(j).FindPropertyRelative("tmFont");
                    var lang = fontAssets.GetArrayElementAtIndex(j).FindPropertyRelative("languagesSuport").enumValueIndex;
                    var langSupport = Project.singleton.GetLanguageDefinition(((Languages)lang).ToString());
                    GUILayout.Label(langSupport.ToString(), GUILayout.Width(100));

                    EditorGUI.BeginChangeCheck();
                    //GUIContent customLabel = new GUIContent("Font");
                    EditorGUILayout.PropertyField(font,false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }

                    EditorGUI.BeginChangeCheck();
                    //customLabel = new GUIContent("TMP font");
                    EditorGUILayout.PropertyField(tmFont, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);

            }

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                Project.singleton.AddNewLocalizeFont("");
            }
            GUILayout.EndScrollView();

            if (deleteIndex != -1)
            {
                if (EditorUtility.DisplayDialog("Confirm delete", "Are you sure you want to delete the selected font", "Yes", "Cancel"))
                {
                    Project.singleton.localizeFonts.RemoveAt(deleteIndex);
                    Project.singleton.setDirty();
                }
            }
        }
    }

}