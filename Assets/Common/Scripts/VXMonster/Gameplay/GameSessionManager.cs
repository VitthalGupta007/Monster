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

        public void BindSaves(RunSessionSave runSession, LifetimeStatsSave lifetimeStats, DailyChallengeSave dailyChallenge, CodexSave codex)
        {
            RunSession = runSession;
            LifetimeStats = lifetimeStats;
            DailyChallenge = dailyChallenge;
            Codex = codex;
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
            RunSession?.ResetForNewRun(1);
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
        }

        public void IncrementEndlessLoop()
        {
            EndlessLoopCount++;
            LifetimeStats?.UpdateEndlessLoopsBest(EndlessLoopCount);
        }

        public float GetEnemyHpMultiplier()
        {
            var mult = Difficulty.EnemyHpMultiplier();
            if (RunMode == RunMode.Endless)
            {
                mult *= 1f + EndlessLoopCount * 0.15f;
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
                mult *= 1f + EndlessLoopCount * 0.15f;
            }

            foreach (var modifier in activeModifiers)
            {
                mult *= modifier.EnemyDamageMultiplier;
            }

            return mult;
        }

        public float GetRewardMultiplier()
        {
            var mult = Difficulty.RewardMultiplier();
            foreach (var modifier in activeModifiers)
            {
                mult *= modifier.RewardMultiplier;
            }

            return mult;
        }

        public int GetPassiveSlotBonus()
        {
            var bonus = 0;
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
            if (RelicsManager.Instance != null)
            {
                rerolls += RelicsManager.Instance.GetBonusRerolls();
            }

            return rerolls;
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
