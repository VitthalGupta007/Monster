#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using VXMonster.Core;
using VXMonster.Core.Bossfight;
using VXMonster.Core.Timeline;
using VXMonster.Core.Timeline.Bossfight;
using VXMonster.Gameplay;

namespace VXMonster.EditorTools
{
    public static class VXContentPipeline
    {
        const string StageFolder = "Assets/Common/Scriptables/Stages";
        const string RelicsFolder = "Assets/Common/Scriptables/Relics";
        const string ResourcesRelicsFolder = "Assets/Resources/VX";
        const string CharactersFolder = "Assets/Common/Scriptables/Characters";
        const string CharactersDbPath = "Assets/Common/Scriptables/Characters Database.asset";
        const string StagePreviewFolder = "Assets/Common/Sprites/UI/Stage";

        const string CharIconFolder = "Assets/Common/Sprites/UI/Char Select/Characters";
        const string CharactersPrefabsFolder = "Assets/Common/Prefabs/Characters";

        [MenuItem("VX Monster/Content/Setup All (Stages+Relics+Characters)")]
        public static void SetupAll()
        {
            EditorUtility.DisplayDialog(
                "Setup All — Disabled",
                "This menu is disabled during the VX visual rebrand.\n\n" +
                "Use individual menus instead:\n" +
                "• VX Monster/Content/Bind Stage Field Data 3-6 Only\n" +
                "• VX Monster/Rebrand/Setup Biome Props 3-6\n" +
                "• VX Monster/Rebrand/Run All Audits\n\n" +
                "Re-enable only after Phase 9 visual gates complete.",
                "OK");
        }

        [MenuItem("VX Monster/Content/Bind Stage Field Data 3-6 Only")]
        public static void BindStageFieldData3To6Only()
        {
            for (var stageNum = 3; stageNum <= 6; stageNum++)
            {
                var stagePath = $"{StageFolder}/Stage {stageNum}.asset";
                var fieldPath = $"{StageFolder}/Stage Field Data/Stage {stageNum} Field Data.asset";
                var stage = AssetDatabase.LoadAssetAtPath<StageData>(stagePath);
                var field = AssetDatabase.LoadAssetAtPath<StageFieldData>(fieldPath);
                if (stage == null || field == null)
                {
                    Debug.LogWarning($"[VX] Bind field skip: stage={stagePath} field={fieldPath}");
                    continue;
                }

                var so = new SerializedObject(stage);
                so.FindProperty("stageFieldData").objectReferenceValue = field;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stage);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[VX] Stages 3-6 bound to their own field data assets.");
        }

        [MenuItem("VX Monster/Content/Setup Stages 3-6 Timelines")]
        public static void SetupStages3To6()
        {
            var stage2Playable = $"{StageFolder}/Stage 2.playable";

            var stageConfigs = new[]
            {
                new StageTune("Stage 3", "Stage 3.playable", 1.12f, 1.05f, "ui_stage_3_preview.png"),
                new StageTune("Stage 4", "Stage 4.playable", 1.25f, 1.10f, "ui_stage_4_preview.png"),
                new StageTune("Stage 5", "Stage 5.playable", 1.40f, 1.15f, "ui_stage_5_preview.png"),
                new StageTune("Stage 6", "Stage 6.playable", 1.55f, 1.20f, "ui_stage_6_preview.png"),
            };

            foreach (var config in stageConfigs)
            {
                var playablePath = $"{StageFolder}/{config.PlayableFile}";
                if (!File.Exists(playablePath))
                {
                    AssetDatabase.CopyAsset(stage2Playable, playablePath);
                }

                var previewPath = $"{StagePreviewFolder}/{config.PreviewFile}";
                // Never overwrite unique generated previews with Stage 2 copies.
                if (!File.Exists(previewPath))
                {
                    Debug.LogWarning($"[VX] Missing preview {previewPath}. Generate unique art before shipping.");
                }

                EnsurePreviewIsSprite(previewPath);

                var stagePath = $"{StageFolder}/{config.StageName}.asset";
                var stage = AssetDatabase.LoadAssetAtPath<StageData>(stagePath);
                if (stage == null) continue;

                var so = new SerializedObject(stage);
                var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(playablePath);
                var preview = AssetDatabase.LoadAssetAtPath<Sprite>(previewPath);
                if (timeline != null) so.FindProperty("timeline").objectReferenceValue = timeline;
                if (preview != null) so.FindProperty("icon").objectReferenceValue = preview;
                else Debug.LogWarning($"[VX] Could not load Sprite at {previewPath} for {config.StageName}.");
                so.FindProperty("enemyHP").floatValue = config.EnemyHp;
                so.FindProperty("enemyDamage").floatValue = config.EnemyDamage;

                var fieldPath = $"{StageFolder}/Stage Field Data/{config.StageName} Field Data.asset";
                var field = AssetDatabase.LoadAssetAtPath<StageFieldData>(fieldPath);
                if (field != null)
                    so.FindProperty("stageFieldData").objectReferenceValue = field;
                else
                    Debug.LogWarning($"[VX] Missing field data {fieldPath} for {config.StageName}.");

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stage);
            }

