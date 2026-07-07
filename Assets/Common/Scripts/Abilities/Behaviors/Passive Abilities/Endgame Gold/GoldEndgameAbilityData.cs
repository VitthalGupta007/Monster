using UnityEngine;

namespace VXMonster.Core.Abilities
{
    [CreateAssetMenu(fileName = "Gold Endgame Ability Data", menuName = "VX Monster/Abilities/Passive/Gold Endgame")]
    public class GoldEndgameAbilityData : GenericAbilityData<GoldEndgameAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.GoldEndgame;
        }

        private void OnValidate()
        {
            type = AbilityType.GoldEndgame;
        }
    }

    [System.Serializable]
    public class GoldEndgameAbilityLevel : AbilityLevel
    {
        [Tooltip("Give the player a specified amount of gold")]
        [SerializeField, Min(1)] int goldAmount = 40;
        public int GoldAmount => goldAmount;
    }
}