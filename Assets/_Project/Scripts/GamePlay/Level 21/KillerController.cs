using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class KillerController : MonoBehaviour
    {
        enum KillerState
        {
            PATROLLING,
            ATTACK,
            RUN_AWAY
        }
        
        #region Private Serializable Fields

        [SerializeField] private KillerState curretState;

        [Header("Flags")]
        [SerializeField] private bool facingRight = true;
        
        [Header("Componqents")]
        [SerializeField] private RectTransform waypointA;
        [SerializeField] private RectTransform waypointB;
        [SerializeField] private RectTransform rectTransform;
        
        [Header("Stats")]
        [SerializeField] private float moveSpeed = 2f;
        private Vector2 target;
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            target = waypointB.anchoredPosition;
            // StartCoroutine(MoveBetweenWaypoints());
            
            
            RectTransform draggableParent = rectTransform.parent as RectTransform;
            Vector3 wp0World = waypointA.TransformPoint(waypointA.anchoredPosition);
            Vector3 wp1World = waypointB.TransformPoint(waypointB.anchoredPosition);

            Vector3 wp0 = draggableParent.InverseTransformPoint(wp0World);
            Vector3 wp1 = draggableParent.InverseTransformPoint(wp1World);
            StartCoroutine(MoveAlongWaypoints(wp0, wp1, 3f));
        }

        private void Update()
        {
            switch (curretState)
            {
                case KillerState.PATROLLING:
                    break;
                case KillerState.ATTACK:
                    break;
                case KillerState.RUN_AWAY:
                    break;
            }
        }

        #endregion

        #region Private Methods

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
                Flip();

                // Move from wp1 back to wp0
                t = 0f;
                
                while (t < duration)
                {
                    t += Time.deltaTime;
                    rectTransform.anchoredPosition = Vector3.Lerp(wp1, wp0, t / duration);
                    yield return null;
                }
                rectTransform.anchoredPosition = wp0;
                
                Flip();
            }
        }
        
        private IEnumerator MoveBetweenWaypoints()
        {
            while (true)
            {
                // Move towards the target anchoredPosition
                while (Vector2.Distance(((RectTransform)transform).anchoredPosition, target) > 0.05f)
                {
                    ((RectTransform)transform).anchoredPosition = Vector3.MoveTowards(((RectTransform)transform).anchoredPosition, target, moveSpeed * Time.deltaTime
                    );
                    yield return null;
                }

                // Snap to target and flip direction
                ((RectTransform)transform).anchoredPosition = target;
                Flip();

                // Switch target
                target = (target == waypointA.anchoredPosition) ? waypointB.anchoredPosition : waypointA.anchoredPosition;
            }
        }
        
        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        
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
    [CustomEditor(typeof(KillerController))]
    [CanEditMultipleObjects]
    public class KillerControllerEditor : Editor
    {
        private KillerController script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (KillerController)target;

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