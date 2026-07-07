using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class Effect
    {
        [SerializeField] protected EffectType type;
        [SerializeField] protected float modifier;
        [SerializeField] protected bool useOnEnemies = true;

        public EffectType EffectType => type;
        public float Modifier => modifier;
        public bool UseOnEnemies => useOnEnemies;

        public Effect(EffectType type, float modifier)
        {
            this.type = type;
            this.modifier = modifier;
        }

        public virtual void SetModifier(float modifier)
        {
            this.modifier = modifier;
        }
    }

    [System.Serializable]
    public enum EffectType
    {
        Speed = 0,
        Damage = 1,
    }
}