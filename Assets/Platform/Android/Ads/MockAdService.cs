using System;

namespace VXMonster.Platform.Ads
{
    public class MockAdService : IAdService
    {
        public bool IsInitialized { get; private set; }
        public bool IsRewardedReady => IsInitialized;
        public bool IsInterstitialReady => IsInitialized;
        public bool IsBannerVisible { get; private set; }

        public void Initialize(Action<bool> onComplete)
        {
            IsInitialized = true;
            onComplete?.Invoke(true);
        }

        public void ShowBanner()
        {
            IsBannerVisible = true;
        }

        public void HideBanner()
        {
            IsBannerVisible = false;
        }

        public void ShowInterstitial(Action onClosed)
        {
            MainThreadDispatcher.Run(() => onClosed?.Invoke());
        }

        public void ShowRewarded(Action onRewardGranted, Action onClosed)
        {
            MainThreadDispatcher.Run(() =>
            {
                onRewardGranted?.Invoke();
                onClosed?.Invoke();
            });
        }

        public void ShowAppOpen(Action onClosed)
        {
            MainThreadDispatcher.Run(() => onClosed?.Invoke());
        }

        public void LoadRewarded() { }

        public void LoadInterstitial() { }
    }
}
