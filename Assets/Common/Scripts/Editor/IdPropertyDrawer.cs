using UnityEngine;
using UnityEditor;

namespace OctoberStudio
{
    [CustomPropertyDrawer(typeof(Id))]
    public class IdPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelRect = new Rect(position);
            labelRect.width = EditorGUIUtility.labelWidth;

            GUI.Label(labelRect, label);

            var valueProperty = property.FindPropertyRelative("value");
            var canEditProperty = property.FindPropertyRelative("canEdit");

            GUI.enabled = canEditProperty.boolValue;

            var idRect = new Rect(position);
            idRect.x += labelRect.width;
            idRect.width = position.width - labelRect.width - 40;
            valueProperty.stringValue =  EditorGUI.TextField(idRect, valueProperty.stringValue);
            if (valueProperty.stringValue == "") valueProperty.stringValue = System.Guid.NewGuid().ToString();

            GUI.enabled = true;

            var buttonRect = new Rect(position);
            buttonRect.width = 40;
            buttonRect.x += labelRect.width + idRect.width;

            if(GUI.Button(buttonRect, canEditProperty.boolValue ? "Close" : "Edit"))
            {
                canEditProperty.boolValue = !canEditProperty.boolValue;
            }
        }
    }
}