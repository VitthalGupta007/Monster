using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core.Currency
{
    [CreateAssetMenu(fileName = "Currencies Database", menuName = "VX Monster/Currencies Database")]
    public class CurrenciesDatabase : ScriptableObject
    {
        [SerializeField] List<CurrencyData> currencies;

        public int Count => currencies.Count;

        public CurrencyData GetCurrency(string id)
        {
            for(int i = 0; i < currencies.Count; i++)
            {
                if (currencies[i].ID == id) return currencies[i];
            }

            return null;
        }

        public CurrencyData GetCurrency(int index)
        {
            return currencies[index];
        }
    }
}