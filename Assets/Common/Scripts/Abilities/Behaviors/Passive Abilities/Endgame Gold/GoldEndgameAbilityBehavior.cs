using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core.Abilities
{
    public class GoldEndgameAbilityBehavior : AbilityBehavior<GoldEndgameAbilityData, GoldEndgameAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            var gold = AbilityLevel.GoldAmount * PlayerBehavior.Player.GoldMultiplier;
            var clampedGold = Mathf.RoundToInt(gold);

            GameController.CurrenciesManager.GetDefaultCurrency(true).Deposit(clampedGold);
        }
    }
}

