using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using System.IO;
using System;
using NOOD.Editor;
using UnityEditor.TerrainTools;

namespace NOOD.ScriptableObjectTool
{
    public class ScriptableObjectToolEditor : EditorWindow
    {
        #region public
        #endregion

        #region Event
        private Action OnTextChange;
        #endregion

        #region private variable
        private Dictionary<FoldHeader, List<ScriptableObjectData>> _scriptableObjectDic = new Dictionary<FoldHeader, List<ScriptableObjectData>>();
        private Dictionary<FoldHeader, List<ScriptableObjectData>> _filterDic = new Dictionary<FoldHeader, List<ScriptableObjectData>>();
        private List<ScriptableObjectData> _allObjects = new List<ScriptableObjectData>();
        private EditorWindow _window;
        private List<ScriptableObjectData> _scriptableObjectDatas = new List<ScriptableObjectData>();
        private List<FoldHeader> _loadedFoldHeaderData = new List<FoldHeader>();
        private Vector2 _scrollViewPosition;
        private bool _isGettingData;
        private string _searchString = "";
        #endregion

        #region Show
        [MenuItem("Tools/ScriptableObjectEditor")]
        static void ShowEditorWindow()
        {
            ScriptableObjectToolEditor editor = EditorWindow.GetWindow<ScriptableObjectToolEditor>();
            editor.Show();
            editor.titleContent = new GUIContent("ScriptableObject Editor Window");
        }
        private void Awake()
        {
            _window = EditorWindow.GetWindow<ScriptableObjectToolEditor>();
        }
        private void OnEnable()
        {
            GetData();
            LoadState();
            OnTextChange += FilterData;
            _filterDic = _scriptableObjectDic;
            SOToolHelper.OnAssetDatabaseChange += GetData;
            SOToolHelper.OnAssetDatabaseChange += LoadState;
            SOToolHelper.OnAssetDatabaseChange += FilterData;
        }
        private void OnDisable()
        {
            OnTextChange -= FilterData;
            SOToolHelper.OnAssetDatabaseChange -= GetData;
            SOToolHelper.OnAssetDatabaseChange -= LoadState;
            SOToolHelper.OnAssetDatabaseChange -= FilterData;
        }
        private void OnGUI()
        {
            if (_isGettingData) return;

            // update draw zone
            DrawSearchZone();
            DrawFoldGroup();
            UpdateDragAndDrop();
            GetPlayerInput();
        }
        #endregion

