using OctoberStudio.Abilities;
using OctoberStudio.Audio;
using OctoberStudio.Currency;
using OctoberStudio.Easing;
using OctoberStudio.Input;
using OctoberStudio.UI.Chest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VXMonster.Gameplay;

namespace OctoberStudio.UI
{
    public class ChestWindowBehavior : MonoBehaviour
    {
        private static readonly int OPEN_TRIGGER = Animator.StringToHash("Open");
        private static readonly int FORCE_OPEN_TRIGGER = Animator.StringToHash("Force Open");

        private static readonly int CHEST_WINDOW_POPUP_HASH = "Chest Window Popup".GetHashCode();
        private static readonly int CHEST_IN_PROGRESS_HASH = "Chest In Progress".GetHashCode();

        [SerializeField] RectTransform windowRect;
        [SerializeField] Animator chestAnimator;
        [SerializeField] ChestCoinsParticleBehavior coinsParticle;
        [SerializeField] ScalingLabelBehavior coinsLabel;

        [Space]
        [SerializeField] Vector2 openedPosition;
        [SerializeField] Vector2 closedPosition;

        [Space]
        [SerializeField] List<ChestLineBehavior> lines;

        [Space]
        [SerializeField] Button backgroundButton;
        [SerializeField] Button takeButton;
        [SerializeField] CanvasGroup takeButtonCanvasGroup;

        [Header("Gold Reward")]
        [SerializeField] protected CurrencyId rewardCurrencyId;
        [SerializeField] int baseRewardAmount = 100;
        [SerializeField] int tierRewardAmount = 50;
        [SerializeField] int randomRewardAmount = 50;

        [Header("Timings")]
        [SerializeField] float linesAnimationDuration = 5;
        [SerializeField] float abilityAppearDelay = 0.5f;

        [Space]
        [SerializeField] Color[] colorsTier1;
        [SerializeField] Color[] colorsTier2;
        [SerializeField] Color[] colorsTier3;

        [Space]
        [SerializeField] float[] pitchTier1;
        [SerializeField] float[] pitchTier2;
        [SerializeField] float[] pitchTier3;

        private Coroutine coinsCoroutine;
        private IEasingCoroutine takeButtonCoroutine;

        public bool IsAnimationPlaying { get; private set;}
        private int coinsReward;

        private List<Color[]> colorTiers;
        private List<float[]> pitchTiers;

        private float cacheMusicVolume;
        private AudioSource chestSound;

        public event UnityAction OnClosed;

        private void Awake()
        {
            takeButton.onClick.AddListener(TakeButton);
            backgroundButton.onClick.AddListener(SkipAnimationButton);

            colorTiers = new List<Color[]>() { colorsTier1, colorsTier2, colorsTier3 };
            pitchTiers = new List<float[]>() { pitchTier1, pitchTier2, pitchTier3 };
        }

        private void Start()
        {
            takeButton.interactable = false;
        }

        public void OpenWindow(int tierId, List<AbilityData> abilities, List<AbilityData> selectedAbilities)
        {
            cacheMusicVolume = GameController.Music.volume;
            GameController.Music.DoVolume(0, 0.3f).SetUnscaledTime(true);

            backgroundButton.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(backgroundButton.gameObject);

            windowRect.anchoredPosition = closedPosition;

            windowRect.DoAnchorPosition(openedPosition, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.SineOut).SetOnFinish(() => 
            {
                chestAnimator.SetTrigger(OPEN_TRIGGER);
            });

            for(int i = 0; i < selectedAbilities.Count; i++)
            {
                var ability = selectedAbilities[i];
                var line = lines[i];
                var color = colorTiers[tierId][i];
                var pitch = pitchTiers[tierId][i];
                line.Launch(abilities, ability, linesAnimationDuration + abilityAppearDelay * i, 0.95f, color, pitch);
            }

            for(int i = selectedAbilities.Count; i < lines.Count; i++)
            {
                lines[i].gameObject.SetActive(false);
            }
            coinsReward = baseRewardAmount + tierId * tierRewardAmount + Random.Range(0, randomRewardAmount);
            coinsCoroutine = StartCoroutine(CoinsCoroutine(coinsReward, linesAnimationDuration + abilityAppearDelay * (selectedAbilities.Count - 2)));

            IsAnimationPlaying = true;

            takeButton.gameObject.SetActive(false);
            takeButton.interactable = false;

            GameController.InputManager.onInputChanged += OnInputChanged;

            GameController.AudioManager.PlaySound(CHEST_WINDOW_POPUP_HASH);
        }

