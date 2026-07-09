using UnityEngine;

namespace VXMonster.Gameplay
{
    [CreateAssetMenu(menuName = "VX Monster/Relic", fileName = "Relic")]
    public class RelicData : ScriptableObject
    {
        [SerializeField] string id;
        [SerializeField] string displayName;
        [SerializeField] string description;
        [SerializeField] Sprite icon;
        [SerializeField] RelicEffectType effectType;
        [SerializeField] float magnitude = 1f;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public RelicEffectType EffectType => effectType;
        public float Magnitude => magnitude;

        public void Configure(string relicId, string name, string relicDescription, RelicEffectType effect, float effectMagnitude = 1f, Sprite relicIcon = null)
        {
            id = relicId;
            displayName = name;
            description = relicDescription;
            effectType = effect;
            magnitude = effectMagnitude;
            icon = relicIcon;
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
}
