using TMPro;
using UnityEngine;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    public class DailyModifierPreviewBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text modifiersLabel;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (modifiersLabel == null) return;

            var seed = RunModifierUtility.GetUtcDailySeed();
            var modifiers = RunModifierUtility.GetDailyModifiers(seed);
            if (modifiers == null || modifiers.Count == 0)
            {
                modifiersLabel.text = string.Empty;
                return;
            }

            var text = "Today: ";
            for (var i = 0; i < modifiers.Count; i++)
            {
                if (i > 0) text += " · ";
                text += modifiers[i].DisplayName;
            }

            modifiersLabel.text = text;
        }
    }
}
