using UnityEngine;

namespace OctoberStudio.Abilities
{
    public abstract class AbilityBehavior<T, K> : MonoBehaviour, IAbilityBehavior where T: GenericAbilityData<K> where K : AbilityLevel
    {
        public T Data { get; private set; }
        public AbilityData AbilityData => Data;
        public AbilityType AbilityType => Data.AbilityType;

        public K AbilityLevel { get; private set; }
        public int LevelId { get; private set; }

        public virtual void Init(AbilityData data, int levelId)
        {
            SetData(data as T);
            SetAbilityLevel(levelId);

            Data.onAbilityUpgraded += OnAbilityUpgraded;
        }

        protected virtual void SetData(T data)
        {
            Data = data;
        }

        protected virtual void SetAbilityLevel(int levelId)
        {
            LevelId = levelId;
            AbilityLevel = Data.GetLevel(levelId);
        }

        protected virtual void OnAbilityUpgraded(int levelId)
        {
            SetAbilityLevel(levelId);
        }

        private void OnDestroy()
        {
            Data.onAbilityUpgraded -= OnAbilityUpgraded;
        }

        public virtual void Clear()
        {
            Destroy(gameObject);
        }
    }
}