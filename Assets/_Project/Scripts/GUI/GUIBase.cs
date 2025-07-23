using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class GUIBase : MonoBehaviour
    {
        #region Private Serializable Fields
        [Header("Stats")] 
        [SerializeField] protected float showDuration = 0.4f;
        [SerializeField] protected float hideDuration = 0.4f;
    
        [Space(10)]
        [SerializeField] protected float showDelay = 0f;
        [SerializeField] protected float hideDelay = 0f;
        
        [Header("Components")]
        [SerializeField] private Image guiMask;

        private Color guiMaskColor;
    
        [Header("Behaviour")]
        private List<Tween> showTweens = new List<Tween>();
        private List<Tween> hideTweens = new List<Tween>();
    
        [Header("Flags")]
        public bool isShowing = false;
        
        public event Action OnShow;
        public event Action OnHide;
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

        #region Protected Methods

        protected void AddEventTrigger(GameObject obj, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        protected void TriggerOnShow()
        {
            OnShow?.Invoke();
        }
    
        protected void TriggerOnHide()
        {
            OnHide?.Invoke();
        }

        #endregion

        #region Public Methods
        
        public virtual void Show(params object[] parameters)
    {
        if (isShowing) return;
        isShowing = true;
        
        transform.gameObject.SetActive(true);
        TriggerOnShow();
        
        if (hideTweens.Count > 0)
        {
            foreach (var tween in hideTweens)
            {
                tween.Kill();
            }
            hideTweens.Clear();
        }

        // Example: Fade in and scale up the GUI element with ease-in effect
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        // canvasGroup.alpha = 0f;

        if (guiMask != null)
        {
            Color initialColor = guiMask.color;
            Color color = guiMask.color;
            color.a = 0f;
            guiMask.color = color;
            showTweens.Add(guiMask.DOFade(initialColor.a, showDuration).SetEase(Ease.InQuad));
        }
        transform.localScale = Vector3.zero;
        
        /*if (canvasGroup != null)
        {
            canvasGroup.DOFade(1, showDuration).SetEase(Ease.InQuad).OnComplete(() => canvasGroup.interactable = true);
        }*/
        
        showTweens.Add(transform.DOScale(Vector3.one, showDuration).SetEase(Ease.InQuad));

        // OnShow?.Invoke();
    }

    public virtual void Hide(params object[] parameters)
    {
        if (!isShowing) return;
        isShowing = false;
        
        
        if (showTweens.Count > 0)
        {
            foreach (var tween in showTweens)
            {
                tween.Kill();
            }
            showTweens.Clear();
        }
        // Example: Fade out and scale down the GUI element with ease-out effect
        // CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        /*if (canvasGroup != null)
        {
            canvasGroup.DOFade(0, hideDuration).SetEase(Ease.OutQuad).OnComplete(() => canvasGroup.interactable = false);
        }*/
        
        hideTweens.Add(transform.DOScale(Vector3.zero, hideDuration).SetEase(Ease.OutQuad).OnComplete(() => transform.gameObject.SetActive(false)));
        
       TriggerOnHide();
    }
        
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(GUIBase))]
    [CanEditMultipleObjects]
    public class GUIBaseEditor : Editor
    {
        private GUIBase script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (GUIBase)target;

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