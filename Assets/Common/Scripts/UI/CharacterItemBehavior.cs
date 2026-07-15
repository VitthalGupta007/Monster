using VXMonster.Core.Abilities;
using VXMonster.Core.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VXMonster.Core.UI
{
    public class CharacterItemBehavior : MonoBehaviour
    {
        [SerializeField] RectTransform rect;
        public RectTransform Rect => rect;

        [Header("Info")]
        [SerializeField] protected Image iconImage;
        [SerializeField] protected TMP_Text titleLabel;
        [SerializeField] protected GameObject startingAbilityObject;
        [SerializeField] protected Image startingAbilityImage;

        [Header("Button")]
        [SerializeField] protected Button upgradeButton;
        [SerializeField] protected Sprite enabledButtonSprite;
        [SerializeField] protected Sprite disabledButtonSprite;
        [SerializeField] protected Sprite selectedButtonSprite;

        [Header("Stats")]
        [SerializeField] protected TMP_Text hpText;
        [SerializeField] protected TMP_Text damageText;

        [Space]
        [SerializeField] protected ScalingLabelBehavior costLabel;
        [SerializeField] protected TMP_Text buttonText;

        public CurrencySave Currency { get; private set; }
        private CharactersSave charactersSave;

        public Selectable Selectable => upgradeButton;

        public CharacterData Data { get; private set; }
        public string CharacterId { get; private set; }

        public bool IsSelected { get; private set; }

        public UnityAction<CharacterItemBehavior> onNavigationSelected;

        protected virtual void Start()
        {
            upgradeButton.onClick.AddListener(SelectButtonClick);
        }

        public virtual void Init(CharacterData characterData, AbilitiesDatabase database)
        {
            if(charactersSave == null)
            {
                charactersSave = GameController.SaveManager.GetSave<CharactersSave>("Characters");
                charactersSave.onSelectedCharacterChanged += RedrawVisuals;
            }

            if (Currency != null)
            {
                Currency.onAmountChanged -= OnCurrencyAmountChanged;
            }

            Currency = GameController.CurrenciesManager != null
                ? GameController.CurrenciesManager.GetCurrency(characterData.Price.CurrencyId, false)
                : null;
            if (Currency == null && GameController.CurrenciesManager != null)
            {
                Currency = GameController.CurrenciesManager.GetDefaultCurrency(false);
            }

            if (Currency != null)
            {
                Currency.onAmountChanged += OnCurrencyAmountChanged;
            }
            else
            {
                Debug.LogError($"[Characters] Missing currency '{characterData.Price.CurrencyId}' for {characterData.Name}.");
            }

            if (startingAbilityObject != null)
            {
                startingAbilityObject.SetActive(characterData.HasStartingAbility);
            }

            if (characterData.HasStartingAbility && database != null && startingAbilityImage != null)
            {
                var abilityData = database.GetAbility(characterData.StartingAbility);
                if (abilityData != null && abilityData.Icon != null)
                {
                    startingAbilityImage.sprite = abilityData.Icon;
                }
                else
                {
                    Debug.LogError($"[Characters] Missing starting ability '{characterData.StartingAbility}' for {characterData.Name}.");
                }
            }

            Data = characterData;
            CharacterId = characterData.Id;

            RedrawVisuals();
        }

        protected virtual void RedrawVisuals()
        {
            titleLabel.text = Data.Name;
            titleLabel.enableAutoSizing = true;
            titleLabel.fontSizeMin = 18f;
            titleLabel.fontSizeMax = titleLabel.fontSize > 18f ? titleLabel.fontSize : 28f;
            titleLabel.overflowMode = TextOverflowModes.Ellipsis;
            titleLabel.textWrappingMode = TextWrappingModes.NoWrap;
            iconImage.sprite = Data.Icon;

            hpText.text = Data.BaseHP.ToString();
            damageText.text = Data.BaseDamage.ToString();

            RedrawButton();
        }

        protected virtual void RedrawButton()
        {
            if (charactersSave.HasCharacterBeenBought(CharacterId))
            {
                costLabel.gameObject.SetActive(false);
                buttonText.gameObject.SetActive(true);

                if(charactersSave.SelectedCharacterId == CharacterId)
                {
                    upgradeButton.interactable = false;
                    upgradeButton.image.sprite = selectedButtonSprite;

                    buttonText.text = "SELECTED";

                } else
                {
                    upgradeButton.interactable = true;
                    upgradeButton.image.sprite = enabledButtonSprite;

                    buttonText.text = "SELECT";
                }
            }
            else
            {
                costLabel.gameObject.SetActive(true);
                buttonText.gameObject.SetActive(false);

                costLabel.SetAmount(Data.Price.Amount);
                if (Currency != null && Currency.Data != null)
                {
                    costLabel.SetIcon(Currency.Data.Icon);
                }

                var canAfford = Currency != null && Currency.CanAfford(Data.Price.Amount);
                upgradeButton.interactable = canAfford;
                upgradeButton.image.sprite = canAfford ? enabledButtonSprite : disabledButtonSprite;
            }
        }

        protected virtual void SelectButtonClick()
        {
            if (!charactersSave.HasCharacterBeenBought(CharacterId))
            {
                if (Currency == null || !Currency.CanAfford(Data.Price.Amount))
                {
                    return;
                }

                Currency.Withdraw(Data.Price.Amount);
                charactersSave.AddBoughtCharacter(CharacterId);
            }

            charactersSave.SetSelectedCharacterId(CharacterId);

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            EventSystem.current.SetSelectedGameObject(upgradeButton.gameObject);
        }

        protected virtual void OnCurrencyAmountChanged(int amount)
        {
            RedrawButton();
        }

        public virtual void Select()
        {
            EventSystem.current.SetSelectedGameObject(upgradeButton.gameObject);
        }

        public virtual void Unselect()
        {
            IsSelected = false;
        }

        protected virtual void Update()
        {
            if(!IsSelected && EventSystem.current.currentSelectedGameObject == upgradeButton.gameObject)
            {
                IsSelected = true;

                onNavigationSelected?.Invoke(this);
            } 
            else if(IsSelected && EventSystem.current.currentSelectedGameObject != upgradeButton.gameObject)
            {
                IsSelected = false;
            } 
        }

        public virtual void Clear()
        {
            if (Currency != null)
            {
                Currency.onAmountChanged -= OnCurrencyAmountChanged;
            }

            if (charactersSave != null)
            {
                charactersSave.onSelectedCharacterChanged -= RedrawVisuals;
            }
        }
    }
}