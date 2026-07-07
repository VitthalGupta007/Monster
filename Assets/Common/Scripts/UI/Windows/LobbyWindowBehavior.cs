using VXMonster.Core.Audio;
using VXMonster.Core.Currency;
using VXMonster.Core.Easing;
using VXMonster.Core.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VXMonster.Gameplay;

namespace VXMonster.Core.UI
{
    public class LobbyWindowBehavior : MonoBehaviour
    {
        [SerializeField] protected StagesDatabase stagesDatabase;

        [Space]
        [SerializeField] protected Image stageIcon;
        [SerializeField] protected Image lockImage;
        [SerializeField] protected TMP_Text stageLabel;
        [SerializeField] protected TMP_Text stageNumberLabel;

        [Space]
        [SerializeField] protected Button playButton;
        [SerializeField] protected Button upgradesButton;
        [SerializeField] protected Button settingsButton;
        [SerializeField] protected Button charactersButton;
        [SerializeField] protected Button leftButton;
        [SerializeField] protected Button rightButton;

        [Space]
        [SerializeField] protected Sprite playButtonEnabledSprite;
        [SerializeField] protected Sprite playButtonDisabledSprite;
        [SerializeField] protected GameObject playText;
        [SerializeField] protected ScalingLabelBehavior purchaseLabel;

        [Space]
        [SerializeField] protected string attemptsFormat = "Attempts Made: {0}";
        [SerializeField] protected TMP_Text attemptsText;

        [Space]
        [SerializeField] protected Image continueBackgroundImage;
        [SerializeField] protected RectTransform contituePopupRect;
        [SerializeField] protected Button confirmButton;
        [SerializeField] protected Button cancelButton;

        [Space]
        [SerializeField] protected Button infoButton;
        [SerializeField] protected LobbyInfoPopup infoPopup;

        protected StageSave save;

        protected PurchasedCondition PurchaseCondition { get; set; }
        protected CurrencySave PurchaseCurrency { get; set; }

        protected virtual void Awake()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            leftButton.onClick.AddListener(DecrementSelectedStageId);
            rightButton.onClick.AddListener(IncremenSelectedStageId);

            confirmButton.onClick.AddListener(ConfirmButtonClicked);
            cancelButton.onClick.AddListener(CancelButtonClicked);
            if(infoButton != null && infoPopup != null) infoButton.onClick.AddListener(InfoButtonClicked);
        }

        protected virtual void Start()
        {
            save = GameController.SaveManager.GetSave<StageSave>("Stage");

            CheckFirstStageUnlockStatus();
            TryUnlockStages();

            save.onSelectedStageChanged += InitStage;

            if (save.IsPlaying && GameController.FirstTimeLoaded)
            {
                continueBackgroundImage.gameObject.SetActive(true);

                contituePopupRect.gameObject.SetActive(true);

                EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);

                InitStage(save.SelectedStageId);
            } else
            {
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);

                var lastUnlockedStageId = GetLastUnlockedStageId();
                if (lastUnlockedStageId >= stagesDatabase.StagesCount)
                {
                    lastUnlockedStageId = stagesDatabase.StagesCount - 1;
                }

