using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    [CreateAssetMenu(fileName = "Drop Database", menuName = "VX Monster/Drop Database")]
    public class DropDatabase : ScriptableObject
    {
        [SerializeField] List<DropData> gems;

        public int GemsCount => gems.Count;

        public DropData GetGemData(int index)
        {
            return gems[index];
        }

        public DropData GetGemData(DropType dropType)
        {
            for(int i = 0; i < gems.Count; i++)
            {
                if (gems[i].DropType == dropType) return gems[i];
            }
            return null;
        }
    }
}