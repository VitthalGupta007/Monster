using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio
{
    public class ShootingStarProjectile : ProjectileBehavior
    {
        IEasingCoroutine scaleEasing;

        public void Spawn()
        {
            if(scaleEasing.ExistsAndActive()) scaleEasing.Stop();

            transform.localScale = Vector3.zero;
            scaleEasing = transform.DoLocalScale(Vector3.one * PlayerBehavior.Player.SizeMultiplier, 0.5f).SetEasing(EasingType.SineOut);
        }

        public void Hide()
        {
            if (scaleEasing.ExistsAndActive()) scaleEasing.Stop();

            scaleEasing = transform.DoLocalScale(Vector3.zero, 0.5f).SetEasing(EasingType.SineIn).SetOnFinish(() => {
                gameObject.SetActive(false);
            });
        }

        public void Clear()
        {
            scaleEasing.StopIfExists();

            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }
    }
}