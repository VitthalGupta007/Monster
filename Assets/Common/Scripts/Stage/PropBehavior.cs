using OctoberStudio.Easing;
using OctoberStudio.Enemy;
using UnityEngine;

namespace OctoberStudio
{
    public class PropBehavior : MonoBehaviour
    {
        protected static readonly int _Overlay = Shader.PropertyToID("_Overlay");
        protected static readonly int _Disolve = Shader.PropertyToID("_Disolve");

        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] DissolveSettings dissolveSettings;

        private Material sharedMaterial;
        private Material effectsMaterial;

        protected virtual void Awake()
        {
            sharedMaterial = spriteRenderer.sharedMaterial;
            effectsMaterial = Instantiate(sharedMaterial);
        }

        public void Dissolve()
        {
            spriteRenderer.material = effectsMaterial;

            effectsMaterial.SetFloat(_Disolve, 0);
            effectsMaterial.DoFloat(_Disolve, 1, dissolveSettings.Duration + 0.02f).SetEasingCurve(dissolveSettings.DissolveCurve).SetOnFinish(() =>
            {
                effectsMaterial.SetColor(_Overlay, Color.clear);
                effectsMaterial.SetFloat(_Disolve, 0);

                gameObject.SetActive(false);
                spriteRenderer.material = sharedMaterial;
            });
        }
    }
}