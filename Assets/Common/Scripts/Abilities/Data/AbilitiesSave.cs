using OctoberStudio.Save;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    [System.Serializable]
    public class AbilitiesSave : ISave
    {
        [SerializeField] AbilitySave[] savedAbilities;

        private Dictionary<AbilityType, int> abilitiesLevels;

        public void Init()
        {
            abilitiesLevels = new Dictionary<AbilityType, int>();

            if(savedAbilities == null) savedAbilities = new AbilitySave[0];

            for(int i = 0; i < savedAbilities.Length; i++)
            {
                var save = savedAbilities[i];

                if (abilitiesLevels.ContainsKey(save.AbilityType))
                {
                    var savedLevel = abilitiesLevels[save.AbilityType];

                    if(save.Level > savedLevel)
                    {
                        abilitiesLevels[save.AbilityType] = save.Level;
                    }
                } else
                {
                    abilitiesLevels.Add(save.AbilityType, save.Level);
                }
            }
        }

        public List<AbilityType> GetSavedAbilities()
        {
            return abilitiesLevels.Keys.ToList();
        }

        public int GetAbilityLevel(AbilityType ability)
        {
            if (abilitiesLevels.ContainsKey(ability))
            {
                return abilitiesLevels[ability];
            } else
            {
                return -1;
            }
        }

        public void SetAbilityLevel(AbilityType ability, int level)
        {
            if (abilitiesLevels.ContainsKey(ability))
            {
                abilitiesLevels[ability] = level;
            } else
            {
                abilitiesLevels.Add(ability, level);
            }
        }

        public void RemoveAbility(AbilityType ability)
        {
            if (abilitiesLevels.ContainsKey(ability))
            {
                abilitiesLevels.Remove(ability);
            }
        }

        public void Flush()
        {
            savedAbilities = new AbilitySave[abilitiesLevels.Count];

            int i = 0;

            foreach(var ability in abilitiesLevels.Keys)
            {
                var abilitySave = new AbilitySave(ability, abilitiesLevels[ability]);
                savedAbilities[i++] = abilitySave;
            }
        }

        public void Clear()
        {
            abilitiesLevels.Clear();
        }

        [System.Serializable]
        private class AbilitySave
        {
            [SerializeField] AbilityType abilityType;
            [SerializeField] int level;

            public AbilityType AbilityType => abilityType; 
            public int Level => level;

            public AbilitySave(AbilityType abilityType, int level)
            {
                this.abilityType = abilityType;
                this.level = level;
            }
        }
    }
}