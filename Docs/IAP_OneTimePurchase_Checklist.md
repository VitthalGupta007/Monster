# One-time IAP — test checklist

All product IDs must match [`IAPProductIds.cs`](../Assets/Platform/Android/IAP/IAPProductIds.cs) exactly.

Play Console path: **Monetize with Play → Products → One-time products**  
Purchase option ID for each: `default` · Purchase type: **Buy**

---

## Active products and India (INR) prices

| Product ID | Play Console type | Unity type | India (INR) | In-game |
|------------|-------------------|------------|---------------|---------|
| `com.vitthalxstudios.monster.remove_ads` | One-time | NonConsumable | **₹250** | Remove ads + free revive |
| `com.vitthalxstudios.monster.starter_bundle` | One-time | NonConsumable | **₹300** | Remove ads + 1,000 gold (once) |
| `com.vitthalxstudios.monster.gold_small` | One-time* | Consumable | **₹100** | +500 gold (repeatable) |
| `com.vitthalxstudios.monster.gold_medium` | One-time* | Consumable | **₹199** | +1,500 gold (repeatable) |
| `com.vitthalxstudios.monster.gold_large` | One-time* | Consumable | **₹300** | +5,000 gold (repeatable) |

\*Play Console lists all as **One-time products**; gold packs are **Consumable** in Unity code (buy again after consume).

---

## License testers (required for test buys)

1. Play Console → **Settings** → **License testing**
2. Add tester Gmail accounts
3. Set **RESPOND_NORMALLY**
4. On device: same Gmail in Play Store; install from **closed testing opt-in link**

---

## Upload AAB (single v6+ build)

1. Unity → **Build App Bundle (Google Play)** with upload keystore
2. Bump **Version Code** to **6+** if Play has **5**
3. Closed testing → upload → roll out → install via opt-in link

---

## Test in-game

1. Wait ~10s after launch (IAP init + shop poll up to 12s)
2. **MENU → SHOP** — all rows show real prices (not `—`)
3. Buy **Remove Ads** → ads stop, Buy disabled
4. Buy gold pack → gold increases, Buy stays enabled
5. Buy **Starter Bundle** on fresh account → ads off + 1,000 gold once
6. Force-stop → reopen → Remove Ads still owned (receipt sync)
7. logcat: `[IAP]`, `[PlayIntegrity]`, Firebase DebugView for `integrity_check`

---

## If purchase fails

| Symptom | Check |
|---------|-------|
| Price `—` | Product inactive, payments profile, not Play-installed, IAP init |
| `Store not ready` | Wait 12s; logcat `[IAP] Store init failed` |
| Billing unavailable | Play Services, tester Gmail, license list |

Logcat filters: `IAP`, `PlayIntegrity`, `Firebase`

---

## Code map

- IDs: `Assets/Platform/Android/IAP/IAPProductIds.cs`
- Init: `UnityPurchasingService.cs`
- Fulfillment: `PurchaseFulfillment.cs` (starter bundle gold grants once)
- Shop: `ShopWindowBehavior.cs` (poll until store ready)
- Entitlements: `EntitlementsSave.cs`
