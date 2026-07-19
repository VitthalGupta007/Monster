using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VXMonster.Platform.Ads;
using VXMonster.Platform.IAP;
using VXMonster.Platform.PlayGames;
using VXMonster.Save;

namespace VXMonster.Platform
{
    public static class PlatformServices
    {
        public static IAdService AdService { get; private set; }
        public static IIapService IapService { get; private set; }
        public static IPlayGamesService PlayGames { get; private set; }
        public static bool IsReady { get; private set; }
        public static bool IsIapReady => IapService != null && IapService.IsReady;

        private static float lastInterstitialTime = -999f;
        private static float lastAppOpenTime = -999f;
        private static AdMobConfig config;
        private static bool wantsMenuBanner;
        private static EntitlementsSave entitlements;

        public static bool AdsEnabled => entitlements == null || !entitlements.RemoveAdsPurchased;

        public static void BindEntitlements(EntitlementsSave save)
        {
            entitlements = save;
        }

        public static void Initialize(AdMobConfig adConfig, IAdService adService, IPlayGamesService playGamesService = null, IIapService iapService = null)
        {
            config = adConfig;
            AdService = adService ?? new MockAdService();
            PlayGames = playGamesService ?? new MockPlayGamesService();
            IapService = iapService ?? new MockIapService();

            IapService.Initialize(success =>
            {
                if (success)
                    Debug.Log("[IAP] Store initialized.");
                else
                    Debug.LogWarning("[IAP] Store init failed.");
            });

            PlayGames.Initialize(_ =>
            {
                InitializeAds();
            });
        }

        private static void InitializeAds()
        {
            if (!GoogleMobileAdsConsentController.CanRequestAds || !AdsEnabled)
            {
                IsReady = true;
                RefreshBannerForActiveScene();
                return;
            }

            AdService.Initialize(success =>
            {
                IsReady = success;
                if (success)
                {
                    RefreshBannerForActiveScene();
                }
            });
        }

        public static void TryPushCloudSave()
        {
            if (PlayGames == null || !PlayGames.IsAuthenticated) return;
            PlayGames.PushCloudSave();
        }

        public static void UnlockFirstCombo()
        {
            PlayGames?.UnlockAchievement(PlayGamesIds.AchievementFirstCombo);
        }

        public static void UnlockDailyComplete()
        {
            PlayGames?.UnlockAchievement(PlayGamesIds.AchievementDailyComplete);
        }

        public static bool TryShowInterstitial(Action onClosed)
        {
            if (!AdsEnabled) return false;
            if (AdService == null || !AdService.IsInterstitialReady) return false;

            var cooldown = config != null ? config.interstitialCooldownSeconds : 90f;
            if (Time.unscaledTime - lastInterstitialTime < cooldown) return false;

            lastInterstitialTime = Time.unscaledTime;
            Analytics.AnalyticsEvents.LogAdImpression("interstitial");
            AdService.ShowInterstitial(onClosed);
            return true;
        }

        public static bool TryShowAppOpen(Action onClosed)
        {
            if (!AdsEnabled) return false;
            if (AdService == null) return false;

            var cooldown = config != null ? config.appOpenCooldownSeconds : 30f;
            if (Time.unscaledTime - lastAppOpenTime < cooldown) return false;

            lastAppOpenTime = Time.unscaledTime;
            Analytics.AnalyticsEvents.LogAdImpression("app_open");
            AdService.ShowAppOpen(onClosed);
            return true;
        }

        public static bool TryShowRewarded(Action onRewardGranted, Action onClosed)
        {
            if (!AdsEnabled) return false;
            if (AdService == null || !AdService.IsRewardedReady) return false;

            Analytics.AnalyticsEvents.LogAdImpression("rewarded");
            AdService.ShowRewarded(onRewardGranted, onClosed);
            return true;
        }

        public static void ShowMainMenuBanner()
        {
            wantsMenuBanner = true;
            RefreshBannerForActiveScene();
        }

        public static void HideBanner()
        {
            wantsMenuBanner = false;
            AdService?.HideBanner();
        }

        public static void RefreshBannerForActiveScene()
        {
            if (!AdsEnabled)
            {
                AdService?.HideBanner();
                return;
            }

            if (!IsReady || AdService == null) return;

            if (wantsMenuBanner && IsMainMenuScene(SceneManager.GetActiveScene().name))
            {
                AdService.ShowBanner();
            }
            else if (!IsMainMenuScene(SceneManager.GetActiveScene().name))
            {
                AdService.HideBanner();
            }
        }

        private static bool IsMainMenuScene(string sceneName)
        {
            return sceneName == "Main Menu";
        }

        public static void SubmitDailyScore(int score)
        {
            PlayGames?.SubmitScore(PlayGamesIds.LeaderboardDailyChallenge, score);
        }

        public static void SubmitEndlessScore(int loops)
        {
            PlayGames?.SubmitScore(PlayGamesIds.LeaderboardEndlessWaves, loops);
        }

        public static void SubmitLifetimeKills(int totalKills)
        {
            PlayGames?.SubmitScore(PlayGamesIds.LeaderboardLifetimeKills, totalKills);
        }
    }
}