        #region GetInput
        private void GetPlayerInput()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Space && SOToolHelper.IsLastActionPressed == false)
                {
                    SOToolHelper.IsLastActionPressed = true;
                    SOToolHelper.LastAction?.Invoke();
                }
            }
            if (Event.current.type == EventType.KeyUp)
            {
                if (Event.current.keyCode == KeyCode.Space && SOToolHelper.IsLastActionPressed == true)
                {
                    SOToolHelper.IsLastActionPressed = false;
                }
            }
        }
        #endregion

        #region data functions
        private void GetData()
        {
            _isGettingData = true;
            _allObjects.Clear();
            // Load all ScriptableObject
            var files = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });
            foreach (var file in files)
            {
                ScriptableObjectData data = new ScriptableObjectData(file);
                _scriptableObjectDatas.Add(data);
                _allObjects.Add(data);
            }

            // Split into types of ScriptableObject
            SplitData(_allObjects);
            _isGettingData = false;
        }
        private void SplitData(List<ScriptableObjectData> scriptableObjectDatas)
        {
            // Split into Type and list
            Dictionary<Type, List<ScriptableObjectData>> tempDic = new Dictionary<Type, List<ScriptableObjectData>>();
            int index = 0;
            while (index < scriptableObjectDatas.Count)
            {
                ScriptableObjectData data = scriptableObjectDatas[index];

                // Add to dictionary if can
                if (tempDic.TryGetValue(data.Type, out List<ScriptableObjectData> list))
                {
                    list.Add(data);
                }
                else
                {
                    var l = new List<ScriptableObjectData>
                    {
                        data
                    };
                    tempDic.Add(data.Type, l);
                }
                index++;
            }

            // convert to FoldHeader and list 
            _scriptableObjectDic = tempDic.ToDictionary(pair => new FoldHeader(pair.Key, false), pair => pair.Value);
            // Sort Dic
            _scriptableObjectDic = _scriptableObjectDic.OrderBy(pair => pair.Key.Name).ToDictionary(pair => pair.Key, pair => pair.Value);
            ApplyLoadedData();
        }
        private void FilterWithString()
        {
            Dictionary<Type, List<ScriptableObjectData>> tempDic = new Dictionary<Type, List<ScriptableObjectData>>();
            for (int i = 0; i < _scriptableObjectDic.Count; i++)
            {
                var pair = _scriptableObjectDic.ElementAt(i); // Get pair at index i
                if (pair.Value.Any(x => x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var item in pair.Value)
                    {
                        // Loop to each child of dic header
                        if (item.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                        {
                            if (tempDic.TryGetValue(item.Type, out List<ScriptableObjectData> list))
                            {
                                list.Add(item);
                            }
                            else
                            {
                                List<ScriptableObjectData> l = new()
                                {
                                    item
                                };
                                tempDic.Add(item.Type, l);
                            }
                        }
                    }
                }
                else
                {
                    // Add header only
                    tempDic.Add(pair.Key.Type, null);
                }
            }

            // Sort tempDic
            tempDic = tempDic.OrderBy(pair => pair.Key.Name).ToDictionary(pair => pair.Key, pair => pair.Value);
            // Convert tempDic to _filterDic
            _filterDic = new();
            _filterDic = tempDic.ToDictionary(pair => new FoldHeader(pair.Key, true), pair => pair.Value);
        }
        private void FilterData()
        {
            if (_searchString != "")
            {
                FilterWithString();
            }
            else
            {
                _filterDic = _scriptableObjectDic;
            }
        }
        #endregion

        #region Draw functions
        private void DrawSearchZone()
        {
            GUILayout.BeginHorizontal(BasicEditorStyles.Toolbar);
            {
                _searchString ??= "";
                string tempString = _searchString;
                tempString = GUILayout.TextField(tempString, BasicEditorStyles.ToolbarSearchTextField);
                if (_searchString != tempString)
                {
                    _searchString = tempString;
                    OnTextChange?.Invoke();
                }
                if (GUILayout.Button("", BasicEditorStyles.ToolbarSearchCancelButton))
                {
                    // Remove focus if cleared
                    _searchString = "";
                    GUI.FocusControl(null);
                }
            }
            GUILayout.EndHorizontal();
        }
        private void DrawFoldGroup()
        {
            _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition);
            {
                for (int i = 0; i < _filterDic.Count; i++)
                {
                    var pairCopy = _filterDic.ElementAt(i);
                    bool isFold;
                    GUILayout.BeginHorizontal();
                    {
                        isFold = EditorGUILayout.Foldout(pairCopy.Key.IsFold, pairCopy.Key.Name, true);

                        DrawAddButton(pairCopy.Key.Type, "Create new Scriptable with type in a new folder");
                        ChangeLoadedData(pairCopy.Key);
                    }
                    GUILayout.EndHorizontal();
                    if (pairCopy.Key.IsFold != isFold)
                    {
                        pairCopy.Key.IsFold = isFold;
                        SaveState();
                    }
                    if (pairCopy.Value == null || isFold == false)
                        continue;

                    foreach (var data in pairCopy.Value)
                    {
                        ScriptableOBjectPanel panel = new ScriptableOBjectPanel(data, _window);
                        GUILayout.Space(SOToolHelper.RowGap);
                        panel.DrawRow();
                    }
                }
            }
            GUILayout.EndScrollView();
        }
        private void DrawAddButton(Type type, string tooltip = "")
        {
            GUIContent addSign = EditorGUIUtility.IconContent("d_ol_plus");
            addSign.tooltip = tooltip;
            if (GUILayout.Button(addSign, BasicEditorStyles.AlignLabelMiddleRight))
            {
                CreateNewFileInNewFolder(type);
                SOToolHelper.LastAction = () =>
                {
                    CreateNewFileInNewFolder(type);
                };
                GUIUtility.ExitGUI();
            }
        }
        private void CreateNewFileInNewFolder(Type type)
        {
            string rawFolderPath = SOToolHelper.ChooseFolder();
            if (Directory.Exists(rawFolderPath) == false) return;

            string folderPath = rawFolderPath.Substring(rawFolderPath.IndexOf("Assets") + 0); // 0 is include A in Assets
            string fileName = EditorInputDialogue.Show("Create new asset", "Enter ScriptableObject name", "");
            if (folderPath != null && folderPath != "" && folderPath != " ")
            {
                if (fileName != null && fileName != "" && fileName != " ")
                {
                    SOToolHelper.CreateAssetAtPath(type, folderPath, fileName);
                }
            }
        }
        #endregion

        #region DragAndDrop
        public void UpdateDragAndDrop()
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
        }
        #endregion

        #region Save Load
        private string GetFilePath()
        {
            MonoScript ms = MonoScript.FromScriptableObject(this);
            string thisFilePath = AssetDatabase.GetAssetPath(ms);

            FileInfo fi = new FileInfo(thisFilePath);
            string thisFileFolder = fi.Directory.ToString();
            thisFileFolder.Replace('\\', '/');

            // Debug.Log( thisFileFolder );

            string filePath = Path.Combine(thisFileFolder, "data.txt");
            return filePath;
        }
        private void SaveState()
        {
            List<FoldHeader> foldHeaders = _scriptableObjectDic.Keys.ToList();
            string json = JsonConvert.SerializeObject(foldHeaders);

            string filePath = GetFilePath();
            if (!File.Exists(filePath))
            {
                File.CreateText(filePath);
            }
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(json);
            }
        }
        private void LoadState()
        {
            // Load data
            string filePath = GetFilePath();
            string json;
            using (StreamReader reader = new StreamReader(filePath))
            {
                json = reader.ReadToEnd();
            }
            try
            {
                _loadedFoldHeaderData = JsonConvert.DeserializeObject<List<FoldHeader>>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Some type can't be loaded: " + e.Message);
            }

            // Apply data
            ApplyLoadedData();
        }
        private void ApplyLoadedData()
        {
            foreach (var pair in _scriptableObjectDic)
            {
                if (_loadedFoldHeaderData.Any(x => x.Compare(pair.Key)))
                {
                    FoldHeader foldHeader = _loadedFoldHeaderData.First(x => x.Name == pair.Key.Name);
                    if (foldHeader != null)
                    {
                        pair.Key.IsFold = foldHeader.IsFold;
                    }
                }
            }
        }
        private void ChangeLoadedData(FoldHeader changedHeader)
        {
            if (_loadedFoldHeaderData.Any(x => x.Compare(changedHeader)))
            {
                _loadedFoldHeaderData.First(x => x.Compare(changedHeader)).IsFold = changedHeader.IsFold;
            }
        }
        #endregion
    }
}