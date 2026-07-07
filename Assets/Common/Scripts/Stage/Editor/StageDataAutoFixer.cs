using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VXMonster.Core
{
    [InitializeOnLoad]
    public static class StageDataAutoFixer
    {
        static StageDataAutoFixer()
        {
            EditorApplication.delayCall += RunCheck;
        }

        private static void RunCheck()
        {
            var guids = AssetDatabase.FindAssets("t:StageData");
            
            var changed = false;

            var stages = new List<StageData>(guids.Length);

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                StageData stage = AssetDatabase.LoadAssetAtPath<StageData>(path);

                if (stage == null) continue;

                if (string.IsNullOrEmpty(stage.Id))
                {
                    AssignId(stage);
                    changed = true;
                }

                stages.Add(stage);
            }

            for (int i = 0; i < stages.Count - 1; i++)
            {
                for (int j = i + 1; j < stages.Count; j++)
                {
                    if (stages[i] == stages[j]) continue;

                    if (stages[i].Id == stages[j].Id)
                    {
                        AssignId(stages[j]);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[Stage Data] Missing IDs were auto-generated.");
            }
        }

        public static void AssignId(StageData stage)
        {
            var newId = System.Guid.NewGuid().ToString();

            var serializedObject = new SerializedObject(stage);
            var stageIdProperty = serializedObject.FindProperty("id");
            var valueProperty = stageIdProperty.FindPropertyRelative("value");

            valueProperty.stringValue = newId;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(stage);
        }
    }
}