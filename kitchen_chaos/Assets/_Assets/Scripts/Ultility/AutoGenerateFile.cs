using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
    using UnityEditor;
#endif
public static class AutoGenerateFile
{
    public static void GenerateEnum(string enumName, List<string> values, bool autoRefesh = false){
        string filePath = Application.dataPath + "/_Assets/Scripts/Enums/" + enumName + ".cs";
        using (StreamWriter writer = new StreamWriter(filePath)) {
            writer.WriteLine("public enum " + enumName + " {");
            //add the None option
            writer.WriteLine("\tNone,");

            for (int i = 0; i < values.Count; i++) {
                writer.WriteLine("\t" + values[i]+"="+EncodeStringToNumber(values[i]) + ",");
            }

            writer.WriteLine("}");
        }
#if UNITY_EDITOR
        if(autoRefesh)
            AssetDatabase.Refresh();
#endif
    }

    public static void GenerateEnum(string enumName, HashSet<string> values){
        //convert hashset to list
        List<string> list = new List<string>();
        foreach (var item in values) {
            list.Add(item);
        }
        GenerateEnum(enumName, list);
    }

    public static bool IsValidNameForEnum(this string name){
        //check if it have space
        if (name.Contains(" "))
            return false;
        //check if it have special character
        if (name.Contains("!") || name.Contains("@") || name.Contains("#") || name.Contains("$") || name.Contains("%") || name.Contains("^") || name.Contains("&") || name.Contains("*") || name.Contains("(") || name.Contains(")") || name.Contains("-") || name.Contains("+") || name.Contains("=") || name.Contains("{") || name.Contains("}") || name.Contains("[") || name.Contains("]") || name.Contains("|") || name.Contains("\\") || name.Contains(":") || name.Contains(";") || name.Contains("\"") || name.Contains("'") || name.Contains("<") || name.Contains(">") || name.Contains(",") || name.Contains(".") || name.Contains("?") || name.Contains("/"))
            return false;
        if (name == "None")
            return false;
        //check if it start with number
        if (name[0] >= '0' && name[0] <= '9')
            return false;

        return true;
    }

    //write a function to check a list of stirng if it valid for enum
    public static bool IsValidNameForEnum(this List<string> names, bool logError = true){
        for (int i = 0; i < names.Count; i++) {
            if (!names[i].IsValidNameForEnum()){
                if(logError)
                    Debug.LogError("name is not valid for enum: " + names[i]);
                return false;
            }
        }
        return true;
    }

    //write a function to check a hashset of stirng if it valid for enum
    public static bool IsValidNameForEnum(this HashSet<string> names, bool logError = true){
        foreach (var name in names) {
            if (!name.IsValidNameForEnum()){
                if(logError)
                    Debug.LogError("name is not valid for enum: " + name);
                return false;
            }
        }
        return true;
    }

    static int EncodeStringToNumber(string input)
    {
        int encodedNumber = 0;

        foreach (char c in input)
        {
            encodedNumber = encodedNumber * 2 + (byte)c;
        }

        return encodedNumber;
    }
}
