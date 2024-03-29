using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
//using OfficeOpenXml;
using System.Diagnostics;
using FastTranslate.Internal;
using static UnityEngine.GraphicsBuffer;

namespace FastTranslate.Editor
{
    public class TermEditor : GridlyEditor
    {
        const bool USE_GRIDLY = false;
        Vector3 m_Scroll = new Vector3();
        public static TermEditor window;

        static int gridSelectIndex;
        bool isTranslating = false;
        public static bool isAutoRecord = false;
        static Internal.Grid grid
        {
            get
            {
                try
                {
                    return Project.singleton.grids[gridSelectIndex];
                }
                catch
                {
                    gridSelectIndex = 0;
                    Refesh();
                    return Project.singleton.grids[gridSelectIndex];
                }
            }
        }


        [MenuItem("Tools/Fast Translate/String Editor", false, 0)]
        private static void Init()
        {
            if (Project.singleton.grids.Count == 0)
            {
                "you need to setup data before using this feature".Print();
                return;
            }

            window = (TermEditor)GetWindow(typeof(TermEditor), false, "String Editor - " + GridlyInfo.ver);
            Vector2 vector2 = new Vector2(400, 400);
            window.minSize = vector2;
            Refesh();
            window.Show();
        }

        public static void RepaintThis()
        {
            if (window != null)
            {
                window.Repaint();

            }
        }


        static string gridSelect = "";
        static List<Record> records;
        static GridlyArrData popupData = new GridlyArrData();
        public static void Refesh()
        {

            OnEnableEditor();
            init = true;
            popupData.RefeshAll(gridSelect, null, null);
            RefeshList();
            selectRecordID = "";
            //Debug.Log("record count " + records.Count );
        }
        static void RefeshList()
        {
            if (popupData.indexGrid == -1)
                return;
            records = getRecord(grid);
            TermListLegth = records.Count;

        }

        static float mRowSize = 40;
        static int TermListLegth = 0;
        float scrollHeight;
        static string selectRecordID;
        float YPos;
        float ScrolSizeY => 280 + position.height - 415;
        static string search;
        static List<Record> getRecord(global::FastTranslate.Internal.Grid grid)
        {
            if (!string.IsNullOrEmpty(search))
                return grid.records.FindAll(x => x.recordID.Contains(search));
            return grid.records;
        }
        static bool init;

        int indexKo = 0;
        private string _prefix;
        Vector3 m_ScrollDetail;
        public void OnGUI()
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if (!init)
            {
                Refesh();
            }

            GUILayout.Space(20);

            #region select data



            EditorGUI.BeginChangeCheck();

            /*
            if (GUILayout.Button("Add jaJP"))
            {
                const string newLangCode = "jaJP";
                indexKo = 0;
                foreach(var i in Project.singleton.grids)
                    foreach(var j in i.records)
                    {
                        string input = j.columns.Find(x => x.columnID == "enUS").text;

                        var col = j.columns.Find(x => x.columnID == newLangCode);
                        if (col == null) {
                            col = new Column() { columnID = newLangCode };
                            j.columns.Add(col);
                        }

                        TranslateAsync(input, "ja", col);
                        indexKo++;
                        
                        await Task.Delay(100);

                    }
            }
            */
            gridSelectIndex = EditorGUILayout.Popup("Table", popupData.indexGrid, popupData.gridArr);
            gridSelect = popupData.gridArr[gridSelectIndex];
            if (EditorGUI.EndChangeCheck())
            {
                m_Scroll = Vector3.zero;
                FastTranslate.Internal.UserData.singleton.setDirty();
                Refesh();
                return;
            }


            #endregion
            GUILayout.Space(5);
            #region Search

            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Search", GUILayout.Width(60));
            search = GUILayout.TextField(search);
            if (EditorGUI.EndChangeCheck())
            {
                RefeshList();
                //selectRecordID = "";
                m_Scroll = Vector3.zero;
                //showCreateKey = false;
            }

