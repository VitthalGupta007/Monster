# One-time IAP — Remove Ads (test checklist)

Product ID (already in code + shop UI):

`com.vitthalxstudios.monster.remove_ads`

Type in Unity: **NonConsumable**  
Type in Play Console: **Managed product / One-time**

---

## 1. Play Console — create the product (if not already)

1. Open [Google Play Console](https://play.google.com/console) → app **VX Monster** (`com.vitthalxstudios.monster`).
2. **Monetize** → **Products** → **In-app products** → **Create product**.
3. Set:
   - **Product ID:** `com.vitthalxstudios.monster.remove_ads` (must match exactly; cannot change later)
   - **Name:** Remove Ads
   - **Description:** Removes banner/interstitial ads; includes one free revive per run
   - **Product type:** One-time (non-consumable)
4. Set a default price (e.g. $2.99 / ₹249 — your choice).
5. **Activate** the product (Inactive products do not bill).

Optional later (same flow, separate IDs already in code):

| Product ID | Type |
|---|---|
| `com.vitthalxstudios.monster.gold_small` | Consumable |
| `com.vitthalxstudios.monster.gold_medium` | Consumable |
| `com.vitthalxstudios.monster.gold_large` | Consumable |
| `com.vitthalxstudios.monster.starter_bundle` | One-time |

---

## 2. License testers (required for unpaid test buys)

1. Play Console → **Settings** (or **Setup**) → **License testing**.
2. Add Gmail accounts that will install / buy on device.
3. Set license response to **RESPOND_NORMALLY** (default).
4. Those accounts get test purchases (no real charge when app is signed with upload key and installed from an internal/closed track or sideloaded as licensed tester — see Google’s current IAP testing notes).

Also: on the test phone, sign in to Play Store with **that same tester Gmail**.

---

## 3. Upload a release build (AAB)

1. Unity → **File → Build Settings → Android** → **Build App Bundle (Google Play)** checked.
2. Use upload keystore (`vx-monster-upload.keystore` / alias `vxmonster`).
3. Bump **Version Code** if Play already has code `5` uploaded (set to `6`+).
4. Play Console → **Testing** → **Internal testing** (fastest) → create release → upload AAB → roll out.
5. Add your tester email to the internal testers list and open the opt-in link on the device.

> Direct “sideload AAB” without Play usually cannot complete real BillingClient flows. Prefer Internal testing install.

---

## 4. Test purchase in-game

1. Install from Internal testing track (Play Store).
2. Open game → Shop → **Remove Ads**.
3. Confirm price shows a real Play price (not `—`). If `—`, IAP did not init or product ID / status is wrong.
4. Buy → Play billing sheet → complete.
5. Expect: “Purchase complete!”, ads stop, Remove Ads button disabled.
6. Kill app and reopen: entitlement should still hold (save + receipt sync).
7. Optional: clear app data → relaunch → owned product should re-grant via receipt sync.

---

## 5. If purchase fails — quick checks

| Symptom | Check |
|---|---|
| Price is `—` | Product inactive, wrong ID, or app not installed from Play / wrong package name |
| `Store not ready` | Wait a few seconds after launch; check device log for `[IAP]` |
| Item unavailable | Product not Activated; AAB package ≠ `com.vitthalxstudios.monster` |
| Already owned | Correct for non-consumable; use another tester account to re-test buy |
| Billing unavailable | Old Play Services / no Play account on device |

Logcat filter: `IAP` or `Unity`.

---

## Code map (already wired)

- IDs: `Assets/Platform/Android/IAP/IAPProductIds.cs`
- Init / purchase: `UnityPurchasingService.cs` (`ProductType.NonConsumable`)
- Grant: `PurchaseFulfillment.GrantRemoveAds()` → `EntitlementsSave.RemoveAdsPurchased`
- Shop row: Main Menu Screen prefab → productId `com.vitthalxstudios.monster.remove_ads`
