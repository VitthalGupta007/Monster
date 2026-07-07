using VXMonster.Core.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    public class RectFieldBehavior : AbstractFieldBehavior
    {
        StageChunkBehavior chunk;

        List<Transform> borders = new List<Transform>();

        public override void Init(StageFieldData stageFieldData, bool spawnProp)
        {
            base.Init(stageFieldData, spawnProp);

            chunk = Object.Instantiate(stageFieldData.GetBackgroundPrefabs().Random()).GetComponent<StageChunkBehavior>();

            chunk.transform.position = Vector3.zero;
            chunk.transform.rotation = Quaternion.identity;
            chunk.transform.localScale = Vector3.one;

            if (stageFieldData.TopPrefab != null)
            {
                var topBorder = Object.Instantiate(stageFieldData.TopPrefab).GetComponent<Transform>();
                topBorder.transform.position = Vector3.up * chunk.Size.y;
                borders.Add(topBorder);
            }

            if (stageFieldData.BottomPrefab != null)
            {
                var bottomBorder = Object.Instantiate(stageFieldData.BottomPrefab).GetComponent<Transform>();
                bottomBorder.transform.position = Vector3.down * chunk.Size.y;
                borders.Add(bottomBorder);
            }

            if (stageFieldData.LeftPrefab != null)
            {
                var leftBorder = Object.Instantiate(stageFieldData.LeftPrefab).GetComponent<Transform>();
                leftBorder.transform.position = Vector3.left * chunk.Size.x;
                borders.Add(leftBorder);
            }

            if (stageFieldData.RightPrefab != null)
            {
                var rightBorder = Object.Instantiate(stageFieldData.RightPrefab).GetComponent<Transform>();
                rightBorder.transform.position = Vector3.right * chunk.Size.x;
                borders.Add(rightBorder);
            }

            if (stageFieldData.TopLeftPrefab != null)
            {
                var topLeftCorner = Object.Instantiate(stageFieldData.TopLeftPrefab).GetComponent<Transform>();
                topLeftCorner.transform.position = new Vector2(-chunk.Size.x, chunk.Size.y);
                borders.Add(topLeftCorner);
            }

            if (stageFieldData.TopRightPrefab != null)
            {
                var topRightCorner = Object.Instantiate(stageFieldData.TopRightPrefab).GetComponent<Transform>();
                topRightCorner.transform.position = new Vector2(chunk.Size.x, chunk.Size.y);
                borders.Add(topRightCorner);
            }

            if (stageFieldData.BottomLeftPrefab != null)
            {
                var bottomLeftCorner = Object.Instantiate(stageFieldData.BottomLeftPrefab).GetComponent<Transform>();
                bottomLeftCorner.transform.position = new Vector2(-chunk.Size.x, -chunk.Size.y);
                borders.Add(bottomLeftCorner);
            }

            if (stageFieldData.BottomRightPrefab != null)
            {
                var bottomRightCorner = Object.Instantiate(stageFieldData.BottomRightPrefab).GetComponent<Transform>();
                bottomRightCorner.transform.position = new Vector2(chunk.Size.x, -chunk.Size.y);
                borders.Add(bottomRightCorner);
            }

            SpawnProp(chunk);
        }

        public override void Update()
        {

        }

        public override bool ValidatePosition(Vector2 position)
        {
            var halfSize = chunk.Size / 2f;

            if (position.x > chunk.transform.position.x + halfSize.x + Data.RightMargin) return false;
            if (position.x < chunk.transform.position.x - halfSize.x - Data.LeftMargin) return false;

            if (position.y > chunk.transform.position.y + halfSize.y + Data.TopMargin) return false;
            if (position.y < chunk.transform.position.y - halfSize.y - Data.BottomMargin) return false;

            return true;
        }

        public override Vector2 GetRandomPositionOnBorder()
        {
            var halfSize = chunk.Size / 2f;

            var leftBound = -halfSize.x - Data.LeftMargin;
            var rightBound = halfSize.x + Data.RightMargin;
            var bottomBound = -halfSize.y - Data.BottomMargin;
            var topBound = halfSize.y + Data.TopMargin;

            var random = Random.value;
            if (random <= 0.25f)
            {
                return new Vector2(leftBound, Random.Range(bottomBound, topBound));
            } 
            else if(random <= 0.5f)
            {
                return new Vector2(rightBound, Random.Range(bottomBound, topBound));
            }
            else if (random <= 0.75f)
            {
                return new Vector2(Random.Range(leftBound, rightBound), bottomBound);
            }
            else
            {
                return new Vector2(Random.Range(leftBound, rightBound), topBound);
            }
        }

        public override Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset)
        {
            var playerPosition = PlayerBehavior.Player.transform.position.XY();
            var desiredPosition = playerPosition + offset;

            var halfSize = chunk.Size / 2f;

            var leftBound = -halfSize.x - Data.LeftMargin;
            var rightBound = halfSize.x + Data.RightMargin;
            var bottomBound = -halfSize.y - Data.BottomMargin;
            var topBound = halfSize.y + Data.TopMargin;

            if (fence is CircleFenceBehavior circleFence)
            {
                bool tooCloseToLeftBorder = desiredPosition.x - circleFence.Radius < leftBound;
                bool tooCloseToRightBorder = desiredPosition.x + circleFence.Radius > rightBound;

                if (tooCloseToLeftBorder && tooCloseToRightBorder) 
                {
                    desiredPosition.x = 0;
                } else if (tooCloseToLeftBorder)
                {
                    desiredPosition.x = leftBound + circleFence.Radius;
                } else if (tooCloseToRightBorder)
                {
                    desiredPosition.x = rightBound - circleFence.Radius;
                }

                bool tooCloseToTopBorder = desiredPosition.y + circleFence.Radius > topBound;
                bool tooCloseToBottomBorder = desiredPosition.y - circleFence.Radius < bottomBound;

                if (tooCloseToTopBorder && tooCloseToBottomBorder)
                {
                    desiredPosition.y = 0;
                }
                else if (tooCloseToTopBorder)
                {
                    desiredPosition.y = topBound - circleFence.Radius;
                }
                else if (tooCloseToBottomBorder)
                {
                    desiredPosition.y = bottomBound + circleFence.Radius;
                }
            }
            else if (fence is RectFenceBehavior rectFence)
            {
                bool tooCloseToLeftBorder = desiredPosition.x - rectFence.Width / 2 < leftBound;
                bool tooCloseToRightBorder = desiredPosition.x + rectFence.Width / 2 > rightBound;

                if (tooCloseToLeftBorder && tooCloseToRightBorder)
                {
                    desiredPosition.x = 0;
                }
                else if (tooCloseToLeftBorder)
                {
                    desiredPosition.x = leftBound + rectFence.Width / 2;
                }
                else if (tooCloseToRightBorder)
                {
                    desiredPosition.x = rightBound - rectFence.Width / 2;
                }

                bool tooCloseToTopBorder = desiredPosition.y + rectFence.Height / 2 > topBound;
                bool tooCloseToBottomBorder = desiredPosition.y - rectFence.Height / 2 < bottomBound;

                if (tooCloseToTopBorder && tooCloseToBottomBorder)
                {
                    desiredPosition.y = 0;
                }
                else if (tooCloseToTopBorder)
                {
                    desiredPosition.y = topBound - rectFence.Height / 2;
                }
                else if (tooCloseToBottomBorder)
                {
                    desiredPosition.y = bottomBound + rectFence.Height / 2;
                }
            }

            return desiredPosition;
        }

        public override bool IsPointOutsideRight(Vector2 point, out float distance)
        {
            var rightBound = chunk.RightBound + Data.RightMargin;
            bool result = point.x > rightBound;
            distance = result ? point.x - rightBound : 0;
            return result;
        }

        public override bool IsPointOutsideLeft(Vector2 point, out float distance)
        {
            var leftBound = chunk.LeftBound - Data.LeftMargin;
            bool result = point.x < leftBound;
            distance = result ? leftBound - point.x : 0;
            return result;
        }

        public override bool IsPointOutsideTop(Vector2 point, out float distance)
        {
            var topBound = chunk.TopBound + Data.TopMargin;
            bool result = point.y > topBound;
            distance = result ? point.y - topBound : 0;
            return result;
        }

        public override bool IsPointOutsideBottom(Vector2 point, out float distance)
        {
            var bottomBound = chunk.BottomBound - Data.BottomMargin;
            bool result = point.y < bottomBound;
            distance = result ? bottomBound - point.y : 0;
            return result;
        }

        public override Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset)
        {
            var path = end - start;
            var distance = path.magnitude;

            // start == end
            if (distance < Mathf.Epsilon) return start;

            var direction = path.normalized;

            float tx1, tx2, ty1, ty2;

            // Create rectangle with offset
            var rect = new Rect(chunk.LeftBound + offset, chunk.BottomBound + offset, (chunk.Size.x - offset) * 2, (chunk.Size.y - offset) * 2);

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

            var tEnter = Mathf.Max(tx1, ty1);
            var tExit = Mathf.Min(tx2, ty2);

            // No intersection
            if (tExit < 0f || tEnter > tExit) return start;

            return start + path * tExit;
        }

        public override void RemovePropFromBossFence(BossFenceBehavior fence)
        {
            chunk.RemovePropFromBossFence(fence);
        }

        public override void Clear()
        {
            Object.Destroy(chunk.gameObject);

            for(int i = 0; i < borders.Count; i++)
            {
                Object.Destroy(borders[i].gameObject);
            }

            borders.Clear();
        }
    }
}