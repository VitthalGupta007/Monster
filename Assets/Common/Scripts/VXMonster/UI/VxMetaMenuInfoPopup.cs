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

        public void Initialize(GameObject root, TMP_Text title, TMP_Text body, Button close)
        {
            popupRoot = root;
            titleLabel = title;
            bodyLabel = body;
            closeButton = close;

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

            Hide();
        }

        public void Show(string title, string body)
        {
            if (titleLabel != null) titleLabel.text = title;
            if (bodyLabel != null) bodyLabel.text = body;
            if (popupRoot != null)
            {
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
