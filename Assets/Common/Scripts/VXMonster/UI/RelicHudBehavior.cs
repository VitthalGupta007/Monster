using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    public class RelicHudBehavior : MonoBehaviour
    {
        [SerializeField] List<Image> slotIcons = new List<Image>();
        [SerializeField] List<TMP_Text> slotLabels = new List<TMP_Text>();
        [SerializeField] Sprite emptySlotSprite;

        private float refreshTimer;

        private void Update()
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer > 0f) return;
            refreshTimer = 0.25f;
            Refresh();
        }

        private void Refresh()
        {
            var session = GameSessionManager.Instance?.RunSession;
            var manager = RelicsManager.Instance;
            if (session == null || manager == null) return;

            for (var i = 0; i < slotIcons.Count; i++)
            {
                var hasRelic = i < session.ActiveRelicIds.Count;
                var icon = slotIcons[i];
                if (icon == null) continue;

                if (!hasRelic)
                {
                    if (emptySlotSprite != null) icon.sprite = emptySlotSprite;
                    icon.color = new Color(1f, 1f, 1f, 0.35f);
                    if (i < slotLabels.Count && slotLabels[i] != null) slotLabels[i].text = string.Empty;
                    continue;
                }

                icon.color = Color.white;
                var relicId = session.ActiveRelicIds[i];
                var relic = manager.GetRelicById(relicId);
                if (relic != null && relic.Icon != null)
                {
                    icon.sprite = relic.Icon;
                }
                else if (emptySlotSprite != null)
                {
                    icon.sprite = emptySlotSprite;
                }

                if (i < slotLabels.Count && slotLabels[i] != null)
                {
                    slotLabels[i].text = relic != null ? relic.DisplayName : relicId;
                }
            }
        }
    }
}
