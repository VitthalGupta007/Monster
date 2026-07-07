using OctoberStudio.UI;
using UnityEngine;

namespace OctoberStudio.Currency
{
    public class CurrencyScreenIncicatorBehavior : ScalingLabelBehavior
    {
        [Tooltip("The unique identificator of the currency that is attached to this ui. There must be an entry with the same id in the Currencies Database")]
        [SerializeField, HideInInspector] protected string currencyID;
        [SerializeField] protected CurrencyId currency;
        [SerializeField] protected bool useTempGameplayCurrency = false;

        public CurrencySave Currency { get; private set; }

        protected virtual void Start()
        {
            string id = currency != "" ? currency : currencyID;

            Currency = GameController.CurrenciesManager.GetCurrency(id, useTempGameplayCurrency);

            if (Currency == null)
            {
                Debug.LogError($"Could not find {id} in currencies database");
                return;
            }

            UpdateIndicator();

            icon.sprite = Currency.Data.Icon;

            Currency.onAmountChanged += OnAmountChanged;
        }

        protected virtual void OnAmountChanged(int amount)
        {
            UpdateIndicator();
        }

        protected virtual void UpdateIndicator()
        {
            SetAmount(Currency.Amount);
        }

        protected virtual void OnDestroy()
        {
            Currency.onAmountChanged -= OnAmountChanged;
        }
    }
}