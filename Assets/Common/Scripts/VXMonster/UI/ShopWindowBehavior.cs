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
        const string IdleStatusMessage =
            "Prices load from Google Play. Restore syncs Remove Ads & Starter Bundle after reinstall.";

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
            public TMP_Text subtitleLabel;
        }

        Coroutine refreshRoutine;
        bool hasTransientStatus;

        private void Awake()
        {
            WireProductRows();
        }

        private void OnEnable()
        {
            if (restoreButton != null)
            {
                restoreButton.onClick.RemoveListener(OnRestoreClicked);
                restoreButton.onClick.AddListener(OnRestoreClicked);
            }
        }

        private void WireProductRows()
        {
            foreach (var row in productRows)
            {
                if (row.buyButton == null || string.IsNullOrEmpty(row.productId)) continue;

                var captured = row;
                row.buyButton.onClick.RemoveAllListeners();
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
            hasTransientStatus = false;
            SetStatus(IdleStatusMessage, transient: false);
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
                SetStatus("Connecting to Google Play…", transient: true);
                yield return new WaitForSeconds(0.5f);
                Refresh();
            }

            if (!PlatformServices.IsIapReady)
                SetStatus("Store not ready. Check Play Console product is Active and you are a license tester.", transient: true);
            else if (hasTransientStatus)
                SetStatus(IdleStatusMessage, transient: false);

            Refresh();
            refreshRoutine = null;
        }

        private void Refresh()
        {
            var iap = PlatformServices.IapService;
            var ready = iap != null && iap.IsReady;

            foreach (var row in productRows)
            {
                if (string.IsNullOrEmpty(row.productId)) continue;

                var info = ShopProductCatalog.Get(row.productId);
                ApplyProductInfo(row, info);

                var owned = IsOwned(row.productId);

                if (row.priceLabel != null)
                {
                    row.priceLabel.text = owned
                        ? "Owned"
                        : ready && iap != null
                            ? IapPriceDisplay.Format(iap.GetLocalizedPrice(row.productId))
                            : "Loading…";
                }

                if (row.buyButton != null)
                {
                    row.buyButton.interactable = ready && !owned;
                    var buyLabel = row.buyButton.GetComponentInChildren<TMP_Text>();
                    if (buyLabel != null) buyLabel.text = owned ? "OWNED" : "BUY";
                }
            }

            if (restoreButton != null)
            {
                restoreButton.interactable = ready;
            }

            if (!hasTransientStatus && statusLabel != null && string.IsNullOrEmpty(statusLabel.text))
                SetStatus(IdleStatusMessage, transient: false);
        }

        static void ApplyProductInfo(ShopProductRow row, ShopProductInfo info)
        {
            if (row.titleLabel == null) return;

            row.titleLabel.text = info.Title;

            if (row.subtitleLabel != null)
            {
                row.subtitleLabel.text = info.Description;
                row.subtitleLabel.gameObject.SetActive(!string.IsNullOrEmpty(info.Description));
                return;
            }

            if (string.IsNullOrEmpty(info.Description)) return;

            row.titleLabel.text =
                $"{info.Title}\n<size=22><color=#B8B8C8>{info.Description}</color></size>";
            row.titleLabel.richText = true;
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
            SetStatus("Processing...", transient: true);

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
                    SetStatus("Purchase complete!", transient: true);
                    Refresh();
                    return;
                }

                SetStatus(string.IsNullOrEmpty(message) ? "Purchase failed." : message, transient: true);
                Refresh();
            });
        }

        private void OnRestoreClicked()
        {
            var iap = PlatformServices.IapService;
            if (iap == null || !iap.IsReady)
            {
                SetStatus("Store not ready yet.", transient: true);
                return;
            }

            SetStatus("Restoring purchases from Google Play…", transient: true);
            iap.RestorePurchases(success =>
            {
                if (GameController.SaveManager != null)
                {
                    PlatformServices.BindEntitlements(
                        GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements"));
                }

                PlatformServices.HideBanner();
                SetStatus(
                    success
                        ? "Restore complete. Remove Ads & Starter Bundle synced to this device."
                        : "Restore failed. Try again while signed into the same Google Play account.",
                    transient: true);
                Refresh();
            });
        }

        private void SetStatus(string message, bool transient = true)
        {
            hasTransientStatus = transient;
            if (statusLabel != null)
            {
                statusLabel.text = message;
            }
        }
    }
}