            GUILayout.EndHorizontal();
            #endregion

            GUILayout.BeginHorizontal();
            GUILayout.Label("Prefix", GUILayout.Width(100));
            _prefix = GUILayout.TextField(_prefix);

            GUILayout.Label("Auto Record", GUILayout.Width(100));
            isAutoRecord = GUILayout.Toggle(isAutoRecord, "");
            GUILayout.EndHorizontal();

            // //draw tick box for auto record
            // GUILayout.BeginHorizontal();

            // GUILayout.EndHorizontal();


            GUILayout.Space(5);

            bool selected = !string.IsNullOrEmpty(selectRecordID);

            if (!selected)
            {
                m_Scroll = GUILayout.BeginScrollView(m_Scroll, TextStyle, GUILayout.MinHeight(ScrolSizeY), GUILayout.ExpandHeight(false));
                if (Event.current != null && Event.current.type == EventType.Layout)
                    scrollHeight = m_Scroll.y;
            }
            else
            {
                m_ScrollDetail = GUILayout.BeginScrollView(m_ScrollDetail, TextStyle, GUILayout.MinHeight(ScrolSizeY), GUILayout.ExpandHeight(false));
                if (Event.current != null && Event.current.type == EventType.Layout)
                    scrollHeight = m_ScrollDetail.y;
            }

            if (selected)
            {
                for (int i = 0; i < TermListLegth; i++)
                {
                    if (records[i].recordID == selectRecordID)
                    {
                        GUILayout.Space(mRowSize);
                        DrawRecord(i + 1, records[i]);
                    }
                }
            }
            else
            {
                #region draw record

                YPos = 0;

                int nDraw = 0;
                float nSkipStart = 0;
                bool spaceStart = false;

                for (int i = 0; i < TermListLegth; i++)
                {


                    if (YPos < scrollHeight - mRowSize && !spaceStart)
                    {
                        spaceStart = true;
                        nSkipStart = ((scrollHeight - mRowSize) / mRowSize);
                        YPos = scrollHeight - mRowSize;
                        i = (int)nSkipStart;
                        GUILayout.Space(nSkipStart * mRowSize);
                    }

                    GUILayout.Space(mRowSize);
                    try
                    {

                        DrawRecord(i + 1, records[i]);
                    }
                    catch
                    {

                        break;
                    }
                    YPos += mRowSize;


                    nDraw++;
                    if (YPos > scrollHeight + ScrolSizeY)
                    {
                        break;
                    }

                    if (i == TermListLegth - 1)
                    {

                    }
                }


                GUILayout.Space((TermListLegth + 1 - (nDraw + nSkipStart)) * (mRowSize));
                DrawAddRecord();

                #endregion
            }
            GUILayout.EndScrollView();

            #region Button
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            if (isTranslating)
            {
                GUILayout.Label("Is traslating...");
            }
            else
            {
                if (GUILayout.Button("Auto Translate All Missing Language"))
                {
                    //transalte all missing language
                    TranslateAllMissingLanguage();
                }
            }
            //if (GUILayout.Button(new GUIContent() { text = "Import grid", tooltip = "Import this grid data from from Gridly" }))
            //{
            //    if (EditorUtility.DisplayDialog("Confirm Import grid", "Are you sure you want to import grid from Gridly?. It will overwrite the old data including translations.", "Yes", "Cancel"))
            //    {
            //        popupData.grid.records.Clear();
            //        Refesh();
            //        RepaintThis();

            //        GridlyFunctionEditor.editor.doneOneProcess += Refesh;
            //        GridlyFunctionEditor.editor.doneOneProcess += RepaintThis;

            //        GridlyFunctionEditor.editor.RefeshDowloadTotal();
            //        GridlyFunctionEditor.editor.SetupRecords(popupData.grid, 0);
            //    }
            //}

