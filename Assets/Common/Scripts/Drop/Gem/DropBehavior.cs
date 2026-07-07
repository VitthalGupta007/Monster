using VXMonster.Core.Easing;
using VXMonster.Core.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core.Drop
{
    public abstract class DropBehavior : MonoBehaviour
    {
        [SerializeField] GameObject pickUpParticle;
        [SerializeField] float particleDisableDelay;
        [SerializeField] string pickUpSoundName;
        private int pickUpSoundHash;
         
        private static Dictionary<DropType, PoolComponent<ParticleSystem>> particlePools = new Dictionary<DropType, PoolComponent<ParticleSystem>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            particlePools = new Dictionary<DropType, PoolComponent<ParticleSystem>>();
        }

        private DropData dropData;
        public DropData DropData => dropData;

        public DropType DropType => dropData.DropType;

        private void Awake()
        {
            pickUpSoundHash = pickUpSoundName.GetHashCode();
        }

        public virtual void Init(DropData dropData)
        {
            this.dropData = dropData;

            if (!particlePools.ContainsKey(dropData.DropType) && pickUpParticle != null)
            {
                var particlePool = new PoolComponent<ParticleSystem>(pickUpParticle, 2);

                particlePools[dropData.DropType] = particlePool;
            }
        }

        public virtual void OnPickedUp()
        {
            if(particlePools.TryGetValue(dropData.DropType, out var particlePool))
            {
                var particle = particlePool.GetEntity();
                particle.transform.position = transform.position;
                particle.Play();

                EasingManager.DoAfter(particleDisableDelay, () => { particle.gameObject.SetActive(false); });
            }

            GameController.AudioManager.PlaySound(pickUpSoundHash);
        }

        private void OnDestroy()
        {
            if (particlePools != null){
                foreach(var pool in particlePools.Values)
                {
                    pool.Destroy();
                }

                particlePools.Clear();
            }
        }
    }
}