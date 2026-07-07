using VXMonster.Core.Save;
using UnityEngine.Events;
using UnityEngine;
using VXMonster.Core.Currency;

namespace VXMonster.Core
{
    public class CurrencySave: ISave
    {
        [SerializeField] protected int amount;
        public int Amount => amount;

        public event UnityAction<int> onAmountChanged;

        public CurrencyData Data { get; protected set; }
        public string Id => Data.ID;

        public virtual void Init(CurrencyData data)
        {
            Data = data;
        }

        public void Deposit(int depositedAmount)
        {
            amount += depositedAmount;

            onAmountChanged?.Invoke(amount);
        }

        public void Withdraw(int withdrawnAmount)
        {
            amount -= withdrawnAmount;
            if (amount < 0) amount = 0;

            onAmountChanged?.Invoke(amount);
        }

        public bool TryWithdraw(int withdrawnAmount)
        {
            var canAfford = CanAfford(withdrawnAmount);

            if(canAfford) 
            {
                amount -= withdrawnAmount;

                onAmountChanged?.Invoke(amount);
            }

            return canAfford;
        }

        public bool CanAfford(int requiredAmount)
        {
            return amount >= requiredAmount;
        }

        public virtual void Clear()
        {
            amount = 0;
        }

        public void Flush()
        {

        }
    }
}