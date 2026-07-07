using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class SpikyTrapAbilityBehavior : AbilityBehavior<SpikyTrapAbilityData, SpikyTrapAbilityLevel>
    {
        public static readonly int SPIKY_TRAP_SET_UP_HASH = "Spiky Trap Set Up".GetHashCode();

        [SerializeField] GameObject spikyTrapPrefab;
        public GameObject SpikyTrapPrefab => spikyTrapPrefab;

        [SerializeField] GameObject spikeBehavior;
        public GameObject SpikeBehavior => spikeBehavior;

        private PoolComponent<SpikyTrapBehavior> trapPool;
        private PoolComponent<SimplePlayerProjectileBehavior> spikePool;

        private void Awake()
        {
            trapPool = new PoolComponent<SpikyTrapBehavior>("Spiky Trap Mine", SpikyTrapPrefab, 6);
            spikePool = new PoolComponent<SimplePlayerProjectileBehavior>("Spiky Trap Mine Spike", SpikeBehavior, 6);
        }

        public override void Init(AbilityData data, int stageId)
        {
            base.Init(data, stageId);

            StartCoroutine(AbilityCoroutine());
        }

        private IEnumerator AbilityCoroutine()
        {
            while (true)
            {
                for (int i = 0; i < AbilityLevel.MinesCount; i++)
                {
                    var mine = trapPool.GetEntity();

                    mine.transform.position = PlayerBehavior.CenterPosition + Random.onUnitSphere.XY().normalized * AbilityLevel.MineSpawnRadius * Random.Range(0.9f, 1.1f);
                    mine.SetData(AbilityLevel, spikePool);
                }

                GameController.AudioManager.PlaySound(SPIKY_TRAP_SET_UP_HASH);

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier);
            }
        }
    }
}