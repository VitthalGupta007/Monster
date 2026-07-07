using OctoberStudio.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio
{
    public class ExperienceUI : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;

        [SerializeField] RectMask2D rectMask;
        [SerializeField] TMP_Text levelText;

        public void SetProgress(float progress)
        {
            Vector4 padding = rectMask.padding;
            padding.z = rectMask.rectTransform.rect.width * (1 - progress);
            rectMask.padding = padding;
        }

        public void SetLevelText(int levelNumber)
        {
            levelText.text = $"LVL {levelNumber}";
        }
    }
}