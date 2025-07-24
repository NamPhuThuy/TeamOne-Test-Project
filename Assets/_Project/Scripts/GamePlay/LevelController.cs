using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class LevelController : MonoBehaviour
    {
        #region Private Serializable Fields
        
        [Header("Stats")]
        [SerializeField] private int objNeedToComplete = 7;

        [Header("Level information")]
        [SerializeField] private int levelId = 21;

        [Header("Components")] 
        [SerializeField] private InteractableArea interactableArea;
        public InteractableArea InteractableArea => interactableArea;
        [SerializeField] private GameObject highTrajectory;
        public GameObject HighTrajectory => highTrajectory;
        
        public PoliceController policeController;
        [SerializeField] private KillerController killerController;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks
        

        #endregion

        #region Private Methods

        private IEnumerator IECompleteLevel()
        {
            yield return Yielders.Get(5f);
            GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIGameOver, true);
        }
        
        #endregion

        #region Public Methods
        
        public void OnCompleteOneObject()
        {
            objNeedToComplete--;
            if (objNeedToComplete <= 0)
            {
                StartCoroutine(IECompleteLevel());
                policeController.ActivePoliceWarning();
                
                // Killer run away
                killerController.ChangeState(KillerController.KillerState.RUN_AWAY);
            }
        }
        
        
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(LevelController))]
    [CanEditMultipleObjects]
    public class LevelControllerEditor : Editor
    {
        private LevelController script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (LevelController)target;

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