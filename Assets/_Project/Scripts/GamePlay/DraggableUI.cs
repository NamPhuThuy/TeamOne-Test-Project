using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region Private Serializable Fields

        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Canvas canvas;
        [SerializeField] private UnityEvent onDragInTargetArea;

        [Header("Images")]
        [SerializeField] private Image objectImage;
        [SerializeField] private Sprite originalSprite;
        [SerializeField] private Sprite onDragSprite;
        [SerializeField] private Sprite activatedSprite;
        #endregion

        #region Private Fields
        
        private Vector3 initialPosition;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            objectImage = GetComponent<Image>();
            canvas = GetComponentInParent<Canvas>();
            
            initialPosition = rectTransform.anchoredPosition;
            ChangeObjectImageTo(originalSprite);
        }
        
        #endregion

        #region Private Methods

        private void ChangeObjectImageTo(Sprite targetSprite)
        {
            if (objectImage != null && targetSprite != null)
            {
                objectImage.sprite = targetSprite;
                objectImage.SetNativeSize();
            }
        }
        
        #endregion

        #region Public Methods
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion

        #region Override Methods

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (onDragSprite != null)
            {
                ChangeObjectImageTo(onDragSprite);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    GUIManager.Ins.GUIGamePlay.CurrentLevel.InteractableArea.RectTransform, rectTransform.position, canvas.worldCamera))
            {
                onDragInTargetArea?.Invoke();
                Debug.Log("Drag onto the target area!");
            }
            
            ChangeObjectImageTo(originalSprite);
            rectTransform.anchoredPosition = initialPosition;
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(DraggableUI))]
    [CanEditMultipleObjects]
    public class DraggableUIEditor : Editor
    {
        private DraggableUI script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (DraggableUI)target;

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