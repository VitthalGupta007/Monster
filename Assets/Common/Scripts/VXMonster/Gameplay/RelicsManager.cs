using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VXMonster.Core;

namespace VXMonster.Gameplay
{
    public class RelicsManager : MonoBehaviour
    {
        public const int MaxRelicSlots = 3;

        public static RelicsManager Instance { get; private set; }
        public static event UnityAction<RelicData> RelicGranted;

        [SerializeField] RelicsDatabase database;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
            RelicGranted = null;
        }

        private void Awake()
        {
            Instance = this;
            if (database == null)
            {
                database = Resources.Load<RelicsDatabase>("VX/Relics Database");
            }

            if (database == null)
            {
                database = CreateRuntimeDatabase();
            }
        }

        private static RelicsDatabase CreateRuntimeDatabase()
        {
            var db = ScriptableObject.CreateInstance<RelicsDatabase>();
            db.SetupDefaultRelics();
            return db;
        }

        public bool TryGrantRandomRelic()
        {
            var session = GameSessionManager.Instance?.RunSession;
            if (session == null || database == null) return false;
            if (session.ActiveRelicIds.Count >= MaxRelicSlots) return false;

            var seed = GameSessionManager.Instance.DailySeed != 0
                ? GameSessionManager.Instance.DailySeed + session.ActiveRelicIds.Count
                : Environment.TickCount;
            var relic = database.GetRandom(new System.Random(seed));
            if (relic == null) return false;

            return TryGrantRelic(relic);
        }

        public bool TryGrantRelic(RelicData relic)
        {
            var session = GameSessionManager.Instance?.RunSession;
            if (session == null || relic == null) return false;
            if (!session.TryAddRelic(relic.Id, MaxRelicSlots)) return false;

            if (GameSessionManager.Instance?.Codex?.DiscoverRelic(relic.Id) == true)
            {
                CodexRewardUtility.OnRelicDiscovered(relic.Id);
            }

            if (relic.EffectType == RelicEffectType.BonusBanish && session != null)
            {
                session.BanishesRemaining += Mathf.Max(1, Mathf.RoundToInt(relic.Magnitude > 0f ? relic.Magnitude : 1f));
                session.BanishUsed = false;
            }

            if (relic.EffectType == RelicEffectType.BonusReroll && session != null)
            {
                session.RerollsRemaining += Mathf.Max(1, Mathf.RoundToInt(relic.Magnitude > 0f ? relic.Magnitude : 1f));
            }

            PlayerBehavior.Player?.RefreshRelicModifiers();
            RelicGranted?.Invoke(relic);
            return true;
        }

        private bool HasRelic(string relicId)
        {
            var session = GameSessionManager.Instance?.RunSession;
            if (session == null) return false;

            var relicIds = session.ActiveRelicIds;
            for (var i = 0; i < relicIds.Count; i++)
            {
                if (relicIds[i] == relicId) return true;
            }

            return false;
        }

        public RelicData GetRelicById(string relicId)
        {
            return database?.GetById(relicId);
        }

        public int GetPassiveSlotBonus()
        {
            return CountEffect(RelicEffectType.ExtraPassiveSlot);
        }

        public int GetBonusRerolls()
        {
            return CountEffect(RelicEffectType.BonusReroll);
        }

        public int GetBonusBanishes()
        {
            return CountEffect(RelicEffectType.BonusBanish);
        }

        public float GetDamageReductionBonusPercent()
        {
            return SumMagnitude(RelicEffectType.DamageReduction, 0.15f) * 100f;
        }

        public float GetMoveSpeedMultiplier()
        {
            return 1f + SumMagnitude(RelicEffectType.MoveSpeed, 0.12f);
        }

        public float GetLifestealFraction()
        {
            return SumMagnitude(RelicEffectType.Lifesteal, 0.03f);
        }

        public bool HasSecondWind()
        {
            return CountEffect(RelicEffectType.SecondWind) > 0;
        }

        public float GetSecondWindHealFraction()
        {
            var mag = SumMagnitude(RelicEffectType.SecondWind, 0.4f);
            return mag > 0f ? mag : 0.4f;
        }

        public float GetDamageMultiplier()
        {
            var mult = 1f;
            mult += SumMagnitude(RelicEffectType.DamageBoost, 0.4f);
            return mult;
        }

        public float GetBossDamageMultiplier()
        {
            return 1f + SumMagnitude(RelicEffectType.BossDamage, 0.5f);
        }

        public float GetPickupRadiusMultiplier()
        {
            return 1f + SumMagnitude(RelicEffectType.PickupRadius, 2f);
        }

        public float GetCooldownMultiplier()
        {
            return 1f - Mathf.Clamp01(SumMagnitude(RelicEffectType.CooldownReduction, 0.1f));
        }

        public float GetGoldMultiplier()
        {
            return 1f + SumMagnitude(RelicEffectType.GoldBoost, 0.25f);
        }

        public float GetMaxHpMultiplier()
        {
            var mult = 1f;
            mult -= SumMagnitude(RelicEffectType.MaxHpPenalty, 0.3f);

            if (HasRelic("glass_soul"))
            {
                var relic = database.GetById("glass_soul");
                var penalty = relic != null && relic.Magnitude > 0f ? relic.Magnitude : 0.4f;
                mult *= 1f - penalty;
            }

            return Mathf.Clamp(mult, 0.25f, 1f);
        }

        public float GetComboAmplifierBonus()
        {
            return SumMagnitude(RelicEffectType.ComboAmplifier, 0.25f);
        }

        public bool HasPhoenixRevive()
        {
            return CountEffect(RelicEffectType.PhoenixRevive) > 0;
        }

        private int CountEffect(RelicEffectType effectType)
        {
            var session = GameSessionManager.Instance?.RunSession;
            if (session == null || database == null) return 0;

            var count = 0;
            foreach (var relicId in session.ActiveRelicIds)
            {
                var relic = database.GetById(relicId);
                if (relic != null && relic.EffectType == effectType) count++;
            }

            return count;
        }

        private float SumMagnitude(RelicEffectType effectType, float defaultMagnitude)
        {
            var session = GameSessionManager.Instance?.RunSession;
            if (session == null || database == null) return 0f;

            var sum = 0f;
            foreach (var relicId in session.ActiveRelicIds)
            {
                var relic = database.GetById(relicId);
                if (relic != null && relic.EffectType == effectType)
                {
                    sum += relic.Magnitude > 0 ? relic.Magnitude : defaultMagnitude;
                }
            }

            return sum;
        }
    }
}
