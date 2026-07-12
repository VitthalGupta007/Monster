#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VXMonster.Core;

namespace VXMonster.EditorTools
{
    public static class VXRebrandGate
    {
        const string WizardFolder = "Assets/Common/Sprites/Characters/Wizard";
        const string CharIconFolder = "Assets/Common/Sprites/UI/Char Select/Characters";
        const string CharactersFolder = "Assets/Common/Scriptables/Characters";
        const string StageFolder = "Assets/Common/Scriptables/Stages";
        const string Stage2FieldGuid = "4784fb39006132f48ad76bf040e6db27";
        const string StockBushPropGuid = "2accda9df7a5e08479ce38f9b320104f";
        const string StockStonePropGuid = "1f9045912c762e44186e2735add53554";

        static readonly (string file, int minW, int maxW, int h)[] WizardDimensionRules =
        {
            ("wizard_idle.png", 2048, 2048, 512),
            ("wizard_walk_new.png", 2560, 2560, 512),
            ("wizard_defeat.png", 2048, 2560, 512),
            ("wizard_revive.png", 2048, 2560, 512),
        };

        static readonly (int stage, float hp, float dmg)[] StageTuneTargets =
        {
            (3, 1.12f, 1.05f),
            (4, 1.25f, 1.10f),
            (5, 1.40f, 1.15f),
            (6, 1.55f, 1.20f),
        };

        [MenuItem("VX Monster/Rebrand/Run All Audits")]
        public static void RunAllAudits()
        {
            var results = new List<string>();
            AuditWizardSheets(results);
            AuditCharIcons(results);
            AuditCharacterIconGuids(results);
            AuditStageFieldBindings(results);
            AuditStagePropBindings(results);
            AuditStageBalance(results);

            foreach (var line in results)
                Debug.Log(line);

            var failCount = results.Count(r => r.StartsWith("FAIL"));
            if (failCount == 0)
                Debug.Log("[VX Gate] All audits PASS.");
            else
                Debug.LogWarning($"[VX Gate] {failCount} audit(s) FAILED.");
        }

        [MenuItem("VX Monster/Rebrand/Audit Stage Balance")]
        public static void AuditStageBalanceMenu()
        {
            var results = new List<string>();
            AuditStageBalance(results);
            foreach (var line in results)
                Debug.Log(line);
        }

        static void AuditWizardSheets(List<string> results)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            foreach (var (file, minW, maxW, h) in WizardDimensionRules)
            {
                var path = $"{WizardFolder}/{file}";
                var full = Path.Combine(projectRoot, path);
                if (!File.Exists(full))
                {
                    results.Add($"FAIL wizard sheet missing: {path}");
                    continue;
                }

                try
                {
                    var (w, height) = VXVisualRebrandPipeline.ReadPngSizeAtAssetPath(path);
                    if (height != h || w < minW || w > maxW)
                        results.Add($"FAIL {file}: {w}x{height} (expected height {h}, width {minW}-{maxW})");
                    else
                        results.Add($"PASS {file}: {w}x{height}");
                }
                catch (System.Exception ex)
                {
                    results.Add($"FAIL {file}: {ex.Message}");
                }
            }
        }