            //if (GUILayout.Button(new GUIContent() { text = "Import all", tooltip = "Import all data from Gridly" }))
            //{
            //    if (EditorUtility.DisplayDialog("Confirm Export", "Are you sure you want to import all data from Gridly?. It will overwrite the old data including translations.", "Yes", "Cancel"))
            //    {
            //        GridlyFunctionEditor.editor.doneOneProcess += Refesh;
            //        GridlyFunctionEditor.editor.doneOneProcess += RepaintThis;
            //        GridlyFunctionEditor.editor.SetupDatabases(); 
            //    }
            //}


            //if (GUILayout.Button(new GUIContent() { text = "Export source languages", tooltip = "Export the source language of the selected grid" }))
            //{
            //    GridlyFunctionEditor.editor.AddUpdateRecordAll(popupData.grid.records, popupData.grid.choesenViewID, false, true);
            //}

            // if (GUILayout.Button("Save"))
            // {
            //     Project.singleton.setDirty();
            // }
            /*
            if (GUILayout.Button(new GUIContent() { text = "Export All", tooltip = "Export all grid to Gridly" }))
            {
                foreach(var i in Project.singleton.databases)
                {
                    foreach(var j in i.grids)
                    {
                        string viewID = j.choesenViewID;
                        GridlyFunctionEditor.editor.AddRecord(j.records, viewID);
                    }
                }
            }
            */
            GUILayout.EndHorizontal();
            #endregion
            //stopwatch.Stop();
            //UnityEngine.Debug.Log(stopwatch.Elapsed);
            //HandleUtility.Repaint();
            //UnityEngine.Debug.Log("update");
            //RepaintThis();

        }

        bool showCreateKey;
        string nameNewKey;
        void DrawAddRecord()
        {

            GUI.backgroundColor = new Color(1, 1, 1, 0.4f);
            GUILayout.BeginHorizontal();
            GUIStyle redPlusButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            redPlusButtonStyle.normal.textColor = Color.green; // Set the text color to red

            if (GUILayout.Button("+", redPlusButtonStyle, GUILayout.Width(30)))
            {
                showCreateKey = !showCreateKey;
            }

            GUI.color = Color.white;
            if (showCreateKey) ShowCreateKey();
            GUILayout.EndHorizontal();

            void ShowCreateKey()
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);

