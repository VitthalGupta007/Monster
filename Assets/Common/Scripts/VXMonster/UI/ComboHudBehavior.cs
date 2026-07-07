using TMPro;
using UnityEngine;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    public class ComboHudBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text comboLabel;
        [SerializeField] string format = "Combos: {0}";

        private int lastDisplayedCount = -1;

        private void Update()
        {
            if (comboLabel == null) return;

            var count = GameSessionManager.Instance?.RunSession?.ComboBurstCount ?? 0;
            if (count == lastDisplayedCount) return;

            lastDisplayedCount = count;
            comboLabel.text = count > 0 ? string.Format(format, count) : string.Empty;
        }
    }
}
