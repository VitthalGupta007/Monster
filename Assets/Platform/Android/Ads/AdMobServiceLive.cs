#if GOOGLE_MOBILE_ADS_AVAILABLE
using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace VXMonster.Platform.Ads
{
    /// <summary>
    /// Live AdMob integration for Android device builds.
    /// Uses Google test ad units in Development builds.
    /// </summary>
    public class AdMobService : IAdService
    {
        private readonly AdMobConfig config;
        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;
        private AppOpenAd appOpenAd;

        private bool rewardedLoaded;
        private bool interstitialLoaded;
        private bool appOpenLoaded;

        public bool IsInitialized { get; private set; }
        public bool IsRewardedReady => IsInitialized && rewardedLoaded && rewardedAd != null && rewardedAd.CanShowAd();
        public bool IsInterstitialReady => IsInitialized && interstitialLoaded && interstitialAd != null && interstitialAd.CanShowAd();
        public bool IsBannerVisible { get; private set; }

        public AdMobService(AdMobConfig adMobConfig)
        {
            config = adMobConfig;
        }

        public void Initialize(Action<bool> onComplete)
        {
            MobileAds.Initialize(_ =>
            {
                IsInitialized = true;
                Debug.Log("[VX Ads] AdMob SDK initialized.");
                LoadRewarded();
                LoadInterstitial();
                LoadAppOpen();
                onComplete?.Invoke(true);
            });
        }

        public void ShowBanner()
        {
            if (!IsInitialized) return;

            if (bannerView == null)
            {
                bannerView = new BannerView(GetBannerUnitId(), AdSize.Banner, AdPosition.Bottom);
                bannerView.OnBannerAdLoadFailed += error =>
                {
                    Debug.LogWarning($"[VX Ads] Banner load failed: {error.GetMessage()}");
                };
            }

            bannerView.LoadAd(new AdRequest());
            bannerView.Show();
            IsBannerVisible = true;
        }

        public void HideBanner()
        {
            bannerView?.Hide();
            IsBannerVisible = false;
        }

        public void ShowInterstitial(Action onClosed)
        {
            if (!IsInterstitialReady)
            {
                onClosed?.Invoke();
                return;
            }

            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                onClosed?.Invoke();
                DestroyInterstitial();
                LoadInterstitial();
            };

            interstitialAd.OnAdFullScreenContentFailed += _ =>
            {
                onClosed?.Invoke();
                DestroyInterstitial();
                LoadInterstitial();
            };

            interstitialAd.Show();
        }

        public void ShowRewarded(Action onRewardGranted, Action onClosed)
        {
            if (!IsRewardedReady)
            {
                onClosed?.Invoke();
                return;
            }

            var rewardGranted = false;

            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                if (rewardGranted)
                {
                    Debug.Log("[VX Ads] Rewarded completed — granting revive.");
                }

                onClosed?.Invoke();
                DestroyRewarded();
                LoadRewarded();
            };

            rewardedAd.OnAdFullScreenContentFailed += _ =>
            {
                onClosed?.Invoke();
                DestroyRewarded();
                LoadRewarded();
            };

            rewardedAd.Show(reward =>
            {
                rewardGranted = true;
                onRewardGranted?.Invoke();
            });
        }

        public void ShowAppOpen(Action onClosed)
        {
            if (!appOpenLoaded || appOpenAd == null || !appOpenAd.CanShowAd())
            {
                onClosed?.Invoke();
                LoadAppOpen();
                return;
            }

            appOpenAd.OnAdFullScreenContentClosed += () =>
            {
                onClosed?.Invoke();
                DestroyAppOpen();
                LoadAppOpen();
            };

            appOpenAd.OnAdFullScreenContentFailed += _ =>
            {
                onClosed?.Invoke();
                DestroyAppOpen();
                LoadAppOpen();
            };

            appOpenAd.Show();
        }

        public void LoadRewarded()
        {
            if (!IsInitialized) return;

            DestroyRewarded();
            RewardedAd.Load(GetRewardedUnitId(), new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    rewardedLoaded = false;
                    Debug.LogWarning($"[VX Ads] Rewarded load failed: {error.GetMessage()}");
                    return;
                }

                rewardedAd = ad;
                rewardedLoaded = true;
            });
        }

        public void LoadInterstitial()
        {
            if (!IsInitialized) return;

            DestroyInterstitial();
            InterstitialAd.Load(GetInterstitialUnitId(), new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    interstitialLoaded = false;
                    Debug.LogWarning($"[VX Ads] Interstitial load failed: {error.GetMessage()}");
                    return;
                }

                interstitialAd = ad;
                interstitialLoaded = true;
            });
        }

        private void LoadAppOpen()
        {
            if (!IsInitialized) return;

            DestroyAppOpen();
            AppOpenAd.Load(GetAppOpenUnitId(), new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    appOpenLoaded = false;
                    return;
                }

                appOpenAd = ad;
                appOpenLoaded = true;
            });
        }

        private bool UseTestUnits()
        {
#if DEVELOPMENT_BUILD
            return true;
#else
            return Debug.isDebugBuild;
#endif
        }

        private string GetAppOpenUnitId() => UseTestUnits() ? AdMobTestIds.AppOpen : config.appOpenUnitId;
        private string GetBannerUnitId() => UseTestUnits() ? AdMobTestIds.Banner : config.bannerUnitId;
        private string GetInterstitialUnitId() => UseTestUnits() ? AdMobTestIds.Interstitial : config.interstitialUnitId;
        private string GetRewardedUnitId() => UseTestUnits() ? AdMobTestIds.Rewarded : config.rewardedUnitId;

        private void DestroyRewarded()
        {
            rewardedAd?.Destroy();
            rewardedAd = null;
            rewardedLoaded = false;
        }

        private void DestroyInterstitial()
        {
            interstitialAd?.Destroy();
            interstitialAd = null;
            interstitialLoaded = false;
        }

        private void DestroyAppOpen()
        {
            appOpenAd?.Destroy();
            appOpenAd = null;
            appOpenLoaded = false;
        }
    }
}
#endif
