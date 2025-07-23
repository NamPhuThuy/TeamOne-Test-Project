using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class GamePlayManager : MonoBehaviour
    {
        #region Private Serializable Fields

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            
        }

        void Update()
        {
            
        }

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
    [CustomEditor(typeof(GamePlayManager))]
    [CanEditMultipleObjects]
    public class GamePlayManagerEditor : Editor
    {
        private GamePlayManager script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (GamePlayManager)target;

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