using System.Collections.Generic;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class BurstWaveBehavior : WaveBehavior
    {
        public int BurstCount { get; set; }

        public List<BurstData> burstData;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            burstData = new List<BurstData>();

            float duration = (float) playable.GetDuration();

            if(BurstCount> 0)
            {
                for(int i = 0; i < BurstCount; i++)
                {
                    float time = duration / BurstCount * i;

                    burstData.Add(new BurstData { time = time, count = EnemiesCount });
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            float time = (float) playable.GetTime();

            for(int i = 0; i < burstData.Count; i++)
            {
                var data = burstData[i];

                if (data.time < time)
                {
                    StageController.EnemiesSpawner.Spawn(EnemyType, WaveOverride, CircularSpawn, data.count);

                    burstData.RemoveAt(i);
                    i--;
                }
                else break;
            }
        }

        public struct BurstData
        {
            public float time;
            public int count;
        }
    }
}