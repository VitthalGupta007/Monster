using VXMonster.Core.Audio;
using VXMonster.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VXMonster.UI;

namespace VXMonster.Core.Upgrades.UI
{
    public class UpgradeItemBehavior : MonoBehaviour
    {
        [SerializeField] RectTransform rect;
        public RectTransform Rect => rect;

        [Header("Info")]
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text levelLabel;
        [SerializeField] TMP_Text descriptionLabel;

        [Header("Button")]
        [SerializeField] Button upgradeButton;
        [SerializeField] Sprite enabledButtonSprite;
        [SerializeField] Sprite disabledButtonSprite;

        [Space]
        [SerializeField] ScalingLabelBehavior costLabel;
        [SerializeField] GameObject upgradedLabel;

        public CurrencySave Currency { get; private set; }

        public UpgradeData Data { get; private set; }
        public int UpgradeLevelId { get; private set; }

        public Selectable Selectable => upgradeButton;
        public bool IsSelected { get; private set; }

        public UnityAction<UpgradeItemBehavior> onNavigationSelected;

        private void Start()
        {
            upgradeButton.onClick.AddListener(UpgradeButtonClick);
        }

        public void Init(UpgradeData data, int levelId)
        {
            Data = data;
            UpgradeLevelId = levelId;

            RedrawVisuals();
        }

        private void RedrawVisuals()
        {
            if(UpgradeLevelId >= Data.LevelsCount - 1)
            {
                levelLabel.text = "Max Level";
            }
            else
            {
                levelLabel.text = $"LEVEL {UpgradeLevelId + 1}";
            }
            
            titleLabel.text = Data.Title;
            iconImage.sprite = Data.Icon;

            if (descriptionLabel != null)
            {
                var nextValue = UpgradeLevelId < Data.LevelsCount ? Data.GetLevel(UpgradeLevelId).Value : 0f;
                var copy = !string.IsNullOrWhiteSpace(Data.Description)
                    ? Data.Description
                    : PlayerFacingCopy.Upgrade(Data.UpgradeType, nextValue);
                descriptionLabel.text = copy;
                descriptionLabel.textWrappingMode = TextWrappingModes.Normal;
                descriptionLabel.fontSize = Mathf.Min(descriptionLabel.fontSize > 0 ? descriptionLabel.fontSize : 22f, 24f);
                descriptionLabel.color = new Color(0.69f, 0.66f, 0.75f, 1f);
            }
            else if (titleLabel != null && string.IsNullOrWhiteSpace(Data.Description) == false)
            {
                // Prefab may lack a dedicated label; keep title clean.
            }
            else if (levelLabel != null)
            {
                var nextValue = UpgradeLevelId < Data.LevelsCount ? Data.GetLevel(UpgradeLevelId).Value : 0f;
                var copy = PlayerFacingCopy.Upgrade(Data.UpgradeType, nextValue);
                if (UpgradeLevelId >= Data.LevelsCount - 1)
                {
                    levelLabel.text = "Max Level\n" + copy;
                }
                else
                {
                    levelLabel.text = $"LEVEL {UpgradeLevelId + 1}\n{copy}";
                }
            }

            RedrawButton();
        }

        private void RedrawButton()
        {
            if (UpgradeLevelId >= Data.LevelsCount)
            {
                costLabel.gameObject.SetActive(false);
                upgradedLabel.gameObject.SetActive(true);

                upgradeButton.interactable = false;
                upgradeButton.image.sprite = disabledButtonSprite;
            }
            else
            {
                costLabel.gameObject.SetActive(true);
                upgradedLabel.gameObject.SetActive(false);

                var level = Data.GetLevel(UpgradeLevelId);
                costLabel.SetAmount(level.Price.Amount);

                if (Currency != null)
                {
                    Currency.onAmountChanged -= OnCurrencyAmountChanged;
                }

                Currency = GameController.CurrenciesManager.GetCurrency(level.Price.CurrencyId, false);
                if (Currency == null) Currency = GameController.CurrenciesManager.GetDefaultCurrency(false);
                Currency.onAmountChanged += OnCurrencyAmountChanged;

                costLabel.SetIcon(Currency.Data.Icon);

                if (Currency.CanAfford(level.Price.Amount))
                {
                    upgradeButton.interactable = true;
                    upgradeButton.image.sprite = enabledButtonSprite;
                } else
                {
                    upgradeButton.interactable = false;
                    upgradeButton.image.sprite = disabledButtonSprite;
                }
            }
        }

        private void UpgradeButtonClick()
        {
            var level = Data.GetLevel(UpgradeLevelId);

            GameController.UpgradesManager.IncrementUpgradeLevel(Data.UpgradeType);
            UpgradeLevelId++;
            Currency.Withdraw(level.Price.Amount);

            RedrawVisuals();

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            EventSystem.current.SetSelectedGameObject(upgradeButton.gameObject);
        }

        private void OnCurrencyAmountChanged(int amount)
        {
            RedrawButton();
        }

        public void Select()
        {
            EventSystem.current.SetSelectedGameObject(upgradeButton.gameObject);
        }

        public void Unselect()
        {
            IsSelected = false;
        }

        private void Update()
        {
            if (!IsSelected && EventSystem.current.currentSelectedGameObject == upgradeButton.gameObject)
            {
                IsSelected = true;

                onNavigationSelected?.Invoke(this);
            }
            else if (IsSelected && EventSystem.current.currentSelectedGameObject != upgradeButton.gameObject)
            {
                IsSelected = false;
            }
        }

        public void Clear()
        {
            if (Currency != null)
            {
                Currency.onAmountChanged -= OnCurrencyAmountChanged;
            }
        }
    }
}