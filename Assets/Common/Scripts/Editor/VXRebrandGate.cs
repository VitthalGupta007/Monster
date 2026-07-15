#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using VXMonster.Core;
using VXMonster.Core.Timeline;
using VXMonster.Core.Timeline.Bossfight;
using VXMonster.Gameplay;

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
            AuditCharacterPrefabUniqueness(results);
            AuditStageFieldBindings(results);
            AuditStagePropBindings(results);
            AuditStageBalance(results);
            AuditStageEnemyMix(results);
            AuditStageEnemyProgression(results);
            AuditStageExperienceYield(results);
            AuditEnemyDropTables(results);
            AuditStagePowerupPools(results);

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

        [MenuItem("VX Monster/Rebrand/Audit Stage Enemy Mix")]
        public static void AuditStageEnemyMixMenu()
        {
            var results = new List<string>();
            AuditStageEnemyMix(results);
            foreach (var line in results)
                Debug.Log(line);
        }

        [MenuItem("VX Monster/Rebrand/Audit Stage Enemy Progression")]
        public static void AuditStageEnemyProgressionMenu()
        {
            var results = new List<string>();
            AuditStageEnemyProgression(results);
            foreach (var line in results)
                Debug.Log(line);
        }

        [MenuItem("VX Monster/Rebrand/Audit Stage Experience Yield")]
        public static void AuditStageExperienceYieldMenu()
        {
            var results = new List<string>();
            AuditStageExperienceYield(results);
            foreach (var line in results)
                Debug.Log(line);
        }

        [MenuItem("VX Monster/Rebrand/Audit Enemy Drop Tables")]
        public static void AuditEnemyDropTablesMenu()
        {
            var results = new List<string>();
            AuditEnemyDropTables(results);
            foreach (var line in results)
                Debug.Log(line);
        }

        [MenuItem("VX Monster/Rebrand/Audit Stage Powerup Pools")]
        public static void AuditStagePowerupPoolsMenu()
        {
            var results = new List<string>();
            AuditStagePowerupPools(results);
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

        static void AuditCharacterPrefabUniqueness(List<string> results)
        {
            results.Add("--- Character combat prefab uniqueness ---");
            var byGuid = new Dictionary<string, List<string>>();

            foreach (var asset in Directory.GetFiles(CharactersFolder, "* Character Data.asset"))
            {
                var path = asset.Replace('\\', '/');
                var data = AssetDatabase.LoadAssetAtPath<VXMonster.Core.CharacterDataSO>(path);
                if (data == null) continue;

                if (data.Prefab == null)
                {
                    results.Add($"FAIL {Path.GetFileName(path)}: null prefab");
                    continue;
                }

                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data.Prefab));
                if (!byGuid.TryGetValue(guid, out var owners))
                {
                    owners = new List<string>();
                    byGuid[guid] = owners;
                }

                owners.Add(data.Name);
            }

            var shared = false;
            foreach (var kv in byGuid)
            {
                if (kv.Value.Count == 1)
                {
                    results.Add($"PASS {kv.Value[0]} → unique prefab");
                    continue;
                }

                shared = true;
                results.Add($"FAIL shared prefab GUID {kv.Key}: {string.Join(", ", kv.Value)}");
            }

            if (!shared && byGuid.Count > 0)
                results.Add($"PASS all {byGuid.Count} character entries have unique combat prefabs");
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

        static void AuditStageEnemyMix(List<string> results)
        {
            results.Add("--- Stage enemy mix (timeline WaveTracks) ---");

            var stage2Fingerprint = GetStageEnemyMixFingerprint($"{StageFolder}/Stage 2.playable");
            if (stage2Fingerprint == null)
                results.Add("FAIL Stage 2: missing or unreadable timeline");
            else
                results.Add($"INFO Stage 2 mix: {stage2Fingerprint.Summary}");

            for (var stageNum = 3; stageNum <= 6; stageNum++)
            {
                var path = $"{StageFolder}/Stage {stageNum}.playable";
                var fingerprint = GetStageEnemyMixFingerprint(path);
                if (fingerprint == null)
                {
                    results.Add($"FAIL Stage {stageNum}: missing or unreadable timeline");
                    continue;
                }

                var waveCount = fingerprint.WaveCount;
                var bossCount = fingerprint.BossCount;
                results.Add($"INFO Stage {stageNum} mix: {fingerprint.Summary} (waves={waveCount}, bosses={bossCount})");

                if (waveCount < 8)
                    results.Add($"FAIL Stage {stageNum}: fewer than 8 wave tracks ({waveCount})");
                else
                    results.Add($"PASS Stage {stageNum}: wave track count");

                if (bossCount < 1)
                    results.Add($"FAIL Stage {stageNum}: no boss clips configured");
                else
                    results.Add($"PASS Stage {stageNum}: boss clips present");

                if (stage2Fingerprint != null && fingerprint.EnemyKey == stage2Fingerprint.EnemyKey)
                    results.Add($"FAIL Stage {stageNum}: enemy mix still matches Stage 2 clone");
                else
                    results.Add($"PASS Stage {stageNum}: enemy mix differentiated from Stage 2");
            }
        }

        sealed class StageEnemyMixFingerprint
        {
            public string EnemyKey { get; }
            public string Summary { get; }
            public int WaveCount { get; }
            public int BossCount { get; }

            public StageEnemyMixFingerprint(string enemyKey, string summary, int waveCount, int bossCount)
            {
                EnemyKey = enemyKey;
                Summary = summary;
                WaveCount = waveCount;
                BossCount = bossCount;
            }
        }

        static StageEnemyMixFingerprint GetStageEnemyMixFingerprint(string playablePath)
        {
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(playablePath);
            if (timeline == null) return null;

            var enemyTypes = new List<EnemyType>();
            var bossTypes = new List<string>();

            foreach (var track in timeline.GetOutputTracks())
            {
                if (track is WaveTrack waveTrack)
                    enemyTypes.Add(waveTrack.EnemyType);

                foreach (var clip in track.GetClips())
                {
                    if (clip.asset is Boss boss)
                        bossTypes.Add(boss.BossType.ToString());
                }
            }

            if (enemyTypes.Count == 0) return null;

            enemyTypes.Sort((a, b) => ((int)a).CompareTo((int)b));
            bossTypes.Sort();

            var enemyKey = string.Join(",", enemyTypes);
            var summary = enemyKey + "|" + string.Join(",", bossTypes);
            return new StageEnemyMixFingerprint(enemyKey, summary, enemyTypes.Count, bossTypes.Count);
        }

        static void AuditStageEnemyProgression(List<string> results)
        {
            results.Add("--- Stage enemy progression (per-enemy threat + monotonic curve) ---");
            results.Add("INFO ThreatScore = prefab HP + damage×10 (see StageEnemyProgression)");

            var previousAverage = 0f;
            var summaries = new List<StageEnemyProgression.StageThreatSummary>();

            for (var stageNum = 1; stageNum <= 6; stageNum++)
            {
                var path = $"{StageFolder}/Stage {stageNum}.playable";
                var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
                if (timeline == null)
                {
                    results.Add($"FAIL Stage {stageNum}: missing timeline");
                    continue;
                }

                var enemies = new List<EnemyType>();
                foreach (var track in timeline.GetOutputTracks())
                {
                    if (track is WaveTrack waveTrack)
                        enemies.Add(waveTrack.EnemyType);
                }

                if (enemies.Count == 0)
                {
                    results.Add($"FAIL Stage {stageNum}: no wave tracks");
                    continue;
                }

                var summary = StageEnemyProgression.SummarizeMix(enemies, stageNum);
                summaries.Add(summary);
                results.Add(
                    $"INFO Stage {stageNum}: avgThreat={summary.AverageThreat:0.#} maxThreat={summary.MaxThreat} peakTier={summary.PeakTier} elites={summary.EliteCount} waves={enemies.Count}");

                var maxAllowedTier = StageEnemyProgression.GetMaxTierForStage(stageNum);
                if (summary.PeakTier > maxAllowedTier)
                    results.Add($"FAIL Stage {stageNum}: peak tier {summary.PeakTier} exceeds allowed {maxAllowedTier}");
                else
                    results.Add($"PASS Stage {stageNum}: peak tier within band");

                if (summary.EliteCount > 1)
                    results.Add($"FAIL Stage {stageNum}: more than one ShadeJellyfish wave ({summary.EliteCount})");
                else if (summary.EliteCount == 1 && stageNum < 6)
                    results.Add($"FAIL Stage {stageNum}: ShadeJellyfish elite only allowed on Stage 6");
                else if (summary.EliteCount == 1)
                    results.Add($"PASS Stage {stageNum}: single elite wave OK");
                else
                    results.Add($"PASS Stage {stageNum}: no elite waves");

                for (var i = 0; i < enemies.Count; i++)
                {
                    var type = enemies[i];
                    var minStage = StageEnemyProgression.GetMinStageForEnemy(type);
                    if (stageNum < minStage)
                    {
                        results.Add($"FAIL Stage {stageNum}: {type} belongs on Stage {minStage}+");
                    }
                }

                if (stageNum > 1 && summary.AverageThreat + 0.01f < previousAverage)
                    results.Add($"FAIL Stage {stageNum}: avg threat {summary.AverageThreat:0.#} regressed from Stage {stageNum - 1} ({previousAverage:0.#})");
                else if (stageNum > 1)
                    results.Add($"PASS Stage {stageNum}: avg threat monotonic");

                previousAverage = summary.AverageThreat;
            }

            if (summaries.Count == 6)
            {
                results.Add($"INFO Curve: S1={summaries[0].AverageThreat:0.#} → S2={summaries[1].AverageThreat:0.#} → S3={summaries[2].AverageThreat:0.#} → S4={summaries[3].AverageThreat:0.#} → S5={summaries[4].AverageThreat:0.#} → S6={summaries[5].AverageThreat:0.#}");
            }
        }

        static void AuditStageExperienceYield(List<string> results)
        {
            results.Add("--- Stage experience yield (gem drops × stage multiplier) ---");
            results.Add("INFO Gem XP: Small=1 Medium=4 Big=10 Large=80");
            results.Add("INFO Runtime also scales gem XP: difficulty × time (+2%/min, cap +50%) × endless loops (+12%/loop, cap +120%) × daily Swift Growth blessing");
            results.Add("INFO Runtime gem tier bias: higher-tier drops more likely + small gems upgrade over time (stage, time, loops, difficulty, daily blessing)");

            var enemiesDb = AssetDatabase.LoadAssetAtPath<EnemiesDatabase>("Assets/Common/Scriptables/Enemies Database.asset");
            if (enemiesDb == null)
            {
                results.Add("FAIL Enemies Database.asset missing");
                return;
            }

            var previousEffective = 0f;

            for (var stageNum = 1; stageNum <= 6; stageNum++)
            {
                var stagePath = $"{StageFolder}/Stage {stageNum}.asset";
                var stageData = AssetDatabase.LoadAssetAtPath<StageData>(stagePath);
                var playablePath = $"{StageFolder}/Stage {stageNum}.playable";
                var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(playablePath);

                if (stageData == null || timeline == null)
                {
                    results.Add($"FAIL Stage {stageNum}: missing stage asset or timeline");
                    continue;
                }

                var xpMult = stageData.XpGainMultiplier;
                var targetMult = 1f;
                foreach (var (stage, multiplier) in StageEnemyProgression.StageXpGainTargets)
                {
                    if (stage == stageNum) targetMult = multiplier;
                }

                if (Mathf.Abs(xpMult - targetMult) > 0.001f)
                    results.Add($"FAIL Stage {stageNum}: xpGainMultiplier {xpMult:0.##} ≠ target {targetMult:0.##}");
                else
                    results.Add($"PASS Stage {stageNum}: xpGainMultiplier {xpMult:0.##}");

                var enemies = new List<EnemyType>();
                foreach (var track in timeline.GetOutputTracks())
                {
                    if (track is WaveTrack waveTrack)
                        enemies.Add(waveTrack.EnemyType);
                }

                if (enemies.Count == 0)
                {
                    results.Add($"FAIL Stage {stageNum}: no wave tracks for XP estimate");
                    continue;
                }

                var totalBaseXp = 0f;
                for (var i = 0; i < enemies.Count; i++)
                {
                    var data = enemiesDb.GetEnemyData(enemies[i]);
                    if (data == null) continue;
                    totalBaseXp += StageEnemyProgression.EstimateXpPerKill(data.EnemyDrop);
                }

                var avgBaseXp = totalBaseXp / enemies.Count;
                var effectiveXp = avgBaseXp * xpMult;
                results.Add($"INFO Stage {stageNum}: avgBaseXp/kill={avgBaseXp:0.##} × {xpMult:0.##} = {effectiveXp:0.##} effective");

                if (stageNum > 1 && effectiveXp + 0.01f < previousEffective)
                    results.Add($"FAIL Stage {stageNum}: effective XP {effectiveXp:0.##} regressed from Stage {stageNum - 1} ({previousEffective:0.##})");
                else if (stageNum > 1)
                    results.Add($"PASS Stage {stageNum}: effective XP monotonic");

                previousEffective = effectiveXp;
            }
        }

        static void AuditEnemyDropTables(List<string> results)
        {
            results.Add("--- Enemy drop tables (graduated loot vs threat tier) ---");

            var enemiesDb = AssetDatabase.LoadAssetAtPath<EnemiesDatabase>("Assets/Common/Scriptables/Enemies Database.asset");
            if (enemiesDb == null)
            {
                results.Add("FAIL Enemies Database.asset missing");
                return;
            }

            for (var i = 0; i < enemiesDb.EnemiesCount; i++)
            {
                var data = enemiesDb.GetEnemyData(i);
                if (data == null) continue;

                var type = data.Type;
                var tier = StageEnemyProgression.GetTier(type);
                var minStage = StageEnemyProgression.GetMinStageForEnemy(type);
                var expected = StageEnemyProgression.GetGraduatedDropProfile(type);
                var actualXp = StageEnemyProgression.EstimateXpPerKill(data.EnemyDrop);
                var (minXp, maxXp) = StageEnemyProgression.GetExpectedXpBand(tier);

                if (Mathf.Abs(actualXp - expected.ExpectedGemXp) > 0.75f)
                {
                    results.Add($"FAIL {type}: gem XP/kill {actualXp:0.#} ≠ target {expected.ExpectedGemXp:0.#} (tier {tier}, min stage {minStage})");
                    continue;
                }

                if (actualXp + 0.01f < minXp || actualXp - 0.01f > maxXp)
                    results.Add($"WARN {type}: gem XP/kill {actualXp:0.#} outside tier band {minXp:0.#}–{maxXp:0.#} (min stage {minStage})");
                else
                    results.Add($"PASS {type}: ~{actualXp:0.#} gem XP/kill, tier {tier}, min stage {minStage}");
            }
        }

        static void AuditStagePowerupPools(List<string> results)
        {
            results.Add("--- Stage powerup pools (ability minStage + chest bonuses) ---");

            var abilitiesDb = AssetDatabase.LoadAssetAtPath<VXMonster.Core.Abilities.AbilitiesDatabase>(
                "Assets/Common/Scriptables/Abilities/Abilities Database.asset");
            if (abilitiesDb == null)
            {
                results.Add("FAIL Abilities Database.asset missing");
                return;
            }

            var previousBonus3 = -1f;
            var previousBonus5 = -1f;

            for (var stageZero = 0; stageZero < 6; stageZero++)
            {
                var stageNum = stageZero + 1;
                var eligible = StageAbilityProgression.CountEligibleAbilities(abilitiesDb, stageZero);
                if (eligible < 8)
                    results.Add($"FAIL Stage {stageNum}: only {eligible} eligible abilities (need ≥8)");
                else
                    results.Add($"PASS Stage {stageNum}: {eligible} eligible abilities");

                // Late-stage exclusives must not appear on Stage 1 pool
                if (stageZero == 0)
                {
                    AbilityType[] lateOnly =
                    {
                        AbilityType.IceShard, AbilityType.Fireball, AbilityType.MagicRune,
                        AbilityType.FlyingDagger, AbilityType.SolarMagnifier, AbilityType.Duration,
                        AbilityType.IncreasedGold,
                    };
                    foreach (var late in lateOnly)
                    {
                        if (StageAbilityProgression.IsEligibleForStage(late, 0))
                            results.Add($"FAIL Stage 1 pool includes late ability {late}");
                    }
                }

                var stagePath = $"{StageFolder}/Stage {stageNum}.asset";
                var stageData = AssetDatabase.LoadAssetAtPath<StageData>(stagePath);
                if (stageData == null)
                {
                    results.Add($"FAIL Missing {stagePath}");
                    continue;
                }

                var target = StageAbilityProgression.GetChestTierBonus(stageZero);
                if (Mathf.Abs(stageData.ChestTier3Bonus - target.Tier3) > 0.001f
                    || Mathf.Abs(stageData.ChestTier5Bonus - target.Tier5) > 0.001f)
                {
                    results.Add(
                        $"FAIL Stage {stageNum}: chest bonuses {stageData.ChestTier3Bonus:0.##}/{stageData.ChestTier5Bonus:0.##} ≠ target {target.Tier3:0.##}/{target.Tier5:0.##}");
                }
                else
                {
                    results.Add($"PASS Stage {stageNum}: chest tier3 +{target.Tier3:0.##}, tier5 +{target.Tier5:0.##}");
                }

                if (previousBonus3 >= 0f && stageData.ChestTier3Bonus + 0.001f < previousBonus3)
                    results.Add($"FAIL Stage {stageNum}: chestTier3Bonus regressed");
                if (previousBonus5 >= 0f && stageData.ChestTier5Bonus + 0.001f < previousBonus5)
                    results.Add($"FAIL Stage {stageNum}: chestTier5Bonus regressed");

                previousBonus3 = stageData.ChestTier3Bonus;
                previousBonus5 = stageData.ChestTier5Bonus;
            }

            // Asset minStageId should match progression table after Apply
            var mismatch = 0;
            for (var i = 0; i < abilitiesDb.AbilitiesCount; i++)
            {
                var ability = abilitiesDb.GetAbility(i);
                if (ability == null) continue;
                var expected = StageAbilityProgression.GetMinStageId(ability.AbilityType);
                if (ability.MinStageId != expected)
                {
                    mismatch++;
                    if (mismatch <= 5)
                        results.Add($"FAIL {ability.AbilityType}: minStageId asset={ability.MinStageId} code={expected}");
                }
            }

            if (mismatch == 0)
                results.Add("PASS Ability asset minStageId matches StageAbilityProgression");
            else if (mismatch > 5)
                results.Add($"FAIL {mismatch} abilities have mismatched minStageId (run Apply Ability Stage Gates)");
        }
    }
}
#endif