        static void AuditCharIcons(List<string> results)
        {
            if (!Directory.Exists(CharIconFolder))
            {
                results.Add("FAIL char icon folder missing");
                return;
            }

            foreach (var file in Directory.GetFiles(CharIconFolder, "ui_char_*.png"))
            {
                var assetPath = file.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    results.Add($"FAIL {assetPath}: no importer");
                    continue;
                }

                if (importer.textureType != TextureImporterType.Sprite ||
                    importer.spriteImportMode != SpriteImportMode.Single)
                    results.Add($"FAIL {Path.GetFileName(assetPath)}: not Single Sprite");
                else
                    results.Add($"PASS {Path.GetFileName(assetPath)}: Single Sprite");
            }
        }

        static void AuditCharacterIconGuids(List<string> results)
        {
            var guids = new HashSet<string>();
            var dupes = new List<string>();

            foreach (var asset in Directory.GetFiles(CharactersFolder, "* Character Data.asset"))
            {
                var path = asset.Replace('\\', '/');
                var data = AssetDatabase.LoadAssetAtPath<VXMonster.Core.CharacterDataSO>(path);
                if (data?.Icon == null) continue;
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data.Icon));
                if (!guids.Add(guid))
                    dupes.Add($"{Path.GetFileName(path)} shares icon GUID {guid}");
            }

            if (dupes.Count == 0)
                results.Add("PASS character icon GUIDs (duplicates OK on stock baseline)");
            else
                results.Add($"INFO character icon duplicates: {string.Join(", ", dupes)}");
        }

        static void AuditStageFieldBindings(List<string> results)
        {
            for (var stageNum = 3; stageNum <= 6; stageNum++)
            {
                var stagePath = $"{StageFolder}/Stage {stageNum}.asset";
                var stageData = AssetDatabase.LoadAssetAtPath<StageData>(stagePath);
                if (stageData == null)
                {
                    results.Add($"FAIL missing {stagePath}");
                    continue;
                }

                var fieldPath = AssetDatabase.GetAssetPath(stageData.StageFieldData);
                var fieldGuid = AssetDatabase.AssetPathToGUID(fieldPath);

                if (fieldGuid == Stage2FieldGuid)
                    results.Add($"FAIL Stage {stageNum} still bound to Stage 2 field data");
                else if (fieldPath.Contains($"Stage {stageNum} Field Data"))
                    results.Add($"PASS Stage {stageNum} field → {fieldPath}");
                else
                    results.Add($"INFO Stage {stageNum} field → {fieldPath}");
            }
        }

        static void AuditStagePropBindings(List<string> results)
        {
            results.Add("--- Stage prop bindings (biome props 3–6) ---");

            for (var stageNum = 3; stageNum <= 6; stageNum++)
            {
                var stagePath = $"{StageFolder}/Stage {stageNum}.asset";
                var stageData = AssetDatabase.LoadAssetAtPath<StageData>(stagePath);
                if (stageData?.StageFieldData == null)
                {
                    results.Add($"FAIL Stage {stageNum}: missing field data");
                    continue;
                }

                var props = stageData.StageFieldData.PropChances;
                if (props == null || props.Count == 0)
                {
                    results.Add($"FAIL Stage {stageNum}: no propChances configured");
                    continue;
                }

                var usesStock = false;
                foreach (var prop in props)
                {
                    if (prop.Prefab == null)
                    {
                        results.Add($"FAIL Stage {stageNum}: null prop prefab");
                        continue;
                    }

                    var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prop.Prefab));
                    if (guid == StockBushPropGuid || guid == StockStonePropGuid)
                        usesStock = true;

                    results.Add($"INFO Stage {stageNum}: {prop.Prefab.name} chance={prop.Chance}% max={prop.MaxAmount}");
                }

                if (usesStock)
                    results.Add($"FAIL Stage {stageNum}: still uses stock Bush/Stone prop");
                else
                    results.Add($"PASS Stage {stageNum}: biome prop wired");
            }
        }

        static void AuditStageBalance(List<string> results)
        {
            results.Add("--- Stage balance (enemyHP / enemyDamage) ---");
            var prevHp = 0f;
            var prevDmg = 0f;

            for (var stageNum = 1; stageNum <= 6; stageNum++)
            {
                var stagePath = $"{StageFolder}/Stage {stageNum}.asset";
                var stageData = AssetDatabase.LoadAssetAtPath<StageData>(stagePath);
                if (stageData == null)
                {
                    results.Add($"FAIL Stage {stageNum}: missing asset");
                    continue;
                }

                var hp = stageData.EnemyHP;
                var dmg = stageData.EnemyDamage;
                results.Add($"INFO Stage {stageNum}: enemyHP={hp:0.##} enemyDamage={dmg:0.##}");

                if (stageNum > 1 && (hp < prevHp || dmg < prevDmg))
                    results.Add($"FAIL Stage {stageNum}: scaling regressed (prev HP={prevHp:0.##} DMG={prevDmg:0.##})");
                else if (stageNum > 1)
                    results.Add($"PASS Stage {stageNum}: monotonic scaling");

                if (stageNum <= 2 && (hp < 1f || dmg < 1f))
                    results.Add($"FAIL Stage {stageNum}: baseline below 1.0");
                else if (stageNum >= 3 && (hp < 1f || dmg < 1f))
                    results.Add($"FAIL Stage {stageNum}: multiplier below 1.0");

                foreach (var (targetStage, targetHp, targetDmg) in StageTuneTargets)
                {
                    if (targetStage != stageNum) continue;
                    if (Mathf.Abs(hp - targetHp) > 0.001f)
                        results.Add($"FAIL Stage {stageNum}: enemyHP {hp:0.##} ≠ target {targetHp:0.##}");
                    else
                        results.Add($"PASS Stage {stageNum}: enemyHP matches StageTune");
                    if (Mathf.Abs(dmg - targetDmg) > 0.001f)
                        results.Add($"FAIL Stage {stageNum}: enemyDamage {dmg:0.##} ≠ target {targetDmg:0.##}");
                    else
                        results.Add($"PASS Stage {stageNum}: enemyDamage matches StageTune");
                }

                prevHp = hp;
                prevDmg = dmg;
            }

            results.Add("--- Hero base stats (baseHP / baseDamage) ---");
            foreach (var asset in Directory.GetFiles(CharactersFolder, "* Character Data.asset").OrderBy(f => f))
            {
                var path = asset.Replace('\\', '/');
                var data = AssetDatabase.LoadAssetAtPath<VXMonster.Core.CharacterDataSO>(path);
                if (data == null)
                {
                    results.Add($"FAIL {Path.GetFileName(path)}: could not load");
                    continue;
                }

                var hp = data.BaseHP;
                var dmg = data.BaseDamage;
                results.Add($"INFO {data.name}: baseHP={hp:0.##} baseDamage={dmg:0.##}");

                if (hp < 1f || dmg <= 0f)
                    results.Add($"FAIL {data.name}: invalid base stats");
                else if (dmg < 1f)
                    results.Add($"INFO {data.name}: baseDamage {dmg:0.##} < 1 (tank tradeoff OK)");
                else
                    results.Add($"PASS {data.name}: base stats valid");

                if (data.name.StartsWith("WIZARD") && (Mathf.Abs(hp - 100f) > 0.001f || Mathf.Abs(dmg - 1f) > 0.001f))
                    results.Add($"INFO WIZARD: non-default baseline (HP={hp:0.##} DMG={dmg:0.##})");
            }
        }
    }
}
#endif
