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
    /// Matches UIStyleGuide (gold title, PanelDark content, NeverMindRounded, green/purple CTAs).
    /// </summary>
    public class VxLoginClaimPopupBehavior : MonoBehaviour
    {
        // UIStyleGuide tokens
        private static readonly Color PanelDark = new Color(0.102f, 0.071f, 0.157f, 0.96f); // #1A1228
        private static readonly Color PrimaryGreen = new Color(0.298f, 0.686f, 0.314f, 1f); // #4CAF50
        private static readonly Color PrimaryPurple = new Color(0.420f, 0.247f, 0.627f, 1f); // #6B3FA0
        private static readonly Color Gold = new Color(1f, 0.757f, 0.027f, 1f); // #FFC107
        private static readonly Color TextPrimary = Color.white;
        private static readonly Color TextMuted = new Color(0.690f, 0.659f, 0.753f, 1f); // #B0A8C0

        [SerializeField] GameObject popupRoot;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text rewardLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] TMP_Text streakLabel;
        [SerializeField] Button claimButton;
        [SerializeField] Button laterButton;

        private bool shownThisSession;
        private UnityAction onClaimed;

        /// <summary>True while the Daily Reward modal is visible.</summary>
        public bool IsShowing => popupRoot != null && popupRoot.activeInHierarchy;

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

            if (titleLabel != null) titleLabel.text = "DAILY REWARD";
            if (rewardLabel != null) rewardLabel.text = $"+{gold} GOLD";
            if (bodyLabel != null) bodyLabel.text = "Welcome back! Claim your login bonus.";
            if (streakLabel != null) streakLabel.text = $"Login streak · {streak}";

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
            TutorialUiAssets.StyleDimmer(dim);
            dim.color = new Color(0.07f, 0.05f, 0.12f, 0.82f);
            dim.raycastTarget = true;

            var overlayCanvas = popupRoot.GetComponent<Canvas>();
            if (overlayCanvas == null) overlayCanvas = popupRoot.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 450;
            if (popupRoot.GetComponent<GraphicRaycaster>() == null)
            {
                popupRoot.AddComponent<GraphicRaycaster>();
            }

            // Outer frame (game window chrome)
            var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.SetParent(popupRt, false);
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(600f, 520f);
            var cardImage = card.GetComponent<Image>();
            TutorialUiAssets.StyleWindow(cardImage);
            cardImage.color = Color.white;
            cardImage.raycastTarget = true;

            // Dark inset so title/body read cleanly on the orange frame
            var inset = new GameObject("Inset", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            var insetRt = inset.GetComponent<RectTransform>();
            insetRt.SetParent(cardRt, false);
            insetRt.anchorMin = new Vector2(0.5f, 0.5f);
            insetRt.anchorMax = new Vector2(0.5f, 0.5f);
            insetRt.pivot = new Vector2(0.5f, 0.5f);
            insetRt.sizeDelta = new Vector2(520f, 430f);
            var insetImage = inset.GetComponent<Image>();
            insetImage.color = PanelDark;
            insetImage.raycastTarget = false;

            var vlg = inset.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(28, 28, 32, 28);
            vlg.spacing = 10f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            titleLabel = CreateLabel(insetRt, "Title", "DAILY REWARD", 50f, 58f, TextAlignmentOptions.Center, true, Gold);
            ApplyLogoGradient(titleLabel);

            rewardLabel = CreateLabel(insetRt, "Reward", "+50 GOLD", 58f, 68f, TextAlignmentOptions.Center, true, Gold);
            ApplyLogoGradient(rewardLabel);

            bodyLabel = CreateLabel(insetRt, "Body", string.Empty, 28f, 72f, TextAlignmentOptions.Center, false, TextPrimary);
            streakLabel = CreateLabel(insetRt, "Streak", string.Empty, 24f, 36f, TextAlignmentOptions.Center, false, TextMuted);

            var spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(insetRt, false);
            spacer.GetComponent<LayoutElement>().flexibleHeight = 1f;
            spacer.GetComponent<LayoutElement>().minHeight = 8f;

            var buttons = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            var buttonsRt = buttons.GetComponent<RectTransform>();
            buttonsRt.SetParent(insetRt, false);
            var buttonsLe = buttons.GetComponent<LayoutElement>();
            buttonsLe.preferredHeight = 78f;
            buttonsLe.minHeight = 78f;
            var hlg = buttons.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 18f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(0, 0, 4, 0);

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
            bool bold,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            ForceGameFont(tmp);
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            tmp.richText = true;
            tmp.characterSpacing = -1.5f;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Color tint, bool confirmStyle)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 78f;
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
            ForceGameFont(tmp);
            tmp.text = label;
            tmp.fontSize = 36f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = TextPrimary;
            tmp.characterSpacing = -1f;
            tmp.raycastTarget = false;
            return button;
        }

        private static void ForceGameFont(TMP_Text tmp)
        {
            if (tmp == null) return;
            var font = TutorialUiAssets.GameFont != null ? TutorialUiAssets.GameFont : TutorialUiAssets.PlainFont;
            if (font == null) return;
            tmp.font = font;
            // Use the font asset's outline/shadow material (NeverMindRounded Shadow Outline).
            if (font.material != null) tmp.fontSharedMaterial = font.material;
        }

        private static void ApplyLogoGradient(TMP_Text tmp)
        {
            if (tmp == null) return;
            var top = new Color(1f, 0.93f, 0.35f, 1f);   // bright yellow
            var bottom = new Color(1f, 0.55f, 0.08f, 1f); // orange like VX MONSTER logo
            tmp.enableVertexGradient = true;
            tmp.colorGradient = new VertexGradient(top, top, bottom, bottom);
            tmp.color = Color.white; // multiply with gradient
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
