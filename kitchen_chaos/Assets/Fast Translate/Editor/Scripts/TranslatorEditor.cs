using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FastTranslate.Internal;
namespace FastTranslate.Editor
{
    
    [CustomEditor(typeof(Translator))]
    public class TranslatorEditor : UnityEditor.Editor
    {
        GridlyArrData popupData = new GridlyArrData();

        static string search = "";
        Column chosenColum;
        private void OnEnable()
        {
            search = "";
        }
        public override void OnInspectorGUI()
        {
            
            base.OnInspectorGUI();
            Translator translator = (Translator)target;

            if (!popupData.init)
            {
                Refesh();
                
            }

            //Font
            if(translator.autoApplyFont){
                GUILayout.BeginHorizontal();
                GUILayout.Label("Font");
                EditorGUI.BeginChangeCheck();
                try{
                var selectIndex = EditorGUILayout.Popup(popupData.indexFontKey, popupData.keyFontArr);
                popupData.indexFontKey = selectIndex;

                translator.fontKey = popupData.keyFontArr[selectIndex];
                }
                catch{
                    popupData.indexFontKey = 0;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(translator);
                    Refesh();
                }
                GUILayout.EndHorizontal();
            }

            if (Project.singleton.grids.Count == 0)
                return;

            //Db
            GUILayout.Space(10);


            //Grid
            if (popupData.indexGrid == -1)
                return;
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Grid");
            EditorGUI.BeginChangeCheck();
            try
            {
                translator.grid = popupData.gridArr[EditorGUILayout.Popup(popupData.indexGrid, popupData.gridArr)];
            }
            catch
            {
                popupData.indexGrid = 0;
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(translator);
                Refesh();
            }
            GUILayout.EndHorizontal();
            

            GUILayout.Space(10);


            //Key


            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Search", GUILayout.Width(50));
            search = GUILayout.TextField(search);
            if (EditorGUI.EndChangeCheck())
            {
                
                popupData.searchKey = search;
                Refesh();
            }
            GUILayout.EndHorizontal();


            if (popupData.keyArr == null)
                return;


            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Key");
            EditorGUI.BeginChangeCheck();
            try
            {
                translator.key = popupData.keyArr[EditorGUILayout.Popup(popupData.indexKey, popupData.keyArr)];
            }
            catch { GUILayout.Label("can't find key"); }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(translator);
                Refesh();

            }
            GUILayout.EndHorizontal();




            GUILayout.Space(10);

            if(GUILayout.Button("AutoSetup")){
                translator.AutoSetup();
            }
            
            
            // try
            // {
            //     Languages main = UserData.singleton.mainLangEditor;
            //     GUILayout.BeginHorizontal();
            //     GUILayout.Label(main.ToString() + ": ");
            //     if (chosenColum != null)
            //     {
            //         chosenColum.text = GUILayout.TextArea(chosenColum.text);
            //         if(GUILayout.Button(new GUIContent() {text = "Export" , tooltip = "Export text to Girdly" }, GUILayout.MinWidth(60)))
            //         {
            //             GridlyFunctionEditor.editor.UpdateRecordLang(popupData.chosenRecord, popupData.grid.choesenViewID, UserData.singleton.mainLangEditor);
            //         }
            //     }
    
            //     GUILayout.EndHorizontal();
            // }
            // catch { }


        }

        

        void Refesh()
        {
            Translator translator = (Translator)target;
            popupData.RefeshAll(translator.grid, translator.key, translator.fontKey);

            try
            {
                Languages main = FastTranslate.Internal.UserData.singleton.mainLangEditor;
                chosenColum = popupData.chosenRecord.columns.Find(x => x.columnID == main.ToString());
            }
            catch { }
            
        }

    }
}