using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class UICheckDoubleClick : MonoBehaviour, IPointerClickHandler
    {
        #region Private Serializable Fields

        [Header("Flags")]
        [SerializeField] private bool isReActivate = false;
        [SerializeField] private bool isActivated = false;
        [SerializeField] private bool isAutoDeadactive = false;
        
        [SerializeField] private UnityEvent onDoubleClick;
        [SerializeField] private UnityEvent onDeadActive;
        
        
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods
        
        public void DeadActive()
        {
            onDeadActive?.Invoke();
        }
        
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isActivated) return;
        
            if (eventData.clickCount == 2)
            {
                onDoubleClick?.Invoke();
                isActivated = true;

                if (isReActivate) isActivated = false;
                if (isAutoDeadactive) Invoke("DeadActive", 3f);
                Debug.Log("Double-click detected!");
            }
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(UICheckDoubleClick))]
    [CanEditMultipleObjects]
    public class UICheckDoubleClickEditor : Editor
    {
        private UICheckDoubleClick script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (UICheckDoubleClick)target;

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