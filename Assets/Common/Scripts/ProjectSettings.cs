using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "ProjectSettings", menuName = "October/Project Settings", order = 1)]
    public class ProjectSettings : ScriptableObject
    {
        [SerializeField] protected string mainMenuSceneName = "Main Menu";
        public string MainMenuSceneName => mainMenuSceneName;

        [SerializeField] protected string gameSceneName = "Game";
        public string GameSceneName => gameSceneName;

        [SerializeField] protected string loadingSceneName = "Loading Screen";
        public string LoadingSceneName => loadingSceneName;

        protected void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this); // Mark asset dirty for saving

            // Dynamically call editor-only method
            var editorType = System.Type.GetType("OctoberStudio.DefaultSceneLoader, Assembly-CSharp-Editor");
            if (editorType != null)
            {
                var method = editorType.GetMethod("OnProjectSettingsChanged", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                method?.Invoke(null, new object[] {});
            }
#endif
        }
    }
}