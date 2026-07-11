#if UNITY_PURCHASING
// IAP v4 listener APIs remain valid until Unity Purchasing v5 migration (tracked separately).
#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

namespace VXMonster.Platform.IAP
{
    public class UnityPurchasingService : IIapService, IDetailedStoreListener
    {
        private IStoreController storeController;
        private IExtensionProvider extensionProvider;
        private Action<bool, string> pendingPurchaseCallback;
        private bool isInitializing;

        public bool IsReady => storeController != null;

        public void Initialize(Action<bool> onComplete)
        {
            if (IsReady)
            {
                onComplete?.Invoke(true);
                return;
            }

            if (isInitializing)
            {
                onComplete?.Invoke(false);
                return;
            }

            isInitializing = true;
            InitializeCallback = onComplete;
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    var options = new InitializationOptions().SetEnvironmentName("production");
                    await UnityServices.InitializeAsync(options);
                }

                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                builder.AddProduct(IAPProductIds.RemoveAds, ProductType.NonConsumable);
                builder.AddProduct(IAPProductIds.StarterBundle, ProductType.NonConsumable);
                builder.AddProduct(IAPProductIds.GoldSmall, ProductType.Consumable);
                builder.AddProduct(IAPProductIds.GoldMedium, ProductType.Consumable);
                builder.AddProduct(IAPProductIds.GoldLarge, ProductType.Consumable);

                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception ex)
            {
                isInitializing = false;
                Debug.LogWarning($"[IAP] UGS/IAP init failed: {ex.Message}");
                InitializeCallback?.Invoke(false);
                InitializeCallback = null;
            }
        }

        private Action<bool> InitializeCallback;

        public string GetLocalizedPrice(string productId)
        {
            if (storeController == null) return "—";

            var product = storeController.products.WithID(productId);
            return product != null && product.availableToPurchase
                ? product.metadata.localizedPriceString
                : "—";
        }

        public void Purchase(string productId, Action<bool, string> onComplete)
        {
            if (!IsReady)
            {
                onComplete?.Invoke(false, "Store not ready.");
                return;
            }

            pendingPurchaseCallback = onComplete;
            storeController.InitiatePurchase(productId);
        }

        public void RestorePurchases(Action<bool> onComplete)
        {
            if (extensionProvider == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            var apple = extensionProvider.GetExtension<IAppleExtensions>();
            if (apple != null)
            {
                apple.RestoreTransactions((success, _) => onComplete?.Invoke(success));
                return;
            }

            onComplete?.Invoke(true);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            extensionProvider = extensions;
            isInitializing = false;
            InitializeCallback?.Invoke(true);
            InitializeCallback = null;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            isInitializing = false;
            Debug.LogWarning($"[IAP] Initialize failed: {error}");
            InitializeCallback?.Invoke(false);
            InitializeCallback = null;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            isInitializing = false;
            Debug.LogWarning($"[IAP] Initialize failed: {error} — {message}");
            InitializeCallback?.Invoke(false);
            InitializeCallback = null;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var productId = purchaseEvent.purchasedProduct.definition.id;
            var success = PurchaseFulfillment.TryFulfill(productId);
            pendingPurchaseCallback?.Invoke(success, success ? string.Empty : "Fulfillment failed.");
            if (success) VXMonster.Platform.Analytics.AnalyticsEvents.LogIapPurchase(productId, true);
            else VXMonster.Platform.Analytics.AnalyticsEvents.LogIapPurchase(productId, false);
            pendingPurchaseCallback = null;
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            pendingPurchaseCallback?.Invoke(false, failureReason.ToString());
            pendingPurchaseCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            pendingPurchaseCallback?.Invoke(false, failureDescription.message);
            pendingPurchaseCallback = null;
        }
    }
}
#pragma warning restore CS0618
#endif
