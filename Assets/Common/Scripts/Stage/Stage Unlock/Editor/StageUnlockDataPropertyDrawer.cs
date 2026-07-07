using System;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    [CustomPropertyDrawer(typeof(StageUnlockData))]
    public class StageUnlockDataPropertyDrawer : PropertyDrawer
    {
        public SerializedProperty ListProperty { get; protected set; }
        public SerializedProperty UnlockDescriptionProperty { get; protected set; }

        protected GUIStyle headerStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ListProperty = property.FindPropertyRelative("unlockConditions");
            UnlockDescriptionProperty = property.FindPropertyRelative("unlockDescription");

            CheckForEmpty();

            if(headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 16;
            }

            EditorGUILayout.LabelField("Unlock Conditions", headerStyle);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            var unlockLabel = new GUIContent("Unlock Description", "This text explains the player how to unlock this stage. Leave it empty if there are no special unlock conditions");
            GUILayout.Label(unlockLabel, GUILayout.Width(120));
            UnlockDescriptionProperty.stringValue = GUILayout.TextArea(UnlockDescriptionProperty.stringValue, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();

            for (int i = 0; i < ListProperty.arraySize; i++)
            {
                var conditionProperty = ListProperty.GetArrayElementAtIndex(i);

                DrawCondition(conditionProperty, i);

                SerializedPropertyExtensions.DrawHorizontalSeparator();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (!HasAllConditions()) DrawAddConditionButton();
        }

        protected virtual void CheckForEmpty()
        {
            if (ListProperty.arraySize == 0)
            {
                ListProperty.arraySize = 1;

                var previousCondition = new PreviousStageCompletedCondition();
                ListProperty.GetArrayElementAtIndex(0).managedReferenceValue = previousCondition;
            }
        }

        protected virtual void DrawCondition(SerializedProperty conditionProperty, int index)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(350));

            var condition = conditionProperty.managedReferenceValue;
            if (condition is PreviousStageCompletedCondition)
            {
                DrawPrevCondition(conditionProperty, index);
            }
            else if (condition is AllStagesCompletedCondition)
            {
                DrawAllCompleteCondition(conditionProperty, index);
            }
            else if (condition is AnyStageCompletedCondition)
            {
                DrawOneOfCompleteCondition(conditionProperty, index);
            }
            else if (condition is PurchasedCondition)
            {
                DrawPurchasedCondition(conditionProperty, index);
            } 
            else if(condition is AlwaysUnlockedCondition)
            {
                DrawAlwaysUnlockedCondition(conditionProperty, index);
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            DrawRemoveButton(index);

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawPrevCondition(SerializedProperty conditionProperty, int index)
        {
            EditorGUILayout.LabelField("Previous Stage Completed", EditorStyles.boldLabel);
        }

        protected virtual void DrawAllCompleteCondition(SerializedProperty conditionProperty, int index)
        {
            EditorGUILayout.LabelField("All Stages Completed", EditorStyles.boldLabel);

            var stagesProperty = conditionProperty.FindPropertyRelative("stages");
            EditorGUILayout.PropertyField(stagesProperty);
        }

        protected virtual void DrawOneOfCompleteCondition(SerializedProperty conditionProperty, int index)
        {
            EditorGUILayout.LabelField("One Of Stages Completed", EditorStyles.boldLabel);

            var stagesProperty = conditionProperty.FindPropertyRelative("stages");
            EditorGUILayout.PropertyField(stagesProperty);
        }

        protected virtual void DrawPurchasedCondition(SerializedProperty conditionProperty, int index)
        {
            EditorGUILayout.LabelField("Purchased", EditorStyles.boldLabel);

            var priceProperty = conditionProperty.FindPropertyRelative("price");
            EditorGUILayout.PropertyField(priceProperty);
        }

        protected virtual void DrawAlwaysUnlockedCondition(SerializedProperty conditionProperty, int index)
        {
            EditorGUILayout.LabelField("Always Unlocked", EditorStyles.boldLabel);
        }

        protected virtual void DrawRemoveButton(int index)
        {
            if (!GUILayout.Button("x", GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                return;

            ListProperty.DeleteArrayElementAtIndex(index);
        }

        protected virtual void DrawAddConditionButton()
        {
            if (!GUILayout.Button("Add Condition", GUILayout.Width(100)))
                return;

            var menu = new GenericMenu();

            var types = StageUnlockConditionTypeCache.GetTypes();

            foreach (var (type, meta) in types)
            {
                if (HasCondition(type)) continue;
                menu.AddItem(new GUIContent(meta.MenuName), false, () =>
                {
                    AddCondition(type);
                });
            }

            menu.ShowAsContext();
        }

        protected bool HasCondition(Type conditionType)
        {
            for(int i = 0; i < ListProperty.arraySize; i++)
            {
                var condition = ListProperty.GetArrayElementAtIndex(i).managedReferenceValue;
                if (condition.GetType() == conditionType) return true;
            }

            return false;
        }

        protected bool HasAllConditions()
        {
            return ListProperty.arraySize == StageUnlockConditionTypeCache.GetTypes().Count;
        }

        protected virtual void AddCondition(Type type)
        {
            Undo.RecordObject(ListProperty.serializedObject.targetObject, "Add Unlock Condition");

            var condition = Activator.CreateInstance(type);
            ListProperty.arraySize++;
            ListProperty.GetLastArrayElement().managedReferenceValue = condition;

            ListProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}