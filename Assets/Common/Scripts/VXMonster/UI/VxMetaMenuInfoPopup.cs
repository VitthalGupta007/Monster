using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.UI
{
    public class VxMetaMenuInfoPopup : MonoBehaviour
    {
        [SerializeField] GameObject popupRoot;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] Button closeButton;

        private void Awake()
        {
            BindCloseButton();
        }

        public void Initialize(GameObject root, TMP_Text title, TMP_Text body, Button close)
        {
            popupRoot = root;
            titleLabel = title;
            bodyLabel = body;
            closeButton = close;

            BindCloseButton();
            Hide();
        }

        void BindCloseButton()
        {
            if (closeButton == null) return;
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        public void Show(string title, string body)
        {
            if (titleLabel != null) titleLabel.text = title;
            if (bodyLabel != null)
            {
                bodyLabel.text = body;
                bodyLabel.ForceMeshUpdate();
            }

            if (popupRoot != null)
            {
                var card = popupRoot.transform.Find("Card") as RectTransform;
                if (card != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(card);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(card.parent as RectTransform);
                }

                popupRoot.SetActive(true);
                popupRoot.transform.SetAsLastSibling();
            }
        }

        public void Hide()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
        }
    }
}
