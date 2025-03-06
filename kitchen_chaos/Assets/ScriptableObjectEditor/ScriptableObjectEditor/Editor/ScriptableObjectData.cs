using System;
using UnityEngine;
using UnityEditor;

namespace NOOD.ScriptableObjectTool
{
    public class ScriptableObjectData 
    {
        public ScriptableObject ScriptableObject;
        public string Path;
        public string FolderPath;
        public string Name;
        public Type Type;
        
        public ScriptableObjectData(string dataPath)
        {
            Path = AssetDatabase.GUIDToAssetPath(dataPath);
            FolderPath = System.IO.Path.GetDirectoryName(Path);
            ScriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(Path);
            Name = ScriptableObject.name;
            Type = ScriptableObject.GetType();
        }
    }
}