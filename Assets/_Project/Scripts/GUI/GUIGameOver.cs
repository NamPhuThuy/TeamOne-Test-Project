using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class GUIGameOver : GUIBase
    {
        #region Private Serializable Fields
        
        [SerializeField] private TextMeshProUGUI textGameOver;
        private string winText = "Level Completed!";
        private string loseText = "You Failed!";
        
        [SerializeField] private Button buttonReplay;
        [SerializeField] private Button buttonContinue;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void OnEnable()
        {
            buttonReplay.onClick.AddListener(OnClickReplay);
            buttonContinue.onClick.AddListener(OnClickContinue);
        }

        private void OnDisable()
        {
            buttonReplay.onClick.RemoveListener(OnClickReplay);
            buttonContinue.onClick.RemoveListener(OnClickContinue);
        }

        #endregion

        

        #region Private Methods
        
        private void OnClickReplay()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        private void OnClickContinue()
        {
            // LOAD NEXT LEVEL
        }
        
        #endregion

        #region Public Methods
        #endregion

        #region Override Methods

        public override void Show(params object[] parameters)
        {
            base.Show(parameters);
            if (parameters.Length > 0 && parameters[0] is bool isWin)
            {
                textGameOver.text = isWin ? winText : loseText;
            }
            else
            {
                textGameOver.text = loseText; 
            }
        }

        public override void Hide(params object[] parameters)
        {
            base.Hide(parameters);
        }

        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(GUIGameOver))]
    [CanEditMultipleObjects]
    public class GUIGameOverEditor : Editor
    {
        private GUIGameOver script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (GUIGameOver)target;

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