using UnityEngine;

namespace OctoberStudio.Enemy
{
    [CreateAssetMenu(fileName = "Enemy Dissolve Settings", menuName = "October/Enemy/Dissolve Settings")]
    public class DissolveSettings : ScriptableObject
    {
        [SerializeField] float duration = 0.5f;
        public float Duration => duration;

        [SerializeField] Color dissolveColor = Color.black;
        public Color DissolveColor => dissolveColor;

        [SerializeField] AnimationCurve dissolveCurve;
        public AnimationCurve DissolveCurve => dissolveCurve;
    }
}