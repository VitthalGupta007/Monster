using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.Currency
{
    [CustomPropertyDrawer(typeof(Price))]
    public class PricePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var currencyIdProperty = property.FindPropertyRelative("currencyId");
            var amountProperty = property.FindPropertyRelative("amount");

            var propertiesRect = EditorGUI.PrefixLabel(position, label);
            var labelRect = new Rect(position);
            labelRect.width -= propertiesRect.width;

            GUI.Label(labelRect, label);

            if (CurrencyCache.Database != null)
            {
                PopupGUI(propertiesRect, currencyIdProperty, amountProperty);
            } else
            {
                SimpleGUI(propertiesRect, currencyIdProperty, amountProperty);
            }
        }

        protected virtual void PopupGUI(Rect rect, SerializedProperty currencyIdProperty, SerializedProperty amountProperty)
        {
            var currencyIdRect = new Rect(rect);
            currencyIdRect.width = currencyIdRect.width / 2f - 1f;

            var ids = new List<string>(CurrencyCache.Database.Count);
            var names = new List<string>(CurrencyCache.Database.Count);

            for (int i = 0; i < CurrencyCache.Database.Count; i++)
            {
                var currency = CurrencyCache.Database.GetCurrency(i);
                ids.Add(currency.ID);
                names.Add(currency.Name);
            }

            var selectedId = ids.IndexOf(currencyIdProperty.stringValue);

            if (selectedId == -1) selectedId = 0;
            selectedId = EditorGUI.Popup(currencyIdRect, selectedId, names.ToArray());

            currencyIdProperty.stringValue = ids[selectedId];

            var amountRect = new Rect(rect);
            amountRect.width = amountRect.width / 2f - 1f;
            amountRect.x += currencyIdRect.width + 2f;

            amountProperty.intValue = EditorGUI.IntField(amountRect, new GUIContent("", "amount"), amountProperty.intValue);
        }

        protected virtual void SimpleGUI(Rect rect, SerializedProperty currencyIdProperty, SerializedProperty amountProperty)
        {
            var currencyIdRect = new Rect(rect);
            currencyIdRect.width = currencyIdRect.width / 2f - 1f;

            currencyIdProperty.stringValue = EditorGUI.TextField(currencyIdRect, new GUIContent("", "currencyId"), currencyIdProperty.stringValue);

            var amountRect = new Rect(rect);
            amountRect.width = amountRect.width / 2f - 1f;
            amountRect.x += currencyIdRect.width + 2f;

            amountProperty.intValue = EditorGUI.IntField(amountRect, new GUIContent("", "amount"), amountProperty.intValue);
        }
    }
}