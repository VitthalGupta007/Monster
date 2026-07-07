using VXMonster.Core.Abilities;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    [CreateAssetMenu(menuName = "VX Monster/Testing/Preset", fileName = "Testing Preset")]
    public class PresetData : ScriptableObject
    {
        [SerializeField] float startTime;
        public float StartTime => startTime;

        [SerializeField] int xpLevel;
        public int XPLevel => xpLevel;

        [SerializeField] List<AbilityDev> abilities; 
        public List<AbilityDev> Abilities => abilities;
    }
}