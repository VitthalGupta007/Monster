using TMPro;
using UnityEngine;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    public class DifficultyBadgeHudBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text badgeLabel;

        private void Awake()
        {
            EnsureTopHudLayout();
        }

        private void EnsureTopHudLayout()
        {
            if (badgeLabel == null) return;

            var rt = badgeLabel.rectTransform;
            // Top-center chip under Safe Area — not mid-screen (old center+360,-80 was unreadable on phones).
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -16f);
            rt.sizeDelta = new Vector2(420f, 36f);

            badgeLabel.fontSize = 22f;
            badgeLabel.enableAutoSizing = false;
            badgeLabel.alignment = TextAlignmentOptions.Center;
            badgeLabel.textWrappingMode = TextWrappingModes.NoWrap;
            badgeLabel.overflowMode = TextOverflowModes.Ellipsis;
            badgeLabel.raycastTarget = false;
        }

        private void Update()
        {
            if (badgeLabel == null) return;
            var session = GameSessionManager.Instance;
            if (session == null)
            {
                badgeLabel.text = string.Empty;
                return;
            }

            badgeLabel.text = $"{session.RunMode} · {session.Difficulty.DisplayLabel()}";
        }
    }
}
