using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class EndlessFieldBehavior : AbstractFieldBehavior
    {
        protected List<PoolComponent<StageChunkBehavior>> pools;

        protected List<List<StageChunkBehavior>> chunks = new List<List<StageChunkBehavior>>();

        protected bool wait = false;

        public override void Init(StageFieldData stageFieldData, bool spawnProp)
        {
            base.Init(stageFieldData, spawnProp);

            pools = new List<PoolComponent<StageChunkBehavior>>();
            foreach (var background in stageFieldData.GetBackgroundPrefabs())
            {
                var pool = new PoolComponent<StageChunkBehavior>(background, 6);
                pools.Add(pool);
            }

            var row = new List<StageChunkBehavior>();
            var chunk = pools.Random().GetEntity();

            chunk.transform.position = Vector3.zero;
            chunk.transform.rotation = Quaternion.identity;
            chunk.transform.localScale = Vector3.one;

            row.Add(chunk);
            chunks.Add(row);

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
            CheckForEmptyField();
            TryAddTopRow();
            TryAddBottomRow();
            TryAddLeftColumn();
            TryAddRightColumn();
        }

        protected virtual void TryAddTopRow()
        {
            if (chunks[0][0].HasEmptyTop)
            {
                var newTopRow = new List<StageChunkBehavior>();

                var columnCounts = chunks[0].Count;

                for (int i = 0; i < columnCounts; i++)
                {
                    var chunk = pools.Random().GetEntity();
                    var chunkBellow = chunks[0][i];

                    chunk.transform.position = chunkBellow.transform.position + Vector3.up * chunk.Size.y;
                    chunk.transform.rotation = Quaternion.identity;
                    chunk.transform.localScale = Vector3.one;

                    SpawnProp(chunk);

                    newTopRow.Add(chunk);
                }

                chunks.Insert(0, newTopRow);
            }
        }

        protected virtual void TryAddBottomRow()
        {
            if (chunks[^1][^1].HasEmptyBottom)
            {
                var newBottomRow = new List<StageChunkBehavior>();

                var columnCounts = chunks[0].Count;

                for (int i = 0; i < columnCounts; i++)
                {
                    var chunk = pools.Random().GetEntity();
                    var chunkOnTop = chunks[^1][i];

                    chunk.transform.position = chunkOnTop.transform.position + Vector3.down * chunk.Size.y;
                    chunk.transform.rotation = Quaternion.identity;
                    chunk.transform.localScale = Vector3.one;

                    SpawnProp(chunk);

                    newBottomRow.Add(chunk);
                }

                chunks.Add(newBottomRow);
            }
        }

        protected virtual void TryAddLeftColumn()
        {
            if (chunks[0][0].HasEmptyLeft)
            {
                for(int i = 0; i < chunks.Count; i++)
                {
                    var row = chunks[i];

                    var chunk = pools.Random().GetEntity();
                    var chunkOnRight = chunks[i][0];

                    chunk.transform.position = chunkOnRight.transform.position + Vector3.left * chunk.Size.x;
                    chunk.transform.rotation = Quaternion.identity;
                    chunk.transform.localScale = Vector3.one;

                    SpawnProp(chunk);

                    row.Insert(0, chunk);
                }
            }
        }

        protected virtual void TryAddRightColumn()
        {
            if (chunks[^1][^1].HasEmptyRight)
            {
                for (int i = 0; i < chunks.Count; i++)
                {
                    var row = chunks[i];

                    var chunk = pools.Random().GetEntity();
                    var chunkOnLeft = chunks[i][^1];

                    chunk.transform.position = chunkOnLeft.transform.position + Vector3.right * chunk.Size.x;
                    chunk.transform.rotation = Quaternion.identity;
                    chunk.transform.localScale = Vector3.one;

                    SpawnProp(chunk);

                    row.Add(chunk);
                }
            }
        }

        #endregion

        #region Remove Invisible Chunks

        protected virtual void RemoveInvisibleChunks()
        {
            if (chunks.Count == 0) return;

            CheckTopRow();
            CheckForEmptyField();
            CheckLeftColumn();
            CheckForEmptyField();
            CheckRightColumn();
            CheckForEmptyField();
            CheckBottomRow();
            CheckForEmptyField();
        }

        protected virtual void CheckForEmptyField()
        {
            if(chunks.Count > 0)
            {
                if (chunks[0].Count > 0) return;
            }

            chunks.Clear();

            var row = new List<StageChunkBehavior>();
            var chunk = pools.Random().GetEntity();

            chunk.transform.position = PlayerBehavior.Player.transform.position.SetZ(0);
            chunk.transform.rotation = Quaternion.identity;
            chunk.transform.localScale = Vector3.one;

            row.Add(chunk);
            chunks.Add(row);
        }

        protected virtual void CheckTopRow()
        {
            if (!chunks[0][0].IsVisible)
            {
                if (!chunks[0][^1].IsVisible)
                {
                    RemoveTopRow();
                }
            }
        }

        protected virtual void CheckLeftColumn()
        {
            if (!chunks[0][0].IsVisible)
            {
                if (!chunks[^1][0].IsVisible)
                {
                    RemoveLeftColumn();
                }
            }
        }

        protected virtual void CheckRightColumn()
        {
            if (!chunks[^1][^1].IsVisible)
            {
                if (!chunks[0][^1].IsVisible)
                {
                    RemoveRightColumn();
                }
            }
        }

        protected virtual void CheckBottomRow()
        {
            if (!chunks[^1][^1].IsVisible)
            {
                if (!chunks[^1][0].IsVisible)
                {
                    RemoveBottomRow();
                }
            }
        }

        protected virtual void RemoveTopRow()
        {
            var topRow = chunks[0];

            for (int i = 0; i < topRow.Count; i++)
            {
                var chunk = topRow[i];

                chunk.Clear();
            }

            topRow.Clear();
            chunks.RemoveAt(0);
        }

        protected virtual void RemoveLeftColumn()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                var row = chunks[i];

                row[0].Clear();
                row.RemoveAt(0);
            }
        }

        protected virtual void RemoveRightColumn()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                var row = chunks[i];

                row[^1].Clear();
                row.RemoveAt(row.Count - 1);
            }
        }

        protected virtual void RemoveBottomRow()
        {
            var bottomRow = chunks[^1];

            for (int i = 0; i < bottomRow.Count; i++)
            {
                var chunk = bottomRow[i];

                chunk.Clear();
            }

            bottomRow.Clear();
            chunks.RemoveAt(chunks.Count - 1);
        }

        #endregion

        public override bool ValidatePosition(Vector2 position) => true;

        public override Vector2 GetRandomPositionOnBorder() => Vector2.zero;

        public override Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset)
        {
            var playerPosition = PlayerBehavior.Player.transform.position.XY();
            return playerPosition + offset;
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
            distance = 0;
            return false;
        }

        public override bool IsPointOutsideBottom(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public override Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset)
        {
            return end;
        }

        public override void RemovePropFromBossFence(BossFenceBehavior fence)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                for(int j = 0; j < chunks[i].Count; j++)
                {
                    chunks[i][j].RemovePropFromBossFence(fence);
                }
            }
        }

        public override void Clear()
        {
            for(int i = 0; i < pools.Count; i++)
            {
                pools[i].Destroy();
            }

            pools.Clear();
        }
    }
}