using UnityEngine;
using VXMonster.Core;

namespace VXMonster.Gameplay
{
    public static class StageBiomeTint
    {
        static readonly (string key, Color tint)[] Biomes =
        {
            ("FROST", new Color(0.72f, 0.92f, 1f)),
            ("EMBER", new Color(1f, 0.52f, 0.22f)),
            ("TOXIC", new Color(0.48f, 0.88f, 0.42f)),
            ("VOID", new Color(0.62f, 0.42f, 0.98f)),
        };

        public static bool TryGetEnemyTint(StageData stage, out Color tint)
        {
            tint = Color.white;
            if (stage == null) return false;

            var name = stage.DisplayName ?? string.Empty;
            foreach (var (key, color) in Biomes)
            {
                if (name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tint = color;
                    return true;
                }
            }

            return false;
        }
    }
}
