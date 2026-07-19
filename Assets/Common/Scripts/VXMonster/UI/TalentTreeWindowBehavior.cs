using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Gameplay;
using VXMonster.Save;

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
            public int cost;
            public Button unlockButton;
            public TMP_Text label;
        }

        private TalentTreeSave talentSave;

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
            Refresh();
        }

        public void Close()
        {
            gameObject.SetActive(false);
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
                    row.label.text = unlocked
                        ? $"{row.title} (Unlocked)"
                        : $"{row.title} ({row.cost} pts)";
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
    }
}
