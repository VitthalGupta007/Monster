using OctoberStudio.Abilities;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Chest
{
    public class ChestLineBehavior : MonoBehaviour
    {
        private static readonly int CHEST_ABILITY_POPUP_HASH = "Chest Ability Popup".GetHashCode();

        [SerializeField] RectTransform lineRect;
        [SerializeField] Image lineImage;
        [SerializeField] ChestAbilityBehavior abilityPreview;
        [SerializeField] GameObject sparkObject;

        [Header("Preview Line")]
        [SerializeField] RectTransform lineStartPosition;
        [SerializeField] RectTransform lineEndPositon;
        [SerializeField] float abilitiesSpacing;
        [SerializeField] float abilitiesSpeed;
        [SerializeField] GameObject abilityImagePrefab;

        private PoolComponent<Image> abilityImagesPool;

        private List<Sprite> abilityIcons;

        private float lineHeight;
        private float abilityMoveDuration;
        private float abilitySpawnDelay;

        private Coroutine scrollCoroutine;

        private List<IEasingCoroutine> imageCoroutines = new List<IEasingCoroutine>();

        private IEasingCoroutine stopCoroutine;
        private IEasingCoroutine scaleCoroutine;

        private float pitch;

        private void Awake()
        {
            abilityImagesPool = new PoolComponent<Image>(abilityImagePrefab, 10, lineRect);

            lineHeight = lineRect.sizeDelta.y;

            abilityMoveDuration = Vector2.Distance(lineStartPosition.anchoredPosition, lineEndPositon.anchoredPosition) / abilitiesSpeed;
            abilitySpawnDelay = abilitiesSpacing / abilitiesSpeed;
        }

        public void Launch(List<AbilityData> abilities, AbilityData selectedAbility, float animationDuration, float startDelay, Color color, float pitch)
        {
            gameObject.SetActive(true);
            sparkObject.SetActive(false);

            this.pitch = pitch;

            lineImage.color = color;

            abilityIcons = abilities.ConvertAll((ability) => ability.Icon);

            abilityPreview.Init(selectedAbility);
            abilityPreview.gameObject.SetActive(false);

            lineRect.sizeDelta = lineRect.sizeDelta.SetY(0);
            scaleCoroutine = lineRect.DoSizeDelta(lineRect.sizeDelta.SetY(lineHeight), 0.1f, startDelay).SetUnscaledTime(true).SetOnFinish(() => 
            {
                scrollCoroutine = StartCoroutine(ScrollCoroutine());
            });

            stopCoroutine = EasingManager.DoAfter(animationDuration, StopScrolling, true);
        }

        private void StopScrolling()
        {
            if(scrollCoroutine != null) StopCoroutine(scrollCoroutine);

            foreach(var coroutine in imageCoroutines)
            {
                coroutine.StopIfExists();
            }

            imageCoroutines.Clear();

            abilityImagesPool.DisableAllEntities();

            sparkObject.SetActive(true);
            EasingManager.DoAfter(0.2f, () => sparkObject.SetActive(false), true);

            abilityPreview.Show();

            GameController.AudioManager.PlaySound(CHEST_ABILITY_POPUP_HASH, 1, pitch);
        }

        private IEnumerator ScrollCoroutine()
        {
            while (true)
            {
                var image = abilityImagesPool.GetEntity();

                image.rectTransform.anchoredPosition = lineStartPosition.anchoredPosition;
                image.rectTransform.localRotation = Quaternion.identity;
                image.rectTransform.localScale = Vector3.one;

                image.sprite = abilityIcons.Random();

                IEasingCoroutine coroutine = null;
                coroutine = image.rectTransform.DoAnchorPosition(lineEndPositon.anchoredPosition, abilityMoveDuration).SetUnscaledTime(true).SetOnFinish(() => {
                    image.gameObject.SetActive(false);

                    imageCoroutines.Remove(coroutine);
                });

                imageCoroutines.Add(coroutine);

                yield return new WaitForSecondsRealtime(abilitySpawnDelay);
            }
        }

        public void ForceFinish()
        {
            scaleCoroutine.StopIfExists();
            lineRect.SetSizeDeltaY(lineHeight);
            if (stopCoroutine.IsActive)
            {
                stopCoroutine.Stop();
                StopScrolling();
            }
        }
    }
}