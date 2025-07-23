using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class GUIGamePlay : MonoBehaviour
    {
        #region Private Serializable Fields

        [SerializeField] private LevelController currentLevel;
        public LevelController CurrentLevel => currentLevel;

        [Header("UI Components")] 
        [SerializeField] private Slider sliderTimer;
        
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
    [CustomEditor(typeof(GUIGamePlay))]
    [CanEditMultipleObjects]
    public class GUIGamePlayEditor : Editor
    {
        private GUIGamePlay script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (GUIGamePlay)target;

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