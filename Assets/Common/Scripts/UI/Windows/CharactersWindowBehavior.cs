using OctoberStudio.Abilities;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class CharactersWindowBehavior : MonoBehaviour
    {
        [SerializeField] protected CharactersDatabase database;
        [SerializeField] protected AbilitiesDatabase abilitiesDatabase;

        [Space]
        [SerializeField] protected GameObject itemPrefab;
        [SerializeField] protected RectTransform itemsParent;

        [Space]
        [SerializeField] protected ScrollRect scrollView;
        [SerializeField] protected Button backButton;

        protected List<CharacterItemBehavior> items = new List<CharacterItemBehavior>();

        public virtual void Init(UnityAction onBackButtonClicked)
        {
            backButton.onClick.AddListener(onBackButtonClicked);

            var charactersSave = GameController.SaveManager.GetSave<CharactersSave>("Characters");
            var selectedCharacter = database.GetCharacterData(charactersSave.SelectedCharacterId);
            if(selectedCharacter == null)
            {
                var firstCharacter = database.GetCharacterData(0);
                if (!charactersSave.HasCharacterBeenBought(firstCharacter.Id))
                {
                    charactersSave.AddBoughtCharacter(firstCharacter.Id);
                }

                charactersSave.SetSelectedCharacterId(firstCharacter.Id);
            }

            for (int i = 0; i < database.CharactersCount; i++)
            {
                var item = Instantiate(itemPrefab, itemsParent).GetComponent<CharacterItemBehavior>();
                item.transform.ResetLocal();

                item.Init(database.GetCharacterData(i), abilitiesDatabase);
                item.onNavigationSelected += OnItemSelected;

                items.Add(item);
            }

            ResetNavigation();
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);

            EasingManager.DoNextFrame(() => {
                if(items.Count > 0)
                {
                    items[0].Select();
                } else
                {
                    EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                }
            });

            GameController.InputManager.InputAsset.UI.Back.performed += OnBackInputClicked;
            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        public virtual void ResetNavigation()
        {
            for(int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var navigation = new Navigation();
                navigation.mode = Navigation.Mode.Explicit;

                int upIndex = i - 2;
                if(upIndex >= 0) navigation.selectOnUp = items[upIndex].Selectable;

                int leftIndex = i - 1;
                if(leftIndex >= 0) navigation.selectOnLeft = items[leftIndex].Selectable;

                int rightIndex = i + 1;
                if (rightIndex < items.Count)
                {
                    navigation.selectOnRight = items[rightIndex].Selectable;
                } else
                {
                    navigation.selectOnRight = backButton;
                }

                int downIndex = i + 2;
                if (downIndex < items.Count)
                {
                    navigation.selectOnDown = items[downIndex].Selectable;
                } else
                {
                    navigation.selectOnDown = backButton;
                }

                item.Selectable.navigation = navigation;
            }

            if(items.Count > 0)
            {
                var navigation = new Navigation();
                navigation.mode = Navigation.Mode.Explicit;

                navigation.selectOnUp = items[^1].Selectable;

                backButton.navigation = navigation;
            }
        }

        public virtual void OnItemSelected(CharacterItemBehavior selectedItem)
        {
            var objPosition = (Vector2)scrollView.transform.InverseTransformPoint(selectedItem.Rect.position);
            var scrollHeight = scrollView.GetComponent<RectTransform>().rect.height;
            var objHeight = selectedItem.Rect.rect.height;

            if (objPosition.y > scrollHeight / 2)
            {
                scrollView.content.localPosition = new Vector2(scrollView.content.localPosition.x,
                    scrollView.content.localPosition.y - objHeight - 37);
            }

            if (objPosition.y < -scrollHeight / 2)
            {
                scrollView.content.localPosition = new Vector2(scrollView.content.localPosition.x,
                    scrollView.content.localPosition.y + objHeight + 37);
            }
        }

        protected virtual bool IsItemOnScreen(CharacterItemBehavior item)
        {
            var objPosition = (Vector2)scrollView.transform.InverseTransformPoint(item.Rect.position);
            var scrollHeight = scrollView.GetComponent<RectTransform>().rect.height;

            return objPosition.y < scrollHeight / 2 && objPosition.y < -scrollHeight / 2;
            
        }

        protected virtual void OnBackInputClicked(InputAction.CallbackContext context)
        {
            backButton.onClick?.Invoke();
        }

        protected virtual void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick)
            {
                var selected = false;

                for(int i = 0; i < items.Count; i++)
                {
                    if (IsItemOnScreen(items[i]))
                    {
                        items[i].Select();

                        selected = true;
                        break;
                    }
                }

                if (!selected)
                {
                    if(items.Count > 0)
                    {
                        items[0].Select();
                    } else
                    {
                        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                    }
                }
            }
        }

        public virtual void Close()
        {
            GameController.InputManager.InputAsset.UI.Back.performed -= OnBackInputClicked;
            GameController.InputManager.onInputChanged -= OnInputChanged;

            gameObject.SetActive(false);
        }

        public virtual void Clear()
        {
            for(int i = 0; i < items.Count; i++)
            {
                items[i].Clear();
            }
        }
    }
}