        private IEnumerator CoinsCoroutine(int coinsAmount, float duration)
        {
            coinsLabel.SetAmount(0);

            coinsParticle.Show();

            yield return new WaitForSecondsRealtime(0.8f);

            chestSound = GameController.AudioManager.PlaySound(CHEST_IN_PROGRESS_HASH);

            coinsParticle.PlayParticle();

            float time = 0;

            float soundTime = 0;

            while (time < duration)
            {
                yield return null;
                time += Time.unscaledDeltaTime;

                var t = time / duration;

                coinsLabel.SetAmount(Mathf.CeilToInt(coinsAmount * t));

                if(Time.unscaledTime - soundTime > 0.1f)
                {
                    soundTime = Time.unscaledTime;
                }
            }
            coinsLabel.SetAmount(coinsAmount);

            coinsParticle.StopParticle();

            IsAnimationPlaying = false;

            takeButton.gameObject.SetActive(true);
            takeButtonCanvasGroup.alpha = 0;
            takeButtonCoroutine = takeButtonCanvasGroup.DoAlpha(1, 0.3f).SetUnscaledTime(true).SetOnFinish(() => 
            { 
                takeButton.interactable = true;
                EventSystem.current.SetSelectedGameObject(takeButton.gameObject);
            });
        }

        private void SkipAnimationButton()
        {
            if (IsAnimationPlaying)
            {
                IsAnimationPlaying = false;

                StopCoroutine(coinsCoroutine);

                coinsParticle.StopParticle();

                coinsLabel.SetAmount(coinsReward);

                chestAnimator.SetTrigger(FORCE_OPEN_TRIGGER);

                for(int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].gameObject.activeSelf)
                    {
                        lines[i].ForceFinish();
                    }
                }

                takeButtonCoroutine.StopIfExists();

                takeButton.gameObject.SetActive(true);
                takeButtonCanvasGroup.alpha = 1;
                takeButton.interactable = true;

                EventSystem.current.SetSelectedGameObject(takeButton.gameObject);

                if(chestSound != null) chestSound.DoVolume(0f, 0.2f).SetUnscaledTime(true).SetOnFinish(() => chestSound.Stop());
            }
        }

        private void TakeButton()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            var currency = GameController.CurrenciesManager.GetCurrency(rewardCurrencyId, true);
            if (currency == null)
            {
                currency = GameController.CurrenciesManager.GetDefaultCurrency(true);
            }
            currency.Deposit(coinsReward);

            RelicsManager.Instance?.TryGrantRandomRelic();

            CloseWindow();
        }

        public void CloseWindow()
        {
            GameController.Music.DoVolume(cacheMusicVolume, 0.3f).SetUnscaledTime(true);

            windowRect.DoAnchorPosition(closedPosition, 0.3f).SetUnscaledTime(true).SetEasing(EasingType.SineIn).SetOnFinish(() =>
            {
                Time.timeScale = 1;
                backgroundButton.gameObject.SetActive(false);

                OnClosed?.Invoke();
            });

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick)
            {
                if (IsAnimationPlaying)
                {
                    EventSystem.current.SetSelectedGameObject(backgroundButton.gameObject);
                } else
                {
                    EventSystem.current.SetSelectedGameObject(takeButton.gameObject);
                }
                
            }
        }
    }
}