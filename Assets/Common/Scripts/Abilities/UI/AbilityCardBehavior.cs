using VXMonster.Core.Audio;
using VXMonster.Core.Easing;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.Core.Abilities.UI
{
    public class AbilityCardBehavior : MonoBehaviour
    {
        [SerializeField] Image abilityIcon;

        [Space]
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text descriptionText;

        [Header("Level Text")]
        [SerializeField] TMP_Text levelText;
        [SerializeField] Image levelBackgroundImage;
        [SerializeField] Color levelBackgroundColor;
        [SerializeField] Color levelBackgroundNewColor;
        [SerializeField] Color levelBackgroundEvoColor;

        [Space]
        [SerializeField] GameObject evolutionBlock;
        [SerializeField] Image evolutionIcon;

        [Space]
        [SerializeField] Button button;
        public Selectable Selectable => button;

        [Space]
        [SerializeField] RectTransform shineRect;

        [Header("Icon Background")]
        [SerializeField] Image iconBackgroundImage;
        [SerializeField] Color iconBackgroundColor;
        [SerializeField] Color iconBackgroundEvoColor;

        private Vector2 shineStartPosition;

        public AbilityData Data { get; private set; }

        private Action<AbilityData> onAbilitySelected;

        private void Awake()
        {
            button.onClick.AddListener(OnAbilitySelected);

            shineStartPosition = shineRect.anchoredPosition;
        }

        public void Init(Action<AbilityData> onAbilitySelected)
        {
            this.onAbilitySelected = onAbilitySelected;
        }

        public void SetData(AbilityData abilityData, int level)
        {
            Data = abilityData;

            abilityIcon.sprite = abilityData.Icon;

            titleText.text = abilityData.Title;
            descriptionText.text = AbilityCardStatText.Build(abilityData, level);
            if (abilityData.IsEvolution && abilityData.EvolutionRequirements != null && abilityData.EvolutionRequirements.Count > 0)
            {
                descriptionText.text += "\n" + BuildEvolutionRequirementText(abilityData);
            }
            else if (abilityData.MinStageId > 0)
            {
                descriptionText.text += $"\nUnlocks from Stage {abilityData.MinStageId + 1}+";
            }

            FitDescription();

            if (abilityData.IsEvolution)
            {
                levelBackgroundImage.color = levelBackgroundEvoColor;
                levelText.text = $"EVO";
            }
            else if(level == -1 || abilityData.IsEndgameAbility)
            {
                levelBackgroundImage.color = levelBackgroundNewColor;
                levelText.text = $"NEW!";
            }
            else
            {
                levelBackgroundImage.color = levelBackgroundColor;
                levelText.text = $"LVL {level + 2}";
            }

            if (abilityData.IsEvolution)
            {
                iconBackgroundImage.color = iconBackgroundEvoColor;
            } else 
            {
                iconBackgroundImage.color = iconBackgroundColor;
            }

            if(StageController.AbilityManager.HasEvolution(Data.AbilityType, out var otherType))
            {
                var otherData = StageController.AbilityManager.GetAbilityData(otherType);
                var otherIcon = otherData.Icon;

                evolutionBlock.SetActive(true);
                evolutionIcon.sprite = otherIcon;
            } else
            {
                evolutionBlock.SetActive(false);
            }
        }

        public void Show(float delay)
        {
            var targetShinePosition = shineStartPosition;
            targetShinePosition.x *= -1;

            shineRect.anchoredPosition = shineStartPosition;
            shineRect.DoAnchorPosition(targetShinePosition, 0.5f, delay).SetUnscaledTime(true);
        }

        private void FitDescription()
        {
            if (descriptionText == null) return;

            descriptionText.textWrappingMode = TextWrappingModes.Normal;
            descriptionText.overflowMode = TextOverflowModes.Ellipsis;
            descriptionText.enableAutoSizing = true;
            descriptionText.fontSizeMin = 20f;
            descriptionText.fontSizeMax = 31f;

            var box = descriptionText.transform.parent as RectTransform;
            if (box != null && box.GetComponent<RectMask2D>() == null)
            {
                box.gameObject.AddComponent<RectMask2D>();
            }

            descriptionText.ForceMeshUpdate();
        }

        private static string BuildEvolutionRequirementText(AbilityData abilityData)
        {
            if (abilityData?.EvolutionRequirements == null || abilityData.EvolutionRequirements.Count == 0)
            {
                return string.Empty;
            }

            var parts = new System.Collections.Generic.List<string>();
            for (var i = 0; i < abilityData.EvolutionRequirements.Count; i++)
            {
                var req = abilityData.EvolutionRequirements[i];
                var reqData = StageController.AbilityManager != null
                    ? StageController.AbilityManager.GetAbilityData(req.AbilityType)
                    : null;
                var title = reqData != null ? reqData.Title : req.AbilityType.ToString();
                parts.Add($"{title} Lv {req.RequiredAbilityLevel + 1}");
            }

            return "Requires " + string.Join(" + ", parts);
        }

        private void OnAbilitySelected()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            onAbilitySelected?.Invoke(Data);
        }
    }
}
