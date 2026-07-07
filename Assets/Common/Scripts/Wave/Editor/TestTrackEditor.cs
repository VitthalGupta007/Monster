using OctoberStudio.Timeline;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace OctoberStudio.Timeline.Editor
{
    [CustomTimelineEditor(typeof(WaveTrack))]
    public class TestTrackEditor : TrackEditor
    {
        private EnemiesDatabase database;
        public EnemiesDatabase Database
        {
            get
            {
                if (database == null)
                {
                    string[] guiID = AssetDatabase.FindAssets("t:EnemiesDatabase");

                    if (guiID != null)
                    {
                        database = AssetDatabase.LoadAssetAtPath<EnemiesDatabase>(AssetDatabase.GUIDToAssetPath(guiID[0]));
                    }
                }

                return database;
            }
        }

        public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding)
        {
            var waveTrack = (WaveTrack)track;

            waveTrack.name = $"Track ({waveTrack.EnemyType})";

            var options = base.GetTrackOptions(track, binding);
            options.trackColor = Color.green;

            var data = Database.GetEnemyData(waveTrack.EnemyType);

            options.minimumHeight = 30;

            if(data.Icon != null)
            {
                options.icon = data.Icon.texture;
            }

            return options;
        }
    }
}