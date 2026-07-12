#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VXMonster.Core;

namespace VXMonster.EditorTools
{
    public static class VXStagePropPipeline
    {
        const string PropFolder = "Assets/Common/Prefabs/Stage Elements/Prop";
        const string FieldFolder = "Assets/Common/Scriptables/Stages/Stage Field Data";
        const string BushTemplate = PropFolder + "/Prop Bush.prefab";
        const string StoneTemplate = PropFolder + "/Prop Stone.prefab";

        static readonly (int stage, string prefabName, string template, Color tint, int maxAmount, float chance)[] BiomeProps =
        {
            (3, "Prop Frost Crystal", StoneTemplate, new Color(0.72f, 0.92f, 1f), 3, 85f),
            (4, "Prop Ember Rock", StoneTemplate, new Color(1f, 0.52f, 0.22f), 2, 85f),
            (5, "Prop Marsh Growth", BushTemplate, new Color(0.48f, 0.88f, 0.42f), 4, 85f),
            (6, "Prop Void Crystal", StoneTemplate, new Color(0.62f, 0.42f, 0.98f), 3, 85f),
        };

        [MenuItem("VX Monster/Rebrand/Setup Biome Props 3-6")]
        public static void SetupBiomeProps3To6()
        {
            foreach (var config in BiomeProps)
            {
                var template = AssetDatabase.LoadAssetAtPath<GameObject>(config.template);
                if (template == null)
                {
                    Debug.LogError($"[VX Props] Missing template {config.template}");
                    continue;
                }

                var instance = Object.Instantiate(template);
                instance.name = config.prefabName;

                var spriteRenderer = instance.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                    spriteRenderer.color = config.tint;

                var prefabPath = $"{PropFolder}/{config.prefabName}.prefab";
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);

                var fieldPath = $"{FieldFolder}/Stage {config.stage} Field Data.asset";
                var fieldData = AssetDatabase.LoadAssetAtPath<StageFieldData>(fieldPath);
                if (fieldData == null)
                {
                    Debug.LogError($"[VX Props] Missing {fieldPath}");
                    continue;
                }

                var serializedField = new SerializedObject(fieldData);
                var propChances = serializedField.FindProperty("propChances");
                propChances.ClearArray();
                propChances.arraySize = 1;

                var entry = propChances.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("prefab").objectReferenceValue = prefab;
                entry.FindPropertyRelative("maxAmount").intValue = config.maxAmount;
                entry.FindPropertyRelative("chance").floatValue = config.chance;
                serializedField.ApplyModifiedPropertiesWithoutUndo();

                Debug.Log($"[VX Props] Stage {config.stage} → {prefabPath} ({config.chance}% chance, max {config.maxAmount})");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VX Props] Biome props 3–6 wired.");
        }
    }
}
#endif
