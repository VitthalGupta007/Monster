using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Abilities Database", menuName = "October/Abilities/Database")]
    public class AbilitiesDatabase : ScriptableObject
    {
        [SerializeField] int activeAbilitiesCapacity = 5;
        [SerializeField] int passiveAbilitiesCapacity = 5;

        public int ActiveAbilitiesCapacity => activeAbilitiesCapacity;
        public int PassiveAbilitiesCapacity => passiveAbilitiesCapacity;

        [Header("Abilities Weight Multipliers")]
        [SerializeField] float firstLevelsActiveAbilityWeightMultiplier = 1.2f;
        [SerializeField] float aquiredAbilityWeightMultiplier = 1.2f;
        [SerializeField] float lessAbilitiesOfTypeWeightMultiplier = 1.2f;
        [SerializeField] float evolutionAbilityWeightMultiplier = 10f;
        [SerializeField] float requiredForEvolutionWeightMultiplier = 1.2f;

        public float FirstLevelsActiveAbilityWeightMultiplier => firstLevelsActiveAbilityWeightMultiplier;
        public float AquiredAbilityWeightMultiplier => aquiredAbilityWeightMultiplier;
        public float LessAbilitiesOfTypeWeightMultiplier => lessAbilitiesOfTypeWeightMultiplier;
        public float EvolutionAbilityWeightMultiplier => evolutionAbilityWeightMultiplier;
        public float RequiredForEvolutionWeightMultiplier => requiredForEvolutionWeightMultiplier;


        [SerializeField] List<AbilityData> abilities;

        public int AbilitiesCount => abilities.Count;

        public AbilityData GetAbility(int index)
        {
            return abilities[index];
        }

        public AbilityData GetAbility(AbilityType type)
        {
            for(int i = 0; i < abilities.Count; i++)
            {
                AbilityData ability = abilities[i];

                if(ability.AbilityType == type) return ability;
            }

            return null;
        }
    }
}