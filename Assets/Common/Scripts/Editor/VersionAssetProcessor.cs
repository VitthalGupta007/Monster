using OctoberStudio.Upgrades;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    public class VersionAssetProcessor : AssetModificationProcessor
    {
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "Called by Unity via reflection")]
        private static void OnWillCreateAsset(string path)
        {
            if (!path.EndsWith(".asset"))
                return;

            EditorApplication.delayCall += () =>
            {
                var scriptable = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (scriptable != null)
                {
                    if (scriptable is UpgradeData)
                    {
                        AssignDataVersion(scriptable);
                    }
                }
            };
        }

        private static void AssignDataVersion(ScriptableObject scriptable)
        {
            var serializedObject = new SerializedObject(scriptable);

            var dataVersionProperty = serializedObject.FindProperty("dataVersion");

            if (dataVersionProperty == null)
            {
                Debug.LogError($"Scriptable Object {scriptable.name} doesn't have dataVersion field. Cannot assign a version to the asset.");
                return;
            }

            dataVersionProperty.intValue = 1;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(scriptable);

            AssetDatabase.SaveAssets();
        }
    }
}