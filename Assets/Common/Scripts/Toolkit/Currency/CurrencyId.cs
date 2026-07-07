using System.Runtime.CompilerServices;
using UnityEngine;

namespace OctoberStudio.Currency
{
    [System.Serializable]
    public class CurrencyId
    {
        [SerializeField] protected string value;
        public string Value => value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(CurrencyId currencyId)
        {
            return currencyId.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator CurrencyId(string currencyId)
        {
            return new CurrencyId { value = currencyId };
        }
    }
}