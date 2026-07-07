using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Bossfight
{
    [CreateAssetMenu(fileName = "Bossfight Database", menuName = "October/Bossfight Database")]
    public class BossfightDatabase : ScriptableObject
    {
        [SerializeField] List<BossfightData> bossfights;

        public int BossfightsCount => bossfights.Count;

        public BossfightData GetBossfight(BossType bossType)
        {
            for(int i = 0; i < bossfights.Count; i++)
            {
                var bossfight = bossfights[i];

                if(bossfight.BossType == bossType) return bossfight;
            }

            return null;
        }
    }
}