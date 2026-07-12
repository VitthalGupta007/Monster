#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VXMonster.EditorTools
{
    public static class VXVisualRebrandPipeline
    {
        const string WizardFolder = "Assets/Common/Sprites/Characters/Wizard";
        const string WizardAnimFolder = "Assets/Common/Animations/Characters/Wizard";
        const string CharIconFolder = "Assets/Common/Sprites/UI/Char Select/Characters";
        const string CharactersFolder = "Assets/Common/Scriptables/Characters";
        const string ArtSourcesPath = "Docs/ArtSources.md";

        static readonly (string sheet, int frames)[] WizardSheets =
        {
            ("wizard_idle.png", 4),
            ("wizard_walk_new.png", 5),
            ("wizard_defeat.png", 5),
            ("wizard_revive.png", 5),
        };

        static readonly (string clip, string sheet)[] WizardClipBindings =
        {
            ("Wizard Idle.anim", "wizard_idle.png"),
            ("Wizard Run.anim", "wizard_walk_new.png"),
            ("Wizard Defeat.anim", "wizard_defeat.png"),
            ("Wizard Revive.anim", "wizard_revive.png"),
        };

        [MenuItem("VX Monster/Rebrand/Import/Apply Wizard Sprite Settings Only")]
        public static void ApplyWizardSpriteSettingsOnly()
        {
            foreach (var (sheet, _) in WizardSheets)
                ApplyWizardSheetSettings($"{WizardFolder}/{sheet}");
            AssetDatabase.SaveAssets();
            Debug.Log("[VX Rebrand] Wizard sprite import settings applied.");
        }

        [MenuItem("VX Monster/Rebrand/Import/Apply Char Select Icon Settings")]
        public static void ApplyCharSelectIconSettingsMenu()
        {
            if (!Directory.Exists(CharIconFolder))
            {
                Debug.LogError("[VX Rebrand] Char icon folder missing.");
                return;
            }

            foreach (var path in Directory.GetFiles(CharIconFolder, "ui_char_*.png"))
            {
                var assetPath = path.Replace('\\', '/');
                ApplyCharSelectIconSettings(assetPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[VX Rebrand] Char select icon import settings applied.");
        }

        [MenuItem("VX Monster/Rebrand/Wire/Character Select Icons")]
        public static void WireCharacterSelectIcons()
        {
            var mappings = new (string assetFile, string iconFile)[]
            {
                ("WIZARD Character Data.asset", "ui_char_wizard.png"),
                ("MAGE Character Data.asset", "ui_char_mage.png"),
                ("ROGUE Character Data.asset", "ui_char_rogue.png"),
                ("PALADIN Character Data.asset", "ui_char_paladin.png"),
                ("NECROMANCER Character Data.asset", "ui_char_necromancer.png"),
                ("RANGER Character Data.asset", "ui_char_ranger.png"),
                ("BERSERKER Character Data.asset", "ui_char_berserker.png"),
                ("SAGE Character Data.asset", "ui_char_sage.png"),
            };

            foreach (var (assetFile, iconFile) in mappings)
            {
                var dataPath = $"{CharactersFolder}/{assetFile}";
                var iconPath = $"{CharIconFolder}/{iconFile}";
                var data = AssetDatabase.LoadAssetAtPath<VXMonster.Core.CharacterDataSO>(dataPath);
                var icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (data == null || icon == null)
                {
                    Debug.LogWarning($"[VX Rebrand] Wire skip: data={dataPath} icon={iconPath}");
                    continue;
                }

                var so = new SerializedObject(data);
                so.FindProperty("icon").objectReferenceValue = icon;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(data);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[VX Rebrand] Character select icons wired.");
        }

        [MenuItem("VX Monster/Rebrand/Restore Production Quality Baseline")]
        public static void RestoreProductionQualityBaseline()
        {
            ApplyWizardSpriteSettingsOnly();
            ApplyCharSelectIconSettingsMenu();
            foreach (var (clip, sheet) in WizardClipBindings)
                RemapClipSprites($"{WizardAnimFolder}/{clip}", $"{WizardFolder}/{sheet}");

            AssetDatabase.SaveAssets();
            Debug.Log("[VX Rebrand] Production quality baseline restored.");
        }

        [MenuItem("VX Monster/Rebrand/Import/Remap Wizard Animation Clips")]
        public static void RemapWizardAnimationClips()
        {
            foreach (var (clip, sheet) in WizardClipBindings)
                RemapClipSprites($"{WizardAnimFolder}/{clip}", $"{WizardFolder}/{sheet}");
            AssetDatabase.SaveAssets();
            Debug.Log("[VX Rebrand] Wizard animation clips remapped.");
        }

        public static void LogArtSource(string packName, string sourceUrlOrPath, string license)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            var path = Path.Combine(projectRoot, ArtSourcesPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            if (!File.Exists(path))
            {
                File.WriteAllText(path,
                    "# VX Monster — Art Sources\n\n| Pack | License | URL/Path | Date |\n|------|---------|----------|------|\n");
            }

            File.AppendAllText(path, $"| {packName} | {license} | {sourceUrlOrPath} | {DateTime.UtcNow:yyyy-MM-dd} |\n");
            Debug.Log($"[VX Rebrand] Logged art source: {packName}");
        }

        public static (int width, int height) ReadPngSize(string absolutePath)
        {
            using var fs = File.OpenRead(absolutePath);
            using var br = new BinaryReader(fs);
            br.ReadBytes(8);
            if (ReadBigEndianInt32(br) != 13) throw new InvalidDataException("Invalid PNG IHDR");
            br.ReadBytes(4);
            var w = ReadBigEndianInt32(br);
            var h = ReadBigEndianInt32(br);
            return (w, h);
        }

        public static (int width, int height) ReadPngSizeAtAssetPath(string assetPath)
        {
            var full = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath);
            return ReadPngSize(full);
        }

        static int ReadBigEndianInt32(BinaryReader br)
        {
            var b = br.ReadBytes(4);
            return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
        }

        public static void ApplyWizardSheetSettings(string assetPath)
        {
            if (!File.Exists(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath))) return;

            var (w, h) = ReadPngSizeAtAssetPath(assetPath);
            if (h <= 0) return;
            var frameWidth = h;
            var frameCount = w / frameWidth;
            if (frameCount <= 0) return;

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.spritePixelsToUnits = 500;
            importer.mipmapEnabled = false;

            var metas = new List<SpriteMetaData>();
            for (var i = 0; i < frameCount; i++)
            {
                metas.Add(new SpriteMetaData
                {
                    name = $"{Path.GetFileNameWithoutExtension(assetPath)}_{i}",
                    rect = new Rect(i * frameWidth, 0, frameWidth, frameWidth),
                    alignment = (int)SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0f),
                });
            }

            importer.spritesheet = metas.ToArray();
            importer.SaveAndReimport();
        }

        static void ApplyCharSelectIconSettings(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.spritePixelsToUnits = 100;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        public static void RemapClipSprites(string clipAssetPath, string textureAssetPath)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            if (clip == null)
            {
                Debug.LogWarning($"[VX Rebrand] Missing clip {clipAssetPath}");
                return;
            }

            var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(textureAssetPath)
                .OfType<Sprite>()
                .OrderBy(s => s.rect.x)
                .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogWarning($"[VX Rebrand] No sprites at {textureAssetPath}");
                return;
            }

            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName != "m_Sprite") continue;
                var curve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                if (curve == null || curve.Length == 0) continue;

                for (var i = 0; i < curve.Length; i++)
                    curve[i].value = sprites[Mathf.Min(i, sprites.Length - 1)];

                AnimationUtility.SetObjectReferenceCurve(clip, binding, curve);
            }

            EditorUtility.SetDirty(clip);
        }
    }
}
#endif
