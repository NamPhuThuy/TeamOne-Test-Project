using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class LevelController : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("Level information")]
        [SerializeField] private int levelId = 0;

        [Header("Components")] 
        [SerializeField] private InteractableArea interactableArea;
        public InteractableArea InteractableArea => interactableArea;
        
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
    [CustomEditor(typeof(LevelController))]
    [CanEditMultipleObjects]
    public class LevelControllerEditor : Editor
    {
        private LevelController script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (LevelController)target;

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