            Debug.Log("[VX] Stages 3-6 timelines and previews updated.");
        }

        [MenuItem("VX Monster/Content/Bind Stage Preview Icons")]
        public static void BindStagePreviewIcons()
        {
            var bindings = new (string stage, string preview)[]
            {
                ("Stage 1", "ui_stage_1_preview.png"),
                ("Stage 2", "ui_stage_2_preview.png"),
                ("Stage 3", "ui_stage_3_preview.png"),
                ("Stage 4", "ui_stage_4_preview.png"),
                ("Stage 5", "ui_stage_5_preview.png"),
                ("Stage 6", "ui_stage_6_preview.png"),
            };

            foreach (var binding in bindings)
            {
                var previewPath = $"{StagePreviewFolder}/{binding.preview}";
                EnsurePreviewIsSprite(previewPath);
                var stage = AssetDatabase.LoadAssetAtPath<StageData>($"{StageFolder}/{binding.stage}.asset");
                var preview = AssetDatabase.LoadAssetAtPath<Sprite>(previewPath);
                if (stage == null || preview == null)
                {
                    Debug.LogWarning($"[VX] Bind failed for {binding.stage} → {previewPath}");
                    continue;
                }

                var so = new SerializedObject(stage);
                so.FindProperty("icon").objectReferenceValue = preview;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stage);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[VX] Stage preview icons bound.");
        }

        [MenuItem("VX Monster/Content/Differentiate Stages 3-6 Enemy Mix")]
        public static void DifferentiateStages3To6EnemyMix()
        {
            ApplyGraduatedEnemyMix1To6();
        }

        [MenuItem("VX Monster/Content/Apply Graduated Enemy Mix 1-6")]
        public static void ApplyGraduatedEnemyMix1To6()
        {
            foreach (var mix in StageEnemyProgression.StageMixes)
            {
                ApplyStageEnemyMix(
                    $"Stage {mix.Stage}.playable",
                    mix.Enemies,
                    mix.Bosses);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[VX] Graduated enemy/boss mixes applied for Stages 1-6.");
        }

        [MenuItem("VX Monster/Content/Apply Stage XP Multipliers 1-6")]
        public static void ApplyStageXpMultipliers1To6()
        {
            foreach (var (stage, multiplier) in StageEnemyProgression.StageXpGainTargets)
            {
                var stagePath = $"{StageFolder}/Stage {stage}.asset";
                var stageData = AssetDatabase.LoadAssetAtPath<StageData>(stagePath);
                if (stageData == null)
                {
                    Debug.LogError($"[VX] Missing {stagePath}");
                    continue;
                }

                var so = new SerializedObject(stageData);
                var xpProp = so.FindProperty("xpGainMultiplier");
                if (xpProp == null)
                {
                    Debug.LogError("[VX] StageData missing xpGainMultiplier field — recompile scripts first.");
                    return;
                }

                xpProp.floatValue = multiplier;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stageData);
                Debug.Log($"[VX] Stage {stage} xpGainMultiplier → {multiplier:0.##}");
            }

            AssetDatabase.SaveAssets();
        }

