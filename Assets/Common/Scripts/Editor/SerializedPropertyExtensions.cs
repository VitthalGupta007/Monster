using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace VXMonster.Core.StageCreator
{
    public static class SerializedPropertyExtensions
    {
        #region Get serialized property value
        public static T GetValue<T>(this SerializedProperty property) where T : class
        {
            object obj = property.serializedObject.targetObject;
            var elements = property.propertyPath.Replace(".Array.data[", "[").Split('.');

            foreach (string element in elements)
            {
                if (element.Contains("["))
                {
                    var fieldName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetFieldValue(obj, fieldName, index);
                }
                else
                {
                    obj = GetFieldValue(obj, element);
                }

                if (obj == null)
                    return null;
            }

            return obj as T;
        }

        private static object GetFieldValue(object source, string fieldName, int index = -1)
        {
            if (source == null)
                return null;

            Type type = source.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
                return null;

            object value = field.GetValue(source);

            if (index >= 0 && value is System.Collections.IList list)
                return list[index];

            return value;
        }

        #endregion

        #region Set serialized property value

        public static void SetValue(this SerializedProperty property, object value)
        {
            object targetObject = property.serializedObject.targetObject;
            object parent = GetParentObjectOfProperty(property.propertyPath, targetObject);

            string fieldName = GetFieldName(property.propertyPath);
            if (parent == null)
            {
                Debug.LogError("Parent object is null.");
                return;
            }

            if (IsArrayElement(fieldName, out string arrayFieldName, out int index))
            {
                FieldInfo field = parent.GetType().GetField(arrayFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var list = field.GetValue(parent) as IList;
                    if (list != null && index >= 0 && index < list.Count)
                    {
                        list[index] = value;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        Debug.LogError("List is null or index out of range.");
                    }
                }
                else
                {
                    Debug.LogError($"Field '{arrayFieldName}' not found on {parent.GetType()}");
                }
            }
            else
            {
                FieldInfo field = parent.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(parent, value);
                    property.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogError($"Field '{fieldName}' not found on {parent.GetType()}");
                }
            }
        }

        private static bool IsArrayElement(string fieldName, out string arrayName, out int index)
        {
            arrayName = null;
            index = -1;

            var match = System.Text.RegularExpressions.Regex.Match(fieldName, @"^(.+)\[(\d+)\]$");
            if (match.Success)
            {
                arrayName = match.Groups[1].Value;
                index = int.Parse(match.Groups[2].Value);
                return true;
            }
            return false;
        }

        public static object GetParentObjectOfProperty(string path, object root)
        {
            string[] elements = path.Split('.');
            object obj = root;

            foreach (var element in elements[..^1]) // everything except the last
            {
                if (element.Contains("["))
                {
                    // array element
                    string fieldName = element.Substring(0, element.IndexOf('['));
                    int index = int.Parse(element.Substring(element.IndexOf('[') + 1).TrimEnd(']'));

                    FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var list = field?.GetValue(obj) as IList;
                    obj = list?[index];
                }
                else
                {
                    FieldInfo field = obj.GetType().GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    obj = field?.GetValue(obj);
                }

                if (obj == null)
                    break;
            }

            return obj;
        }

        public static string GetFieldName(string path)
        {
            string[] parts = path.Split('.');
            return parts[^1]; // last part
        }

        #endregion

        public static void CopyFromSerializedObject(this SerializedObject target, SerializedObject source)
        {
            if (target == null || source == null) throw new ArgumentNullException();

            var sourceProp = source.GetIterator();

            bool enterChildren = true;
            while (sourceProp.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Skip script reference
                if (sourceProp.propertyPath == "m_Script") continue;

                var targetProp = target.FindProperty(sourceProp.propertyPath);
                if (targetProp == null) continue;

                try
                {
                    CopyPropertyValue(targetProp, sourceProp);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to copy property '{sourceProp.propertyPath}': {ex.Message}");
                }
            }

            target.ApplyModifiedProperties();
        }

        private static void CopyPropertyValue(SerializedProperty targetProp, SerializedProperty sourceProp)
        {
            if (sourceProp.propertyType == SerializedPropertyType.ManagedReference)
            {
                CopyManagedReference(targetProp, sourceProp);
                return;
            }

            if (sourceProp.isArray && sourceProp.propertyType != SerializedPropertyType.String)
            {
                targetProp.arraySize = sourceProp.arraySize;

                for (int i = 0; i < sourceProp.arraySize; i++)
                {
                    var sourceElement = sourceProp.GetArrayElementAtIndex(i);
                    var targetElement = targetProp.GetArrayElementAtIndex(i);

                    CopyPropertyValue(targetElement, sourceElement);
                }

                return;
            }

            // Fallback to Unity built-in copy (works for most primitive types)
            targetProp.serializedObject.CopyFromSerializedProperty(sourceProp);
        }

        private static void CopyManagedReference(SerializedProperty targetProp, SerializedProperty sourceProp)
        {
            var sourceValue = sourceProp.managedReferenceValue;

            if (sourceValue == null)
            {
                targetProp.managedReferenceValue = null;
                return;
            }

            var type = sourceValue.GetType();

            var json = JsonUtility.ToJson(sourceValue);
            var clone = JsonUtility.FromJson(json, type);

            targetProp.managedReferenceValue = clone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Shrink(this Rect rect, float amount)
        {
            return new Rect(rect.x + amount, rect.y + amount, rect.width - 2 * amount, rect.height - 2 * amount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Shrink(this Rect rect, float amountX, float amountY)
        {
            return new Rect(rect.x + amountX, rect.y + amountY, rect.width - 2 * amountX, rect.height - 2 * amountY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Grow(this Rect rect, float amount)
        {
            return new Rect(rect.x - amount, rect.y - amount, rect.width + 2 * amount, rect.height + 2 * amount);
        }

        public static void DrawVerticalSeparator(float width = 2f)
        {
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(width, 0, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false)), new Color(0.1f, 0.1f, 0.1f));
        }

        public static void DrawHorizontalSeparator(float height = 2f)
        {
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false)), new Color(0.1f, 0.1f, 0.1f));
        }

        public static void DrawVerticalSeparator(float width, Color color)
        {
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(width, 0, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false)), color);
        }

        public static SerializedProperty GetLastArrayElement(this SerializedProperty arrayProperty)
        {
            return arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
        }
    }
}