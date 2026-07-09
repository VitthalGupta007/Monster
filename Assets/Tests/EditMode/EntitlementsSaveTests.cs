#if UNITY_EDITOR
using NUnit.Framework;
using VXMonster.Save;

namespace VXMonster.Tests.EditMode
{
    public class EntitlementsSaveTests
    {
        [Test]
        public void RemoveAdsPurchased_DefaultsFalse()
        {
            var save = new EntitlementsSave();
            Assert.IsFalse(save.RemoveAdsPurchased);
        }

        [Test]
        public void RemoveAdsPurchased_CanBeSet()
        {
            var save = new EntitlementsSave
            {
                RemoveAdsPurchased = true
            };

            Assert.IsTrue(save.RemoveAdsPurchased);
        }
    }
}
#endif
