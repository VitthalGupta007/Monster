using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Gameplay;
using VXMonster.Save;
using VXMonster.Tutorial;

namespace VXMonster.UI
{
    /// <summary>
    /// Daily Reward modal shown on lobby open when UTC daily claim is available.
    /// Matches UIStyleGuide / info-popup look (PanelDark card, tutorial fonts/sprites).
    /// </summary>
    public class VxLoginClaimPopupBehavior : MonoBehaviour
    {
        private static readonly Color PanelDark = new Color(0.12f, 0.08f, 0.2f, 0.98f);
        private static readonly Color PrimaryGreen = new Color(0.3f, 0.69f, 0.31f, 1f);
        private static readonly Color PrimaryPurple = new Color(0.42f, 0.25f, 0.63f, 1f);

        [SerializeField] GameObject popupRoot;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] Button claimButton;
        [SerializeField] Button laterButton;

        private bool shownThisSession;
        private UnityAction onClaimed;

        public static VxLoginClaimPopupBehavior Ensure(Transform parent)
        {
            if (parent == null) return null;

            var existing = parent.GetComponentInChildren<VxLoginClaimPopupBehavior>(true);
            if (existing != null) return existing;

            var host = new GameObject("VX Login Claim Popup Host", typeof(RectTransform), typeof(VxLoginClaimPopupBehavior));
            var hostRt = host.GetComponent<RectTransform>();
            hostRt.SetParent(parent, false);
            Stretch(hostRt);

            var behavior = host.GetComponent<VxLoginClaimPopupBehavior>();
            behavior.BuildUi(hostRt);
            return behavior;
        }

        public void SetOnClaimed(UnityAction callback)
        {
            onClaimed = callback;
        }

        public bool TryShowIfUnclaimed()
        {
            if (shownThisSession) return false;
            if (!LoginClaimUtility.CanClaimToday()) return false;
            if (GameController.SaveManager == null) return false;

            EnsureBuilt();
            var save = GameController.SaveManager.GetSave<LoginClaimSave>(LoginClaimSave.SaveKey);
            var dateKey = GameSessionManager.GetUtcDateKey();
            var gold = LoginClaimUtility.PreviewGold(save, dateKey);
            var streak = LoginClaimUtility.PreviewStreak(save, dateKey);

            if (titleLabel != null) titleLabel.text = "Daily Reward";
            if (bodyLabel != null)
            {
                bodyLabel.text =
                    $"Welcome back!\n\nClaim <b>+{gold} gold</b> for logging in today.\nLogin streak: <b>{streak}</b>";
                bodyLabel.ForceMeshUpdate();
            }

            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
                popupRoot.transform.SetAsLastSibling();
            }

            shownThisSession = true;
            return true;
        }

        public void Hide()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
        }

        private void EnsureBuilt()
        {
            if (popupRoot != null) return;
            var rt = transform as RectTransform;
            if (rt == null)
            {
                rt = gameObject.AddComponent<RectTransform>();
            }

            BuildUi(rt);
        }

        private void BuildUi(RectTransform hostRt)
        {
            Stretch(hostRt);

            popupRoot = new GameObject("Daily Reward Popup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var popupRt = popupRoot.GetComponent<RectTransform>();
            popupRt.SetParent(hostRt, false);
            Stretch(popupRt);
            var dim = popupRoot.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.6f);
            dim.raycastTarget = true;

            var overlayCanvas = popupRoot.GetComponent<Canvas>();
            if (overlayCanvas == null) overlayCanvas = popupRoot.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 450;
            if (popupRoot.GetComponent<GraphicRaycaster>() == null)
            {
                popupRoot.AddComponent<GraphicRaycaster>();
            }

            var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.SetParent(popupRt, false);
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(560f, 420f);
            var cardImage = card.GetComponent<Image>();
            TutorialUiAssets.StyleWindow(cardImage);
            if (cardImage.sprite == null) cardImage.color = PanelDark;
            else cardImage.color = Color.white;

            var vlg = card.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(28, 28, 28, 28);
            vlg.spacing = 16f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            titleLabel = CreateLabel(cardRt, "Title", "Daily Reward", 40f, 56f, TextAlignmentOptions.Center, true);
            bodyLabel = CreateLabel(cardRt, "Body", string.Empty, 28f, 160f, TextAlignmentOptions.Center, false);
            bodyLabel.richText = true;

            var buttons = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            var buttonsRt = buttons.GetComponent<RectTransform>();
            buttonsRt.SetParent(cardRt, false);
            var buttonsLe = buttons.GetComponent<LayoutElement>();
            buttonsLe.preferredHeight = 72f;
            buttonsLe.minHeight = 72f;
            var hlg = buttons.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            claimButton = CreateButton(buttonsRt, "Claim Button", "CLAIM", PrimaryGreen, true);
            laterButton = CreateButton(buttonsRt, "Later Button", "LATER", PrimaryPurple, false);

            claimButton.onClick.AddListener(OnClaimClicked);
            laterButton.onClick.AddListener(Hide);

            popupRoot.SetActive(false);
        }

        private void OnClaimClicked()
        {
            if (!LoginClaimUtility.TryClaim(out var gold, out var streak))
            {
                VxToastBehavior.Show("Already claimed today.");
                Hide();
                onClaimed?.Invoke();
                return;
            }

            VxToastBehavior.Show($"Daily reward: +{gold} gold (streak {streak})");
            Hide();
            onClaimed?.Invoke();
        }

        private static TMP_Text CreateLabel(
            Transform parent,
            string name,
            string text,
            float fontSize,
            float height,
            TextAlignmentOptions align,
            bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            TutorialUiFactory.ApplyGameFont(tmp);
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Color tint, bool confirmStyle)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 72f;
            var image = go.GetComponent<Image>();
            if (confirmStyle) TutorialUiAssets.StyleConfirmButton(image);
            else TutorialUiAssets.StyleSkipButton(image);
            image.color = tint;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.SetParent(go.transform, false);
            Stretch(labelRt);
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            TutorialUiFactory.ApplyGameFont(tmp);
            tmp.text = label;
            tmp.fontSize = 30f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return button;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
