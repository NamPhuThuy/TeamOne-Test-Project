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
        public enum DragType
        {
            GetBackAfterActive,
            FloatAfterActive
        }
        
        #region Private Serializable Fields
        
        [Header("Stats")]
        [SerializeField] private DragType dragType;
        
        [Header("Flags")]
        [SerializeField] private bool isInteractable = true;

        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Canvas canvas;
        [SerializeField] private UnityEvent onDragInTargetArea;
        [SerializeField] private UnityEvent onActivated;

        [Header("Images")]
        [SerializeField] private Image objectImage;
        [SerializeField] private Sprite originalSprite;
        [SerializeField] private Sprite onDragSprite;
        [SerializeField] private Sprite activatedSprite;
        
        [Header("Components")]
        [SerializeField] private UIRotateable uiRotateable;
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
            
            onDragInTargetArea.AddListener(OnDragInTargetArea);
            
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
        
        private void OnDragInTargetArea()
        {
            isInteractable = false;
            GUIManager.Ins.GUIGamePlay.ResetSliderTimer();
            
            StartCoroutine(IEOnDragInTargetArea());
        }
        
        private IEnumerator IEOnDragInTargetArea()
        {
            yield return Yielders.Get(1.8f);
            
            onActivated?.Invoke();
            
            ChangeObjectImageTo(activatedSprite);
            GUIManager.Ins.GUIGamePlay.ResetSliderTimer();
            
            
            switch (dragType)
            {
                case DragType.FloatAfterActive:
                    uiRotateable.ActiveRotate();
                    GameObject waypointsParent = GUIManager.Ins.GUIGamePlay.CurrentLevel.HighTrajectory;
                    
                    RectTransform draggableParent = rectTransform.parent as RectTransform;
                    RectTransform wp0Rect = waypointsParent.transform.GetChild(0) as RectTransform;
                    RectTransform wp1Rect = waypointsParent.transform.GetChild(1) as RectTransform;

                    Vector3 wp0World = wp0Rect.TransformPoint(wp0Rect.anchoredPosition);
                    Vector3 wp1World = wp1Rect.TransformPoint(wp1Rect.anchoredPosition);

                    Vector3 wp0 = draggableParent.InverseTransformPoint(wp0World);
                    Vector3 wp1 = draggableParent.InverseTransformPoint(wp1World);
                    
                    yield return StartCoroutine(MoveAlongWaypoints(wp0, wp1, 3f));
                    break;
                case DragType.GetBackAfterActive:
                    rectTransform.anchoredPosition = initialPosition;
                    break;
            }
        }
        
        // AFTER ACTIVATED
        private IEnumerator MoveAlongWaypoints(Vector3 wp0, Vector3 wp1, float duration)
        {
            while (true)
            {
                // Move from wp0 to wp1
                float t = 0f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    rectTransform.anchoredPosition = Vector3.Lerp(wp0, wp1, t / duration);
                    yield return null;
                }
                rectTransform.anchoredPosition = wp1;

                // Move from wp1 back to wp0
                t = 0f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    rectTransform.anchoredPosition = Vector3.Lerp(wp1, wp0, t / duration);
                    yield return null;
                }
                rectTransform.anchoredPosition = wp0;
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
            if (!isInteractable) return;
            if (onDragSprite != null)
            {
                ChangeObjectImageTo(onDragSprite);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isInteractable) return;
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    GUIManager.Ins.GUIGamePlay.CurrentLevel.InteractableArea.RectTransform, rectTransform.position, canvas.worldCamera))
            {
                rectTransform.anchoredPosition =
                    GUIManager.Ins.GUIGamePlay.CurrentLevel.InteractableArea.PivotInteract.anchoredPosition;
                onDragInTargetArea?.Invoke();
                Debug.Log("Drag onto the target area!");
                return;
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