        [MenuItem("VX Monster/Content/Apply Graduated Enemy Drop Tables")]
        public static void ApplyGraduatedEnemyDropTables()
        {
            const string dbPath = "Assets/Common/Scriptables/Enemies Database.asset";
            var db = AssetDatabase.LoadAssetAtPath<EnemiesDatabase>(dbPath);
            if (db == null)
            {
                Debug.LogError($"[VX] Missing {dbPath}");
                return;
            }

            var so = new SerializedObject(db);
            var enemiesProp = so.FindProperty("enemies");
            if (enemiesProp == null)
            {
                Debug.LogError("[VX] Enemies Database missing enemies list.");
                return;
            }

            for (var i = 0; i < enemiesProp.arraySize; i++)
            {
                var enemyProp = enemiesProp.GetArrayElementAtIndex(i);
                var typeProp = enemyProp.FindPropertyRelative("type");
                var dropProp = enemyProp.FindPropertyRelative("enemyDrop");
                if (typeProp == null || dropProp == null) continue;

                var type = (EnemyType)typeProp.intValue;
                var profile = StageEnemyProgression.GetGraduatedDropProfile(type);
                ApplyGraduatedDropProfile(dropProp, profile);
                Debug.Log($"[VX] {type} drop table → ~{profile.ExpectedGemXp:0.#} gem XP/kill (min stage {StageEnemyProgression.GetMinStageForEnemy(type)})");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            Debug.Log("[VX] Graduated enemy drop tables applied to Enemies Database.");
        }

        static void ApplyGraduatedDropProfile(SerializedProperty dropList, StageEnemyProgression.GraduatedDropProfile profile)
        {
            dropList.ClearArray();
            var entries = profile.ToEntries();
            for (var i = 0; i < entries.Length; i++)
            {
                dropList.InsertArrayElementAtIndex(i);
                var elem = dropList.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("dropType").intValue = (int)entries[i].type;
                elem.FindPropertyRelative("chance").floatValue = entries[i].chance;
            }
        }

        [MenuItem("VX Monster/Content/Differentiate Stages 3-6 Enemy Mix", true)]
        [MenuItem("VX Monster/Content/Apply Graduated Enemy Mix 1-6", true)]
        [MenuItem("VX Monster/Content/Apply Graduated Enemy Drop Tables", true)]
        static bool ValidateGraduatedEnemyMixMenu()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        static void ApplyStageEnemyMix(string playableFile, EnemyType[] enemyCycle, BossType[] bossCycle)
        {
            var path = $"{StageFolder}/{playableFile}";
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
            if (timeline == null)
            {
                Debug.LogError($"[VX] Missing timeline {path}");
                return;
            }

            var enemyIndex = 0;
            var bossIndex = 0;

            foreach (var track in timeline.GetOutputTracks())
            {
                if (track is WaveTrack waveTrack)
                {
                    var so = new SerializedObject(waveTrack);
                    var enemyProp = so.FindProperty("enemyType");
                    if (enemyProp != null && enemyCycle.Length > 0)
                    {
                        enemyProp.enumValueIndex = EnemyTypeToEnumIndex(enemyCycle[enemyIndex % enemyCycle.Length]);
                        enemyIndex++;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(waveTrack);
                    }
                }

                foreach (var clip in track.GetClips())
                {
                    if (clip.asset is Boss bossAsset)
                    {
                        var so = new SerializedObject(bossAsset);
                        var bossProp = so.FindProperty("bossType");
                        if (bossProp != null && bossCycle.Length > 0)
                        {
                            bossProp.enumValueIndex = (int)bossCycle[bossIndex % bossCycle.Length];
                            bossIndex++;
                            so.ApplyModifiedPropertiesWithoutUndo();
                            EditorUtility.SetDirty(bossAsset);
                        }
                    }
                }
            }

            EditorUtility.SetDirty(timeline);
        }

        static int EnemyTypeToEnumIndex(EnemyType type)
        {
            // EnemyType has gaps (5→8). SerializedProperty.enumValueIndex is declaration order, not raw int.
            var values = (EnemyType[])System.Enum.GetValues(typeof(EnemyType));
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].Equals(type)) return i;
            }

            return 0;
        }

        static void EnsurePreviewIsSprite(string previewPath)
        {
            if (!File.Exists(previewPath)) return;
            var importer = AssetImporter.GetAtPath(previewPath) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        [MenuItem("VX Monster/Content/Setup Relics Database")]
        public static void SetupRelicsDatabase()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/VX")) AssetDatabase.CreateFolder("Assets/Resources", "VX");
            if (!AssetDatabase.IsValidFolder("Assets/Common/Scriptables/Relics")) AssetDatabase.CreateFolder("Assets/Common/Scriptables", "Relics");

            var dbPath = $"{ResourcesRelicsFolder}/Relics Database.asset";
            var db = AssetDatabase.LoadAssetAtPath<RelicsDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<RelicsDatabase>();
                if (db == null)
                {
                    Debug.LogError("[VX] Failed to create RelicsDatabase. Ensure RelicsDatabase.cs compiles (class name must match file name).");
                    return;
                }

                AssetDatabase.CreateAsset(db, dbPath);
            }

            var relicSpecs = new (string id, string name, string desc, RelicEffectType effect, float mag, string iconPath)[]
            {
                ("vx_core", "VX Core", "Gain one extra passive slot.", RelicEffectType.ExtraPassiveSlot, 1f, "Assets/Common/Sprites/UI/Abilities/ui_ab_star.png"),
                ("glass_soul", "Glass Soul", "More damage, less health.", RelicEffectType.DamageBoost, 0.4f, "Assets/Common/Sprites/UI/Abilities/ui_ab_hp.png"),
                ("magnet_heart", "Magnet Heart", "Huge pickup radius.", RelicEffectType.PickupRadius, 2f, "Assets/Common/Sprites/UI/Abilities/ui_ab_magnet.png"),
                ("combo_lens", "Combo Lens", "Element combos hit harder.", RelicEffectType.ComboAmplifier, 0.25f, "Assets/Common/Sprites/UI/Abilities/ui_ab_lightning.png"),
                ("boss_hunter", "Boss Hunter", "Bonus damage against bosses.", RelicEffectType.BossDamage, 0.5f, "Assets/Common/Sprites/UI/Abilities/ui_ab_sword.png"),
                ("time_shard", "Time Shard", "Reduced ability cooldowns.", RelicEffectType.CooldownReduction, 0.1f, "Assets/Common/Sprites/UI/Abilities/ui_ab_hourglass.png"),
                ("lucky_coin", "Lucky Coin", "More gold from runs.", RelicEffectType.GoldBoost, 0.25f, "Assets/Common/Sprites/UI/Generic/ui_coin_spin.png"),
                ("phoenix_ash", "Phoenix Ash", "Revive once per run at low HP.", RelicEffectType.PhoenixRevive, 1f, "Assets/Common/Sprites/UI/Abilities/ui_ab_heal.png"),
                ("reroll_token", "Reroll Token", "Extra ability reroll.", RelicEffectType.BonusReroll, 1f, "Assets/Common/Sprites/UI/Abilities/ui_ab_wand.png"),
            };

            var relics = new List<RelicData>();
            foreach (var spec in relicSpecs)
            {
                var assetPath = $"{RelicsFolder}/{spec.name.Replace(" ", "")}.asset";
                var relic = AssetDatabase.LoadAssetAtPath<RelicData>(assetPath);
                if (relic == null)
                {
                    relic = ScriptableObject.CreateInstance<RelicData>();
                    if (relic == null)
                    {
                        Debug.LogError("[VX] Failed to create RelicData. Ensure RelicData.cs compiles.");
                        return;
                    }

                    AssetDatabase.CreateAsset(relic, assetPath);
                }

                var icon = AssetDatabase.LoadAssetAtPath<Sprite>(spec.iconPath);
                relic.Configure(spec.id, spec.name, spec.desc, spec.effect, spec.mag, icon);
                EditorUtility.SetDirty(relic);
                relics.Add(relic);
            }

            var dbSo = new SerializedObject(db);
            var list = dbSo.FindProperty("relics");
            list.ClearArray();
            for (var i = 0; i < relics.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                list.GetArrayElementAtIndex(i).objectReferenceValue = relics[i];
            }

            dbSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(db);
            Debug.Log("[VX] Relics Database created with icons.");
        }

        [MenuItem("VX Monster/Content/Setup Character Roster (8)")]
        public static void SetupCharacterRoster()
        {
            var wizard = AssetDatabase.LoadAssetAtPath<CharacterDataSO>($"{CharactersFolder}/WIZARD Character Data.asset");
            var mage = AssetDatabase.LoadAssetAtPath<CharacterDataSO>($"{CharactersFolder}/MAGE Character Data.asset");
            if (wizard == null || mage == null)
            {
                Debug.LogError("[VX] WIZARD/MAGE character data missing.");
                return;
            }

            var specs = new (string file, string name, int price, float hp, float dmg, bool startAbility, AbilityType ability, bool useMage)[]
            {
                ("ROGUE Character Data", "ROGUE", 800, 90f, 1.15f, true, AbilityType.FlyingDagger, false),
                ("PALADIN Character Data", "PALADIN", 1000, 140f, 0.95f, true, AbilityType.GuardianEye, true),
                ("NECROMANCER Character Data", "NECROMANCER", 1200, 100f, 1.2f, true, AbilityType.MagicRune, false),
                ("RANGER Character Data", "RANGER", 1400, 105f, 1.1f, true, AbilityType.Boomerang, false),
                ("BERSERKER Character Data", "BERSERKER", 1600, 85f, 1.35f, true, AbilityType.RollingStone, true),
                ("SAGE Character Data", "SAGE", 1800, 110f, 1.05f, true, AbilityType.SolarMagnifier, false),
            };

            var db = AssetDatabase.LoadAssetAtPath<CharactersDatabase>(CharactersDbPath);
            if (db == null)
            {
                Debug.LogError("[VX] Characters Database missing.");
                return;
            }

            var dbSo = new SerializedObject(db);
            var list = dbSo.FindProperty("characterDatas");

            void UpsertInDatabase(CharacterDataSO data)
            {
                for (var i = 0; i < list.arraySize; i++)
                {
                    var elem = list.GetArrayElementAtIndex(i).objectReferenceValue as CharacterDataSO;
                    if (elem != data) continue;
                    return;
                }

                var idx = list.arraySize;
                list.InsertArrayElementAtIndex(idx);
                list.GetArrayElementAtIndex(idx).objectReferenceValue = data;
            }

            UpsertInDatabase(wizard);
            UpsertInDatabase(mage);

            foreach (var spec in specs)
            {
                var path = $"{CharactersFolder}/{spec.file}.asset";
                var data = AssetDatabase.LoadAssetAtPath<CharacterDataSO>(path);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<CharacterDataSO>();
                    AssetDatabase.CreateAsset(data, path);
                }

                var template = spec.useMage ? mage : wizard;
                var iconPath = $"{CharIconFolder}/ui_char_{spec.name.ToLowerInvariant()}.png";
                var icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (icon == null) icon = template.Icon;

                var prefabPath = $"{CharactersPrefabsFolder}/{spec.name}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) prefab = template.Prefab;

                var so = new SerializedObject(data);
                var idProp = so.FindProperty("id");
                var idValue = idProp?.FindPropertyRelative("value")?.stringValue;
                if (idProp != null && string.IsNullOrEmpty(idValue))
                    idProp.FindPropertyRelative("value").stringValue = System.Guid.NewGuid().ToString();

                so.FindProperty("characterName").stringValue = spec.name;
                so.FindProperty("price").FindPropertyRelative("currencyId").stringValue = "gold";
                so.FindProperty("price").FindPropertyRelative("amount").intValue = spec.price;
                so.FindProperty("icon").objectReferenceValue = icon;
                so.FindProperty("prefab").objectReferenceValue = prefab;
                so.FindProperty("hasStartingAbility").boolValue = spec.startAbility;
                so.FindProperty("startingAbility").intValue = (int)spec.ability;
                so.FindProperty("baseHP").floatValue = spec.hp;
                so.FindProperty("baseDamage").floatValue = spec.dmg;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(data);
                UpsertInDatabase(data);
            }

            dbSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(db);
            Debug.Log("[VX] Character roster expanded to 8.");
        }

        readonly struct StageTune
        {
            public StageTune(string stageName, string playableFile, float enemyHp, float enemyDamage, string previewFile)
            {
                StageName = stageName;
                PlayableFile = playableFile;
                EnemyHp = enemyHp;
                EnemyDamage = enemyDamage;
                PreviewFile = previewFile;
            }

            public string StageName { get; }
            public string PlayableFile { get; }
            public float EnemyHp { get; }
            public float EnemyDamage { get; }
            public string PreviewFile { get; }
        }
    }
}
#endif
