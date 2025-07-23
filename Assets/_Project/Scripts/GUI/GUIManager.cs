using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MoreMountains.Tools;
using UnityEditor.SceneManagement;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    public class GUIManager : Singleton<GUIManager>
    {
        /*
Performance Comparison:

1. Serialized Fields:
   - **Pros**: Fast direct access, editor-friendly, type-safe.
   - **Cons**: Poor scalability, hardcoded, manual iteration for all GUIs.
   - **Performance**: Best for small, static GUI setups.

2. Dictionary-Based:
   - **Pros**: Dynamic mapping, scalable, flexible.
   - **Cons**: Runtime overhead for initialization, type casting, debugging complexity.
   - **Performance**: Slightly slower access, but better for large, dynamic projects.

**Summary**:
- Use Serialized Fields for small, static GUIs.
- Use Dictionary for large, dynamic GUIs needing flexibility.
*/

        #region GUI References

        [Header("GUI")] 
        [SerializeField] private GUIGamePlay guiGamePlay;

        [SerializeField] private GUIGameOver guiGameOver;


        #endregion

        [SerializeField] private List<GUIBase> guiList = new List<GUIBase>();

        #region GUI Properties

        public GUIGamePlay GUIGamePlay => guiGamePlay;
        public GUIGameOver GUIGameOver => guiGameOver;
        
        #endregion

        public List<GUIBase> GUIList => guiList;

        [Header("Flags")] 
        [SerializeField] private bool isShowingGUI = false;

        #region MonoBehaviour

        private void OnEnable()
        {
            MMEventManager.RegisterAllCurrentEvents(this);
        }

        private void OnDisable()
        {
            MMEventManager.UnregisterAllCurrentEvents(this);
        }

        private void Start()
        {
            var guiComponents = new List<GUIBase>
            {
                GUIGamePlay, GUIGameOver
            };

            foreach (var gui in guiComponents)
            {
                gui.OnShow += OnGUIShow;
                gui.OnHide += OnGUIHide;
            }
        }

        #endregion

        #region Public Methods

#if UNITY_EDITOR
        public void FindAllGUIBase()
        {
            guiList.Clear();
            GUIBase[] guiBases = GetComponentsInChildren<GUIBase>(true);
            guiList.AddRange(guiBases);

            var fields = GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (typeof(GUIBase).IsAssignableFrom(field.FieldType))
                {
                    var component = GetComponentInChildren(field.FieldType, true);
                    if (component != null)
                    {
                        field.SetValue(this, component);
                    }
                }
            }
        }
#endif

        public void ShowGUI(GUIBase guiShow, params object[] parameters)
        {
            StartCoroutine(IEShowGUI(guiShow, parameters));
        }

        public void ShowGUI(GUIBase guiShow, float delay, params object[] parameters)
        {
            StartCoroutine(IEShowGUI(guiShow, delay, parameters));
        }


        #endregion

        #region Power-ups



        #endregion

        #region Private Methods

        private IEnumerator IEShowGUI(GUIBase guiShow, params object[] parameters)
        {
            yield return null;
            guiShow.Show(parameters);
        }

        private IEnumerator IEShowGUI(GUIBase guiShow, float f, params object[] parameters)
        {
            yield return Yielders.Get(f);
            guiShow.Show(parameters);
        }

        private void OnGUIShow()
        {
            isShowingGUI = true;
        }

        private void OnGUIHide()
        {
            isShowingGUI = false;
        }

        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(GUIManager))]
    public class GUIManagerEditor : Editor
    {
        private GUIManager guiManager;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            guiManager = (GUIManager)target;

            ButtonFindAllGUIBases();
            ButtonShowAndHide();

        }

        private void ButtonFindAllGUIBases()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find All GUIBase", GUILayout.Width(ConstInspector.BUTTON_WIDTH_LARGE)))
            {
                guiManager.FindAllGUIBase();
                EditorSceneManager.MarkSceneDirty(guiManager.gameObject.scene);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void ButtonShowAndHide()
        {
            // Iterate through all serialized GUI fields
            var fields = typeof(GUIManager).GetFields(BindingFlags.NonPublic |
                                                      BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (typeof(GUIBase).IsAssignableFrom(field.FieldType))
                {
                    GUIBase gui = field.GetValue(guiManager) as GUIBase;
                    if (gui != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(field.Name, GUILayout.Width(150));

                        if (GUILayout.Button("Show", GUILayout.Width(ConstInspector.BUTTON_WIDTH_SMALL)))
                        {
                            gui.Show();
                            EditorUtility.SetDirty(guiManager);
                        }

                        if (GUILayout.Button("Hide", GUILayout.Width(ConstInspector.BUTTON_WIDTH_SMALL)))
                        {
                            gui.Hide();
                            EditorUtility.SetDirty(guiManager);
                        }

                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
    }
#endif
}