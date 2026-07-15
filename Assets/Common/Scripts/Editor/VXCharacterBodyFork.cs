#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VXMonster.Core;

namespace VXMonster.EditorTools
{
    /// <summary>
    /// Forks unique combat prefab + sheets + animator per CharacterDataSO that still shares a template body.
    /// Idempotent: skips entries that already own a unique {Name}.prefab.
    /// </summary>
    public static class VXCharacterBodyFork
    {
        const string PrefabFolder = "Assets/Common/Prefabs/Characters";
        const string SpriteRoot = "Assets/Common/Sprites/Characters";
        const string AnimRoot = "Assets/Common/Animations/Characters";
        const string CharactersFolder = "Assets/Common/Scriptables/Characters";
        const string CharactersDbPath = "Assets/Common/Scriptables/Characters Database.asset";

        static readonly string[] SheetNames =
        {
            "wizard_idle.png",
            "wizard_walk_new.png",
            "wizard_defeat.png",
            "wizard_revive.png",
        };

        static readonly (string clipSuffix, string sheet)[] ClipMap =
        {
            ("Idle.anim", "wizard_idle.png"),
            ("Run.anim", "wizard_walk_new.png"),
            ("Defeat.anim", "wizard_defeat.png"),
            ("Revive.anim", "wizard_revive.png"),
        };

        static readonly Dictionary<string, (string spriteFolder, string animFolder, Color tint, float strength)> Specs =
            new Dictionary<string, (string, string, Color, float)>(StringComparer.OrdinalIgnoreCase)
            {
                { "ROGUE", ("Wizard", "Wizard", new Color(0.25f, 0.55f, 0.28f), 0.55f) },
                { "NECROMANCER", ("Wizard", "Wizard", new Color(0.45f, 0.22f, 0.55f), 0.55f) },
                { "RANGER", ("Wizard", "Wizard", new Color(0.40f, 0.52f, 0.22f), 0.50f) },
                { "SAGE", ("Wizard", "Wizard", new Color(0.22f, 0.55f, 0.62f), 0.50f) },
                { "PALADIN", ("Wizard Crimson", "Wizard Crimson", new Color(0.85f, 0.78f, 0.45f), 0.45f) },
                { "BERSERKER", ("Wizard Crimson", "Wizard Crimson", new Color(0.75f, 0.28f, 0.18f), 0.55f) },
            };

