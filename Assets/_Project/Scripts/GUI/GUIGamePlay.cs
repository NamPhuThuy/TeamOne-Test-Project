using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class GUIGamePlay : GUIBase
    {
        #region Private Serializable Fields

        [SerializeField] private LevelController currentLevel;
        public LevelController CurrentLevel => currentLevel;

        [Header("UI Components")] 
        [SerializeField] private Slider sliderTimer;

        [SerializeField] private float sliderDuration = 20f;
        
        [SerializeField] private float levelDuration = 240f;
        [SerializeField] private TextMeshProUGUI textTimer;
        
        #endregion

        #region Private Fields
        private Coroutine sliderCoroutine;
        private Coroutine timerCoroutine;

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            StartSliderTimer();
            StartTextTimer();
        }

        #endregion

        #region Private Methods

        
        

        private string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{secs:00}";
        }
        
        private IEnumerator FillSliderOverTime()
        {
            sliderTimer.value = 0f;
            float elapsed = 0f;
            while (elapsed < sliderDuration)
            {
                elapsed += Time.deltaTime;
                sliderTimer.value = Mathf.Lerp(0f, sliderTimer.maxValue, elapsed / sliderDuration);
                yield return null;
            }
            sliderTimer.value = sliderTimer.maxValue;
            
            // LOSE THE GAME  
            TriggerLose();
        }
        
        private IEnumerator CountdownTextTimer()
        {
            float timeLeft = levelDuration;
            while (timeLeft > 0f)
            {
                textTimer.text = FormatTime(Mathf.CeilToInt(timeLeft));
                yield return null;
                timeLeft -= Time.deltaTime;
            }
            textTimer.text = "0";
            
            // LOSE THE GAME
            TriggerLose();
        }
        
        #endregion

        #region Public Methods
        public void TriggerLose()
        {
            GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIGameOver, false);
        }
        
        public void StartSliderTimer()
        {
            if (sliderCoroutine != null)
                StopCoroutine(sliderCoroutine);

            sliderCoroutine = StartCoroutine(FillSliderOverTime());
        }
        
        public void StartTextTimer()
        {
            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);

            timerCoroutine = StartCoroutine(CountdownTextTimer());
        }

        public void ResetSliderTimer()
        {
            if (sliderCoroutine != null)
                StopCoroutine(sliderCoroutine);

            sliderTimer.value = 0f;
            sliderCoroutine = StartCoroutine(FillSliderOverTime());
        }
        
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(GUIGamePlay))]
    [CanEditMultipleObjects]
    public class GUIGamePlayEditor : Editor
    {
        private GUIGamePlay script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (GUIGamePlay)target;

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