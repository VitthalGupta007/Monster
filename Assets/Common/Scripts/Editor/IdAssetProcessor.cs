using UnityEngine;
using UnityEditor;
using System.Diagnostics.CodeAnalysis;

namespace VXMonster.Core
{

    public class IdAssetProcessor : AssetModificationProcessor
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
                    TryGenerateId(scriptable);
                }
            };
        }

        private static void TryGenerateId(ScriptableObject scriptable)
        {
            SerializedObject serializedObject = new SerializedObject(scriptable);
            SerializedProperty property = serializedObject.GetIterator();

            var assigned = false;
            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.Generic && property.type == typeof(Id).Name)
                {
                    property.FindPropertyRelative("value").stringValue = System.Guid.NewGuid().ToString();
                    assigned = true;
                }
            }

            if (assigned)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(scriptable);
            }
        }
    }
}