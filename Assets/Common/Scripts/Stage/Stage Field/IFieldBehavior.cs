using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public interface IFieldBehavior
    {
        void Init(StageFieldData stageFieldData, bool spawnProp);
        void Update();
        void Clear();

        bool ValidatePosition(Vector2 position);
        Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset);
        Vector2 GetRandomPositionOnBorder();
        Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset);

        bool IsPointOutsideRight(Vector2 point, out float distance);
        bool IsPointOutsideLeft(Vector2 point, out float distance);
        bool IsPointOutsideTop(Vector2 point, out float distance);
        bool IsPointOutsideBottom(Vector2 point, out float distance);

        void RemovePropFromBossFence(BossFenceBehavior fence);
    }
}