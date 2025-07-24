using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class InteractableArea : MonoBehaviour
    {
        #region Private Serializable Fields

        [SerializeField] private RectTransform rectTransform;
        public RectTransform RectTransform => rectTransform;
        
        [SerializeField] private RectTransform pivotInteract;
        public RectTransform PivotInteract => pivotInteract;
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
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
    [CustomEditor(typeof(InteractableArea))]
    [CanEditMultipleObjects]
    public class InteractableAreaEditor : Editor
    {
        private InteractableArea script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (InteractableArea)target;

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