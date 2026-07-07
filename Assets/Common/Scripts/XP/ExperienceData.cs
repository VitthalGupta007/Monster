using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Experience Data", menuName = "October/Experience Data")]
    public class ExperienceData: ScriptableObject 
    {
        public List<ExperienceDataLevel> levels;

        public float GetXP(int levelId)
        {
            if(levelId >= levels.Count) levelId = levels.Count - 1;
            return levels[levelId].xp;
        }

        private void OnValidate()
        {
            if (levels == null || levels.Count < 0) return;

            var prevMultiplier = levels[0].multiplier;
            for(int i = 1;  i < levels.Count; i++)
            {
                var prev = levels[i - 1];
                var level = levels[i];

                var multiplier = level.multiplier;
                if (multiplier <= 0)
                {
                    multiplier = prevMultiplier;
                } else
                {
                    prevMultiplier = multiplier;
                }

                level.xp = prev.xp * multiplier;

                levels[i] = level;
            }
        }
    }

    [System.Serializable]
    public struct ExperienceDataLevel
    {
        public float multiplier;
        public float xp;
    }
}