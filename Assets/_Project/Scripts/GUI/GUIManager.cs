using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class GUIManager : Singleton<GUIManager>
    {
        #region Private Serializable Fields

        [SerializeField] private GUIGamePlay guiGamePlay;
        
        
        #endregion

        #region Public Fields

        public GUIGamePlay GUIGamePlay => guiGamePlay;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks
        

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(GUIManager))]
    [CanEditMultipleObjects]
    public class GUIManagerEditor : Editor
    {
        private GUIManager script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (GUIManager)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset Values", GUILayout.Width(ConstInspector.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif
}