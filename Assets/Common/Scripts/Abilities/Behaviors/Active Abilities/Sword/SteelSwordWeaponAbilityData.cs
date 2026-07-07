using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Steel Sword Data", menuName = "October/Abilities/Active/Steel Sword")]
    public class SteelSwordWeaponAbilityData : GenericAbilityData<SteelSwordWeaponAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.SteelSword;
            isWeaponAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.SteelSword;
            isWeaponAbility = true;
        }
    }

    [System.Serializable]
    public class SteelSwordWeaponAbilityLevel : AbilityLevel
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

        [Tooltip("Size of slash")]
        [SerializeField] float slashSize;
        public float SlashSize => slashSize;

        [Tooltip("Delay Before each slash")]
        [SerializeField] float timeBetweenSlashes;
        public float TimeBetweenSlashes => timeBetweenSlashes;
    }
}