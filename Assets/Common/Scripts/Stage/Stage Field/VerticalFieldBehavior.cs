using VXMonster.Core.Easing;
using VXMonster.Core.Extensions;
using VXMonster.Core.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    public class VerticalFieldBehavior : AbstractFieldBehavior
    {
        protected List<PoolComponent<StageChunkBehavior>> backgroundPools;
        protected PoolComponent<Transform> leftPool;
        protected PoolComponent<Transform> rightPool;

        protected List<StageChunkBehavior> chunks = new List<StageChunkBehavior>();

        protected bool wait = false;

        public override void Init(StageFieldData stageFieldData, bool spawnProp)
        {
            base.Init(stageFieldData, spawnProp);

            foreach (var background in stageFieldData.GetBackgroundPrefabs())
            {
                var pool = new PoolComponent<StageChunkBehavior>(background, 6);
            }

            backgroundPools = new List<PoolComponent<StageChunkBehavior>>();
            foreach (var background in stageFieldData.GetBackgroundPrefabs())
            {
                var pool = new PoolComponent<StageChunkBehavior>(background, 3);
                backgroundPools.Add(pool);
            }

            if(stageFieldData.LeftPrefab != null )
            {
                leftPool = new PoolComponent<Transform>("Field Left Border", stageFieldData.LeftPrefab, 4);
            }

            if (stageFieldData.RightPrefab != null)
            {
                rightPool = new PoolComponent<Transform>("Field Right Border", stageFieldData.RightPrefab, 4);
            }

            var chunk = backgroundPools.Random().GetEntity();

            chunk.transform.position = Vector3.zero;
            chunk.transform.rotation = Quaternion.identity;
            chunk.transform.localScale = Vector3.one;

            AddBordersToChunk(chunk);

            chunks.Add(chunk);

            wait = false;
            EasingManager.DoNextFrame().SetOnFinish(() => wait = true);
        }

        public override void Update()
        {
            if (!wait) return;

            RemoveInvisibleChunks();
            CheckForNewChunks();
        }

        #region Add New Chunks

        protected virtual void CheckForNewChunks()
        {
            TryAddTopRow();
            TryAddBottomRow();
        }

        protected virtual void TryAddTopRow()
        {
            if (chunks[0].HasEmptyTop)
            {
                var chunk = backgroundPools.Random().GetEntity();
                var chunkBellow = chunks[0];

                chunk.transform.position = chunkBellow.transform.position + Vector3.up * chunk.Size.y;
                chunk.transform.rotation = Quaternion.identity;
                chunk.transform.localScale = Vector3.one;

                AddBordersToChunk(chunk);
                SpawnProp(chunk);

                chunks.Insert(0, chunk);
            }
        }

        protected virtual void TryAddBottomRow()
        {
            if (chunks[^1].HasEmptyBottom)
            {
                var chunk = backgroundPools.Random().GetEntity();
                var chunkOnTop = chunks[^1];

                chunk.transform.position = chunkOnTop.transform.position + Vector3.down * chunk.Size.y;
                chunk.transform.rotation = Quaternion.identity;
                chunk.transform.localScale = Vector3.one;

                AddBordersToChunk(chunk);
                SpawnProp(chunk);

                chunks.Add(chunk);
            }
        }

        protected virtual void AddBordersToChunk(StageChunkBehavior chunk)
        {
            if (leftPool != null)
            {
                var leftBorder = leftPool.GetEntity();
                leftBorder.transform.position = chunk.transform.position + Vector3.left * chunk.Size.x;

                chunk.AddBorder(leftBorder);
            }

            if (rightPool != null)
            {
                var rightBorder = rightPool.GetEntity();
                rightBorder.transform.position = chunk.transform.position + Vector3.right * chunk.Size.x;

                chunk.AddBorder(rightBorder);
            }
        }

        #endregion

        #region Remove Invisible Chunks

        protected virtual void RemoveInvisibleChunks()
        {
            if (chunks.Count == 0) return;

            CheckTopRow();
            CheckBottomRow();
        }

        protected virtual void CheckTopRow()
        {
            if (!chunks[0].IsVisible)
            {
                RemoveTopRow(); 
            }
        }

        protected virtual void CheckBottomRow()
        {
            if (!chunks[^1].IsVisible)
            {
                RemoveBottomRow();
            }
        }

        protected virtual void RemoveTopRow()
        {
            chunks[0].Clear();
            chunks.RemoveAt(0);
        }

        protected virtual void RemoveBottomRow()
        {
            chunks[^1].Clear();
            chunks.RemoveAt(chunks.Count - 1);
        }

        #endregion


        public override bool ValidatePosition(Vector2 position)
        {
            if (position.x > chunks[0].transform.position.x + chunks[0].Size.x / 2 + Data.RightMargin) return false;
            if (position.x < chunks[0].transform.position.x - chunks[0].Size.x / 2 - Data.LeftMargin) return false;

            return true;
        }

        public override Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset)
        {
            var path = end - start;
            var distance = path.magnitude;

            // start == end
            if (distance < Mathf.Epsilon) return start;

            var direction = path.normalized;

            float tx1, tx2;

            // Create rectangle with offset
            var rect = new Rect(chunks[0].LeftBound + offset, chunks[0].BottomBound + offset, (chunks[0].Size.x - offset) * 2, (chunks[0].Size.y - offset) * 2);

            // X axis
            if (Mathf.Abs(path.x) > Mathf.Epsilon)
            {
                // Calculate intersection points
                tx1 = (rect.xMin - start.x) / path.x;
                tx2 = (rect.xMax - start.x) / path.x;

                // Swapping if needed
                if (tx1 > tx2) (tx1, tx2) = (tx2, tx1);
            }
            else
            {
                // No intersection
                tx1 = float.NegativeInfinity;
                tx2 = float.PositiveInfinity;
            }

            var tEnter = tx1;
            var tExit = tx2;

            // No intersection
            if (tExit < 0f || tEnter > tExit) return start;

            return start + path * tExit;
        }

        public override Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset)
        {
            var playerPosition = PlayerBehavior.Player.transform.position.XY();

            if (fence is CircleFenceBehavior circleFence)
            {
                var halfSize = (chunks[0].Size.x + Data.LeftMargin + Data.RightMargin) / 2f;
                if (circleFence.Radius <= halfSize)
                {
                    circleFence.SetRadiusOverride(halfSize * 1.1f);
                }

                var positionWithOffset = new Vector2(0, playerPosition.y + offset.y);
                if (Vector3.Distance(positionWithOffset, playerPosition) < circleFence.Radius)
                {
                    return positionWithOffset;
                }
                else
                {
                    return new Vector2(0, playerPosition.y);
                }
            }
            else if (fence is RectFenceBehavior rectFence)
            {
                var maxSize = chunks[0].Size.x + Data.LeftMargin + Data.RightMargin;
                if (rectFence.Width <= maxSize)
                {
                    rectFence.SetSizeOverride(maxSize * 1.1f, rectFence.Height);
                }

                return new Vector2(0, playerPosition.y + offset.y);
            }

            return playerPosition + offset;
        }

        public override Vector2 GetRandomPositionOnBorder()
        {
            float x = Random.Range(-chunks[0].Size.x / 2 - Data.LeftMargin, chunks[0].Size.x / 2 + Data.RightMargin) + chunks[0].transform.position.x;

            float sign = Random.Range(0, 2) * 2 - 1;
            float y = PlayerBehavior.Player.transform.position.y + CameraManager.HalfHeight * 1.05f * sign;

            return new Vector2(x, y);
        }

        public override bool IsPointOutsideRight(Vector2 point, out float distance)
        {
            var rightBound = chunks[0].RightBound + Data.RightMargin;
            bool result = point.x > rightBound;
            distance = result ? point.x - rightBound : 0;
            return result;
        }

        public override bool IsPointOutsideLeft(Vector2 point, out float distance)
        {
            var leftBound = chunks[0].LeftBound - Data.LeftMargin;
            bool result = point.x < leftBound;
            distance = result ? leftBound - point.x : 0;
            return result;
        }

        public override bool IsPointOutsideTop(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public override bool IsPointOutsideBottom(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public override void RemovePropFromBossFence(BossFenceBehavior fence)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].RemovePropFromBossFence(fence);
            }
        }

        public override void Clear()
        {
            for(int i = 0; i < backgroundPools.Count; i++)
            {
                backgroundPools[i].Destroy();
            }

            backgroundPools.Clear();
        }
    }
}