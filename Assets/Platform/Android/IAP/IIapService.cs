using System;

namespace VXMonster.Platform.IAP
{
    public interface IIapService
    {
        bool IsReady { get; }
        void Initialize(Action<bool> onComplete);
        string GetLocalizedPrice(string productId);
        void Purchase(string productId, Action<bool, string> onComplete);
        void RestorePurchases(Action<bool> onComplete);
    }
}
