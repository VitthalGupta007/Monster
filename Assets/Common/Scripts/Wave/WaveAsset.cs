using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public abstract class WaveAsset : PlayableAsset
    {
        public abstract int EnemiesCount { get; }
        public EnemyType EnemyType { get; set; }

        [SerializeField] protected WaveOverride waveOverride;
        [Tooltip("Ignored in Rush Waves")]
        [SerializeField] protected bool circularSpawn;
    }

    [System.Serializable]
    public class WaveOverride
    {
        [SerializeField] protected bool useDamageOverride;
        [SerializeField] protected float damageOverride;

        public float ApplyDamageOverride(float damage)
        {
            return useDamageOverride ? damageOverride : damage;
        }

        [Space]
        [SerializeField] protected bool useHPOverride;
        [SerializeField] protected float hpOverride;

        public float ApplyHPOverride(float hp)
        {
            return useHPOverride ? hpOverride : hp;
        }

        [Space]
        [SerializeField] protected bool useSpeedOverride;
        [SerializeField] protected float speedOverride;

        public float ApplySpeedOverride(float speed)
        {
            return useSpeedOverride ? speedOverride : speed;
        }

        [Space]
        [SerializeField] protected bool useDropOverride;
        [SerializeField] protected List<EnemyDropData> dropOverride;

        public List<EnemyDropData> ApplyDropOverride(List<EnemyDropData> drop)
        {
            return useDropOverride ? dropOverride : drop;
        }

        [Space]
        [Tooltip("Ignored in rush waves. Rush waves do not teleport")]
        [SerializeField] protected bool disableOffscreenTeleport;
        public bool DisableOffscreenTeleport => disableOffscreenTeleport;
    }
}