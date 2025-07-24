using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class PoliceController : MonoBehaviour
    {
        #region Private Serializable Fields
        
        [Header("Flags")]
        [SerializeField] private bool isActive = false;

        [SerializeField] private Image policeImage;
        [SerializeField] private Sprite spriteA;
        [SerializeField] private Sprite spriteB;
        [SerializeField] private float switchInterval = .1f;
        [SerializeField] private float fadeDuration = .2f;

        
        
        #endregion

        #region Private Fields

        private float timer = 0f;
        private bool useA = true;
        private bool isFading = false;
        
        
        #endregion

        #region MonoBehaviour Callbacks

        private void Update()
        {
            if (!isActive)
            {
                return;
            }
            timer += Time.deltaTime;
            if (timer >= switchInterval)
            {
                StartCoroutine(FadeAndSwitchSprite());
                timer = 0f;
            }
        }

        #endregion

        #region Private Methods
        
        private IEnumerator FadeAndSwitchSprite()
        {
            isFading = true;
            // Fade out
            yield return FadeAlpha(1f, 0f, fadeDuration / 2f);

            // Switch sprite
            useA = !useA;
            policeImage.sprite = useA ? spriteA : spriteB;

            // Fade in
            yield return FadeAlpha(0f, 1f, fadeDuration / 2f);

            isFading = false;
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
        {
            float elapsed = 0f;
            Color color = policeImage.color;
            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(from, to, elapsed / duration);
                policeImage.color = new Color(color.r, color.g, color.b, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }
            policeImage.color = new Color(color.r, color.g, color.b, to);
        }
        
        #endregion

        #region Public

        public void ActivePoliceWarning()
        {
            isActive = true;
            policeImage.enabled = true;
            
        }

        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(PoliceController))]
    [CanEditMultipleObjects]
    public class PoliceControllerEditor : Editor
    {
        private PoliceController script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (PoliceController)target;

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