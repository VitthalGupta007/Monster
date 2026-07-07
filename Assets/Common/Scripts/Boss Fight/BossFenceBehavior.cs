using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public abstract class BossFenceBehavior : MonoBehaviour
    {
        [SerializeField] protected GameObject fenceLink;
        [SerializeField] protected float linkDistance;

        protected PoolObject fenceLinkPool;
        protected List<GameObject> fenceLinks;

        public Vector2 Center { get; protected set; }

        protected int FencePoolSize { get; set; }

        public virtual void Init()
        {
            if (FencePoolSize < 1) FencePoolSize = 1;

            fenceLinkPool = new PoolObject(fenceLink, FencePoolSize);
        }

        public virtual void SpawnFence(Vector2 center)
        {
            Center = center;

            fenceLinks = new List<GameObject>();
        }

        public virtual void RemoveFence()
        {
            fenceLinkPool.DisableAllEntities();
        }

        public abstract bool ValidatePosition(Vector2 position, Vector2 offset);
        public abstract Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset);
        public abstract Vector2 GetRandomPointInside(float offset);
    }
}