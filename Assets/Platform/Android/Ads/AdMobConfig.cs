using UnityEngine;

namespace VXMonster.Platform.Ads
{
    [CreateAssetMenu(menuName = "VX Monster/AdMob Config", fileName = "ProductionAdConfig")]
    public class AdMobConfig : ScriptableObject
    {
        public string appId = "ca-app-pub-8542754238593082~9088842908";
        public string appOpenUnitId = "ca-app-pub-8542754238593082/4230021059";
        public string bannerUnitId = "ca-app-pub-8542754238593082/2868837928";
        public string interstitialUnitId = "ca-app-pub-8542754238593082/7738021229";
        public string rewardedUnitId = "ca-app-pub-8542754238593082/6145737953";

        public float interstitialCooldownSeconds = 90f;
        public float appOpenCooldownSeconds = 30f;
    }
}