                nameNewKey = EditorGUILayout.TextField(nameNewKey, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Create Record", redPlusButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    var newRecordId = _prefix + nameNewKey;
                    if (CheckExitKey(newRecordId))
                    {
                        EditorUtility.DisplayDialog("Error", "This key is already exist", "OK");
                        return;
                    }
                    else
                    {

                        Record record = new Record();
                        record.recordID = newRecordId;
                        // if(USE_GRIDLY)
                        //     GridlyFunctionEditor.editor.AddRecord(record, grid.choesenViewID);

                        foreach (var i in Project.singleton.langSupports)
                        {
                            record.columns.Add(new Column(i.languagesSuport.ToString(), ""));
                        }

                        if (isAutoRecord)
                        {
                            var autoText = newRecordId;
                            //Uppercase fist character
                            autoText = autoText.Substring(0, 1).ToUpper() + autoText.Substring(1);
                            //replace _ to space
                            autoText = autoText.Replace("_", " ");
                            //assign auto text to main lang
                            record.columns.Find(x => x.columnID == FastTranslate.Internal.UserData.singleton.mainLangEditor.ToString()).text = autoText;

                        }

                        grid.records.Add(record);
                        Project.singleton.setDirty();

                        showCreateKey = false;
                        //selectRecordID = nameNewKey;
                        nameNewKey = "";
                        RefeshList();
                    }
                }
            }

        }

        void DrawRecord(int i, Record record)
        {
            bool selected = selectRecordID == record.recordID;
            if (selected)
            {
                Color color = Color.Lerp(Color.cyan, Color.white, 0.3f);
                color.a = 0.4f;
                GUI.backgroundColor = color;
                SelectStyle.normal.textColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = darkLightColor;
                SelectStyle.normal.textColor = new Color(1, 1, 1, 0.2f);
            }

            const float copyWidthSize = 70;
            Rect rect = new Rect(2, YPos, position.width - copyWidthSize, mRowSize);

            //check if the record have the language that empty
            bool haveEmpty = false;
            foreach (var j in Project.singleton.langSupports)
            {
                if (string.IsNullOrEmpty(record.columns.Find(x => x.columnID == j.languagesSuport.ToString()).text))
                {
                    haveEmpty = true;
                    break;
                }
            }


            var textForGuiContent = record.recordID;
            //color yellow if empty else white
            if (haveEmpty)
            {
                SelectStyle.normal.textColor = Color.yellow;
                textForGuiContent = "*" + textForGuiContent;
                GUI.tooltip = "This record have missing language";
            }
            else
            {
                SelectStyle.normal.textColor = Color.white;
                GUI.tooltip = "";
            }




            if (GUI.Button(rect, new GUIContent(i + ". " + textForGuiContent + " |  " + GetShowText(ref record), GUI.tooltip), SelectStyle))
            {

                if (selectRecordID == record.recordID)
                    selectRecordID = "";
                else selectRecordID = record.recordID;

                //-turn off rename feature
                isRename = false;
                theNameToRename = "";
                ///

                GUI.FocusControl(null);
            }

            //draw copy
            rect.width = copyWidthSize;
            rect.x = 2 + position.width - copyWidthSize;
            if (GUI.Button(rect, new GUIContent("Copy")))
            {
                GUIUtility.systemCopyBuffer = record.recordID;
            }

            if (selected)
                DrawDetailRecord(record);

        }

        string GetShowText(ref Record record)
        {

            foreach (var i in record.columns)
            {
                if (i.columnID == FastTranslate.Internal.UserData.singleton.mainLangEditor.ToString())
                    return LimitText(ref i.text);
            }
            return string.Empty;
        }


        async void TranslateAsync(string input, string source, string code, Column col)
        {
            if (col == null)
            {
                UnityEngine.Debug.LogError("coll is null");
                return;
            }

            await Task.Delay(800 * indexKo);

            var client = new HttpClient();

            var apiKey = global::FastTranslate.Internal.UserData.singleton.keyAPI;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://microsoft-translator-text.p.rapidapi.com/translate?to%5B0%5D=" + code + "&api-version=3.0&profanityAction=NoAction&textType=plain"),
                Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", "microsoft-translator-text.p.rapidapi.com" },
                },
                Content = new StringContent("[{\"Text\": \"" + input + "\"}]")
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var n = JSON.Parse(body);

                //string output = n[0][1][0][0];
                string output = n[0][1][0][0];
                UnityEngine.Debug.Log(output);
                if (string.IsNullOrEmpty(output))
                {

                    UnityEngine.Debug.Log("output empty: " + body);
                }

                col.text = output;
                //UnityEngine.Debug.Log(col.columnID);
                Project.singleton.setDirty();

            }
        }

        string LimitText(ref string input)
        {
            string final = "";
            const int length = 50;
            for (int i = 0; i < input.Length; i++)
            {
                if (i >= length)
                {
                    final += "...";
                    break;
                }
                final += input[i];
            }
            return final;
        }

        bool isRename;
        string theNameToRename = "";
        private string _fromText;
        private string _toText;
        Dictionary<string, UnityEngine.Object> tempObjects = new Dictionary<string, UnityEngine.Object>();

        public static string GetResourcePath(UnityEngine.Object resource)
        {
            // Use AssetDatabase to find the path of the object in the Resources folder
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(resource);
            var splits = assetPath.Split('/');
            var startFile = 0;
            for(int i =0; i< splits.Length; i++)
            {
                if (splits[i] == "Resources")
                {
                    startFile = i;
                    break;
                }

            }
            var finalPath = "";

            for(int i = startFile+1; i < splits.Length; i++)
            {

                var stringPath = "";

                if (i == splits.Length - 1)
                {
                    stringPath = splits[i].Split('.')[0];
                }
                else
                {
                    stringPath = splits[i];
                }

                if (i > startFile + 1)
                    finalPath += "/" + stringPath;
                else finalPath += stringPath;

            }


            return finalPath;
            
            
        }

        void DrawDetailRecord(Record record)
        {
            foreach (var i in Project.singleton.langSupports)
            {

                GUILayout.BeginHorizontal(TextStyle);
                string name = i.name;

                GUIContent contenLabel;
                if (i.languagesSuport == FastTranslate.Internal.UserData.singleton.mainLangEditor)
                {
                    name = "*" + name;
                    contenLabel = new GUIContent() { text = name, tooltip = "This is the source language" };
                }
                else contenLabel = new GUIContent() { text = name };

                var labelStyle = new GUIStyle(EditorStyles.boldLabel);

                //check if this record have the missing language
                if (string.IsNullOrEmpty(record.columns.Find(x => x.columnID == i.languagesSuport.ToString()).text))
                    labelStyle.normal.textColor = Color.yellow;
                else
                {
                    labelStyle.normal.textColor = Color.green;
                }

                GUILayout.Label(contenLabel, labelStyle, GUILayout.Width(70 + (position.width - 400) * 0.1f));

                Column col = record.columns.Find(x => x.columnID == i.languagesSuport.ToString());
                string text = "";
                if (col != null)
                    text = col.text;

                if (record.objectType)
                {
                    var langSupCode = i.languagesSuport.ToString();
                    if (!tempObjects.ContainsKey(langSupCode)) { 
                        tempObjects.Add(langSupCode, null);
                    }
                    var loadOb = Resources.Load(col.text);
                    if(loadOb == null)
                    {
                        UnityEngine.Debug.Log("cant find ob at path: " + col.text);
                    }
                    tempObjects[langSupCode] = loadOb;
                    var obj = tempObjects[langSupCode];
                    
                    EditorGUI.BeginChangeCheck();
                    obj = EditorGUILayout.ObjectField(obj,typeof(UnityEngine.Object),false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if(col != null)
                        {
                            col.text = GetResourcePath(obj);
                            Project.singleton.setDirty();
                        }
                    }

                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    text = GUILayout.TextField(text);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (col != null)
                        {
                            col.text = text;
                            Project.singleton.setDirty();
                        }
                        else
                        {
                            //Debug.Log("you cannot edit this field because there is no column " +i.languagesSuport + " on Gridly. To fix this please login to Gridly and add column with columnID as \""+i.languagesSuport+"\"");
                        }
                    }
                }


                // if(GUILayout.Button(new GUIContent() { text = "Export", tooltip = "Export this field to Gridly" }, GUILayout.Width(60) ))
                // {
                //     GridlyFunctionEditor.editor.UpdateRecordLang(record, grid.choesenViewID, i.languagesSuport);
                // }
                if (!record.objectType)
                {
                    if (GUILayout.Button("Translate", GUILayout.Width(70)))
                    {
                        var sourceLang = FastTranslate.Internal.UserData.singleton.mainLangEditor.ToString();
                        var toCode = i.languagesSuport.ToString();
                        TranslateAsync(record.getSourceLangText, sourceLang, toCode.Substring(0, 2), col);
                    }
                }


                GUILayout.EndHorizontal();
            }

            GUILayout.Space(2);

            if (isRename)
            {
                ShowRename();
                void ShowRename()
                {
                    GUILayout.BeginHorizontal(TextStyle);
                    GUILayout.Label(record.recordID);



                    theNameToRename = GUILayout.TextField(theNameToRename, GUILayout.Width(position.width * 0.7f));
                    if (GUILayout.Button("Save", GUILayout.Width(50)))
                    {
                        if (CheckExitKey(theNameToRename))
                        {
                            EditorUtility.DisplayDialog("Error", "This key is already exist", "OK");
                            return;
                        }
                        else
                        {
                            selectRecordID = theNameToRename;
                            //if(USE_GRIDLY)
                            //    GridlyFunctionEditor.editor.DeleteRecord(record.recordID, grid.choesenViewID, null); //delete old record before rename
                            record.recordID = theNameToRename;
                            //if(USE_GRIDLY)
                            //    GridlyFunctionEditor.editor.AddRecord(record, grid.choesenViewID);
                            Project.singleton.setDirty();
                        }
                    }


                    GUILayout.EndHorizontal();
                }
            }

            #region delete, rename, TranslateAll
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Delete"))
            {
                EditorUtility.SetDirty(FastTranslate.Internal.UserData.singleton);
                //if (USE_GRIDLY)
                //    GridlyFunctionEditor.editor.DeleteRecord(record.recordID, grid.choesenViewID, null);
                grid.records.Remove(record);
                Refesh();
                Project.singleton.setDirty();
                //if(EditorUtility.DisplayDialog("Confirm delete", "Are you sure you want to delete this record", "Yes", "Cancel"))
                //{

                //}

            }



            if (GUILayout.Button("Rename"))
            {
                isRename = !isRename;
            }
            // if (GUILayout.Button("Dublicate"))
            // {
            //     grid.records.Add(new Record(record));
            //     Refesh();
            //     Project.singleton.setDirty();
            // }
            if (!record.objectType)
            {
                if (GUILayout.Button("Translate All"))
                {
                    TranslateAll(record);
                }
            }

            if (record.objectType)
            {
                if (GUILayout.Button("String Type"))
                {
                    record.objectType = false;
                }
            }
            else
            {
                if (GUILayout.Button("Object Type"))
                {
                    record.objectType = true;
                }
            }
            GUILayout.EndHorizontal();
            #endregion

            #region Replace Text
            GUILayout.BeginHorizontal();
            GUILayout.Label("Replace Text", GUILayout.Width(120));
            GUILayout.Label("From", GUILayout.Width(50));
            _fromText = GUILayout.TextField(_fromText);
            GUILayout.Label("To", GUILayout.Width(50));
            _toText = GUILayout.TextField(_toText);
            if (GUILayout.Button("Replace All"))
            {
                foreach (var i in record.columns)
                {
                    i.text = i.text.Replace(_fromText, _toText);
                }

                RepaintThis();
                Project.singleton.setDirty();
            }
            #endregion

            GUILayout.EndHorizontal();

            YPos = GUILayoutUtility.GetLastRect().y;
            GUILayout.Space(mRowSize);
        }

        bool CheckExitKey(string key)
        {
            foreach (var i in grid.records)
            {
                if (i.recordID == key)
                    return true;
            }
            return false;
        }

        async void TranslateAllMissingLanguage()
        {
            UnityEngine.Debug.Log("Start Translating Missing Records: " + records.Count);

            foreach (var record in records)
            {
                if (string.IsNullOrEmpty(record.getSourceLangText))
                {
                    UnityEngine.Debug.Log("source lang is empty: " + record.recordID);
                    continue;
                }
                if (!record.objectType)
                {
                    foreach (var col in record.columns)
                    {
                        if (string.IsNullOrEmpty(col.text))
                        {
                            isTranslating = true;
                            string sourceLang = FastTranslate.Internal.UserData.singleton.mainLangEditor.ToString();
                            var toCode = col.columnID;
                            TranslateAsync(record.getSourceLangText, sourceLang, toCode.Substring(0, 2), col);
                            await Task.Delay(100);

                        }
                    }
                }


            }


            UnityEngine.Debug.Log("Done!");
            isTranslating = false;
        }
        async void TranslateAll(Record record)
        {
            string sourceLang = FastTranslate.Internal.UserData.singleton.mainLangEditor.ToString();
            string input = record.columns.Find(x => x.columnID == sourceLang).text;

            foreach (var col in record.columns)
            {
                if (col.columnID == sourceLang)
                    continue;

                TranslateAsync(input, sourceLang, col.columnID.Substring(0, 2), col);
                await Task.Delay(100);
            }
        }
    }


}
