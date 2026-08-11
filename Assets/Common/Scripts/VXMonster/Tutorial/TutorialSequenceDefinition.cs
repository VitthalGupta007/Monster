using System;
using UnityEngine;

namespace VXMonster.Tutorial
{
    [Serializable]
    public class TutorialStepData
    {
        public string stepId;
        public string title;
        [TextArea(2, 6)] public string body;
    }

    [CreateAssetMenu(fileName = "Tutorial Sequence", menuName = "VX Monster/Tutorial Sequence")]
    public class TutorialSequenceDefinition : ScriptableObject
    {
        public string sequenceId;
        public TutorialStepData[] steps;
    }
}
