using System;

namespace VXMonster.Platform.iOS
{
    /// <summary>
    /// Placeholder for iOS ad mediation (Phase 8).
    /// </summary>
    public class IOSAdServiceStub : VXMonster.Platform.Ads.IAdService
    {
        public bool IsInitialized => true;
        public bool IsRewardedReady => false;
        public bool IsInterstitialReady => false;
        public bool IsBannerVisible => false;

        public void Initialize(Action<bool> onComplete) => onComplete?.Invoke(true);
        public void ShowBanner() { }
        public void HideBanner() { }
        public void ShowInterstitial(Action onClosed) => onClosed?.Invoke();
        public void ShowRewarded(Action onRewardGranted, Action onClosed)
        {
            onRewardGranted?.Invoke();
            onClosed?.Invoke();
        }
        public void ShowAppOpen(Action onClosed) => onClosed?.Invoke();
        public void LoadRewarded() { }
        public void LoadInterstitial() { }
    }
}
