using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Core.UI;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    public class TalentPointsHudBehavior : ScalingLabelBehavior
    {
        const string HudName = "Talent Points Indicator";

        public static TalentPointsHudBehavior EnsureOn(Transform gameScreen)
        {
            if (gameScreen == null) return null;

            var existing = gameScreen.GetComponentInChildren<TalentPointsHudBehavior>(true);
            if (existing != null)
            {
                existing.Refresh();
                return existing;
            }

            var gold = FindNamed(gameScreen, "Gold Indicator");
            if (gold == null) return null;

            var clone = Instantiate(gold.gameObject, gold.parent);
            clone.name = HudName;

            var currency = clone.GetComponent<VXMonster.Core.Currency.CurrencyScreenIncicatorBehavior>();
            if (currency != null) DestroyImmediate(currency);

            var hud = clone.GetComponent<TalentPointsHudBehavior>();
            if (hud == null) hud = clone.AddComponent<TalentPointsHudBehavior>();

            var goldRt = gold as RectTransform;
            var rt = clone.transform as RectTransform;
            if (goldRt != null && rt != null)
            {
                rt.anchorMin = goldRt.anchorMin;
                rt.anchorMax = goldRt.anchorMax;
                rt.pivot = goldRt.pivot;
                rt.sizeDelta = goldRt.sizeDelta;
                rt.anchoredPosition = goldRt.anchoredPosition + new Vector2(0f, -75f);
            }

            hud.BindChildren();
            hud.ApplyStarIcon();
            hud.Refresh();
            return hud;
        }

        public void Refresh()
        {
            BindChildren();
            if (label == null) return;
            var save = TalentPointUtility.ResolveTalentSave();
            SetAmount(save != null ? save.TalentPoints : 0);
        }

        public void PlayGain(int gained, Vector3 worldPosition)
        {
            Refresh();

            if (gained > 0 && StageController.IsLoaded && StageController.WorldSpaceTextManager != null)
            {
                StageController.WorldSpaceTextManager.SpawnText(
                    (Vector2)worldPosition + Vector2.up * 0.6f,
                    $"+{gained}");
            }
        }

        protected override void Awake()
        {
            BindChildren();
            if (label != null && icon != null)
            {
                base.Awake();
            }
        }

        void OnEnable()
        {
            BindChildren();
            Refresh();
        }

        void BindChildren()
        {
            if (label == null) label = GetComponentInChildren<TMP_Text>(true);
            if (icon == null)
            {
                foreach (var image in GetComponentsInChildren<Image>(true))
                {
                    if (image.transform != transform)
                    {
                        icon = image;
                        break;
                    }
                }
            }
        }

        void ApplyStarIcon()
        {
            if (icon == null) return;
            var sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null && sprites[i].name == "ui_ab_star")
                {
                    icon.sprite = sprites[i];
                    return;
                }
            }
        }

        static Transform FindNamed(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindNamed(root.GetChild(i), name);
                if (found != null) return found;
            }

            return null;
        }
    }
}
