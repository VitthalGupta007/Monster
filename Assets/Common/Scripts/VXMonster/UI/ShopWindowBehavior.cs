using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

        Coroutine refreshRoutine;

        private void Awake()
        {
            if (restoreButton != null)
            {
                restoreButton.onClick.AddListener(OnRestoreClicked);
            }

            foreach (var row in productRows)
            {
                if (row.buyButton == null || string.IsNullOrEmpty(row.productId)) continue;

                var captured = row;
                row.buyButton.onClick.AddListener(() => OnBuyClicked(captured));
            }
        }

        public void Init(UnityAction onBackClicked)
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(() =>
                {
                    Close();
                    onBackClicked?.Invoke();
                });
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Refresh();
            if (refreshRoutine != null) StopCoroutine(refreshRoutine);
            refreshRoutine = StartCoroutine(RefreshUntilStoreReady());
        }

        public void Close()
        {
            if (refreshRoutine != null)
            {
                StopCoroutine(refreshRoutine);
                refreshRoutine = null;
            }
            gameObject.SetActive(false);
        }

        private IEnumerator RefreshUntilStoreReady()
        {
            for (var i = 0; i < 24 && !PlatformServices.IsIapReady; i++)
            {
                SetStatus("Connecting to Google Play…");
                yield return new WaitForSeconds(0.5f);
                Refresh();
            }

            if (!PlatformServices.IsIapReady)
                SetStatus("Store not ready. Check Play Console product is Active and you are a license tester.");

            Refresh();
            refreshRoutine = null;
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
            if (GameController.SaveManager == null) return false;

            var entitlements = GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements");
            if (productId == IAPProductIds.RemoveAds)
                return entitlements.RemoveAdsPurchased;

            if (productId == IAPProductIds.StarterBundle)
                return entitlements.StarterBundlePurchased;

            return false;
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
