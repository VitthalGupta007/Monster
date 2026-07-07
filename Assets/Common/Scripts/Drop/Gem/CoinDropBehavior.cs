using VXMonster.Core.Currency;
using UnityEngine;

namespace VXMonster.Core.Drop
{
    public class CoinDropBehavior : DropBehavior
    {
        [SerializeField] protected int amount;
        [SerializeField] protected CurrencyId currencyId;

        private static float leftoverDifference;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            leftoverDifference = 0;
        }

        public override void OnPickedUp()
        {
            base.OnPickedUp();

            gameObject.SetActive(false);

            var gold = amount * PlayerBehavior.Player.GoldMultiplier + leftoverDifference;
            var clampedGold = Mathf.FloorToInt(gold);
            leftoverDifference = gold - clampedGold;

            var currency = GameController.CurrenciesManager.GetCurrency(currencyId, true);
            if(currency == null)
            {
                currency = GameController.CurrenciesManager.GetDefaultCurrency(true);
            }

            currency.Deposit(clampedGold);
        }
    }
}