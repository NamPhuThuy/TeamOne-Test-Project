using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Object that auto rotate through time
/// </summary>

namespace NamPhuThuy
{
    public class UIRotateable : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] float speed = 360f;
        private Vector3 _rotateDirection;
        private float _directionMultiply = 1;

    
        [Header("Flags")]
        [SerializeField] private bool isRotateX = false;
        [SerializeField] private bool isRotateY = false;
        [SerializeField] private bool isRotateZ = false;
        [SerializeField] private bool isRotateSpaceWorld = false;
        [SerializeField] private bool isUseUnscaledDeltaTime = false;
        private Space _space;

        #region MonoBehaviour

        private void Start()
        {
            Setup();
        }

        #endregion

        #region Private Methods

        private void Setup()
        {
            _rotateDirection = new Vector3(isRotateX ? 1 : 0, isRotateY ? 1 : 0, isRotateZ ? 1 : 0) * _directionMultiply;
            _space = isRotateSpaceWorld ? Space.World : Space.Self;
        }

        private IEnumerator IEActiveRotate()
        {
            while (true)
            {
                if (isUseUnscaledDeltaTime)
                {
                    transform.Rotate((speed * Time.unscaledDeltaTime) * _rotateDirection, _space);
                }
                else transform.Rotate((speed * Time.deltaTime) * _rotateDirection, _space);

                yield return null;
            }
        }

        #endregion

        #region Public Methods

        public void UpdateRotateDirection()
        {
            Setup();
        }

        public void ActiveRotate()
        {
            Debug.Log("Active Rotate");
            StartCoroutine(IEActiveRotate());
        }
        
        #endregion
    }

    #if UNITY_EDITOR
        [CustomEditor(typeof(UIRotateable))]
        public class UIRotateableEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                // Add custom text at the highest place
                EditorGUILayout.LabelField("Rotate around a specific axis (notice the color of the axis when press W-key)", EditorStyles.boldLabel);
            
                DrawDefaultInspector();

                UIRotateable objRotateable = (UIRotateable)target;

                if (GUILayout.Button("Update Rotate Direction"))
                    objRotateable.UpdateRotateDirection();
                
                if (GUILayout.Button("Reset Rotation"))
                {
                    objRotateable.transform.rotation = Quaternion.identity;
                    objRotateable.UpdateRotateDirection();
                }
            }
        }
    #endif
}