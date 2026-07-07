using VXMonster.Core.Easing;
using VXMonster.Core.Extensions;
using VXMonster.Core.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    public class HorizontalFieldBehavior : AbstractFieldBehavior
    {
        protected List<PoolComponent<StageChunkBehavior>> backgroundPools;
        protected PoolComponent<Transform> topPool;
        protected PoolComponent<Transform> bottomPool;

        protected List<StageChunkBehavior> chunks = new List<StageChunkBehavior>();

        protected bool wait = false;

        public override void Init(StageFieldData stageFieldData, bool spawnProp)
        {
            base.Init(stageFieldData, spawnProp);

            backgroundPools = new List<PoolComponent<StageChunkBehavior>>();
            foreach (var background in stageFieldData.GetBackgroundPrefabs())
            {
                var pool = new PoolComponent<StageChunkBehavior>(background, 3);
                backgroundPools.Add(pool);
            }

            if (stageFieldData.LeftPrefab != null)
            {
                topPool = new PoolComponent<Transform>("Field Top Border", stageFieldData.TopPrefab, 4);
            }

            if (stageFieldData.RightPrefab != null)
            {
                bottomPool = new PoolComponent<Transform>("Field Bottom Border", stageFieldData.BottomPrefab, 4);
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
            TryAddRightRow();
            TryAddLeftRow();
        }

        protected virtual void TryAddRightRow()
        {
            if (chunks[0].HasEmptyRight)
            {
                var chunk = backgroundPools.Random().GetEntity();
                var chunkBellow = chunks[0];

                chunk.transform.position = chunkBellow.transform.position + Vector3.right * chunk.Size.y;
                chunk.transform.rotation = Quaternion.identity;
                chunk.transform.localScale = Vector3.one;

                AddBordersToChunk(chunk);
                SpawnProp(chunk);

                chunks.Insert(0, chunk);
            }
        }

        protected virtual void TryAddLeftRow()
        {
            if (chunks[^1].HasEmptyLeft)
            {
                var chunk = backgroundPools.Random().GetEntity();
                var chunkOnTop = chunks[^1];

                chunk.transform.position = chunkOnTop.transform.position + Vector3.left * chunk.Size.y;
                chunk.transform.rotation = Quaternion.identity;
                chunk.transform.localScale = Vector3.one;

                AddBordersToChunk(chunk);
                SpawnProp(chunk);

                chunks.Add(chunk);
            }
        }

        protected virtual void AddBordersToChunk(StageChunkBehavior chunk)
        {
            if (topPool != null)
            {
                var topBorder = topPool.GetEntity();
                topBorder.transform.position = chunk.transform.position + Vector3.up * chunk.Size.y;

                chunk.AddBorder(topBorder);
            }

            if (bottomPool != null)
            {
                var bottomBorder = bottomPool.GetEntity();
                bottomBorder.transform.position = chunk.transform.position + Vector3.down * chunk.Size.y;

                chunk.AddBorder(bottomBorder);
            }
        }

        #endregion

        #region Remove Invisible Chunks

        protected virtual void RemoveInvisibleChunks()
        {
            if (chunks.Count == 0) return;

            CheckRightRow();
            CheckLeftRow();
        }

        protected virtual void CheckRightRow()
        {
            if (!chunks[0].IsVisible)
            {
                RemoveRightRow();
            }
        }

        protected virtual void CheckLeftRow()
        {
            if (!chunks[^1].IsVisible)
            {
                RemoveLeftRow();
            }
        }

        protected virtual void RemoveRightRow()
        {
            chunks[0].Clear();
            chunks.RemoveAt(0);
        }

        protected virtual void RemoveLeftRow()
        {
            chunks[^1].Clear();
            chunks.RemoveAt(chunks.Count - 1);
        }

        #endregion


        public override bool ValidatePosition(Vector2 position)
        {
            if (position.y > chunks[0].transform.position.y + chunks[0].Size.y / 2 + Data.TopMargin) return false;
            if (position.y < chunks[0].transform.position.y - chunks[0].Size.y / 2 - Data.BottomMargin) return false;

            return true;
        }

        public override Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset)
        {
            var path = end - start;
            var distance = path.magnitude;

            // start == end
            if (distance < Mathf.Epsilon) return start;

            var direction = path.normalized;

            float ty1, ty2;

            // Create rectangle with offset
            var rect = new Rect(chunks[0].LeftBound + offset, chunks[0].BottomBound + offset, (chunks[0].Size.x - offset) * 2, (chunks[0].Size.y - offset) * 2);

            // Y axis
            if (Mathf.Abs(path.y) > Mathf.Epsilon)
            {
                // Calculate intersection points
                ty1 = (rect.yMin - start.y) / path.y;
                ty2 = (rect.yMax - start.y) / path.y;

                // Swapping if needed
                if (ty1 > ty2) (ty1, ty2) = (ty2, ty1);
            }
            else
            {
                // No intersection
                ty1 = float.NegativeInfinity;
                ty2 = float.PositiveInfinity;
            }

            var tEnter = ty1;
            var tExit = ty2;

            // No intersection
            if (tExit < 0f || tEnter > tExit) return start;

            return start + path * tExit;
        }

        public override Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset)
        {
            var playerPosition = PlayerBehavior.Player.transform.position.XY();

            if (fence is CircleFenceBehavior circleFence)
            {
                var halfSize = (chunks[0].Size.y + Data.BottomMargin + Data.TopMargin) / 2f;
                if(circleFence.Radius <= halfSize)
                {
                    circleFence.SetRadiusOverride(halfSize * 1.1f);
                }

                var positionWithOffset = new Vector2(playerPosition.x, offset.y);
                if(Vector3.Distance(positionWithOffset, playerPosition) < circleFence.Radius)
                {
                    return positionWithOffset;
                } else
                {
                    return new Vector2(playerPosition.x, 0);
                }
            }
            else if(fence is RectFenceBehavior rectFence)
            {
                var size = chunks[0].Size.y + Data.BottomMargin + Data.TopMargin;
                if (rectFence.Height <= size)
                {
                    rectFence.SetSizeOverride(rectFence.Width, size * 1.1f);
                }

                return new Vector2(playerPosition.x, 0);
            }

            return playerPosition + offset;
        }

        public override Vector2 GetRandomPositionOnBorder()
        {
            float y = Random.Range(-chunks[0].Size.y / 2 - Data.BottomMargin, chunks[0].Size.y / 2 + Data.TopMargin) + chunks[0].transform.position.y;

            float sign = Random.Range(0, 2) * 2 - 1;
            float x = PlayerBehavior.Player.transform.position.x + CameraManager.HalfWidth * 1.05f * sign;

            return new Vector2(x, y);
        }

        public override bool IsPointOutsideRight(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public override bool IsPointOutsideLeft(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public override bool IsPointOutsideTop(Vector2 point, out float distance)
        {
            var topBound = chunks[0].TopBound + Data.TopMargin;
            bool result = point.y > topBound;
            distance = result ? point.y - topBound : 0;
            return result;
        }

        public override bool IsPointOutsideBottom(Vector2 point, out float distance)
        {
            var bottomBound = chunks[0].BottomBound - Data.BottomMargin;
            bool result = point.y < bottomBound;
            distance = result ? bottomBound - point.y : 0;
            return result;
        }

        public override void RemovePropFromBossFence(BossFenceBehavior fence)
        {
            for(int i = 0; i < chunks.Count; i++)
            {
                chunks[i].RemovePropFromBossFence(fence);
            }
        }

        public override void Clear()
        {
            for (int i = 0; i < backgroundPools.Count; i++)
            {
                backgroundPools[i].Destroy();
            }

            backgroundPools.Clear();
        }
    }
}