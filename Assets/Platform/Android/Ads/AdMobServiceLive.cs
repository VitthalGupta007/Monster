#if GOOGLE_MOBILE_ADS_AVAILABLE
using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace VXMonster.Platform.Ads
{
    public class AdMobService : IAdService
    {
        private const string LogTag = "[VX Ads]";

        private readonly AdMobConfig config;
        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;
        private AppOpenAd appOpenAd;

        private bool rewardedLoaded;
        private bool interstitialLoaded;
        private bool appOpenLoaded;

        private bool rewardedLoading;
        private bool interstitialLoading;
        private bool appOpenLoading;

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
                    Debug.Log($"{LogTag} AdMob SDK initialized.");
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
                Debug.Log($"{LogTag} Banner skipped — SDK not initialized.");
                return;
            }

            if (bannerView == null)
            {
                bannerView = new BannerView(config.bannerUnitId, AdSize.Banner, AdPosition.Bottom);
                bannerView.OnBannerAdLoaded += () =>
                    MainThreadDispatcher.Run(() => Debug.Log($"{LogTag} Banner loaded."));
                bannerView.OnBannerAdLoadFailed += error =>
                    MainThreadDispatcher.Run(() =>
                        Debug.LogWarning($"{LogTag} Banner load failed: {error.GetMessage()}"));
            }

            Debug.Log($"{LogTag} Banner requested.");
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
                Debug.Log($"{LogTag} Interstitial not ready — skipping.");
                MainThreadDispatcher.Run(() => onClosed?.Invoke());
                return;
            }

            Debug.Log($"{LogTag} Interstitial showing.");

            void HandleClosed()
            {
                MainThreadDispatcher.Run(() =>
                {
                    Debug.Log($"{LogTag} Interstitial closed.");
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
                Debug.Log($"{LogTag} Rewarded not ready — skipping.");
                MainThreadDispatcher.Run(() => onClosed?.Invoke());
                return;
            }

            Debug.Log($"{LogTag} Rewarded showing.");

            var rewardGranted = false;

            void HandleClosed()
            {
                MainThreadDispatcher.Run(() =>
                {
                    if (rewardGranted)
                    {
                        Debug.Log($"{LogTag} Rewarded completed — granting reward.");
                        onRewardGranted?.Invoke();
                    }
                    else
                    {
                        Debug.Log($"{LogTag} Rewarded closed without reward.");
                    }

                    onClosed?.Invoke();
                    DestroyRewarded();
                    LoadRewarded();
                });
            }

            rewardedAd.OnAdFullScreenContentClosed += HandleClosed;
            rewardedAd.OnAdFullScreenContentFailed += error =>
            {
                MainThreadDispatcher.Run(() =>
                {
                    Debug.LogWarning($"{LogTag} Rewarded display failed: {error}");
                    onClosed?.Invoke();
                    DestroyRewarded();
                    LoadRewarded();
                });
            };

            rewardedAd.Show(_ => { rewardGranted = true; });
        }

        public void ShowAppOpen(Action onClosed)
        {
            if (!appOpenLoaded || appOpenAd == null || !appOpenAd.CanShowAd())
            {
                Debug.Log($"{LogTag} App open not ready — skipping.");
                MainThreadDispatcher.Run(() => onClosed?.Invoke());
                LoadAppOpen();
                return;
            }

            Debug.Log($"{LogTag} App open showing.");

            void HandleClosed()
            {
                MainThreadDispatcher.Run(() =>
                {
                    Debug.Log($"{LogTag} App open closed.");
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
            if (!IsInitialized || IsRewardedReady || rewardedLoading) return;

            rewardedLoading = true;
            DestroyRewarded();
            RewardedAd.Load(config.rewardedUnitId, new AdRequest(), (ad, error) =>
            {
                MainThreadDispatcher.Run(() =>
                {
                    rewardedLoading = false;

                    if (error != null)
                    {
                        rewardedLoaded = false;
                        Debug.LogWarning($"{LogTag} Rewarded load failed: {error.GetMessage()}");
                        return;
                    }

                    rewardedAd = ad;
                    rewardedLoaded = true;
                    Debug.Log($"{LogTag} Rewarded loaded.");
                });
            });
        }

        public void LoadInterstitial()
        {
            if (!IsInitialized || IsInterstitialReady || interstitialLoading) return;

            interstitialLoading = true;
            DestroyInterstitial();
            InterstitialAd.Load(config.interstitialUnitId, new AdRequest(), (ad, error) =>
            {
                MainThreadDispatcher.Run(() =>
                {
                    interstitialLoading = false;

                    if (error != null)
                    {
                        interstitialLoaded = false;
                        Debug.LogWarning($"{LogTag} Interstitial load failed: {error.GetMessage()}");
                        return;
                    }

                    interstitialAd = ad;
                    interstitialLoaded = true;
                    Debug.Log($"{LogTag} Interstitial loaded.");
                });
            });
        }

        private void LoadAppOpen()
        {
            if (!IsInitialized || appOpenLoading) return;
            if (appOpenLoaded && appOpenAd != null && appOpenAd.CanShowAd()) return;

            appOpenLoading = true;
            DestroyAppOpen();
            AppOpenAd.Load(config.appOpenUnitId, new AdRequest(), (ad, error) =>
            {
                MainThreadDispatcher.Run(() =>
                {
                    appOpenLoading = false;

                    if (error != null)
                    {
                        appOpenLoaded = false;
                        Debug.LogWarning($"{LogTag} App open load failed: {error.GetMessage()}");
                        return;
                    }

                    appOpenAd = ad;
                    appOpenLoaded = true;
                    Debug.Log($"{LogTag} App open loaded.");
                });
            });
        }

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
