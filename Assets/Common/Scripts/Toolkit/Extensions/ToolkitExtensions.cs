using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OctoberStudio.Extensions
{
    public static class ToolkitExtensions
    {
        public static List<K> GetAssets<T, K>(this PlayableDirector director) where T : TrackAsset where K : PlayableAsset
        {
            var result = new List<K>();

            foreach (var output in director.playableAsset.outputs)
            {
                if (output.sourceObject is T track)
                {
                    foreach (var clip in track.GetClips())
                    {
                        if (clip.asset is K asset)
                        {
                            result.Add(asset);
                        }
                    }
                }
            }

            return result;
        }

        public static List<TimelineClip> GetClips<T, K>(this PlayableDirector director) where T : TrackAsset where K : PlayableAsset
        {
            var result = new List<TimelineClip>();

            foreach (var output in director.playableAsset.outputs)
            {
                if (output.sourceObject is T track)
                {
                    foreach (var clip in track.GetClips())
                    {
                        if (clip.asset is K)
                        {
                            result.Add(clip);
                        }
                    }
                }
            }

            return result;
        }
    }
}