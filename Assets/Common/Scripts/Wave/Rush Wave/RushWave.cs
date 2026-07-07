using UnityEngine;

namespace OctoberStudio.Timeline
{
    public abstract class RushWave : WaveAsset
    {
        [SerializeField] protected RushSpawn rushSpawnData = new RushSpawn();
        public RushSpawn RushSpawnData => rushSpawnData;
    }
}