                save.SetSelectedStageId(lastUnlockedStageId);
            }

            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        protected virtual void OnCurrencyAmountChanged(int amount)
        {
            if (purchaseLabel == null || !purchaseLabel.gameObject.activeSelf) return;

            if (!PurchaseCurrency.CanAfford(PurchaseCondition.Price.Amount))
            {
                playButton.interactable = false;
                playButton.image.sprite = playButtonDisabledSprite;
            }
            else
            {
                playButton.interactable = true;
                playButton.image.sprite = playButtonEnabledSprite;
            }
        }

        public virtual void Init(UnityAction onUpgradesButtonClicked, UnityAction onSettingsButtonClicked, UnityAction onCharactersButtonClicked)
        {
            upgradesButton.onClick.AddListener(onUpgradesButtonClicked);
            settingsButton.onClick.AddListener(onSettingsButtonClicked);
            charactersButton.onClick.AddListener(onCharactersButtonClicked);
        }

        public virtual void InitStage(int stageId)
        {
            var stage = stagesDatabase.GetStage(stageId);

            stageLabel.text = stage.DisplayName;
            stageNumberLabel.text = $"Stage {stageId + 1}";
            stageIcon.sprite = stage.Icon;

            PurchaseCondition = null;

            if(!save.IsStageUnlocked(stage))
            {
                InitLockedStage(stage);
            } else
            {
                InitUnlockedStage(stage);
            }

            leftButton.gameObject.SetActive(!save.IsFirstStageSelected);
            rightButton.gameObject.SetActive(save.SelectedStageId != stagesDatabase.StagesCount - 1);
        }

        protected virtual void InitLockedStage(StageData stage)
        {
            lockImage.gameObject.SetActive(true);
            if(attemptsText != null) attemptsText.gameObject.SetActive(false);

            if (purchaseLabel != null)
            {
                PurchaseCondition = stage.GetPurchaseUnlockCondition();
            }
                
            if (PurchaseCondition != null)
            {
                if (PurchaseCurrency != null)
                {
                    PurchaseCurrency.onAmountChanged -= OnCurrencyAmountChanged;
                }

                PurchaseCurrency = GameController.CurrenciesManager.GetCurrency(PurchaseCondition.Price.CurrencyId, false);
                if (PurchaseCurrency == null) PurchaseCurrency = GameController.CurrenciesManager.GetDefaultCurrency(false);
                PurchaseCurrency.onAmountChanged += OnCurrencyAmountChanged;

                if (!PurchaseCurrency.CanAfford(PurchaseCondition.Price.Amount))
                {
                    // Not enought money for purchase
                    playButton.interactable = false;
                    playButton.image.sprite = playButtonDisabledSprite;
                }
                else
                {
                    // Can purchase
                    playButton.interactable = true;
                    playButton.image.sprite = playButtonEnabledSprite;
                }

                if (purchaseLabel != null)
                {
                    purchaseLabel.gameObject.SetActive(true);
                    purchaseLabel.SetIcon(PurchaseCurrency.Data.Icon);
                    purchaseLabel.SetAmount(PurchaseCondition.Price.Amount);
                }
                playText.SetActive(false);
            }
            else
            {
                playButton.interactable = false;
                playButton.image.sprite = playButtonDisabledSprite;

                if (purchaseLabel != null) purchaseLabel.gameObject.SetActive(false);
                playText.SetActive(true);
            }

            if (infoButton != null)
            {
                infoButton.gameObject.SetActive(!string.IsNullOrEmpty(stage.StageUnlockData.UnlockDescription));
            }
        }

        protected virtual void InitUnlockedStage(StageData stage)
        {
            lockImage.gameObject.SetActive(false);
            playButton.interactable = true;
            playButton.image.sprite = playButtonEnabledSprite;

            if(purchaseLabel != null) purchaseLabel.gameObject.SetActive(false);
            playText.SetActive(true);

            if(attemptsText != null)
            {
                var attemptsCount = save.GetStageAttempts(stage);
                attemptsText.gameObject.SetActive(attemptsCount > 0);
                attemptsText.text = string.Format(attemptsFormat, attemptsCount);
            }

            if(infoButton != null)
            {
                infoButton.gameObject.SetActive(false);
            }
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
            EasingManager.DoNextFrame(() => EventSystem.current.SetSelectedGameObject(playButton.gameObject));

            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);

            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsInputClicked;

            if(PurchaseCurrency != null) PurchaseCurrency.onAmountChanged -= OnCurrencyAmountChanged;
        }

        public virtual void OnPlayButtonClicked()
        {
            if (purchaseLabel != null && purchaseLabel.gameObject.activeSelf) 
            {
                PurchaseStage();
            } else
            {
                StartStage();
            }     
        }

        protected virtual void PurchaseStage()
        {
            if (PurchaseCondition == null) return;
            if (!PurchaseCurrency.CanAfford(PurchaseCondition.Price.Amount)) return;

            PurchaseCurrency.Withdraw(PurchaseCondition.Price.Amount);
            var stage = stagesDatabase.GetStage(save.SelectedStageId);
            save.UnlockStage(stage);

            InitStage(save.SelectedStageId);
        }

        protected virtual void StartStage()
        {
            GameSessionManager.Instance?.ConfigureCampaign(VXDifficultySelection.Selected);

            save.IsPlaying = true;
            save.ResetStageData = true;
            save.Time = 0f;
            save.XP = 0f;
            save.XPLEVEL = 0;
            save.EnemiesKilled = 0;

            var stage = stagesDatabase.GetStage(save.SelectedStageId);
            save.IncrementStageAttempts(stage);

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        public void StartDailyChallenge(bool scoredAttempt)
        {
            GameSessionManager.Instance?.ConfigureDailyChallenge(scoredAttempt);

            save.IsPlaying = true;
            save.ResetStageData = true;
            save.Time = 0f;
            save.XP = 0f;
            save.XPLEVEL = 0;
            save.EnemiesKilled = 0;

            var stage = stagesDatabase.GetStage(save.SelectedStageId);
            save.IncrementStageAttempts(stage);

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        public void StartEndlessRun(DifficultyTier difficulty)
        {
            GameSessionManager.Instance?.ConfigureEndless(difficulty);

            save.IsPlaying = true;
            save.ResetStageData = true;
            save.Time = 0f;
            save.XP = 0f;
            save.XPLEVEL = 0;
            save.EnemiesKilled = 0;

            var stage = stagesDatabase.GetStage(save.SelectedStageId);
            save.IncrementStageAttempts(stage);

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        protected virtual void IncremenSelectedStageId()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            save.SetSelectedStageId(save.SelectedStageId + 1);

            if (!rightButton.gameObject.activeSelf)
            {
                if (leftButton.gameObject.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(leftButton.gameObject);
                } else
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }
        }

        protected virtual void DecrementSelectedStageId()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            save.SetSelectedStageId(save.SelectedStageId - 1);

            if (!leftButton.gameObject.activeSelf)
            {
                if (rightButton.gameObject.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(rightButton.gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }
        }

        protected virtual int GetLastUnlockedStageId()
        {
            for (int i = stagesDatabase.StagesCount - 1; i >= 0; i--)
            {
                var stageData = stagesDatabase.GetStage(i);
                if (save.IsStageUnlocked(stageData)) return i;
            }

            return 0;
        }

        protected virtual void CheckFirstStageUnlockStatus()
        {
            var firstStage = stagesDatabase.GetStage(0);

            if (!save.IsStageUnlocked(firstStage))
            {
                save.UnlockStage(firstStage);
            }
        }

        protected virtual void TryUnlockStages()
        {
            var unlocked = false;

            for (int i = 1; i < stagesDatabase.StagesCount; i++)
            {
                var stageData = stagesDatabase.GetStage(i);

                if (CheckStageUnlockStatus(stageData, i))
                {
                    unlocked = true;
                }
            }

            // Unlocking a stage might have fulfilled a condition for other stage
            if (unlocked) TryUnlockStages();
        }

        protected virtual bool CheckStageUnlockStatus(StageData stageData, int stageIndex)
        {
            if (save.IsStageUnlocked(stageData)) return false;

            for (int i = 0; i < stageData.StageUnlockData.UnlockConditions.Count; i++)
            {
                var condition = stageData.StageUnlockData.UnlockConditions[i];

                if (condition.IsMet(stagesDatabase, stageIndex, save))
                {
                    save.UnlockStage(stageData);
                    return true;
                }
            }

            // If the stage does not have conditions, we use default PreviousStageCompletedCondition
            if (stageData.StageUnlockData.UnlockConditions.Count == 0)
            {
                if (new PreviousStageCompletedCondition().IsMet(stagesDatabase, stageIndex, save))
                {
                    save.UnlockStage(stageData);
                    return true;
                }
            }

            return false;
        }

        protected virtual void OnDestroy()
        {
            if (save != null) save.onSelectedStageChanged -= InitStage;
            if (GameController.InputManager != null) GameController.InputManager.onInputChanged -= OnInputChanged;
            if(PurchaseCurrency != null) PurchaseCurrency.onAmountChanged -= OnCurrencyAmountChanged;
        }

        protected virtual void OnSettingsInputClicked(InputAction.CallbackContext context)
        {
            settingsButton.onClick?.Invoke();
        }

        protected virtual void ConfirmButtonClicked()
        {
            save.ResetStageData = false;

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        protected virtual void CancelButtonClicked()
        {
            save.IsPlaying = false;

            continueBackgroundImage.DoAlpha(0, 0.3f).SetOnFinish(() => continueBackgroundImage.gameObject.SetActive(false));
            contituePopupRect.DoAnchorPosition(Vector2.down * 2500, 0.3f).SetEasing(EasingType.SineIn).SetOnFinish(() => contituePopupRect.gameObject.SetActive(false));

            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        protected virtual void InfoButtonClicked()
        {
            if (infoPopup == null) return;

            if (!infoPopup.gameObject.activeSelf)
            {
                var stage = stagesDatabase.GetStage(save.SelectedStageId);
                infoPopup.Show(stage);
            } else
            {
                infoPopup.Hide();
            }
        }

        protected virtual void OnInputChanged(InputType prevInputType, InputType inputType)
        {
            if(prevInputType == InputType.UIJoystick)
            {
                if (continueBackgroundImage.gameObject.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }
        }
    }
}