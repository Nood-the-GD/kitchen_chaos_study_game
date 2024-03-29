using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace FastTranslate.Internal{
    public static class FTEditorHelper{

        public static void ImportCSV(TextAsset textAsset){
            var gridName = textAsset.name;
            //check if exit gridName
            var find = Project.singleton.grids.Find(x=>x.nameGrid == gridName);
            if(find != null){
                if (UnityEditor.EditorUtility.DisplayDialog("Replace grid", "Do you want to replace grid: " + gridName, "Yes", "No"))
                {
                    Project.singleton.grids.Remove(find);
                }
            }


            var grid = new Internal.Grid();
            grid.nameGrid = gridName;
            var lines = textAsset.text.Split('\n');
            var firstLine = lines[0].Split(',');
            var langSupports = new List<string>();
            for (int i = 1; i < firstLine.Length-1; i++)
            {
                langSupports.Add(firstLine[i]);
            }

            //get all missing language
            var missingLang = new List<string>();
            foreach (var i in Project.singleton.langSupports)
            {
                var findLang = langSupports.Find(x=> Project.singleton.GetLanguageCode(x) == i.languagesSuport.ToString());
                if(findLang == null){
                    missingLang.Add(i.languagesSuport.ToString());
                }
            }

            for(int i = 1; i < lines.Length; i++){
                var line = lines[i].Split(',');
                var record = new Internal.Record();
                record.recordID = line[0];
                var isObjectType = line[line.Length - 1];
                if(isObjectType == "True")
                {
                    record.objectType = true;
                }

                var cols = new List<Internal.Column>();

                for (int j = 1; j < line.Length-1; j++)
                {
                    var column = new Internal.Column();
                    column.columnID = Project.singleton.GetLanguageCode(langSupports[j-1]);
                    column.text = line[j];
                    cols.Add(column);
                }

                foreach(var j in missingLang){
                    cols.Add(new Internal.Column(j,""));
                }

                foreach (var j in cols)
                {
                    record.columns.Add(j);
                }
                if(record.columns.Count == missingLang.Count + langSupports.Count){
                    grid.records.Add(record);
                }
            }

            Project.singleton.grids.Add(grid);
            Project.singleton.setDirty();
            Debug.Log("Import CSV complete");
            
        }

        public static void ExportCSV(Internal.Grid grid){
            
            //check if exit folder Application.dataPath +"/Fast Translate/Export" if not create
            if (!Directory.Exists(Application.dataPath +"/Fast Translate/Export"))
            {
                Directory.CreateDirectory(Application.dataPath +"/Fast Translate/Export");
            }

            
            var filePath = Application.dataPath +"/Fast Translate/Export/"+ grid.nameGrid + ".csv";
            
            using (StreamWriter writer = new(filePath))
            {
                // Write CSV header
                var firstRow = "RecordId";
                foreach (var i in Project.singleton.langSupports)
                {
                    firstRow += "," + i.name;
                }

                firstRow += ",objectType";

                writer.WriteLine(firstRow);
                
                foreach(var i in grid.records){
                    var row = i.recordID;
                    foreach (var j in i.columns)
                    {
                        row += "," + j.text;
                    }
                    row += "," + i.objectType.ToString();
                    writer.WriteLine(row);
                    
                }
                
            }

            Debug.Log("CSV export complete: "+ filePath);
            //refesh database
            UnityEditor.AssetDatabase.Refresh();
        }

    }
}