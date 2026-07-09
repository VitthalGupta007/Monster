using System;
using System.Collections.Generic;
using UnityEngine;
using VXMonster.Save;

namespace VXMonster.Gameplay
{
    public class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance { get; private set; }

        public RunMode RunMode { get; private set; } = RunMode.Campaign;
        public DifficultyTier Difficulty { get; private set; } = DifficultyTier.Normal;
        public int EndlessLoopCount { get; private set; }
        public int DailySeed { get; private set; }
        public bool IsDailyScoredRun { get; private set; }

        public RunSessionSave RunSession { get; private set; }
        public LifetimeStatsSave LifetimeStats { get; private set; }
        public DailyChallengeSave DailyChallenge { get; private set; }
        public CodexSave Codex { get; private set; }
        public TalentTreeSave TalentTree { get; private set; }

        private readonly List<RunModifierDefinition> activeModifiers = new List<RunModifierDefinition>();

        public IReadOnlyList<RunModifierDefinition> ActiveModifiers => activeModifiers;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void BindSaves(RunSessionSave runSession, LifetimeStatsSave lifetimeStats, DailyChallengeSave dailyChallenge, CodexSave codex, TalentTreeSave talentTree)
        {
            RunSession = runSession;
            LifetimeStats = lifetimeStats;
            DailyChallenge = dailyChallenge;
            Codex = codex;
            TalentTree = talentTree;
        }

        public void ConfigureCampaign(DifficultyTier difficulty)
        {
            RunMode = RunMode.Campaign;
            Difficulty = difficulty;
            EndlessLoopCount = 0;
            IsDailyScoredRun = false;
            DailySeed = 0;
            activeModifiers.Clear();
            RunSession?.ResetForNewRun(GetBonusRerolls());
            CaptureSessionToSave();
        }

        public void ConfigureDailyChallenge(bool scoredAttempt)
        {
            RunMode = RunMode.DailyChallenge;
            Difficulty = DifficultyTier.Normal;
            EndlessLoopCount = 0;
            IsDailyScoredRun = scoredAttempt;
            DailySeed = RunModifierUtility.GetUtcDailySeed();
            activeModifiers.Clear();
            activeModifiers.AddRange(RunModifierUtility.GetDailyModifiers(DailySeed));
            RunSession?.ResetForNewRun(GetBonusRerolls());
            CaptureSessionToSave();
        }

        public void ConfigureEndless(DifficultyTier difficulty)
        {
            RunMode = RunMode.Endless;
            Difficulty = difficulty;
            EndlessLoopCount = 0;
            IsDailyScoredRun = false;
            DailySeed = 0;
            activeModifiers.Clear();
            RunSession?.ResetForNewRun(GetBonusRerolls());
            CaptureSessionToSave();
        }

        public void IncrementEndlessLoop()
        {
            EndlessLoopCount++;
            LifetimeStats?.UpdateEndlessLoopsBest(EndlessLoopCount);
            CaptureSessionToSave();
        }

        public void CaptureSessionToSave()
        {
            RunSession?.CaptureSessionContext(RunMode, Difficulty, EndlessLoopCount, DailySeed, IsDailyScoredRun);
        }

        public bool TryRestoreSessionFromSave()
        {
            if (RunSession == null || !RunSession.HasSessionContext) return false;

            RunMode = RunSession.SavedRunMode;
            Difficulty = RunSession.SavedDifficulty;
            EndlessLoopCount = RunSession.SavedEndlessLoopCount;
            DailySeed = RunSession.SavedDailySeed;
            IsDailyScoredRun = RunSession.SavedIsDailyScoredRun;

            activeModifiers.Clear();
            if (RunMode == RunMode.DailyChallenge && DailySeed != 0)
            {
                activeModifiers.AddRange(RunModifierUtility.GetDailyModifiers(DailySeed));
            }

            VXDifficultySelection.Set(Difficulty);
            return true;
        }

        public void ClearSessionContext()
        {
            RunSession?.ClearSessionContext();
        }

        public float GetEnemyHpMultiplier()
        {
            var mult = Difficulty.EnemyHpMultiplier();
            if (RunMode == RunMode.Endless)
            {
                var loops = EndlessLoopCount;
                var linear = loops * 0.15f;
                var softCap = 3f;
                mult *= 1f + Mathf.Min(linear, softCap) + Mathf.Max(0f, linear - softCap) * 0.05f;
            }

            foreach (var modifier in activeModifiers)
            {
                mult *= modifier.EnemyHpMultiplier;
            }

            return mult;
        }

        public float GetEnemyDamageMultiplier()
        {
            var mult = Difficulty.EnemyDamageMultiplier();
            if (RunMode == RunMode.Endless)
            {
                var loops = EndlessLoopCount;
                var linear = loops * 0.15f;
                var softCap = 3f;
                mult *= 1f + Mathf.Min(linear, softCap) + Mathf.Max(0f, linear - softCap) * 0.05f;
            }

            foreach (var modifier in activeModifiers)
            {
                mult *= modifier.EnemyDamageMultiplier;
            }

            return mult;
        }

        public float GetRewardMultiplier()
        {
            var mult = Difficulty.RewardMultiplier() * GetTalentGoldMultiplier();
            foreach (var modifier in activeModifiers)
            {
                mult *= modifier.RewardMultiplier;
            }

            return mult;
        }

        public int GetPassiveSlotBonus()
        {
            var bonus = 0;
            if (TalentTree != null && TalentTree.IsUnlocked(TalentTreeIds.ExpandedMind))
            {
                bonus++;
            }

            foreach (var modifier in activeModifiers)
            {
                bonus += modifier.PassiveSlotBonus;
            }

            if (RelicsManager.Instance != null)
            {
                bonus += RelicsManager.Instance.GetPassiveSlotBonus();
            }

            return bonus;
        }

        public int GetBonusRerolls()
        {
            var rerolls = 1;
            if (TalentTree != null && TalentTree.IsUnlocked(TalentTreeIds.ExtraReroll))
            {
                rerolls++;
            }

            if (RelicsManager.Instance != null)
            {
                rerolls += RelicsManager.Instance.GetBonusRerolls();
            }

            return rerolls;
        }

        public float GetTalentGoldMultiplier()
        {
            if (TalentTree != null && TalentTree.IsUnlocked(TalentTreeIds.GoldenInstinct))
            {
                return 1.1f;
            }

            return 1f;
        }

        public float GetTalentMoveSpeedMultiplier()
        {
            if (TalentTree != null && TalentTree.IsUnlocked(TalentTreeIds.QuickFeet))
            {
                return 1.08f;
            }

            return 1f;
        }

        public float GetTalentMaxHpBonus()
        {
            if (TalentTree != null && TalentTree.IsUnlocked(TalentTreeIds.IronWill))
            {
                return 20f;
            }

            return 0f;
        }

        public int CalculateDailyScore(int kills, float survivalTime, int comboBursts)
        {
            var baseScore = kills * 10 + Mathf.FloorToInt(survivalTime) + comboBursts * 25;
            return Mathf.RoundToInt(baseScore * GetRewardMultiplier());
        }

        public static string GetUtcDateKey()
        {
            return DateTime.UtcNow.ToString("yyyyMMdd");
        }
    }
}
