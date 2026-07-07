using OctoberStudio.Abilities;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(menuName = "October/Testing/Preset", fileName = "Testing Preset")]
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