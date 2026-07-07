using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class GenericAbilityData<T>: AbilityData where T : AbilityLevel
    {
        [SerializeField] T[] levels;
        public override AbilityLevel[] Levels => levels;

        public new T GetLevel(int index)
        {
            return levels[index];
        }
    }
}