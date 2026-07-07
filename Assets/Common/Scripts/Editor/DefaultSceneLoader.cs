using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OctoberStudio
{
    [InitializeOnLoad]
    public static class DefaultSceneLoader
    {
        static DefaultSceneLoader()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;

            RewriteStartScene();
        }

        public static void OnProjectSettingsChanged()
        {
            ActivateGameScene();
        }

        private static void RewriteStartScene()
        {
            ActivateGameScene();
        }

        private static void ActivateGameScene()
        {
            if (!DefaultSceneLoaderActions.Enabled)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var mainMenuSceneName = GetMainMenuSceneName();

            if(mainMenuSceneName != null)
            {
                SceneAsset gameScene = GetAsset<SceneAsset>(mainMenuSceneName);
                if (gameScene != null)
                {
                    EditorSceneManager.playModeStartScene = gameScene;
                }
            } else
            {
                EditorSceneManager.playModeStartScene = null;
            }
        }

        private static T GetAsset<T>(string name = "") where T : Object
        {
            string[] assets = AssetDatabase.FindAssets((string.IsNullOrEmpty(name) ? "" : name + " ") + "t:" + typeof(T).Name);
            if (assets.Length > 0)
            {
                return (T)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(T));
            }

            return null;
        }

        private static void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            RewriteStartScene();
        }

        private static string GetMainMenuSceneName()
        {
            // Looking for the ProjectSettings asset in the project
            var guids = AssetDatabase.FindAssets($"t:{typeof(ProjectSettings).Name}");
            ProjectSettings projectSettings = null;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ProjectSettings>(path);
                if (asset != null)
                {
                    projectSettings = asset;
                }
            }

            if (projectSettings != null && SceneExistsInAssets(projectSettings.MainMenuSceneName))
            {
                // If the scene specified in ProjectSettings exists, return it
                return projectSettings.MainMenuSceneName;
            }
            else if (SceneExistsInAssets("Main Menu"))
            {
                // If the default "Main Menu" scene exists, return it
                return "Main Menu";
            }
            else
            {
                // If no valid scene is found, return null
                return null;
            }
        }

        private static bool SceneExistsInAssets(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            return guids.Any(guid =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return System.IO.Path.GetFileNameWithoutExtension(path) == sceneName;
            });
        }
    }
}