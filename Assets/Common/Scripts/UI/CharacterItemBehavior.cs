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

            Currency = GameController.CurrenciesManager.GetCurrency(characterData.Price.CurrencyId, false);
            Currency.onAmountChanged += OnCurrencyAmountChanged;

            startingAbilityObject.SetActive(characterData.HasStartingAbility);

            if(characterData.HasStartingAbility)
            {
                var abilityData = database.GetAbility(characterData.StartingAbility);
                startingAbilityImage.sprite = abilityData.Icon;
            }

            Data = characterData;
            CharacterId = characterData.Id;

            RedrawVisuals();
        }

        protected virtual void RedrawVisuals()
        {
            titleLabel.text = Data.Name;
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
                costLabel.SetIcon(Currency.Data.Icon);

                if (Currency.CanAfford(Data.Price.Amount))
                {
                    upgradeButton.interactable = true;
                    upgradeButton.image.sprite = enabledButtonSprite;
                }
                else
                {
                    upgradeButton.interactable = false;
                    upgradeButton.image.sprite = disabledButtonSprite;
                }
            }
        }

        protected virtual void SelectButtonClick()
        {
            if (!charactersSave.HasCharacterBeenBought(CharacterId))
            {
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