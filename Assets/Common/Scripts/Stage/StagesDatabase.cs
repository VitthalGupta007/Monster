using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    [CreateAssetMenu(fileName = "Stage Database", menuName = "VX Monster/Stages Database")]
    public class StagesDatabase : ScriptableObject
    {
        [SerializeField] protected List<StageData> stages;

        public int StagesCount => stages.Count;

        public virtual StageData GetStage(int stageId)
        {
            return stages[stageId];
        }
    }
}