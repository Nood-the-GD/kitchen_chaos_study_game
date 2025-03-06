using System;
using UnityEditor;
using UnityEngine;

namespace NOOD.ScriptableObjectTool
{
    public class EditorInputDialogue : EditorWindow
    {
        private Action onOKButton;
        private string description, inputText;
        private string okButton, cancelButton;
        private bool initializedPosition = false;
        private bool shouldClose = false;
    
        #region OnGUI()
        void OnGUI()
        {
            // Check if Esc/Return have been pressed
            var e = Event.current;
            if( e.type == EventType.KeyDown )
            {
                switch( e.keyCode )
                {
                    // Escape pressed
                    case KeyCode.Escape:
                        shouldClose = true;
                        break;
    
                    // Enter pressed
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        onOKButton?.Invoke();
                        shouldClose = true;
                        break;
                }
            }
    
            if( shouldClose ) 
            {  // Close this dialog
                Close();
                //return;
            }
    
            // Draw our control
            var rect = EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.Space( 12 );
                EditorGUILayout.LabelField( description );
        
                EditorGUILayout.Space( 8 );
                GUI.SetNextControlName( "inText" );
                inputText = EditorGUILayout.TextField( "", inputText );
                GUI.FocusControl( "inText" );   // Focus text field
                EditorGUILayout.Space( 12 );
                var r = EditorGUILayout.GetControlRect();
                r.width /= 2;

                // Draw OK / Cancel buttons
                if( GUI.Button( r, okButton ) ) {
                    onOKButton?.Invoke();
                    shouldClose = true;
                }
        
                r.x += r.width;
                if( GUI.Button( r, cancelButton ) ) {
                    inputText = null;   // Cancel - delete inputText
                    shouldClose = true;
                }
        
                EditorGUILayout.Space( 8 );
            }
            EditorGUILayout.EndVertical();
    
    
            // Force change size of the window
            if( rect.width != 0 && minSize != rect.size ) {
                minSize = maxSize = rect.size;
            }
    
            // Set dialog position next to mouse position
            if( !initializedPosition ) {
                var mousePos = GUIUtility.GUIToScreenPoint( Event.current.mousePosition );
                position = new Rect( mousePos.x + 32, mousePos.y, position.width, position.height );
                initializedPosition = true;
            }
        }
        #endregion OnGUI()
    
        #region Show()
        /// <summary>
        /// Returns text player entered, or null if player cancelled the dialog.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="inputText"></param>
        /// <param name="okButton"></param>
        /// <param name="cancelButton"></param>
        /// <returns></returns>
        public static string Show( string title, string description, string inputText, string okButton = "OK", string cancelButton = "Cancel" )
        {
            string ret = null;
            // var window = EditorWindow.GetWindow<InputDialog>();
            var window = CreateInstance<EditorInputDialogue>();
            window.titleContent = new GUIContent( title );
            window.description = description;
            window.inputText = inputText;
            window.okButton = okButton;
            window.cancelButton = cancelButton;
            window.onOKButton += () => ret = window.inputText;
            window.ShowModal(); // Ignore the OnGUIDepth error (if has)
    
            return ret;
        }
        #endregion Show()
    }
}