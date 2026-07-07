using UnityEngine;

namespace VXMonster.Platform.Ads
{
    [CreateAssetMenu(menuName = "VX Monster/AdMob Config", fileName = "ProductionAdConfig")]
    public class AdMobConfig : ScriptableObject
    {
        [Header("Production IDs (Release builds only)")]
        public string appId = "ca-app-pub-8542754238593082~9088842908";
        public string appOpenUnitId = "ca-app-pub-8542754238593082/4230021059";
        public string bannerUnitId = "ca-app-pub-8542754238593082/2868837928";
        public string interstitialUnitId = "ca-app-pub-8542754238593082/7738021229";
        public string rewardedUnitId = "ca-app-pub-8542754238593082/6145737953";

        [Header("Rules")]
        public float interstitialCooldownSeconds = 90f;
        public float appOpenCooldownSeconds = 30f;
    }

    public static class AdMobTestIds
    {
        public const string AppOpen = "ca-app-pub-3940256099942544/9257395921";
        public const string Banner = "ca-app-pub-3940256099942544/6300978111";
        public const string Interstitial = "ca-app-pub-3940256099942544/1033173712";
        public const string Rewarded = "ca-app-pub-3940256099942544/5224354917";
    }
}
