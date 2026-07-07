using UnityEngine;

namespace OctoberStudio.Bossfight
{
    [System.Serializable]
    public class BossfightData
    {
        [Tooltip("Unique type of the boss")]
        [SerializeField] BossType bossType;
        public BossType BossType => bossType;

        [SerializeField] string displayName;
        public string DisplayName => displayName;

        [Tooltip("Prefab of the boss. Should have BossBehavior script or it's descendant attached to it's root GameObject")]
        [SerializeField] GameObject bossPrefab;
        public GameObject BossPrefab => bossPrefab;

        [Tooltip("Prefab of the fence. Should have BossfightFenceBehavior attached to it's root GameObject")]
        [SerializeField] GameObject fencePrefab;
        public GameObject FencePrefab => fencePrefab;
    }
}