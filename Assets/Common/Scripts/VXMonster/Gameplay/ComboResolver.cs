using VXMonster.Core;
using UnityEngine;
using VXMonster.Platform;

namespace VXMonster.Gameplay
{
    public static class ComboResolver
    {
        public static bool TryTriggerCombo(EnemyElementStatus status, EnemyBehavior sourceEnemy, float playerDamage)
        {
            if (status == null || sourceEnemy == null) return false;

            var comboDamage = playerDamage * GetComboDamageMultiplier();
            var elements = status.ActiveElements;
            var triggered = false;

            if (Has(elements, ElementType.Burn) && Has(elements, ElementType.Chill))
            {
                TriggerSteamBurst(sourceEnemy, comboDamage);
                triggered = true;
            }

            if (Has(elements, ElementType.Chill) && Has(elements, ElementType.Shock))
            {
                TriggerStaticFreeze(sourceEnemy);
                triggered = true;
            }

            if (Has(elements, ElementType.Shock) && Has(elements, ElementType.Burn))
            {
                status.TickBurnDamage(sourceEnemy, comboDamage * 2f);
                triggered = true;
            }

            if (triggered)
            {
                GameSessionManager.Instance?.RunSession?.IncrementComboBurstCount();
                if (GameSessionManager.Instance?.Codex?.MarkFirstCombo() == true)
                {
                    CodexRewardUtility.OnFirstComboDiscovered();
                    PlatformServices.UnlockFirstCombo();
                }
            }

            return triggered;
        }

        private static float GetComboDamageMultiplier()
        {
            return 1f + (RelicsManager.Instance?.GetComboAmplifierBonus() ?? 0f);
        }

        private static bool Has(ElementType mask, ElementType flag)
        {
            return (mask & flag) == flag;
        }

        private static void TriggerSteamBurst(EnemyBehavior source, float playerDamage)
        {
            var radius = 2.5f;
            var enemies = StageController.EnemiesSpawner?.GetEnemiesInRadius(source.transform.position, radius);
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                enemy.TakeDamage(playerDamage * 0.75f);
            }
        }

        private static void TriggerStaticFreeze(EnemyBehavior source)
        {
            var radius = 2f;
            var enemies = StageController.EnemiesSpawner?.GetEnemiesInRadius(source.transform.position, radius);
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                var status = enemy.GetComponent<EnemyElementStatus>();
                status?.ApplyElement(ElementType.Chill, 0.5f);
            }
        }
    }
}
