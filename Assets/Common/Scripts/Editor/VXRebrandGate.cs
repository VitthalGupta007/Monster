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

        static readonly (string file, int minW, int maxW, int h)[] WizardDimensionRules =
        {
            ("wizard_idle.png", 2048, 2048, 512),
            ("wizard_walk_new.png", 2560, 2560, 512),
            ("wizard_defeat.png", 2048, 2560, 512),
            ("wizard_revive.png", 2048, 2560, 512),
        };

        [MenuItem("VX Monster/Rebrand/Run All Audits")]
        public static void RunAllAudits()
        {
            var results = new List<string>();
            AuditWizardSheets(results);
            AuditCharIcons(results);
            AuditCharacterIconGuids(results);
            AuditStageFieldBindings(results);

            foreach (var line in results)
                Debug.Log(line);

            var failCount = results.Count(r => r.StartsWith("FAIL"));
            if (failCount == 0)
                Debug.Log("[VX Gate] All audits PASS.");
            else
                Debug.LogWarning($"[VX Gate] {failCount} audit(s) FAILED.");
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
    }
}
#endif
