using VXMonster.Core;
using VXMonster.Save;
using UnityEngine;
using VXMonster.Platform;
using VXMonster.Platform.Analytics;

namespace VXMonster.Platform.IAP
{
    public static class PurchaseFulfillment
    {
        public const int GoldSmallAmount = 500;
        public const int GoldMediumAmount = 1500;
        public const int GoldLargeAmount = 5000;
        public const int StarterBundleGoldAmount = 1000;

        public static bool TryFulfill(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return false;

            switch (productId)
            {
                case IAPProductIds.RemoveAds:
                    GrantRemoveAds();
                    return true;
                case IAPProductIds.GoldSmall:
                    GrantGold(GoldSmallAmount);
                    return true;
                case IAPProductIds.GoldMedium:
                    GrantGold(GoldMediumAmount);
                    return true;
                case IAPProductIds.GoldLarge:
                    GrantGold(GoldLargeAmount);
                    return true;
                case IAPProductIds.StarterBundle:
                    return FulfillStarterBundle();
                default:
                    return false;
            }
        }

        public static void SyncOwnedNonConsumable(string productId)
        {
            if (productId == IAPProductIds.RemoveAds)
            {
                GrantRemoveAds();
                return;
            }

            if (productId == IAPProductIds.StarterBundle)
                FulfillStarterBundle();
        }

        private static bool FulfillStarterBundle()
        {
            GrantRemoveAds();

            if (GameController.SaveManager == null) return true;

            var entitlements = GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements");
            entitlements.StarterBundlePurchased = true;

            if (!entitlements.StarterBundleGoldGranted)
            {
                entitlements.StarterBundleGoldGranted = true;
                GrantGold(StarterBundleGoldAmount);
            }

            GameController.SaveManager.Save(false);
            return true;
        }

        private static void GrantRemoveAds()
        {
            if (GameController.SaveManager == null) return;

            var entitlements = GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements");
            if (entitlements.RemoveAdsPurchased) return;

            entitlements.RemoveAdsPurchased = true;
            GameController.SaveManager.Save(false);
            PlatformServices.BindEntitlements(entitlements);
            PlatformServices.HideBanner();
        }

        private static void GrantGold(int amount)
        {
            if (GameController.CurrenciesManager == null || amount <= 0) return;

            var currency = GameController.CurrenciesManager.GetDefaultCurrency(false);
            currency?.Deposit(amount);
            GameController.SaveManager?.Save(false);
        }
    }
}
