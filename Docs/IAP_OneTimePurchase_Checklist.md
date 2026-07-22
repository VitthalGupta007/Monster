# IAP — Production & pre-launch checklist

Product IDs must match [`IAPProductIds.cs`](../Assets/Platform/Android/IAP/IAPProductIds.cs) exactly.

Play Console: **Monetize with Play → Products → One-time products**  
Purchase option ID: `default` · Purchase type: **Buy**

---

## Products (India INR)

| Product ID | Play type | Unity type | INR | In-game |
|------------|-----------|------------|-----|---------|
| `com.vitthalxstudios.monster.remove_ads` | One-time | NonConsumable | **₹250** | Remove ads + free revive |
| `com.vitthalxstudios.monster.starter_bundle` | One-time | NonConsumable | **₹300** | Remove ads + 1,000 gold (once) |
| `com.vitthalxstudios.monster.gold_small` | One-time* | Consumable | **₹100** | +500 gold |
| `com.vitthalxstudios.monster.gold_medium` | One-time* | Consumable | **₹199** | +1,500 gold |
| `com.vitthalxstudios.monster.gold_large` | One-time* | Consumable | **₹300** | +5,000 gold |

\*Gold packs are **Consumable** in Unity (repeatable purchases).

**Starter Bundle** calls `GrantRemoveAds()` — buyer gets Remove Ads without a separate purchase.

---

## Pre-launch QA (closed testing track)

Use **license testers** only for final QA before Production — not for live players.

1. Play Console → **Settings → License testing** → add QA Gmail accounts → **RESPOND_NORMALLY**
2. Install from **closed testing opt-in link** (same Gmail on device Play Store)
3. Build **release-signed AAB** with incremented version code
4. Upload to **Closed testing** → roll out

### In-game tests

1. Wait ~10s after launch (IAP init; shop polls up to 12s)
2. **MENU → SHOP** — titles, descriptions, prices visible
3. **Restore** — status text; syncs Remove Ads & Starter Bundle from Google Play
4. Buy **Remove Ads** → ads stop; row shows **Owned**
5. Buy gold pack → gold increases
6. **Starter Bundle** (fresh save) → ads off + 1,000 gold once
7. Force-stop → reopen → entitlements persist
8. logcat: `[IAP]`, `[PlayIntegrity]`; Firebase `iap_purchase` event

---

## Production (live on Play Store)

After **Production rollout**:

- **No license testers required** — any user with a valid payment method can purchase
- Prices shown are localized by Google Play (INR set in Console)
- **Restore** uses `IGooglePlayStoreExtensions.RestoreTransactions` + `SyncOwnedNonConsumables()` for Remove Ads and Starter Bundle
- Gold packs are consumable — **not** restored (by design)

Monitor: Play Console → **Monetize → Monetization setup** for order issues.

---

## Restore — how it works

| Step | What happens |
|------|----------------|
| User taps **RESTORE** | Shop calls `IapService.RestorePurchases` |
| Google Play (Android) | `RestoreTransactions` queries past non-consumable purchases |
| App | `SyncOwnedNonConsumables()` reads receipts → updates `EntitlementsSave` |
| Remove Ads | `RemoveAdsPurchased = true` → ads hidden |
| Starter Bundle | `StarterBundlePurchased = true` + Remove Ads + gold (once) |
| Gold packs | Not restored (consumables) |

Shop footer: *"Restore syncs Remove Ads & Starter Bundle after reinstall."*

---

## If purchase fails

| Symptom | Check |
|---------|-------|
| Price `Loading…` forever | Product inactive, payments profile incomplete, not installed from Play |
| `Store not ready` | Wait 12s; logcat `[IAP] Store init failed` |
| Billing unavailable | Play Services updated, signed into Play Store |
| Restore does nothing | Same Google account as original purchase; store must be ready |

Logcat filters: `IAP`, `PlayIntegrity`, `Firebase`

---

## Code map

| File | Role |
|------|------|
| `IAPProductIds.cs` | Product ID constants |
| `UnityPurchasingService.cs` | Google Play billing + restore |
| `PurchaseFulfillment.cs` | Grants ads off, gold, bundle |
| `ShopWindowBehavior.cs` | Shop UI + restore button |
| `ShopProductCatalog.cs` | Product titles & descriptions |
| `EntitlementsSave.cs` | Persistent ownership flags |
