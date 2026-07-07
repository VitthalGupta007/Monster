using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class HealEndgameAbilityBehavior : AbilityBehavior<HealEndgameAbilityData, HealEndgameAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            PlayerBehavior.Player.RestoreHP(AbilityLevel.HealPersentage);
        }
    }
}

