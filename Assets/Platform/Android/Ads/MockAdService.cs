using System;
using UnityEngine;

namespace VXMonster.Platform.Ads
{
    /// <summary>
    /// Safe fallback for Editor and dev builds until Google Mobile Ads SDK resolves in Unity.
    /// </summary>
    public class MockAdService : IAdService
    {
        public bool IsInitialized { get; private set; }
        public bool IsRewardedReady => IsInitialized;
        public bool IsInterstitialReady => IsInitialized;
        public bool IsBannerVisible { get; private set; }

        public void Initialize(Action<bool> onComplete)
        {
            IsInitialized = true;
            Debug.Log("[VX Ads] MockAdService initialized.");
            onComplete?.Invoke(true);
        }

        public void ShowBanner()
        {
            IsBannerVisible = true;
            Debug.Log("[VX Ads] Mock banner shown.");
        }

        public void HideBanner()
        {
            IsBannerVisible = false;
            Debug.Log("[VX Ads] Mock banner hidden.");
        }

        public void ShowInterstitial(Action onClosed)
        {
            Debug.Log("[VX Ads] Mock interstitial shown.");
            MainThreadDispatcher.Run(() => onClosed?.Invoke());
        }

        public void ShowRewarded(Action onRewardGranted, Action onClosed)
        {
            Debug.Log("[VX Ads] Mock rewarded completed — granting revive.");
            MainThreadDispatcher.Run(() =>
            {
                onRewardGranted?.Invoke();
                onClosed?.Invoke();
            });
        }

        public void ShowAppOpen(Action onClosed)
        {
            Debug.Log("[VX Ads] Mock app open shown.");
            MainThreadDispatcher.Run(() => onClosed?.Invoke());
        }

        public void LoadRewarded()
        {
            Debug.Log("[VX Ads] Mock rewarded load requested.");
        }

        public void LoadInterstitial()
        {
            Debug.Log("[VX Ads] Mock interstitial load requested.");
        }
    }
}