        [MenuItem("VX Monster/Rebrand/Characters/Fork Unique Combat Bodies")]
        public static void ForkAllSharedBodies()
        {
            var assets = LoadDatabaseEntries();
            if (assets.Count == 0)
            {
                Debug.LogError("[VX BodyFork] No character data found.");
                return;
            }

            var prefabOwners = new Dictionary<string, string>();
            var forked = 0;
            var skipped = 0;

            foreach (var data in assets)
            {
                if (data.Prefab == null)
                {
                    Debug.LogWarning($"[VX BodyFork] {data.Name}: null prefab, skip.");
                    continue;
                }

                var prefabPath = AssetDatabase.GetAssetPath(data.Prefab);
                var prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
                var entryName = data.Name;
                var title = ToTitleCase(entryName);
                var expectedUnique = $"{PrefabFolder}/{title}.prefab";

                // Already forked?
                if (File.Exists(ToFull(expectedUnique)))
                {
                    var uniquePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(expectedUnique);
                    if (uniquePrefab != null && data.Prefab != uniquePrefab)
                    {
                        BindPrefab(data, uniquePrefab);
                    }
                    skipped++;
                    prefabOwners[AssetDatabase.AssetPathToGUID(expectedUnique)] = entryName;
                    Debug.Log($"[VX BodyFork] {entryName}: already has {title}.prefab — bound.");
                    continue;
                }

                if (prefabOwners.TryGetValue(prefabGuid, out var owner))
                {
                    if (!Specs.TryGetValue(entryName, out var spec))
                    {
                        Debug.LogError($"[VX BodyFork] {entryName} shares prefab with {owner} but has no fork spec.");
                        continue;
                    }

                    try
                    {
                        if (ForkEntry(data, spec))
                            forked++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[VX BodyFork] {entryName} failed: {ex}");
                    }
                }
                else
                {
                    prefabOwners[prefabGuid] = entryName;
                    Debug.Log($"[VX BodyFork] {entryName} owns template {Path.GetFileName(prefabPath)}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[VX BodyFork] Done. Forked={forked} skipped/bound={skipped}");
        }

        static List<CharacterDataSO> LoadDatabaseEntries()
        {
            var assets = new List<CharacterDataSO>();
            var db = AssetDatabase.LoadAssetAtPath<CharactersDatabase>(CharactersDbPath);
            if (db != null)
            {
                var soDb = new SerializedObject(db);
                var list = soDb.FindProperty("characterDatas");
                if (list != null && list.isArray)
                {
                    for (var i = 0; i < list.arraySize; i++)
                    {
                        var d = list.GetArrayElementAtIndex(i).objectReferenceValue as CharacterDataSO;
                        if (d != null) assets.Add(d);
                    }
                }
            }

            if (assets.Count == 0)
            {
                assets = Directory.GetFiles(CharactersFolder, "* Character Data.asset")
                    .Select(p => p.Replace('\\', '/'))
                    .Select(AssetDatabase.LoadAssetAtPath<CharacterDataSO>)
                    .Where(d => d != null)
                    .ToList();
            }

            return assets;
        }

        static bool ForkEntry(CharacterDataSO data, (string spriteFolder, string animFolder, Color tint, float strength) spec)
        {
            var entryName = data.Name;
            var title = ToTitleCase(entryName);
            var spriteFolder = $"{SpriteRoot}/{title}";
            var animFolder = $"{AnimRoot}/{title}";
            var prefabPath = $"{PrefabFolder}/{title}.prefab";
            var srcSprite = $"{SpriteRoot}/{spec.spriteFolder}";
            var srcAnim = $"{AnimRoot}/{spec.animFolder}";

            EnsureFolder(spriteFolder);
            EnsureFolder(animFolder);

            // 1) Copy sheets as raw files (no import yet)
            foreach (var sheet in SheetNames)
            {
                var src = ToFull($"{srcSprite}/{sheet}");
                var dst = ToFull($"{spriteFolder}/{sheet}");
                if (!File.Exists(src))
                {
                    Debug.LogError($"[VX BodyFork] Missing source sheet {src}");
                    return false;
                }

                File.Copy(src, dst, overwrite: true);
                var meta = dst + ".meta";
                if (File.Exists(meta)) File.Delete(meta);
            }

            // 2) Tint PNGs on disk without Unity TextureImporter (avoids domain thrash)
            foreach (var sheet in SheetNames)
            {
                TintPngFile(ToFull($"{spriteFolder}/{sheet}"), spec.tint, spec.strength);
            }

            AssetDatabase.Refresh();

            // 3) Import settings once per sheet
            foreach (var sheet in SheetNames)
                VXVisualRebrandPipeline.ApplyWizardSheetSettings($"{spriteFolder}/{sheet}");

            // 4) Copy clips
            var clipPaths = new Dictionary<string, string>();
            foreach (var (clipSuffix, _) in ClipMap)
            {
                var srcClipName = ResolveSourceClipName(spec.animFolder, clipSuffix);
                var srcClip = $"{srcAnim}/{srcClipName}";
                var dstClip = $"{animFolder}/{title} {clipSuffix}";
                if (!File.Exists(ToFull(srcClip)))
                {
                    Debug.LogError($"[VX BodyFork] Missing clip {srcClip}");
                    return false;
                }

                if (File.Exists(ToFull(dstClip)))
                    AssetDatabase.DeleteAsset(dstClip);

                if (!AssetDatabase.CopyAsset(srcClip, dstClip))
                {
                    Debug.LogError($"[VX BodyFork] CopyAsset failed {srcClip} → {dstClip}");
                    return false;
                }

                clipPaths[clipSuffix] = dstClip;
            }

            foreach (var (clipSuffix, sheet) in ClipMap)
                VXVisualRebrandPipeline.RemapClipSprites(clipPaths[clipSuffix], $"{spriteFolder}/{sheet}");

            // 5) Controller
            var srcController = $"{srcAnim}/{ResolveSourceControllerName(spec.animFolder)}";
            var dstController = $"{animFolder}/{title} Controller.controller";
            if (File.Exists(ToFull(dstController)))
                AssetDatabase.DeleteAsset(dstController);

            if (!AssetDatabase.CopyAsset(srcController, dstController))
            {
                Debug.LogError($"[VX BodyFork] Controller copy failed {srcController}");
                return false;
            }

            RewireController(dstController, clipPaths);

            // 6) Prefab
            var templatePrefabPath = AssetDatabase.GetAssetPath(data.Prefab);
            if (File.Exists(ToFull(prefabPath)))
                AssetDatabase.DeleteAsset(prefabPath);

            if (!AssetDatabase.CopyAsset(templatePrefabPath, prefabPath))
            {
                Debug.LogError($"[VX BodyFork] Prefab copy failed {templatePrefabPath}");
                return false;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var animator = prefabRoot.GetComponentInChildren<Animator>(true);
                var sr = prefabRoot.GetComponentInChildren<SpriteRenderer>(true);
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(dstController);
                if (animator != null && controller != null)
                    animator.runtimeAnimatorController = controller;

                var idleSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath($"{spriteFolder}/wizard_idle.png")
                    .OfType<Sprite>().OrderBy(s => s.rect.x).ToArray();
                if (sr != null && idleSprites.Length > 0)
                    sr.sprite = idleSprites[0];

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            BindPrefab(data, newPrefab);
            Debug.Log($"[VX BodyFork] Forked {entryName} → {prefabPath}");
            return true;
        }

        static void BindPrefab(CharacterDataSO data, GameObject prefab)
        {
            var so = new SerializedObject(data);
            so.FindProperty("prefab").objectReferenceValue = prefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
        }

        static void TintPngFile(string absolutePath, Color tint, float strength)
        {
            if (!File.Exists(absolutePath)) return;

            var bytes = File.ReadAllBytes(absolutePath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
            {
                UnityEngine.Object.DestroyImmediate(tex);
                return;
            }

            var pixels = tex.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                if (p.a < 8) continue;
                var lum = (p.r + p.g + p.b) / 3f / 255f;
                if (lum < 0.12f || lum > 0.92f) continue;

                var c = new Color(p.r / 255f, p.g / 255f, p.b / 255f, p.a / 255f);
                var mixed = Color.Lerp(c, c * tint, strength);
                pixels[i] = (Color32)mixed;
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            File.WriteAllBytes(absolutePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static string ResolveSourceClipName(string animFolder, string clipSuffix) =>
            animFolder == "Wizard Crimson" ? $"Wizard Crimson {clipSuffix}" : $"Wizard {clipSuffix}";

        static string ResolveSourceControllerName(string animFolder) =>
            animFolder == "Wizard Crimson"
                ? "Wizard Crimson Controller.controller"
                : "Wizard Controller.controller";

        static void RewireController(string controllerPath, Dictionary<string, string> clipPaths)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) return;

            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    var motion = state.state.motion as AnimationClip;
                    if (motion == null) continue;

                    foreach (var kv in clipPaths)
                    {
                        var stem = Path.GetFileNameWithoutExtension(kv.Key);
                        if (!motion.name.EndsWith(stem, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(kv.Value);
                        if (newClip != null)
                            state.state.motion = newClip;
                    }
                }
            }

            EditorUtility.SetDirty(controller);
        }

        static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            var parts = assetFolder.Split('/');
            var cur = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        static string ToFull(string assetPath) =>
            Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath).Replace('\\', '/');

        static string ToTitleCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToUpperInvariant(name[0]) + name.Substring(1).ToLowerInvariant();
        }
    }
}
#endif
