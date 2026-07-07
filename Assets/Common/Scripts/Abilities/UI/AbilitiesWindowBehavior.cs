using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Input;
using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.Abilities.UI
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

        private PoolComponent<AbilityCardBehavior> cardsPool;

        private List<AbilityCardBehavior> cards = new List<AbilityCardBehavior>();

        private AbilitiesSave abilitiesSave;

        public UnityAction onPanelClosed;
        public UnityAction onPanelStartedClosing;

        public void Init()
        {
            cardsPool = new PoolComponent<AbilityCardBehavior>(abilityCardPrefab, 3);
            abilitiesSave = GameController.SaveManager.GetSave<AbilitiesSave>("Abilities Save");

            panelPosition = panelRect.anchoredPosition;
            panelRect.anchoredPosition = panelHiddenPosition;
        }

        public void SetData(List<AbilityData> abilities)
        {
            for(int i = 0; i < cards.Count; i++)
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
        }

        public void Show(bool isLevelUp)
        {
            Time.timeScale = 0;

            gameObject.SetActive(true);

            levelUpTextObject.SetActive(isLevelUp);
            weaponSelectTextObject.SetActive(!isLevelUp);

            panelCoroutine.StopIfExists();
            panelCoroutine = panelRect.DoAnchorPosition(panelPosition, 0.3f).SetEasing(EasingType.SineOut).SetUnscaledTime(true);            

            for(int i = 0; i < cards.Count; i++)
            {
                cards[i].Show(i * 0.1f + 0.15f);
            }

            EasingManager.DoNextFrame(() => {
                for (int i = 0; i < cards.Count; i++)
                {
                    var navigation = new Navigation();
                    navigation.mode = Navigation.Mode.Explicit;

                    if (i != 0) navigation.selectOnUp = cards[i - 1].Selectable;
                    if (i != cards.Count - 1) navigation.selectOnDown = cards[i + 1].Selectable;

                    cards[i].Selectable.navigation = navigation;
                }

                EventSystem.current.SetSelectedGameObject(cards[0].gameObject);
            });

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        public void Hide()
        {
            onPanelStartedClosing?.Invoke();

            panelCoroutine.StopIfExists();
            panelCoroutine = panelRect.DoAnchorPosition(panelHiddenPosition, 0.3f).SetEasing(EasingType.SineIn).SetUnscaledTime(true).SetOnFinish(() => {
                Time.timeScale = 1;

                for(int i = 0; i < cards.Count; i++)
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

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick && cards.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(cards[0].gameObject);
            }
        }

        private void OnAbilitySelected(AbilityData ability)
        {
            if (StageController.AbilityManager.IsAbilityAquired(ability.AbilityType))
            {
                var level = abilitiesSave.GetAbilityLevel(ability.AbilityType);

                if(!ability.IsEndgameAbility) level++;

                if (level < 0) level = 0;
                if (level >= ability.LevelsCount) level = ability.LevelsCount - 1;

                abilitiesSave.SetAbilityLevel(ability.AbilityType, level);

                ability.Upgrade(level);
            } else
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