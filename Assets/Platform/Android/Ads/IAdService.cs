using System;
using UnityEngine;

namespace VXMonster.Platform.Ads
{
    public interface IAdService
    {
        bool IsInitialized { get; }
        bool IsRewardedReady { get; }
        bool IsInterstitialReady { get; }
        bool IsBannerVisible { get; }

        void Initialize(Action<bool> onComplete);
        void ShowBanner();
        void HideBanner();
        void ShowInterstitial(Action onClosed);
        void ShowRewarded(Action onRewardGranted, Action onClosed);
        void ShowAppOpen(Action onClosed);
        void LoadRewarded();
        void LoadInterstitial();
    }
}
