using UnityEditor;
using UnityEngine;

namespace VXMonster.Core.Timeline.Editor
{
    [CustomPropertyDrawer(typeof(RushSpawn.SpawnData))]
    public class RushSpawnDataPropertyDrawer : PropertyDrawer
    {
        public const float CompassSize = 200;
        public static float PropertyHeight => CompassSize + 24 * 2;

        protected const float AngleSnapStep = 45f;
        protected const float AngleSnapThreshold = 6f;
        protected const float AngleLabelOffset = 4f;
        protected const float HandleSize = 6f;

        protected GUIStyle angleStyle;

        protected WidthPositionData wpData = new WidthPositionData();
        protected CompassData cData = new CompassData();

        protected SerializedProperty angleProperty;
        protected SerializedProperty widthProperty;
        protected SerializedProperty positionProperty;

        protected GUIContent tooltop;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            angleProperty = property.FindPropertyRelative("angle");
            widthProperty = property.FindPropertyRelative("width");
            positionProperty = property.FindPropertyRelative("position");

            cData.Init(position, angleProperty);

            DrawCompass();

            if(tooltop == null)
            {
                tooltop = new GUIContent("", "The width is a fraction (0 - 1) of a bigger side of the screen." +
                "For example, for a phone the bigger side is it's height, while for a pc monitor it's width.");
            }
            GUI.Label(position, tooltop);
        }

        protected virtual void DrawCompassRings()
        {
            var color = Handles.color;

            Handles.color = new Color(color.r, color.g, color.b, 0.5f);
            Handles.DrawWireDisc(cData.center, Vector3.forward, cData.radius, 2);
            Handles.DrawWireDisc(cData.center, Vector3.forward, cData.radius - 10, 1);
            Handles.color = color;
        }

