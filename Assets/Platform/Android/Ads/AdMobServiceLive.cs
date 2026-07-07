#if GOOGLE_MOBILE_ADS_AVAILABLE
using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace VXMonster.Platform.Ads
{
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
                    LoadRewarded();
                    LoadInterstitial();
                    LoadAppOpen();
                    onComplete?.Invoke(true);
                });
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
                    MainThreadDispatcher.Run(() =>
                        Debug.LogWarning($"[VX Ads] Banner load failed: {error.GetMessage()}"));
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
                MainThreadDispatcher.Run(() => onClosed?.Invoke());
                return;
            }

            void HandleClosed()
            {
                MainThreadDispatcher.Run(() =>
                {
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
                        onRewardGranted?.Invoke();
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
                MainThreadDispatcher.Run(() => onClosed?.Invoke());
                LoadAppOpen();
                return;
            }

            void HandleClosed()
            {
                MainThreadDispatcher.Run(() =>
                {
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
