using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Sacred Blade Data", menuName = "October/Abilities/Evolution/Sacred Blade")]
    public class SacredBladeAbilityData : GenericAbilityData<SacredBladeAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.SacredBlade;
            isWeaponAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.SacredBlade;
            isWeaponAbility = true;
        }
    }

    [System.Serializable]
    public class SacredBladeAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Amount of slashes in the attack")]
        [SerializeField] int slashesCount;
        public int SlashesCount => slashesCount;

        [Tooltip("Damage of slashes calculates like this: Player.Damage * Damage")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Damage of waves calculates like this: Player.Damage * WaveDamage")]
        [SerializeField] float waveDamage;
        public float WaveDamage => waveDamage;

        [Tooltip("Size of slash")]
        [SerializeField] float slashSize;
        public float SlashSize => slashSize;

        [Tooltip("Delay Before each slash")]
        [SerializeField] float timeBetweenSlashes;
        public float TimeBetweenSlashes => timeBetweenSlashes;
    }
}