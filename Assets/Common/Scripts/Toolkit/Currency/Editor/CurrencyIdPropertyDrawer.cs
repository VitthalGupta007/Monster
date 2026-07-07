using System;
using UnityEditor;
using UnityEngine;

namespace VXMonster.Core.Currency
{
    [CustomPropertyDrawer(typeof(CurrencyId))]
    public class CurrencyIdPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var names = CurrencyCache.GetNames();
            var ids = CurrencyCache.GetIds();

            var valueProperty = property.FindPropertyRelative("value");

            if (names == null)
            {
                EditorGUI.PropertyField(position, valueProperty, label);
            }
            else
            {
                var selectedId = valueProperty.stringValue;
                var selectedIndex = Array.IndexOf(ids, selectedId);
                if (selectedIndex < 0) selectedIndex = 0;

                selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, names);
                selectedId = ids[selectedIndex];

                valueProperty.stringValue = selectedId;
            }
        }
    }
}