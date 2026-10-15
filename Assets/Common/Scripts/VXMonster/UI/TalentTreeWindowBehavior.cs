using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Core.Save;
using VXMonster.Gameplay;
using VXMonster.Save;
using VXMonster.Tutorial;

namespace VXMonster.UI
{
    public class TalentTreeWindowBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text pointsLabel;
        [SerializeField] Button backButton;
        [SerializeField] List<TalentNodeRow> nodeRows = new List<TalentNodeRow>();

        [System.Serializable]
        public class TalentNodeRow
        {
            public string nodeId;
            public string title;
            [TextArea] public string description;
            public int cost;
            public Button unlockButton;
            public TMP_Text label;
        }

        private TalentTreeSave talentSave;
        private TutorialHighlightOverlay pointsTip;

        private void Awake()
        {
            if (GameController.SaveManager != null)
            {
                talentSave = GameController.SaveManager.GetSave<TalentTreeSave>("VX Talent Tree");
            }

            foreach (var row in nodeRows)
            {
                if (row.unlockButton == null || string.IsNullOrEmpty(row.nodeId)) continue;

                var captured = row;
                row.unlockButton.onClick.AddListener(() => TryUnlockRow(captured));
            }
        }

        public void Init(UnityAction onBackClicked)
        {
            if (backButton != null) backButton.onClick.AddListener(onBackClicked);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
            TryShowFirstPointsTip();
        }

        public void Close()
        {
            pointsTip?.Hide();
            gameObject.SetActive(false);
        }

        private void TryShowFirstPointsTip()
        {
            if (talentSave == null || talentSave.TalentPoints <= 0) return;
            if (GameController.SaveManager == null) return;

            var tutorial = GameController.SaveManager.GetSave<TutorialSave>(TutorialSave.SaveHash);
            if (tutorial == null || tutorial.HasSeenStep(TutorialCopy.TalentPointsId)) return;

            if (pointsTip == null)
            {
                pointsTip = TutorialHighlightOverlay.Create(transform);
            }

            var focus = pointsLabel != null ? pointsLabel.rectTransform : transform as RectTransform;
            pointsTip.Show(TutorialCopy.TalentPointsTip, focus);
            if (pointsTip.SkipButton != null)
            {
                pointsTip.SkipButton.onClick.RemoveListener(DismissFirstPointsTip);
                pointsTip.SkipButton.onClick.AddListener(DismissFirstPointsTip);
            }

            tutorial.MarkStepSeen(TutorialCopy.TalentPointsId);
            if (GameController.SaveManager is SaveManager saveManager)
            {
                saveManager.SaveImmediate();
            }
            else
            {
                GameController.SaveManager.Save(false);
            }
        }

        private void DismissFirstPointsTip()
        {
            pointsTip?.Hide();
        }

        private void Refresh()
        {
            if (pointsLabel != null && talentSave != null)
            {
                pointsLabel.text = $"Talent Points: {talentSave.TalentPoints}";
            }

            if (talentSave == null) return;

            foreach (var row in nodeRows)
            {
                if (string.IsNullOrEmpty(row.nodeId)) continue;

                var unlocked = talentSave.IsUnlocked(row.nodeId);
                if (row.label != null)
                {
                    var effect = !string.IsNullOrWhiteSpace(row.description)
                        ? row.description
                        : PlayerFacingCopy.Talent(row.nodeId);
                    row.label.text = unlocked
                        ? $"<b>{row.title}</b> (Unlocked)\n{effect}"
                        : $"<b>{row.title}</b> ({row.cost} pts)\n{effect}";
                    row.label.richText = true;
                    row.label.textWrappingMode = TextWrappingModes.Normal;
                    row.label.overflowMode = TextOverflowModes.Ellipsis;
                    row.label.alignment = TextAlignmentOptions.Left;
                    row.label.enableAutoSizing = true;
                    row.label.fontSizeMin = 20f;
                    row.label.fontSizeMax = 28f;
                    ExpandTalentRowForDescription(row);
                }

                if (row.unlockButton != null)
                {
                    row.unlockButton.interactable = !unlocked;
                }
            }
        }

        private void TryUnlockRow(TalentNodeRow row)
        {
            if (talentSave == null || string.IsNullOrEmpty(row.nodeId)) return;
            if (!talentSave.TryUnlock(row.nodeId, row.cost)) return;

            GameController.SaveManager?.Save(false);
            VXMonster.Platform.Analytics.AnalyticsEvents.LogTalentUnlock(row.nodeId);
            Refresh();
        }

        private static void ExpandTalentRowForDescription(TalentNodeRow row)
        {
            const float rowH = 128f;
            var buttonRt = row.unlockButton != null ? row.unlockButton.transform as RectTransform : null;
            if (buttonRt != null)
            {
                buttonRt.sizeDelta = new Vector2(
                    buttonRt.sizeDelta.x > 0f ? buttonRt.sizeDelta.x : 900f,
                    rowH);

                var layout = buttonRt.GetComponent<LayoutElement>();
                if (layout == null) layout = buttonRt.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = rowH;
                layout.preferredHeight = rowH;
                layout.flexibleHeight = 0f;
            }

            if (row.label != null)
            {
                var labelRt = row.label.rectTransform;
                labelRt.anchorMin = Vector2.zero;
                labelRt.anchorMax = Vector2.one;
                labelRt.offsetMin = new Vector2(20f, 10f);
                labelRt.offsetMax = new Vector2(-20f, -10f);
            }

            // Grow the nodes container so taller rows still fit.
            var container = buttonRt != null ? buttonRt.parent as RectTransform : null;
            if (container != null && container.name.Contains("Nodes"))
            {
                container.sizeDelta = new Vector2(
                    container.sizeDelta.x > 0f ? container.sizeDelta.x : 920f,
                    1100f);
            }
        }
    }
}
