using OctoberStudio.Bossfight;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.Timeline.Bossfight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio
{
    public class StageFieldManager : MonoBehaviour
    {
        private static StageFieldManager instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
        }

        [SerializeField] BossfightDatabase bossfightDatabase;

        public StageType StageType { get; private set; }
        public GameObject BackgroundPrefab { get; private set; }

        public BossFenceBehavior Fence { get; private set; }

        private IFieldBehavior field;
        private Dictionary<BossType, BossFenceBehavior> fences;

        private void Awake()
        {
            instance = this;
        }

        public void Init(StageData stageData, PlayableDirector director)
        {
            switch (stageData.StageType)
            {
                case StageType.Endless: field = new EndlessFieldBehavior(); break;
                case StageType.VerticalEndless: field = new VerticalFieldBehavior(); break;
                case StageType.HorizontalEndless: field = new HorizontalFieldBehavior(); break;
                case StageType.Rect: field = new RectFieldBehavior(); break;
            }

            field.Init(stageData.StageFieldData, stageData.SpawnProp);

            fences = new Dictionary<BossType, BossFenceBehavior>();

            var bossAssets = director.GetAssets<BossTrack, Boss>();

            for(int i = 0; i < bossAssets.Count; i++)
            {
                var bossAsset = bossAssets[i];
                var bossData = bossfightDatabase.GetBossfight(bossAsset.BossType);

                if (!fences.ContainsKey(bossData.BossType))
                {
                    var fence = Instantiate(bossData.FencePrefab).GetComponent<BossFenceBehavior>();
                    fence.gameObject.SetActive(false);
                    fence.Init();

                    fences.Add(bossData.BossType, fence);
                }
            }
        }

        public Vector2 SpawnFence(BossType bossType, Vector2 offset)
        {
            Fence = fences[bossType];

            var center = field.GetBossSpawnPosition(Fence, offset);

            Fence.SpawnFence(center);

            return center;
        }

        public void RemoveFence()
        {
            Fence.RemoveFence();
            Fence = null;
        }

        public void RemovePropFromFence()
        {
            field.RemovePropFromBossFence(Fence);
        }

        private void Update()
        {
            field.Update();
        }

        public bool ValidatePosition(Vector2 position, Vector2 offset, bool withFence = true)
        {
            var isFenceValid = true;
            if(Fence != null && withFence)
            {
                isFenceValid = Fence.ValidatePosition(position, offset);
            }
            return instance.field.ValidatePosition(position) && isFenceValid;
        }

        public virtual Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset, bool withFence)
        {
            if(Fence != null && withFence)
            {
                return Fence.GetIntersectionPoint(start, end, offset);
            }
            return instance.field.GetIntersectionPoint(start, end, offset);
        }

        public Vector2 GetRandomPositionOnBorder()
        {
            return instance.field.GetRandomPositionOnBorder();
        }

        public bool IsPointOutsideFieldRight(Vector2 point, out float distance)
        {
            return field.IsPointOutsideRight(point, out distance);
        }

        public bool IsPointOutsideFieldLeft(Vector2 point, out float distance)
        {
            return field.IsPointOutsideLeft(point, out distance);
        }

        public bool IsPointOutsideFieldTop(Vector2 point, out float distance)
        {
            return field.IsPointOutsideTop(point, out distance);
        }

        public bool IsPointOutsideFieldBottom(Vector2 point, out float distance)
        {
            return field.IsPointOutsideBottom(point, out distance);
        }
    }
}