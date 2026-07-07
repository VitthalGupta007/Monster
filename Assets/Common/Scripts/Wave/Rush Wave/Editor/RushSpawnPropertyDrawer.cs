using UnityEditor;
using UnityEngine;

namespace VXMonster.Core.Timeline.Editor 
{
    [CustomPropertyDrawer(typeof(RushSpawn))]
    public class RushSpawnPropertyDrawer : PropertyDrawer
    {
        protected SerializedProperty dataTypeProperty;
        protected SerializedProperty spawnDataProperty;
        protected SerializedProperty additionalSpawnDataProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var dataTypeProperty = property.FindPropertyRelative("dataType");

            if (dataTypeProperty.intValue == 0 || EditorGUIUtility.currentViewWidth > (RushSpawnDataPropertyDrawer.CompassSize + 48f) * 2f)
            {
                return RushSpawnDataPropertyDrawer.PropertyHeight + EditorGUIUtility.singleLineHeight * 4;
            } else
            {
                return RushSpawnDataPropertyDrawer.PropertyHeight * 2f + EditorGUIUtility.singleLineHeight * 4;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            dataTypeProperty = property.FindPropertyRelative("dataType");
            spawnDataProperty = property.FindPropertyRelative("spawnData");

            var rect = DrawDataTypeProperty(position, dataTypeProperty);

            if(dataTypeProperty.intValue == 1)
            {
                additionalSpawnDataProperty = property.FindPropertyRelative("additionalSpawnData");

                if (EditorGUIUtility.currentViewWidth > (RushSpawnDataPropertyDrawer.CompassSize + 48f) * 2f)
                {
                    var spawnDatarect = DrawSpawnDataProperty(rect, spawnDataProperty);
                    DrawSpawnDataProperty(rect, additionalSpawnDataProperty, RushSpawnDataPropertyDrawer.CompassSize + 62f);
                    rect = spawnDatarect;
                }
                else
                {
                    rect = DrawSpawnDataProperty(rect, spawnDataProperty);
                    rect = DrawSpawnDataProperty(rect, additionalSpawnDataProperty);
                }
            } else
            {
                rect = DrawSpawnDataProperty(rect, spawnDataProperty);
            }

            rect.width = position.width;

            DrawLabels(rect);
        }

        protected virtual void DrawLabels(Rect position)
        {
            var angleLabelRect = new Rect(position) { y = position.y + position.height, height = EditorGUIUtility.singleLineHeight };
            var widthLabelRect = new Rect(angleLabelRect) { y = angleLabelRect.y + angleLabelRect.height };
            var positionLabelRect = new Rect(widthLabelRect) { y = widthLabelRect.y + widthLabelRect.height };

            DrawAngleLabel(angleLabelRect);
            DrawWidthLabel(widthLabelRect);
            DrawPositionLabel(positionLabelRect);
        }

        protected virtual void DrawPositionLabel(Rect position)
        {
            var label = "Position: ";

            var positionProperty = spawnDataProperty.FindPropertyRelative("position");
            var positionValue = positionProperty.floatValue;

            if (dataTypeProperty.intValue == 0)
            {
                label += positionValue.ToString("F2");
            }
            else
            {
                var additionalPositionProperty = additionalSpawnDataProperty.FindPropertyRelative("position");
                var additionalPositionValue = additionalPositionProperty.floatValue;

                var minPosition = Mathf.Min(positionValue, additionalPositionValue);
                var maxPosition = Mathf.Max(positionValue, additionalPositionValue);

                label += minPosition.ToString("F2") + " or " + maxPosition.ToString("F2");
            }

            EditorGUI.LabelField(position, label);
        }

        protected virtual void DrawWidthLabel(Rect position)
        {
            var label = "Width: ";

            var widthProperty = spawnDataProperty.FindPropertyRelative("width");
            var widthValue = widthProperty.floatValue;

            if (dataTypeProperty.intValue == 0)
            {
                label += widthValue.ToString("F2");
            }
            else
            {
                var additionalWidthProperty = additionalSpawnDataProperty.FindPropertyRelative("width");
                var additionalWidthValue = additionalWidthProperty.floatValue;

                var minWidth = Mathf.Min(widthValue, additionalWidthValue);
                var maxWidth = Mathf.Max(widthValue, additionalWidthValue);

                label += minWidth.ToString("F2") + " or " + maxWidth.ToString("F2");
            }

            EditorGUI.LabelField(position, label);
        }

        protected virtual void DrawAngleLabel(Rect position)
        {
            var label = "Angle: ";

            var angleProperty = spawnDataProperty.FindPropertyRelative("angle");
            var angleValue = angleProperty.floatValue;

            if (dataTypeProperty.intValue == 0)
            {
                label += angleValue.ToString("F1");
            } else
            {
                var additionalAngleProperty = additionalSpawnDataProperty.FindPropertyRelative("angle");
                var additionalAngleValue = additionalAngleProperty.floatValue;

                var minAngle = Mathf.Min(angleValue, additionalAngleValue);
                var maxAngle = Mathf.Max(angleValue, additionalAngleValue);

                label += minAngle.ToString("F1") + " or " + maxAngle.ToString("F1");
            }

            EditorGUI.LabelField(position, label);
        }

        protected virtual Rect DrawDataTypeProperty(Rect position, SerializedProperty property)
        { 
            var dataTypeRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            EditorGUI.PropertyField(dataTypeRect, property, new GUIContent("Rush Data Type"));

            return dataTypeRect;
        }

        protected virtual Rect DrawSpawnDataProperty(Rect position, SerializedProperty property, float xOffset = 0f)
        {
            var spawnDataRect = new Rect() { x = position.x + xOffset, y = position.y + position.height, width = RushSpawnDataPropertyDrawer.CompassSize + 62f, height = RushSpawnDataPropertyDrawer.PropertyHeight };
            EditorGUI.PropertyField(spawnDataRect, property);

            return spawnDataRect;
        }
    }
}