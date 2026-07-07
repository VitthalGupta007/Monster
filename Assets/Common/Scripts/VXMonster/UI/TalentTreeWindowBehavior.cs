using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    public class TalentTreeWindowBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text pointsLabel;
        [SerializeField] Transform nodesContainer;
        [SerializeField] GameObject nodeButtonPrefab;

        private TalentTreeSave talentSave;
        private readonly List<TalentNodeDefinition> nodes = new List<TalentNodeDefinition>
        {
            new TalentNodeDefinition(TalentTreeIds.ExtraReroll, "Extra Reroll", "Start runs with +1 reroll.", 3),
            new TalentNodeDefinition(TalentTreeIds.ExpandedMind, "Expanded Mind", "+1 passive ability slot.", 5),
            new TalentNodeDefinition(TalentTreeIds.GoldenInstinct, "Golden Instinct", "+10% gold from runs.", 8),
        };

        private void Awake()
        {
            if (GameController.SaveManager != null)
            {
                talentSave = GameController.SaveManager.GetSave<TalentTreeSave>("VX Talent Tree");
            }
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

            if (nodesContainer == null || nodeButtonPrefab == null || talentSave == null) return;

            for (var i = nodesContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(nodesContainer.GetChild(i).gameObject);
            }

            foreach (var node in nodes)
            {
                var go = Instantiate(nodeButtonPrefab, nodesContainer);
                var label = go.GetComponentInChildren<TMP_Text>();
                var unlocked = talentSave.IsUnlocked(node.Id);
                if (label != null)
                {
                    label.text = unlocked
                        ? $"{node.Title} (Unlocked)"
                        : $"{node.Title} ({node.Cost} pts)";
                }

                var button = go.GetComponent<Button>();
                if (button != null)
                {
                    var captured = node;
                    button.interactable = !unlocked;
                    button.onClick.AddListener(() => TryUnlockNode(captured));
                }
            }
        }

        private void TryUnlockNode(TalentNodeDefinition node)
        {
            if (talentSave == null) return;
            if (!talentSave.TryUnlock(node.Id, node.Cost)) return;
            GameController.SaveManager?.Save(false);
            Refresh();
        }

        private readonly struct TalentNodeDefinition
        {
            public TalentNodeDefinition(string id, string title, string description, int cost)
            {
                Id = id;
                Title = title;
                Description = description;
                Cost = cost;
            }

            public string Id { get; }
            public string Title { get; }
            public string Description { get; }
            public int Cost { get; }
        }
    }
}
