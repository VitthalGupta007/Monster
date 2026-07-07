using VXMonster.Core.Abilities;
using VXMonster.Core.Easing;
using VXMonster.Core.Extensions;
using VXMonster.Core.Input;
using VXMonster.Core.Pool;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VXMonster.Gameplay;

namespace VXMonster.Core.Abilities.UI
{
    public class AbilitiesWindowBehavior : MonoBehaviour
    {
        [SerializeField] GameObject levelUpTextObject;
        [SerializeField] GameObject weaponSelectTextObject;

        [Space]
        [SerializeField] RectTransform panelRect;
        private Vector2 panelPosition;
        private Vector2 panelHiddenPosition = Vector2.up * 2000;
        private IEasingCoroutine panelCoroutine;

        [SerializeField] GameObject abilityCardPrefab;

        [SerializeField] RectTransform abilitiesHolder;

        [SerializeField] Button rerollButton;
        [SerializeField] Button banishButton;
        [SerializeField] TMP_Text rerollLabel;
        [SerializeField] TMP_Text banishLabel;

        private PoolComponent<AbilityCardBehavior> cardsPool;

        private List<AbilityCardBehavior> cards = new List<AbilityCardBehavior>();

        private AbilitiesSave abilitiesSave;
        private bool banishModeActive;
        private bool isLevelUpPanel;

        public UnityAction onPanelClosed;
        public UnityAction onPanelStartedClosing;

        public void Init()
        {
            cardsPool = new PoolComponent<AbilityCardBehavior>(abilityCardPrefab, 3);
            abilitiesSave = GameController.SaveManager.GetSave<AbilitiesSave>("Abilities Save");

            panelPosition = panelRect.anchoredPosition;
            panelRect.anchoredPosition = panelHiddenPosition;

            if (rerollButton != null) rerollButton.onClick.AddListener(OnRerollClicked);
            if (banishButton != null) banishButton.onClick.AddListener(OnBanishClicked);
        }

        public void SetData(List<AbilityData> abilities)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];

                card.transform.SetParent(null);
                card.gameObject.SetActive(false);
            }
            cards.Clear();

            for (int i = 0; i < abilities.Count; i++)
            {
                var card = cardsPool.GetEntity();

                card.transform.SetParent(abilitiesHolder);
                card.transform.ResetLocal();
                card.transform.SetAsLastSibling();

                card.Init(OnAbilitySelected);

                var abilityLevel = abilitiesSave.GetAbilityLevel(abilities[i].AbilityType);
                card.SetData(abilities[i], abilityLevel);

                cards.Add(card);
            }

            RefreshActionButtons();
        }

        public void Show(bool isLevelUp)
        {
            isLevelUpPanel = isLevelUp;
            banishModeActive = false;
            Time.timeScale = 0;

            gameObject.SetActive(true);

            levelUpTextObject.SetActive(isLevelUp);
            weaponSelectTextObject.SetActive(!isLevelUp);

            panelCoroutine.StopIfExists();
            panelCoroutine = panelRect.DoAnchorPosition(panelPosition, 0.3f).SetEasing(EasingType.SineOut).SetUnscaledTime(true);

            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].Show(i * 0.1f + 0.15f);
            }

            EasingManager.DoNextFrame(() =>
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    var navigation = new Navigation();
                    navigation.mode = Navigation.Mode.Explicit;

                    if (i != 0) navigation.selectOnUp = cards[i - 1].Selectable;
                    if (i != cards.Count - 1) navigation.selectOnDown = cards[i + 1].Selectable;

                    cards[i].Selectable.navigation = navigation;
                }

                if (cards.Count > 0)
                {
                    EventSystem.current.SetSelectedGameObject(cards[0].gameObject);
                }
            });

            GameController.InputManager.onInputChanged += OnInputChanged;
            RefreshActionButtons();
        }

        public void Hide()
        {
            onPanelStartedClosing?.Invoke();
            banishModeActive = false;

            panelCoroutine.StopIfExists();
            panelCoroutine = panelRect.DoAnchorPosition(panelHiddenPosition, 0.3f).SetEasing(EasingType.SineIn).SetUnscaledTime(true).SetOnFinish(() =>
            {
                Time.timeScale = 1;

                for (int i = 0; i < cards.Count; i++)
                {
                    cards[i].transform.SetParent(null);
                    cards[i].gameObject.SetActive(false);
                }
                cards.Clear();

                gameObject.SetActive(false);

                onPanelClosed?.Invoke();
            });

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void RefreshActionButtons()
        {
            if (!isLevelUpPanel)
            {
                if (rerollButton != null) rerollButton.gameObject.SetActive(false);
                if (banishButton != null) banishButton.gameObject.SetActive(false);
                return;
            }

            var session = GameSessionManager.Instance?.RunSession;
            var rerolls = session?.RerollsRemaining ?? 0;
            var banishAvailable = session != null && !session.BanishUsed;

            if (rerollButton != null)
            {
                rerollButton.gameObject.SetActive(true);
                rerollButton.interactable = rerolls > 0;
            }

            if (rerollLabel != null)
            {
                rerollLabel.text = rerolls > 0 ? $"Reroll ({rerolls})" : "Reroll";
            }

            if (banishButton != null)
            {
                banishButton.gameObject.SetActive(true);
                banishButton.interactable = banishAvailable;
            }

            if (banishLabel != null)
            {
                banishLabel.text = banishModeActive ? "Pick to banish" : (banishAvailable ? "Banish" : "Banished");
            }
        }

        private void OnRerollClicked()
        {
            if (!StageController.AbilityManager.TryRerollAbilityChoices(out var newChoices)) return;
            SetData(newChoices);
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].Show(i * 0.1f);
            }
        }

        private void OnBanishClicked()
        {
            var session = GameSessionManager.Instance?.RunSession;
            if (session == null || session.BanishUsed) return;
            banishModeActive = !banishModeActive;
            RefreshActionButtons();
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick && cards.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(cards[0].gameObject);
            }
        }

        private void OnAbilitySelected(AbilityData ability)
        {
            if (banishModeActive)
            {
                if (!StageController.AbilityManager.TryBanishAbility(ability.AbilityType)) return;

                banishModeActive = false;
                var refreshed = StageController.AbilityManager.RefreshAbilityChoicesAfterBanish();
                if (refreshed.Count > 0)
                {
                    SetData(refreshed);
                    for (int i = 0; i < cards.Count; i++)
                    {
                        cards[i].Show(i * 0.05f);
                    }
                }
                else
                {
                    Hide();
                }

                return;
            }

            if (StageController.AbilityManager.IsAbilityAquired(ability.AbilityType))
            {
                var level = abilitiesSave.GetAbilityLevel(ability.AbilityType);

                if (!ability.IsEndgameAbility) level++;

                if (level < 0) level = 0;
                if (level >= ability.LevelsCount) level = ability.LevelsCount - 1;

                abilitiesSave.SetAbilityLevel(ability.AbilityType, level);

                ability.Upgrade(level);
            }
            else
            {
                StageController.AbilityManager.AddAbility(ability);
            }

            Hide();
        }

        private void OnDestroy()
        {
            cardsPool.Destroy();
        }
    }
}
