using UnityEngine;
using UnityEngine.Timeline;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Stage Data", menuName = "October/Stage Data")]
    public class StageData : ScriptableObject
    {
        [SerializeField] protected Id id;
        public string Id => id;

        [Header("Display Data")]
        [SerializeField] protected Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] protected string displayName;
        public string DisplayName => displayName;

        [Header("Timeline Data")]
        [SerializeField] protected TimelineAsset timeline;
        public TimelineAsset Timeline => timeline;

        [Header("Stage Settings")]
        [SerializeField] protected StageType stageType;
        public StageType StageType => stageType;

        [SerializeField] protected StageFieldData stageFieldData;
        public StageFieldData StageFieldData => stageFieldData;

        [SerializeField] protected bool spawnProp;
        public bool SpawnProp => spawnProp;

        [SerializeField] protected bool removePropFromBossfight;
        public bool RemovePropFromBossfight => removePropFromBossfight;

        [Space]
        [SerializeField] protected Color spotlightColor;
        public Color SpotlightColor => spotlightColor;

        [SerializeField] protected Color spotlightShadowColor;
        public Color SpotlightShadowColor => spotlightShadowColor;

        [Space]
        [SerializeField] protected float enemyDamage;
        public float EnemyDamage => enemyDamage;

        [SerializeField] protected float enemyHP;
        public float EnemyHP => enemyHP;

        [Space]
        [SerializeField] protected bool useCustomMusic;
        public bool UseCustomMusic => useCustomMusic;

        [SerializeField] protected string musicName;
        public string MusicName => musicName;

        [SerializeField] protected StageUnlockData stageUnlockData;
        public StageUnlockData StageUnlockData => stageUnlockData;

        public virtual PurchasedCondition GetPurchaseUnlockCondition()
        {
            return stageUnlockData.GetPurchaseUnlockCondition();
        }
    }

    public enum StageType
    {
        Endless,
        VerticalEndless,
        HorizontalEndless,
        Rect
    }
}