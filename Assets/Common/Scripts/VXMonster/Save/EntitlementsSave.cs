using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class EntitlementsSave : ISave
    {
        [SerializeField] protected bool removeAdsPurchased;

        public bool RemoveAdsPurchased
        {
            get => removeAdsPurchased;
            set => removeAdsPurchased = value;
        }

        public void Flush() { }
    }
}
