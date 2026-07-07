using System;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Gameplay
{
    [CreateAssetMenu(menuName = "VX Monster/Relic", fileName = "Relic")]
    public class RelicData : ScriptableObject
    {
        [SerializeField] string id;
        [SerializeField] string displayName;
        [SerializeField] string description;
        [SerializeField] RelicEffectType effectType;
        [SerializeField] float magnitude = 1f;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public RelicEffectType EffectType => effectType;
        public float Magnitude => magnitude;

        public void Configure(string relicId, string name, string relicDescription, RelicEffectType effect, float effectMagnitude = 1f)
        {
            id = relicId;
            displayName = name;
            description = relicDescription;
            effectType = effect;
            magnitude = effectMagnitude;
        }
    }

    public enum RelicEffectType
    {
        ExtraPassiveSlot,
        DamageBoost,
        MaxHpPenalty,
        PickupRadius,
        ComboAmplifier,
        BossDamage,
        CooldownReduction,
        GoldBoost,
        PhoenixRevive,
        BonusReroll
    }

    [CreateAssetMenu(menuName = "VX Monster/Relics Database", fileName = "Relics Database")]
    public class RelicsDatabase : ScriptableObject
    {
        [SerializeField] List<RelicData> relics = new List<RelicData>();

        public IReadOnlyList<RelicData> Relics => relics;

        public RelicData GetById(string relicId)
        {
            for (var i = 0; i < relics.Count; i++)
            {
                if (relics[i].Id == relicId) return relics[i];
            }

            return null;
        }

        public RelicData GetRandom(System.Random rng)
        {
            if (relics.Count == 0) return null;
            return relics[rng.Next(relics.Count)];
        }

        public void SetupDefaultRelics()
        {
            if (relics.Count > 0) return;

            relics.Add(CreateRelic("vx_core", "VX Core", "Gain one extra passive slot.", RelicEffectType.ExtraPassiveSlot));
            relics.Add(CreateRelic("glass_soul", "Glass Soul", "More damage, less health.", RelicEffectType.DamageBoost, 0.4f));
            relics.Add(CreateRelic("magnet_heart", "Magnet Heart", "Huge pickup radius.", RelicEffectType.PickupRadius, 2f));
            relics.Add(CreateRelic("combo_lens", "Combo Lens", "Element combos hit harder.", RelicEffectType.ComboAmplifier, 0.25f));
            relics.Add(CreateRelic("boss_hunter", "Boss Hunter", "Bonus damage against bosses.", RelicEffectType.BossDamage, 0.5f));
            relics.Add(CreateRelic("time_shard", "Time Shard", "Reduced ability cooldowns.", RelicEffectType.CooldownReduction, 0.1f));
            relics.Add(CreateRelic("lucky_coin", "Lucky Coin", "More gold from runs.", RelicEffectType.GoldBoost, 0.25f));
            relics.Add(CreateRelic("phoenix_ash", "Phoenix Ash", "Revive once per run at low HP.", RelicEffectType.PhoenixRevive));
            relics.Add(CreateRelic("reroll_token", "Reroll Token", "Extra ability reroll.", RelicEffectType.BonusReroll));
        }

        private static RelicData CreateRelic(string relicId, string name, string description, RelicEffectType effect, float effectMagnitude = 1f)
        {
            var relic = CreateInstance<RelicData>();
            relic.Configure(relicId, name, description, effect, effectMagnitude);
            return relic;
        }
    }

    public class RelicsManager : MonoBehaviour
    {
        public const int MaxRelicSlots = 3;

        public static RelicsManager Instance { get; private set; }

        [SerializeField] RelicsDatabase database;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
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

            GameSessionManager.Instance?.Codex?.DiscoverRelic(relic.Id);
            return true;
        }

        public int GetPassiveSlotBonus()
        {
            return CountEffect(RelicEffectType.ExtraPassiveSlot);
        }

        public int GetBonusRerolls()
        {
            return CountEffect(RelicEffectType.BonusReroll);
        }

        public float GetDamageMultiplier()
        {
            var mult = 1f;
            mult += SumMagnitude(RelicEffectType.DamageBoost, 0.4f);
            mult += SumMagnitude(RelicEffectType.BossDamage, 0f); // applied contextually
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
