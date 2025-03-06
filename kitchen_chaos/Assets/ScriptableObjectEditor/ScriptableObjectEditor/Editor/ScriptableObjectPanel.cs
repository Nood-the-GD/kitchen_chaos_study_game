using UnityEngine;
using UnityEditor;
using NOOD.Editor;

namespace NOOD.ScriptableObjectTool
{
    public class ScriptableOBjectPanel
    {
        private ScriptableObjectData _data;
        public bool IsData => _data != null;
        private EditorWindow _window;

        public ScriptableOBjectPanel(ScriptableObjectData data, EditorWindow editorWindow)
        {
            _data = data;
            _window = editorWindow;
        }

        public void DrawRow()
        {
            Rect backgroundRect;
            GUILayout.BeginHorizontal();
            {
                // Draw BG
                GUILayout.Space(30);
                var space = GUILayoutUtility.GetLastRect();
                backgroundRect = new Rect(space.position.x, space.position.y, _window.position.width, SOToolHelper.RowHeight);
                if (ColorUtility.TryParseHtmlString("#303030", out Color color))
                    EditorGUI.DrawRect(backgroundRect, color);
                else
                    EditorGUI.DrawRect(backgroundRect, Color.black);
                backgroundRect.width -= 60;
                if(backgroundRect.Contains(Event.current.mousePosition))
                {
                    if(Event.current.type == EventType.MouseDown)
                    {
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new[] { _data.ScriptableObject };
                        DragAndDrop.StartDrag("Start drag");
                    }
                }

                // Draw content
                Texture2D icon = AssetPreview.GetMiniThumbnail(_data.ScriptableObject);
                GUILayout.Label(icon,BasicEditorStyles.AlignLabelMiddleLeft , GUILayout.Width(SOToolHelper.IconWidth), GUILayout.Height(SOToolHelper.IconHeight)); // Draw icon
                DrawNameLabels();
                DrawAddButton("Create a new ScriptableObject in the same folder then <b>Press Space to quick create</b>");
                DrawDeleteButton();
            }
            GUILayout.EndHorizontal();
        }
        private void DrawNameLabels()
        {
            GUILayout.BeginVertical();
            {
                // Draw name labels
                GUILayout.Space(5);
                GUIStyle labelNameStyle;
                if(DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] == _data.ScriptableObject)
                {
                    // Current object is dragging
                    labelNameStyle = SOToolHelper.ChosenNameLabelStyle;
                }
                else
                {
                    labelNameStyle = SOToolHelper.NormalNameLabelStyle;
                }
                GUILayout.Label(_data.Name, labelNameStyle);
                {
                    // Assign press event for name label 
                    var labelName = GUILayoutUtility.GetLastRect();
                    if (labelName.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown)
                        {
                            AssetDatabase.OpenAsset(_data.ScriptableObject);
                            EditorGUIUtility.PingObject(_data.ScriptableObject);
                        }
                    }
                }

                GUILayout.Label(_data.Path, SOToolHelper.PathTextLabelStyle);
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }
        private void DrawAddButton(string tooltip = "")
        {
            GUIContent addSign = EditorGUIUtility.IconContent("d_ol_plus_act");
            addSign.tooltip = tooltip;
            if(GUILayout.Button(addSign, SOToolHelper.AddButtonStyle, GUILayout.Height(SOToolHelper.RowHeight)))
            {
                SOToolHelper.LastAction = () =>
                {
                    InputNewFileName();
                };
                InputNewFileName();
            }
        }
        private void InputNewFileName()
        {
            string fileName = EditorInputDialogue.Show("Create new asset", "Enter ScriptableObject name", "");
            if(fileName != null && fileName != "" && fileName != " ")
            {
                SOToolHelper.CreateAssetAtPath(_data.Type, _data.FolderPath, fileName);
            }
            GUIUtility.ExitGUI();
        }
        private void DrawDeleteButton()
        {
            GUIContent minusSign = EditorGUIUtility.IconContent("d_ol_minus");
            minusSign.tooltip = "Delete this ScriptableObject";
            if(GUILayout.Button(minusSign, SOToolHelper.AddButtonStyle, GUILayout.Height(SOToolHelper.RowHeight)))
            {
                if(EditorUtility.DisplayDialog("Confirm delete", $"Do you want to delete {_data.Name}", "Delete", "Cancel"))
                    SOToolHelper.DeleteAsset(_data.Path);
            }
        }
    }
}
