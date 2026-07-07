using UnityEditor;
using UnityEngine;

namespace OctoberStudio.Currency
{
    [System.Serializable]
    public class Price
    {
        [SerializeField] protected string currencyId;
        [SerializeField] protected int amount;

        public string CurrencyId => currencyId;
        public int Amount => amount;

        public Price(string currencyId, int amount) 
        {
            this.currencyId = currencyId;
            this.amount = amount;
        }

#if UNITY_EDITOR
        public void SaveToSerializedProperty(SerializedProperty property)
        {
            var currencyIdProperty = property.FindPropertyRelative("currencyId");
            currencyIdProperty.stringValue = currencyId;

            var amountProperty = property.FindPropertyRelative("amount");
            amountProperty.intValue = amount;
        }
#endif
    }
}