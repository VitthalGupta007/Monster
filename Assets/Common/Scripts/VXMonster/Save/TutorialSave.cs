using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class TutorialSave : ISave
    {
        [SerializeField] protected bool completed;
        [SerializeField] protected int lastStepIndex;

        public bool Completed
        {
            get => completed;
            set => completed = value;
        }

        public int LastStepIndex
        {
            get => lastStepIndex;
            set => lastStepIndex = value;
        }

        public void Flush() { }
    }
}
