using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class GUIGameOver : GUIBase
    {
        #region Private Serializable Fields
        
        [SerializeField] private TextMeshProUGUI textGameOver;
        private string winText = "Level Completed!";
        private string loseText = "you Failed!";

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
    [CustomEditor(typeof(GUIGameOver))]
    [CanEditMultipleObjects]
    public class GUIGameOverEditor : Editor
    {
        private GUIGameOver script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (GUIGameOver)target;

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