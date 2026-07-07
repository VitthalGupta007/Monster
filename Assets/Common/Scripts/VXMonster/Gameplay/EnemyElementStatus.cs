using System.Collections.Generic;
using UnityEngine;
using VXMonster.Core;

namespace VXMonster.Gameplay
{
    public class EnemyElementStatus : MonoBehaviour
    {
        private const int MaxBurnStacks = 3;

        private readonly Dictionary<ElementType, float> expiryTimes = new Dictionary<ElementType, float>();
        private int burnStacks;

        public ElementType ActiveElements
        {
            get
            {
                ElementType active = ElementType.None;
                if (burnStacks > 0) active |= ElementType.Burn;
                if (HasElement(ElementType.Chill)) active |= ElementType.Chill;
                if (HasElement(ElementType.Shock)) active |= ElementType.Shock;
                return active;
            }
        }

        public int BurnStacks => burnStacks;

        public float DamageTakenMultiplier
        {
            get
            {
                var mult = 1f;
                if (burnStacks > 0) mult += 0.05f * burnStacks;
                if (HasElement(ElementType.Chill)) mult += 0.15f;
                return mult;
            }
        }

        public float SpeedMultiplier => HasElement(ElementType.Chill) ? 0.7f : 1f;

        private void Update()
        {
            if (expiryTimes.Count == 0) return;

            var expired = ListPool<ElementType>.Get();
            foreach (var pair in expiryTimes)
            {
                if (Time.time >= pair.Value) expired.Add(pair.Key);
            }

            foreach (var element in expired)
            {
                expiryTimes.Remove(element);
            }

            ListPool<ElementType>.Release(expired);
        }

        public void ApplyElement(ElementType element, float duration)
        {
            if (element == ElementType.Burn)
            {
                burnStacks = Mathf.Min(MaxBurnStacks, burnStacks + 1);
                expiryTimes[ElementType.Burn] = Time.time + duration;
                return;
            }

            expiryTimes[element] = Time.time + duration;
        }

        public bool HasElement(ElementType element)
        {
            if (element == ElementType.Burn) return burnStacks > 0 && Time.time < GetExpiry(ElementType.Burn);
            return expiryTimes.TryGetValue(element, out var expiry) && Time.time < expiry;
        }

        private float GetExpiry(ElementType element)
        {
            return expiryTimes.TryGetValue(element, out var expiry) ? expiry : 0f;
        }

        public void TickBurnDamage(EnemyBehavior enemy, float baseDamage)
        {
            if (burnStacks <= 0 || enemy == null || !enemy.IsAlive) return;
            enemy.TakeDamage(baseDamage * 0.15f * burnStacks);
        }

        private void LateUpdate()
        {
            if (burnStacks <= 0) return;
            if (!HasElement(ElementType.Burn))
            {
                burnStacks = 0;
                return;
            }

            var enemy = GetComponent<EnemyBehavior>();
            if (enemy == null || !enemy.IsAlive) return;

            if (Time.frameCount % 30 == 0)
            {
                TickBurnDamage(enemy, PlayerBehavior.Player != null ? PlayerBehavior.Player.Damage : 1f);
            }
        }
    }

    internal static class ListPool<T>
    {
        private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

        public static List<T> Get()
        {
            return Pool.Count > 0 ? Pool.Pop() : new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            Pool.Push(list);
        }
    }
}
