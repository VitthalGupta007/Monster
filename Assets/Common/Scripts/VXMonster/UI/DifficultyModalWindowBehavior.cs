using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    public class DifficultyModalWindowBehavior : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button closeButton;
        [SerializeField] Button easyButton;
        [SerializeField] Button normalButton;
        [SerializeField] Button hardButton;
        [SerializeField] Button nightmareButton;
        [SerializeField] TMP_Text previewLabel;

        private System.Action onClosed;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (easyButton != null) easyButton.onClick.AddListener(() => Select(DifficultyTier.Easy));
            if (normalButton != null) normalButton.onClick.AddListener(() => Select(DifficultyTier.Normal));
            if (hardButton != null) hardButton.onClick.AddListener(() => Select(DifficultyTier.Hard));
            if (nightmareButton != null) nightmareButton.onClick.AddListener(() => Select(DifficultyTier.Nightmare));
        }

        public void Open(System.Action closed = null)
        {
            onClosed = closed;
            gameObject.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            RefreshPreview(VXDifficultySelection.Selected);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            onClosed?.Invoke();
            onClosed = null;
        }

        private void Select(DifficultyTier tier)
        {
            VXDifficultySelection.Set(tier);
            RefreshPreview(tier);
            Close();
        }

        private void RefreshPreview(DifficultyTier tier)
        {
            if (previewLabel == null) return;

            previewLabel.text =
                $"{tier.DisplayLabel()}\n" +
                $"Enemy HP: {tier.EnemyHpMultiplier():0.##}x  Damage: {tier.EnemyDamageMultiplier():0.##}x\n" +
                $"Rewards: {tier.RewardMultiplier():0.##}x  Boss talent: +{tier.TalentPointsPerBossKill()}";
        }
    }
}
