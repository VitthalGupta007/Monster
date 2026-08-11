using UnityEngine;
using VXMonster.Core;
using VXMonster.Core.Save;
using VXMonster.Platform.Analytics;
using VXMonster.Save;

namespace VXMonster.Gameplay
{
    public static class TalentPointUtility
    {
        public const string TalentTreeSaveName = "VX Talent Tree";

        public static void AwardBossKillPoints()
        {
            AwardBossKillPoints(Vector3.zero, false);
        }

        public static void AwardBossKillPoints(Vector3 worldPosition, bool showHud = true)
        {
            var save = ResolveTalentSave();
            if (save == null) return;

            var session = GameSessionManager.Instance;
            var difficulty = session != null ? session.Difficulty : DifficultyTier.Normal;
            var points = difficulty.TalentPointsPerBossKill();
            save.AddPoints(points);
            AnalyticsEvents.LogBossKilled(points);
            FlushTalentSave();

            if (!showHud) return;

            var hud = Object.FindAnyObjectByType<VXMonster.UI.TalentPointsHudBehavior>();
            if (hud != null)
            {
                hud.PlayGain(points, worldPosition);
            }
        }

        public static TalentTreeSave ResolveTalentSave()
        {
            var session = GameSessionManager.Instance;
            if (session?.TalentTree != null) return session.TalentTree;

            if (GameController.SaveManager == null) return null;

            var save = GameController.SaveManager.GetSave<TalentTreeSave>(TalentTreeSaveName);
            session?.BindTalentTree(save);
            return save;
        }

        static void FlushTalentSave()
        {
            if (GameController.SaveManager is SaveManager saveManager)
            {
                saveManager.SaveImmediate();
                return;
            }

            GameController.SaveManager?.Save(true);
        }
    }
}
