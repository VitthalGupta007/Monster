using TMPro;
using UnityEngine;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    public class DailyModifierPreviewBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text modifiersLabel;
        [SerializeField] bool useWeeklySeed;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (modifiersLabel == null) return;

            var seed = useWeeklySeed
                ? RunModifierUtility.GetUtcWeeklySeed()
                : RunModifierUtility.GetUtcDailySeed();
            var modifiers = useWeeklySeed
                ? RunModifierUtility.GetWeeklyModifiers(seed)
                : RunModifierUtility.GetDailyModifiers(seed);
            if (modifiers == null || modifiers.Count == 0)
            {
                modifiersLabel.text = string.Empty;
                return;
            }

            var prefix = useWeeklySeed ? "This week:\n" : "Today:\n";
            var text = prefix;
            for (var i = 0; i < modifiers.Count; i++)
            {
                if (i > 0) text += "\n";
                var mod = modifiers[i];
                var effect = string.IsNullOrWhiteSpace(mod.Description) ? mod.DisplayName : $"{mod.DisplayName} — {mod.Description}";
                text += "• " + effect;
            }

            modifiersLabel.text = text;
            modifiersLabel.textWrappingMode = TextWrappingModes.Normal;
        }
    }
}
