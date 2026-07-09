using VXMonster.Core;
using VXMonster.Save;

namespace VXMonster.Gameplay
{
    public static class TalentTreeIds
    {
        public const string ExtraReroll = "talent_reroll";
        public const string ExpandedMind = "talent_passive";
        public const string GoldenInstinct = "talent_gold";
        public const string IronWill = "talent_hp";
        public const string QuickFeet = "talent_speed";
        public const string ScholarsEye = "talent_codex";

        public static bool ScholarsEyeUnlocked()
        {
            if (GameSessionManager.Instance?.TalentTree != null)
            {
                return GameSessionManager.Instance.TalentTree.IsUnlocked(ScholarsEye);
            }

            if (GameController.SaveManager == null) return false;
            return GameController.SaveManager.GetSave<TalentTreeSave>("VX Talent Tree").IsUnlocked(ScholarsEye);
        }
    }
}
