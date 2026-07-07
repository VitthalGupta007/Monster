using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class BackgroundTintUI : MonoBehaviour
    {
        [SerializeField] Image image;
        [SerializeField, Range(0, 1)] float alpha;
        [SerializeField, Range(0, 1)] float duration;

        private IEasingCoroutine alphaCoroutine;

        public void Show(bool instantly = false)
        {
            alphaCoroutine.StopIfExists();

            image.enabled = true;

            if (instantly)
            {
                image.SetAlpha(alpha);
            }
            else
            {
                alphaCoroutine = image.DoAlpha(alpha, duration).SetUnscaledTime(true);
            }
        }

        public void Hide(bool instantly = false)
        {
            alphaCoroutine.StopIfExists();

            if (instantly)
            {
                image.SetAlpha(0);

                image.enabled = false;
            }
            else
            {
                alphaCoroutine = image.DoAlpha(0, duration).SetUnscaledTime(true).SetOnFinish(() => image.enabled = false);
            }
        }
    }
}