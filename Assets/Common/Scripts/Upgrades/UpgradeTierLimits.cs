namespace VXMonster.Core.Upgrades
{
    /// <summary>
    /// v1.0 meta upgrade depth — each UpgradeData asset should define up to this many levels.
    /// </summary>
    public static class UpgradeTierLimits
    {
        public const int MaxTiersPerStat = 8;
    }
}
