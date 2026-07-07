using OctoberStudio.Abilities;
using OctoberStudio.Abilities.UI;
using OctoberStudio.Audio;
using OctoberStudio.Bossfight;
using OctoberStudio.Easing;
using OctoberStudio.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace OctoberStudio
{
    public class GameScreenBehavior : MonoBehaviour
    {
        private Canvas canvas;

        [SerializeField] BackgroundTintUI blackgroundTint;
        [SerializeField] JoystickBehavior joystick;

        [Header("Abilities")]
        [FormerlySerializedAs("abilitiesPanel")]
        [SerializeField] AbilitiesWindowBehavior abilitiesWindow;
        [SerializeField] ChestWindowBehavior chestWindow;
        [SerializeField] List<AbilitiesIndicatorsListBehavior> abilitiesLists;

        public AbilitiesWindowBehavior AbilitiesWindow => abilitiesWindow;
        public ChestWindowBehavior ChestWindow => chestWindow;

        [Header("Top UI")]
        [SerializeField] CanvasGroup topUI;

        [Header("Pause")]
        [SerializeField] Button pauseButton;
        [SerializeField] PauseWindowBehavior pauseWindow;

        [Header("Bossfight")]
        [SerializeField] CanvasGroup bossfightWarning;
        [SerializeField] BossfightHealthbarBehavior bossHealthbar;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            abilitiesWindow.onPanelClosed += OnAbilitiesPanelClosed;
            abilitiesWindow.onPanelStartedClosing += OnAbilitiesPanelStartedClosing;

            pauseButton.onClick.AddListener(PauseButtonClick);

            pauseWindow.OnStartedClosing += OnPauseWindowStartedClosing;
            pauseWindow.OnClosed += OnPauseWindowClosed;

            chestWindow.OnClosed += OnChestWindowClosed;
        }

        private void Start()
        {
            abilitiesWindow.Init();

            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        private void OnSettingsInputClicked(InputAction.CallbackContext context)
        {
            pauseButton.onClick?.Invoke();
        }

        public void Show(Action onFinish = null)
        {
            canvas.enabled = true;
            onFinish?.Invoke();
        }

        public void Hide(Action onFinish = null)
        {
            canvas.enabled = false;
            onFinish?.Invoke();
        }

        public void ShowBossfightWarning()
        {
            bossfightWarning.gameObject.SetActive(true);
            bossfightWarning.alpha = 0;
            bossfightWarning.DoAlpha(1f, 0.3f);
        }

        public void HideBossFightWarning()
        {
            bossfightWarning.DoAlpha(0f, 0.3f).SetOnFinish(() => bossfightWarning.gameObject.SetActive(false));
            topUI.DoAlpha(0, 0.3f);
        }

        public void ShowBossHealthBar(BossfightData data)
        {
            bossHealthbar.Init(data);
            bossHealthbar.Show();
        }

        public void HideBossHealthbar()
        {
            bossHealthbar.Hide();
            topUI.DoAlpha(1, 0.3f);
        }

        public void LinkBossToHealthbar(EnemyBehavior enemy)
        {
            bossHealthbar.SetBoss(enemy);
        }

        public void ShowAbilitiesPanel(List<AbilityData> abilities, bool isLevelUp)
        {
            abilitiesWindow.SetData(abilities);

            EasingManager.DoAfter(0.2f, () =>
            {
                for (int i = 0; i < abilitiesLists.Count; i++)
                {
                    var abilityList = abilitiesLists[i];

                    abilityList.Show();
                    abilityList.Refresh();
                }
            }, true);

            blackgroundTint.Show();

            abilitiesWindow.Show(isLevelUp);

            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsInputClicked;
        }

        private void OnAbilitiesPanelStartedClosing()
        {
            for (int i = 0; i < abilitiesLists.Count; i++)
            {
                var abilityList = abilitiesLists[i];

                abilityList.Hide();
            }

            blackgroundTint.Hide();
        }

        private void OnAbilitiesPanelClosed()
        {
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        public void ShowChestWindow(int tierId, List<AbilityData> abilities, List<AbilityData> selectedAbilities)
        {
            chestWindow.OpenWindow(tierId, abilities, selectedAbilities);

            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsInputClicked;
        }

        private void OnChestWindowClosed()
        {
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        private void PauseButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            joystick.Disable();

            blackgroundTint.Show();
            pauseWindow.Open();

            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsInputClicked;
        }
        
        private void OnPauseWindowClosed()
        {
            if(GameController.InputManager.ActiveInput == Input.InputType.UIJoystick)
            {
                joystick.Enable();
            }

            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        private void OnPauseWindowStartedClosing()
        {
            blackgroundTint.Hide();
        }
    }
}