        protected virtual void HandleCompasInput(SerializedProperty angleProp)
        {
            var e = Event.current;

            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (cData.RectContains(e.mousePosition))
                {
                    var delta = e.mousePosition - cData.center;
                    cData.angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

                    if (delta.magnitude >= cData.radius - 15)
                    {
                        ApplyAngleSnapping();
                    }

                    angleProp.floatValue = cData.angle;
                    angleProp.serializedObject.ApplyModifiedProperties();

                    e.Use();
                }
            }
        }

        protected virtual void DrawCompass()
        {
            DrawCompassRings();
            DrawArrow();
            
            if (!DrawWidthHandle(widthProperty, positionProperty))
            {
                DrawAngleLabels();
            }
            
            DrawAngleTicks();
            HandleCompasInput(angleProperty);
        }

        #region Handles

        protected virtual void DrawWidth(Vector2 center, float radius, float angleDeg)
        {
            Handles.color = Color.white;

            var angleRad = angleDeg * Mathf.Deg2Rad;

            var direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            var perpendicular = new Vector2(-direction.y, direction.x);

            var start = center - perpendicular * radius;
            var end = center + perpendicular * radius;

            Handles.DrawAAPolyLine(4, start + direction * 10, end + direction * 10);
            Handles.DrawAAPolyLine(4, start - direction * 10, end - direction * 10);

            Event.current.Use();
        }

        protected virtual void DrawWidthLinesAndHandles()
        {
            var fullLeftPoint = cData.center - cData.perpendicular * cData.radius;
            var fullRightpoint = cData.center + cData.perpendicular * cData.radius;

            Handles.color = new Color(1f, 1f, 1f, 0.15f);
            Handles.DrawAAPolyLine(2f, fullLeftPoint, fullRightpoint);

            wpData.leftHandle = wpData.offsetCenter - cData.perpendicular * wpData.currentHalfWidth;
            wpData.rightHandle = wpData.offsetCenter + cData.perpendicular * wpData.currentHalfWidth;

            Handles.color = Color.green;
            Handles.DrawAAPolyLine(3f, wpData.leftHandle, wpData.rightHandle);

            Handles.DrawSolidDisc(wpData.leftHandle, Vector3.forward, HandleSize);
            Handles.DrawSolidDisc(wpData.rightHandle, Vector3.forward, HandleSize);
        }

        protected virtual void HandleWidthInput()
        {
            var e = Event.current;

            wpData.widthControlId = GUIUtility.GetControlID(FocusType.Passive);

            bool nearA = IsNearHandle(e.mousePosition, wpData.leftHandle);
            bool nearB = IsNearHandle(e.mousePosition, wpData.rightHandle);

            switch (e.GetTypeForControl(wpData.widthControlId))
            {
                case EventType.MouseDown:
                    {
                        if (nearA || nearB)
                        {
                            GUIUtility.hotControl = wpData.widthControlId;
                            e.Use();
                        }
                        break;
                    }

                case EventType.MouseDrag:
                    {
                        if (GUIUtility.hotControl == wpData.widthControlId)
                        {
                            var mouseDeltaFromOffset = e.mousePosition - wpData.offsetCenter;

                            var projectedFromOffset = Vector2.Dot(mouseDeltaFromOffset, cData.perpendicular);

                            var newHalfWidth = Mathf.Abs(projectedFromOffset);

                            var offsetDistance = Mathf.Abs(Vector2.Dot(wpData.offsetCenter - cData.center, cData.perpendicular));
                            var maxHalfWidthAllowed = cData.radius - offsetDistance;

                            newHalfWidth = Mathf.Min(newHalfWidth, maxHalfWidthAllowed);

                            widthProperty.floatValue = Mathf.Clamp01(newHalfWidth / cData.radius);
                            
                            widthProperty.serializedObject.ApplyModifiedProperties();
                            e.Use();
                        }
                        break;
                    }

                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == wpData.widthControlId)
                        {
                            GUIUtility.hotControl = 0;
                            e.Use();
                        }
                        break;
                    }
            }
        }

        protected virtual void HandlePositionInput()
        {
            var e = Event.current;

            wpData.positionControlId = GUIUtility.GetControlID(FocusType.Passive);

            var distanceToLine = HandleUtility.DistancePointToLineSegment(Event.current.mousePosition, wpData.leftHandle, wpData.rightHandle);
            var isNearLine = distanceToLine < 10;

            switch (e.GetTypeForControl(wpData.positionControlId))
            {
                case EventType.MouseDown:
                    {
                        if (isNearLine)
                        {
                            GUIUtility.hotControl = wpData.positionControlId;
                            e.Use();
                        }
                        break;
                    }

                case EventType.MouseDrag:
                    {
                        if (GUIUtility.hotControl == wpData.positionControlId)
                        {
                            var mouseDeltaFromCenter = e.mousePosition - cData.center;
                            var projectedFromCenter = Vector2.Dot(mouseDeltaFromCenter, cData.perpendicular);

                            var newOffset = projectedFromCenter / wpData.maxOffset / 2f;
                            positionProperty.floatValue = Mathf.Clamp(newOffset, -wpData.maxOffsetNormalized, wpData.maxOffsetNormalized);

                            widthProperty.serializedObject.ApplyModifiedProperties();
                            e.Use();
                        }
                        break;
                    }

                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == wpData.positionControlId)
                        {
                            GUIUtility.hotControl = 0;
                            e.Use();

                            widthProperty.floatValue = wpData.widthNormalized;
                            widthProperty.serializedObject.ApplyModifiedProperties();
                        }
                        break;
                    }
            }
        }

        protected virtual bool DrawWidthHandle(SerializedProperty widthProperty, SerializedProperty offsetProperty)
        {
            wpData.Init(cData, widthProperty, offsetProperty);
            DrawWidthLinesAndHandles();

            HandleWidthInput();
            HandlePositionInput();

            return DrawWidthLabel();
        }

        protected virtual bool DrawWidthLabel()
        {
            var isInteracting = GUIUtility.hotControl == wpData.widthControlId || GUIUtility.hotControl == wpData.positionControlId;
            var isHovering = IsNearHandle(Event.current.mousePosition, wpData.leftHandle) ||
                              IsNearHandle(Event.current.mousePosition, wpData.rightHandle) ||
                              HandleUtility.DistancePointToLineSegment(Event.current.mousePosition, wpData.leftHandle, wpData.rightHandle) < 10;
            isHovering = false;
            var interacted = false;
            if (isInteracting || isHovering)
            {
                var text = $"{Mathf.RoundToInt(wpData.widthNormalized * 100f)}%";

                var labelPos = cData.center + cData.perpendicular * (cData.radius + 14f);

                if (Vector2.Dot(cData.perpendicular, Vector2.up) < 0f)
                {
                    labelPos = cData.center - cData.perpendicular * (cData.radius + 14f);
                }

                var style = new GUIStyle(EditorStyles.boldLabel);
                style.alignment = TextAnchor.MiddleCenter;

                Handles.BeginGUI();
                GUI.Label(new Rect(labelPos.x - 20, labelPos.y - 10, 40, 20), text);
                Handles.EndGUI();

                interacted = true;
            }

            return interacted;
        }


        protected virtual bool IsNearHandle(Vector2 mouse, Vector2 handlePos)
        {
            return Vector2.Distance(mouse, handlePos) <= HandleSize * 1.5f;
        }

        #endregion

        #region Angle

        protected virtual void DrawAngleLabels()
        {
            if(angleStyle == null) angleStyle = new GUIStyle(EditorStyles.boldLabel);

            for (int angle = 0; angle < 360; angle += 45)
            {
                var angleRad = angle * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

                var position = cData.center + direction * (cData.radius + AngleLabelOffset);

                angleStyle.alignment = GetAlignment(direction);

                Handles.color = (angle % 90 == 0) ? Color.white : new Color(1f, 1f, 1f, 0.5f);
                Handles.Label(position, angle + "�", angleStyle);
            }
        }

        protected virtual TextAnchor GetAlignment(Vector2 direction)
        {
            var x = direction.x;
            var y = direction.y;

            if (Mathf.Abs(x) < 0.3f)
                return y > 0 ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;

            if (Mathf.Abs(y) < 0.3f)
                return x > 0 ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;

            if (x > 0 && y > 0) return TextAnchor.UpperLeft;
            if (x > 0 && y < 0) return TextAnchor.LowerLeft;
            if (x < 0 && y > 0) return TextAnchor.UpperRight;

            return TextAnchor.LowerRight;
        }

        protected virtual void DrawArrow()
        {
            Handles.color = Color.white;

            var shaftLength = cData.radius * 0.65f;
            var headLength = cData.radius * 0.35f;

            var shaftWidth = cData.radius * 0.12f;
            var headWidth = cData.radius * 0.28f;

            var shaftStart = cData.center;
            var shaftEnd = cData.center + cData.direction * shaftLength;

            var tip = cData.center + cData.direction * cData.radius;

            var s1 = shaftStart + cData.perpendicular * (shaftWidth * 0.5f);
            var s2 = shaftStart - cData.perpendicular * (shaftWidth * 0.5f);
            var s3 = shaftEnd - cData.perpendicular * (shaftWidth * 0.5f);
            var s4 = shaftEnd + cData.perpendicular * (shaftWidth * 0.5f);

            Handles.DrawAAConvexPolygon(s1, s2, s3, s4);

            var hBaseCenter = shaftEnd;
            var h1 = hBaseCenter + cData.perpendicular * (headWidth * 0.5f);
            var h2 = hBaseCenter - cData.perpendicular * (headWidth * 0.5f);

            Handles.DrawAAConvexPolygon(h1, h2, tip);
        }

        protected virtual void DrawAngleTicks()
        {
            var handlesColor = Handles.color;

            for (int angle = 0; angle < 360; angle += 45)
            {
                var angleRad = angle * Mathf.Deg2Rad;

                var direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

                var isMajor = angle % 90 == 0;

                var length = isMajor ? cData.radius * 0.18f : cData.radius * 0.1f;
                var thickness = isMajor ? 2.5f : 1.5f;

                var start = cData.center + direction * (cData.radius - length);
                var end = cData.center + direction * cData.radius;

                var color = isMajor
                    ? new Color(1f, 1f, 1f, 0.9f)
                    : new Color(1f, 1f, 1f, 0.5f);

                Handles.color = color;
                Handles.DrawAAPolyLine(thickness, start, end);
            }

            Handles.color = handlesColor;
        }

        protected virtual void ApplyAngleSnapping()
        {
            var snapped = Mathf.Round(cData.angle / AngleSnapStep) * AngleSnapStep;

            if (Mathf.Abs(Mathf.DeltaAngle(cData.angle, snapped)) < AngleSnapThreshold)
            {
                cData.angle = snapped;
            }
        }

        #endregion

        protected class CompassData
        {
            public Rect rect;
            public Vector2 center;
            public float radius;
            public float angle;
            public Vector2 direction;
            public Vector2 perpendicular;

            public virtual void Init(Rect position, SerializedProperty angleProperty)
            {
                rect = new Rect(position.x + 24, position.y + 24, CompassSize, CompassSize);
                center = rect.center;
                radius = rect.width * 0.5f;
                angle = angleProperty.floatValue;

                direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                perpendicular = new Vector2(-direction.y, direction.x);
            }

            public virtual bool RectContains(Vector2 position)
            {
                return rect.Contains(position);
            }
        }

        protected class WidthPositionData
        {
            public int widthControlId;
            public int positionControlId;
            public Vector2 leftHandle;
            public Vector2 rightHandle;
            public float widthNormalized;
            public Vector2 offsetCenter;
            public float currentHalfWidth;
            public float maxHalfWidth;
            public float maxOffset;
            public float maxOffsetNormalized;

            public void Init(CompassData cData, SerializedProperty widthProperty, SerializedProperty offsetProperty)
            {
                widthNormalized = widthProperty.floatValue;

                float offsetNormalized = offsetProperty.floatValue;
                maxOffsetNormalized = 0.4f;
                if (offsetNormalized > maxOffsetNormalized) offsetNormalized = maxOffsetNormalized;
                if (offsetNormalized < -maxOffsetNormalized) offsetNormalized = -maxOffsetNormalized;

                if (offsetNormalized + widthNormalized / 2f > 0.5f)
                {
                    widthNormalized = (0.5f - offsetNormalized) * 2f;

                }
                if (offsetNormalized - widthNormalized / 2f < -0.5f)
                {
                    widthNormalized = (0.5f + offsetNormalized) * 2f;
                }

                maxHalfWidth = cData.radius;
                currentHalfWidth = widthNormalized * cData.radius;

                maxOffset = cData.radius * 0.9f;

                offsetCenter = cData.center + cData.perpendicular * (offsetNormalized * 2 * maxOffset);
            }
        }
    }
}