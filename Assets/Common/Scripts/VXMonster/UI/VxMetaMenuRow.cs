using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VXMonster.UI
{
    public class VxMetaMenuRow : MonoBehaviour
    {
        [SerializeField] Button rowButton;
        [SerializeField] Button infoButton;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text subtitleLabel;
        [SerializeField] VxMetaMenuInfoPopup infoPopup;
        [SerializeField] string infoTitle;
        [SerializeField] string infoBody;

        public void Configure(string title, string subtitle, string popupTitle, string popupBody, UnityAction onRowClicked)
        {
            if (titleLabel != null) titleLabel.text = title;
            if (subtitleLabel != null) subtitleLabel.text = subtitle;
            infoTitle = popupTitle;
            infoBody = popupBody;

            if (rowButton != null)
            {
                rowButton.onClick.RemoveAllListeners();
                if (onRowClicked != null) rowButton.onClick.AddListener(onRowClicked);
            }

            if (infoButton != null)
            {
                infoButton.onClick.RemoveAllListeners();
                infoButton.onClick.AddListener(ShowInfo);
            }
        }

        private void ShowInfo()
        {
            if (infoPopup != null)
            {
                infoPopup.Show(infoTitle, infoBody);
            }
        }
    }
}
