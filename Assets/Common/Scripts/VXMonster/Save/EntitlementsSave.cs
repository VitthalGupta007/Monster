using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class EntitlementsSave : ISave
    {
        [SerializeField] protected bool removeAdsPurchased;
        [SerializeField] protected bool starterBundlePurchased;
        [SerializeField] protected bool starterBundleGoldGranted;

        public bool RemoveAdsPurchased
        {
            get => removeAdsPurchased;
            set => removeAdsPurchased = value;
        }

        public bool StarterBundlePurchased
        {
            get => starterBundlePurchased;
            set => starterBundlePurchased = value;
        }

        public bool StarterBundleGoldGranted
        {
            get => starterBundleGoldGranted;
            set => starterBundleGoldGranted = value;
        }

        public void Flush() { }
    }
}
