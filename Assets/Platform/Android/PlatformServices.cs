using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VXMonster.Platform.Ads;
using VXMonster.Platform.PlayGames;

namespace VXMonster.Platform
{
    public static class PlatformServices
    {
        public static IAdService AdService { get; private set; }
        public static IPlayGamesService PlayGames { get; private set; }
        public static bool IsReady { get; private set; }

        private static float lastInterstitialTime = -999f;
        private static float lastAppOpenTime = -999f;
        private static AdMobConfig config;
        private static bool wantsMenuBanner;

        public static void Initialize(AdMobConfig adConfig, IAdService adService, IPlayGamesService playGamesService = null)
        {
            config = adConfig;
            AdService = adService ?? new MockAdService();
            PlayGames = playGamesService ?? new MockPlayGamesService();

            PlayGames.Initialize(_ =>
            {
                AdService.Initialize(success =>
                {
                    IsReady = success;
                    if (success)
                    {
                        AdService.LoadRewarded();
                        AdService.LoadInterstitial();
                        RefreshBannerForActiveScene();
                    }
                });
            });
        }

        public static bool TryShowInterstitial(Action onClosed)
        {
            if (AdService == null || !AdService.IsInterstitialReady) return false;

            var cooldown = config != null ? config.interstitialCooldownSeconds : 90f;
            if (Time.unscaledTime - lastInterstitialTime < cooldown) return false;

            lastInterstitialTime = Time.unscaledTime;
            AdService.ShowInterstitial(onClosed);
            AdService.LoadInterstitial();
            return true;
        }

        public static bool TryShowAppOpen(Action onClosed)
        {
            if (AdService == null) return false;

            var cooldown = config != null ? config.appOpenCooldownSeconds : 30f;
            if (Time.unscaledTime - lastAppOpenTime < cooldown) return false;

            lastAppOpenTime = Time.unscaledTime;
            AdService.ShowAppOpen(onClosed);
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
            PlayGames?.SubmitScore(GPGSIds.LeaderboardDailyChallenge, score);
        }

        public static void SubmitEndlessScore(int loops)
        {
            PlayGames?.SubmitScore(GPGSIds.LeaderboardEndlessWaves, loops);
        }

        public static void SubmitLifetimeKills(int totalKills)
        {
            PlayGames?.SubmitScore(GPGSIds.LeaderboardLifetimeKills, totalKills);
        }
    }
}
