using OctoberStudio.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.Bossfight
{
    public class BossfightHealthbarBehavior : MonoBehaviour
    {
        [SerializeField] RectMask2D rectMask;
        [SerializeField] TMP_Text bossNameLabel;
        [SerializeField] CanvasGroup canvasGroup;

        public EnemyBehavior Boss { get ; private set; }

        public void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DoAlpha(1f, 0.3f);
        }

        public void Hide()
        {
            canvasGroup.DoAlpha(0f, 0.3f).SetOnFinish(() => gameObject.SetActive(false));
        }

        public void SetBoss(EnemyBehavior boss)
        {
            Boss = boss;

            Boss.onEnemyDied += OnBossDied;
            Boss.onHealthChanged += OnBossHealthChanged;
        }

        private void OnBossHealthChanged(float hp, float maxHP)
        {
            SetProgress(hp / maxHP);
        }

        private void OnBossDied(EnemyBehavior enemy)
        {
            Boss.onEnemyDied -= OnBossDied;
            Boss.onHealthChanged -= OnBossHealthChanged;
        }

        public void Init(BossfightData data)
        {
            bossNameLabel.text = data.DisplayName;

            SetProgress(1);
        }

        private void SetProgress(float progress)
        {
            Vector4 padding = rectMask.padding;
            padding.z = rectMask.rectTransform.rect.width * (1 - progress);
            rectMask.padding = padding;
        }
    }
}