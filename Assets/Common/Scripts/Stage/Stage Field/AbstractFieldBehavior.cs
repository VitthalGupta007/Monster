using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public abstract class AbstractFieldBehavior : IFieldBehavior
    {
        public StageFieldData Data { get; private set; }

        private bool spawnProp;

        public abstract void Clear();
        public abstract Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset);
        public abstract Vector2 GetRandomPositionOnBorder();
        public abstract bool IsPointOutsideBottom(Vector2 point, out float distance);
        public abstract bool IsPointOutsideLeft(Vector2 point, out float distance);
        public abstract bool IsPointOutsideRight(Vector2 point, out float distance);
        public abstract bool IsPointOutsideTop(Vector2 point, out float distance);
        public abstract void Update();
        public abstract bool ValidatePosition(Vector2 position);
        public abstract void RemovePropFromBossFence(BossFenceBehavior fence);
        public abstract Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset);

        private List<PoolComponent<PropBehavior>> propPools = new List<PoolComponent<PropBehavior>>();

        public virtual void Init(StageFieldData stageFieldData, bool spawnProp)
        {
            Data = stageFieldData;

            this.spawnProp = spawnProp;

            if (!spawnProp) return;

            for (int i = 0; i < stageFieldData.PropChances.Count; i++)
            {
                var propData = stageFieldData.PropChances[i];
                var pool = new PoolComponent<PropBehavior>(propData.Prefab, propData.MaxAmount * 9);

                propPools.Add(pool);
            }
        }

        protected void SpawnProp(StageChunkBehavior chunk)
        {
            if (!spawnProp) return;

            for (int i = 0; i < Data.PropChances.Count; i++)
            {
                var propData = Data.PropChances[i];

                if(Random.value * 100 < propData.Chance)
                {
                    int amount = Mathf.RoundToInt(Mathf.Lerp(1, propData.MaxAmount, Random.value));

                    var pool = propPools[i];
                    for (int j = 0; j < amount; j++)
                    {
                        var prop = pool.GetEntity();

                        chunk.AddProp(prop);
                    }
                }
            }
        }
    }
}