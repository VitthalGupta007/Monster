using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Platform;
using VXMonster.Platform.IAP;
using VXMonster.Save;

namespace VXMonster.UI
{
    public class ShopWindowBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text statusLabel;
        [SerializeField] Button restoreButton;
        [SerializeField] Button backButton;
        [SerializeField] List<ShopProductRow> productRows = new List<ShopProductRow>();

        [System.Serializable]
        public class ShopProductRow
        {
            public string productId;
            public Button buyButton;
            public TMP_Text priceLabel;
            public TMP_Text titleLabel;
        }

        private void Awake()
        {
            if (restoreButton != null)
            {
                restoreButton.onClick.AddListener(OnRestoreClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(Close);
            }

            foreach (var row in productRows)
            {
                if (row.buyButton == null || string.IsNullOrEmpty(row.productId)) continue;

                var captured = row;
                row.buyButton.onClick.AddListener(() => OnBuyClicked(captured));
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            SetStatus(string.Empty);

            var iap = PlatformServices.IapService;
            var ready = iap != null && iap.IsReady;

            foreach (var row in productRows)
            {
                if (row.priceLabel != null && iap != null)
                {
                    row.priceLabel.text = iap.GetLocalizedPrice(row.productId);
                }

                if (row.buyButton != null)
                {
                    row.buyButton.interactable = ready && !IsOwned(row.productId);
                }
            }

            if (restoreButton != null)
            {
                restoreButton.interactable = ready;
            }
        }

        private static bool IsOwned(string productId)
        {
            if (productId != IAPProductIds.RemoveAds && productId != IAPProductIds.StarterBundle) return false;
            if (GameController.SaveManager == null) return false;

            return GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements").RemoveAdsPurchased;
        }

        private void OnBuyClicked(ShopProductRow row)
        {
            var iap = PlatformServices.IapService;
            if (iap == null || !iap.IsReady)
            {
                SetStatus("Store not ready yet.");
                return;
            }

            if (row.buyButton != null) row.buyButton.interactable = false;
            SetStatus("Processing...");

            iap.Purchase(row.productId, (success, message) =>
            {
                if (success)
                {
                    if (GameController.SaveManager != null)
                    {
                        PlatformServices.BindEntitlements(
                            GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements"));
                    }

                    PlatformServices.HideBanner();
                    SetStatus("Purchase complete!");
                    Refresh();
                    return;
                }

                SetStatus(string.IsNullOrEmpty(message) ? "Purchase failed." : message);
                Refresh();
            });
        }

        private void OnRestoreClicked()
        {
            var iap = PlatformServices.IapService;
            if (iap == null || !iap.IsReady)
            {
                SetStatus("Store not ready yet.");
                return;
            }

            SetStatus("Restoring...");
            iap.RestorePurchases(success =>
            {
                SetStatus(success ? "Restore complete." : "Restore failed.");
                Refresh();
            });
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.text = message;
            }
        }
    }
}
