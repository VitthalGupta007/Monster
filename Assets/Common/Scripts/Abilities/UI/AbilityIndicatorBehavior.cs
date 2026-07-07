using OctoberStudio.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.Abilities.UI
{
    public class AbilityIndicatorBehavior : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text levelText;

        public void Show(Sprite icon, int level, bool showLevel)
        {
            gameObject.SetActive(true);

            iconImage.sprite = icon;
            iconImage.SetAlpha(1);

            levelText.gameObject.SetActive(showLevel);
            levelText.text = (level + 1).ToString();
        }

        public void Show()
        {
            gameObject.SetActive(true);

            iconImage.sprite = null;
            iconImage.SetAlpha(0f);

            levelText.text = "";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}