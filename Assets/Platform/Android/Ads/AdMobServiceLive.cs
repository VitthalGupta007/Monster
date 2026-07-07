#if GOOGLE_MOBILE_ADS_AVAILABLE
using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace VXMonster.Platform.Ads
{
    /// <summary>
    /// Live AdMob integration for Android/iOS device builds.
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
                MainThreadDispatcher.Run(() =>
                {
                    IsInitialized = true;
                    Debug.Log("[VX Ads] AdMob SDK initialized.");
                    LoadRewarded();
                    LoadInterstitial();
                    LoadAppOpen();
                    onComplete?.Invoke(true);
                });
            });
        }

        public void ShowBanner()
        {
            if (!IsInitialized)
            {
                Debug.Log("[VX Ads] Banner skipped — SDK not initialized yet.");
                return;
            }

            if (bannerView == null)
            {
                bannerView = new BannerView(GetBannerUnitId(), AdSize.Banner, AdPosition.Bottom);
                bannerView.OnBannerAdLoaded += () =>
                {
                    MainThreadDispatcher.Run(() => Debug.Log("[VX Ads] Banner ad loaded."));
                };
                bannerView.OnBannerAdLoadFailed += error =>
                {
                    MainThreadDispatcher.Run(() =>
                        Debug.LogWarning($"[VX Ads] Banner load failed: {error.GetMessage()}"));
                };
            }

            Debug.Log("[VX Ads] Banner requested.");
            bannerView.LoadAd(new AdRequest());
            bannerView.Show();
            IsBannerVisible = true;
            Debug.Log("[VX Ads] Banner shown.");
        }

        public void HideBanner()
        {
            bannerView?.Hide();
            IsBannerVisible = false;
            Debug.Log("[VX Ads] Banner hidden.");
        }

        public void ShowInterstitial(Action onClosed)
        {
            if (!IsInterstitialReady)
            {
                Debug.Log("[VX Ads] Interstitial not ready — skipping.");
                MainThreadDispatcher.Run(() => onClosed?.Invoke());
                return;
            }

            Debug.Log("[VX Ads] Interstitial showing.");

            void HandleClosed()
            {
                MainThreadDispatcher.Run(() =>
                {
                    Debug.Log("[VX Ads] Interstitial closed.");
                    onClosed?.Invoke();
                    DestroyInterstitial();
                    LoadInterstitial();
                });
            }

            interstitialAd.OnAdFullScreenContentClosed += HandleClosed;
            interstitialAd.OnAdFullScreenContentFailed += _ => HandleClosed();
            interstitialAd.Show();
        }

        public void ShowRewarded(Action onRewardGranted, Action onClosed)
        {
            if (!IsRewardedReady)
            {
                Debug.Log("[VX Ads] Rewarded not ready — skipping.");
                MainThreadDispatcher.Run(() => onClosed?.Invoke());
                return;
            }

            var rewardGranted = false;

            void HandleClosed()
            {
                MainThreadDispatcher.Run(() =>
                {
                    if (rewardGranted)
                    {
                        Debug.Log("[VX Ads] Rewarded completed — granting revive.");
                        onRewardGranted?.Invoke();
                    }
                    else
                    {
                        Debug.Log("[VX Ads] Rewarded closed without reward.");
                    }

                    onClosed?.Invoke();
                    DestroyRewarded();
                    LoadRewarded();
                });
            }

            rewardedAd.OnAdFullScreenContentClosed += HandleClosed;
            rewardedAd.OnAdFullScreenContentFailed += _ =>
            {
                MainThreadDispatcher.Run(() =>
                {
                    Debug.LogWarning("[VX Ads] Rewarded display failed.");
                    onClosed?.Invoke();
                    DestroyRewarded();
                    LoadRewarded();
                });
            };

            Debug.Log("[VX Ads] Rewarded showing.");
            rewardedAd.Show(_ => { rewardGranted = true; });
        }

        public void ShowAppOpen(Action onClosed)
        {
            if (!appOpenLoaded || appOpenAd == null || !appOpenAd.CanShowAd())
            {
                Debug.Log("[VX Ads] App open not ready — skipping.");
                MainThreadDispatcher.Run(() => onClosed?.Invoke());
                LoadAppOpen();
                return;
            }

            Debug.Log("[VX Ads] App open showing.");

            void HandleClosed()
            {
                MainThreadDispatcher.Run(() =>
                {
                    Debug.Log("[VX Ads] App open closed.");
                    onClosed?.Invoke();
                    DestroyAppOpen();
                    LoadAppOpen();
                });
            }

            appOpenAd.OnAdFullScreenContentClosed += HandleClosed;
            appOpenAd.OnAdFullScreenContentFailed += _ => HandleClosed();
            appOpenAd.Show();
        }

        public void LoadRewarded()
        {
            if (!IsInitialized) return;

            DestroyRewarded();
            RewardedAd.Load(GetRewardedUnitId(), new AdRequest(), (ad, error) =>
            {
                MainThreadDispatcher.Run(() =>
                {
                    if (error != null)
                    {
                        rewardedLoaded = false;
                        Debug.LogWarning($"[VX Ads] Rewarded load failed: {error.GetMessage()}");
                        return;
                    }

                    rewardedAd = ad;
                    rewardedLoaded = true;
                    Debug.Log("[VX Ads] Rewarded ad loaded.");
                });
            });
        }

        public void LoadInterstitial()
        {
            if (!IsInitialized) return;

            DestroyInterstitial();
            InterstitialAd.Load(GetInterstitialUnitId(), new AdRequest(), (ad, error) =>
            {
                MainThreadDispatcher.Run(() =>
                {
                    if (error != null)
                    {
                        interstitialLoaded = false;
                        Debug.LogWarning($"[VX Ads] Interstitial load failed: {error.GetMessage()}");
                        return;
                    }

                    interstitialAd = ad;
                    interstitialLoaded = true;
                    Debug.Log("[VX Ads] Interstitial ad loaded.");
                });
            });
        }

        private void LoadAppOpen()
        {
            if (!IsInitialized) return;

            DestroyAppOpen();
            AppOpenAd.Load(GetAppOpenUnitId(), new AdRequest(), (ad, error) =>
            {
                MainThreadDispatcher.Run(() =>
                {
                    if (error != null)
                    {
                        appOpenLoaded = false;
                        Debug.LogWarning($"[VX Ads] App open load failed: {error.GetMessage()}");
                        return;
                    }

                    appOpenAd = ad;
                    appOpenLoaded = true;
                    Debug.Log("[VX Ads] App open ad loaded.");
                });
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
