using OctoberStudio.Timeline;
using System;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace OctoberStudio.Timeline.Editor
{
    [CustomTimelineEditor(typeof(WaveAsset))]
    public class WaveEditor : ClipEditor
    {
        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var clipOptions = base.GetClipOptions(clip);

            if(clip.asset is BurstWave || clip.asset is BurstRushWave)
            {
                clipOptions.highlightColor = Color.blue;
                clip.displayName = "Burst";
            } else if (clip.asset is ContinuousWave)
            {
                clipOptions.highlightColor = Color.yellow;
                clip.displayName = "Continuous";
            } else if (clip.asset is MaintainWave)
            {
                clipOptions.highlightColor = Color.red;
                clip.displayName = "Maintain";
            }

            var fromTo = $"{Math.Round(clip.start, 2)}s - {Math.Round(clip.end, 2)}s";
            clipOptions.tooltip = fromTo;

            return clipOptions;
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            base.DrawBackground(clip, region);

            var rushClip = clip.asset as RushWave;
            if (rushClip == null)
                return;

            var rect = region.position;

            var angle = rushClip.RushSpawnData.Angle + 90;
            if(rushClip.RushSpawnData.Type == RushSpawn.DataType.RandomBetweenTwo)
            {
                angle = (angle + rushClip.RushSpawnData.AdditionalAngle + 90) / 2f;
            }

            DrawArrowIcon(rect, angle);
        }

        protected virtual void DrawArrowIcon(Rect rect, float angle)
        {
            var center = rect.center;

            var size = Mathf.Min(rect.width, rect.height) * 0.6f;

            var arrowRect = new Rect(
                center.x + rect.width / 2 - size,
                center.y - size * 0.5f,
                size * 0.8f,
                size);

            var prev = GUI.matrix;

            GUIUtility.RotateAroundPivot(angle, arrowRect.center);

            DrawTriangle(arrowRect);

            GUI.matrix = prev;
        }

        protected virtual void DrawTriangle(Rect rect)
        {
            var top = new Vector2(rect.center.x, rect.yMin);
            var left = new Vector2(rect.xMin, rect.yMax);
            var right = new Vector2(rect.xMax, rect.yMax);

            Handles.BeginGUI();
            Handles.color = Color.white;
            Handles.DrawAAConvexPolygon(top, left, right);
            Handles.EndGUI();
        }
    }
}