using TMPro;
using UnityEngine;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    public class DifficultyBadgeHudBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text badgeLabel;

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
