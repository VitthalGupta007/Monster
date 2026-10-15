using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Gameplay
{
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
            relics.Add(CreateRelic("vampiric_fang", "Vampiric Fang", "Heal a portion of damage you deal.", RelicEffectType.Lifesteal, 0.03f));
            relics.Add(CreateRelic("iron_shell", "Iron Shell", "Take less damage from hits.", RelicEffectType.DamageReduction, 0.15f));
            relics.Add(CreateRelic("haste_charm", "Haste Charm", "Move faster.", RelicEffectType.MoveSpeed, 0.12f));
            relics.Add(CreateRelic("treasure_map", "Treasure Map", "Earn more gold from runs.", RelicEffectType.GoldBoost, 0.35f));
            relics.Add(CreateRelic("second_wind", "Second Wind", "Once per run, heal when critically low.", RelicEffectType.SecondWind, 0.4f));
            relics.Add(CreateRelic("banishers_seal", "Banisher's Seal", "Extra ability banish.", RelicEffectType.BonusBanish, 1f));
        }

        private static RelicData CreateRelic(string relicId, string name, string description, RelicEffectType effect, float effectMagnitude = 1f)
        {
            var relic = CreateInstance<RelicData>();
            relic.Configure(relicId, name, description, effect, effectMagnitude);
            return relic;
        }
    }
}
