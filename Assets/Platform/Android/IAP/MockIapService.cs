using System;
using System.Collections.Generic;
using VXMonster.Platform.Analytics;

namespace VXMonster.Platform.IAP
{
    public class MockIapService : IIapService
    {
        private static readonly Dictionary<string, string> DefaultPrices = new Dictionary<string, string>
        {
            { IAPProductIds.RemoveAds, "$4.99" },
            { IAPProductIds.GoldSmall, "$0.99" },
            { IAPProductIds.GoldMedium, "$2.99" },
            { IAPProductIds.GoldLarge, "$7.99" },
            { IAPProductIds.StarterBundle, "$9.99" }
        };

        public bool IsReady { get; private set; }

        public void Initialize(Action<bool> onComplete)
        {
            IsReady = true;
            onComplete?.Invoke(true);
        }

        public string GetLocalizedPrice(string productId)
        {
            return DefaultPrices.TryGetValue(productId, out var price) ? price : "—";
        }

        public void Purchase(string productId, Action<bool, string> onComplete)
        {
            if (!IsReady)
            {
                onComplete?.Invoke(false, "Store not ready.");
                return;
            }

            if (!PurchaseFulfillment.TryFulfill(productId))
            {
                onComplete?.Invoke(false, "Unknown product.");
                AnalyticsEvents.LogIapPurchase(productId, false);
                return;
            }

            AnalyticsEvents.LogIapPurchase(productId, true);
            onComplete?.Invoke(true, string.Empty);
        }

        public void RestorePurchases(Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }
}
