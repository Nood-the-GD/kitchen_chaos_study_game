using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace NOOD.ScriptableObjectTool
{
    public class SOToolEditor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            SOToolHelper.OnAssetDatabaseChange?.Invoke();
        }
    }

    public static class SOToolHelper
    {
        public static Action OnAssetDatabaseChange;
        public static Action LastAction;
        public static float RowHeight = 45;
        public static float RowGap = 4;
        public static float IconWidth = 20;
        public static float IconHeight = 35;
        public static bool IsLastActionPressed = false;
        public static GUIStyle NormalNameLabelStyle
        {
            get
            {
                GUIStyle style = new GUIStyle("AssetLabel")
                {
                    alignment = TextAnchor.UpperLeft,
                    fontStyle = FontStyle.Bold,
                };
                style.normal.textColor = Color.green;
                return style;
            }
        }
        public static GUIStyle ChosenNameLabelStyle
        {
            get
            {
                GUIStyle style = new GUIStyle("AssetLabel")
                {
                    alignment = TextAnchor.UpperLeft,
                    fontStyle = FontStyle.Bold,
                };
                style.normal.textColor = Color.white;
                return style;
            }
        }
        public static GUIStyle PathTextLabelStyle => new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            alignment = TextAnchor.LowerLeft,
            fontStyle = FontStyle.Italic
        };

        public static GUIStyle AddButtonStyle
        {
            get
            {
                GUIStyle style = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fixedHeight = RowHeight - 4,
                    fixedWidth = RowHeight - 20
                };
                return style;
            }
        }

        #region Create new asset
        public static string ChooseFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select save folder", Application.dataPath, "folder");
            return path;
        }
        public static void CreateAssetAtPath(Type T, string folderPath, string name) 
        {
            string filePath = Path.Combine(folderPath, name + ".asset");
            ScriptableObject scriptableObject = ScriptableObject.CreateInstance(T);
            scriptableObject.name = name;
            AssetDatabase.CreateAsset(scriptableObject, filePath);
            AssetDatabase.Refresh();
            Selection.activeObject = scriptableObject;
        }
        public static void DeleteAsset(string filePath)
        {
            AssetDatabase.DeleteAsset(filePath);
        }
        #endregion
    }

    public class FoldHeader : IComparable
    {
        public Type Type;
        public string Name;
        public bool IsFold;

        public FoldHeader(Type type, bool defaultFold)
        {
            Name = type.Name;
            IsFold = defaultFold;
            Type = type;
        }

        public bool Compare(FoldHeader foldHeader)
        {
            if(foldHeader.Name == this.Name && foldHeader.Type == this.Type)
            {
                return true;
            }
            return false;
        }
        public int CompareTo(object obj)
        {
            if(obj.GetType() != typeof(FoldHeader))
                return 0;
            else
            {
                FoldHeader foldHeader = (FoldHeader) obj;
                if (foldHeader.Name == this.Name)
                    return 1;
                else return 0;
            }
        }
    }
}