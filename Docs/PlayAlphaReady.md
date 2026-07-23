# VX Monster — Play Store Production Launch

**Target:** Public production release on Google Play (not closed alpha / test mode).  
**Last updated:** 22 July 2026

Use this as the single launch checklist. Complete every **Required** item before **Production → Create release**.

---

## Already in the project

| Item | Status |
|------|--------|
| Package ID | `com.vitthalxstudios.monster` |
| Min / target SDK | 26 / 36 |
| Upload keystore | `vx-monster-upload.keystore` (alias `vxmonster`) — passwords **not** in repo |
| AdMob + UMP | `AdMobConfig` |
| IAP product IDs | Match [IAPProductIds.cs](../Assets/Platform/Android/IAP/IAPProductIds.cs) |
| Privacy policy (web) | https://www.vitthalxstudios.com/privacy.html |
| In-app legal text | `LegalTexts.cs` |
| Store assets | `Docs/StoreAssets/` |
| Firebase SDK | Analytics + Crashlytics (config: [FirebaseSetup.md](FirebaseSetup.md)) |

---

## 5-day launch order

### Day 1 — Console & Firebase (production, not test)

1. **Firebase Realtime Database** → **Start in locked mode** → Enable → re-download `google-services.json`  
   See [FirebaseSetup.md](FirebaseSetup.md)
2. **Play Console → Monetize → Products → One-time** — all 5 products **Active** (INR prices below)
3. **Play Console → App content** — finish every red item (privacy, ads, content rating, data safety, target audience)
4. Restrict Firebase **Android API key** to release SHA-1 + package name

### Day 2 — Production gates on device

Run [ProductionGates.md](ProductionGates.md) on **release-signed** build (or closed track AAB):

- UMP consent (EEA geography test)
- IAP purchase + Restore (license tester account until live)
- Core gameplay smoke (lobby, shop, settings, talent, codex)
- Firebase DebugView: `run_start`, `iap_purchase`

### Day 3 — Store listing & assets

Paste store copy below. Upload feature graphic + screenshots from `Docs/StoreAssets/`.

#### Refresh screenshots (current UI)

Existing phone/tablet shots are from early July — **re-capture before Production** so Play Console shows the latest lobby, shop, settings, and meta menu.

**Strict QA rule:** Nothing goes into `Docs/StoreAssets/UPLOAD/` until every file passes automated + visual QA individually (up to 20 recapture attempts per file).

1. Open Unity → **Main Menu** scene → **Play**
2. **VX Monster → Store → Capture All Store Set + Tablets (Play Mode Now)**
3. Wait ~10–12 minutes (automated lobby → combat → upgrades → meta, staggered tablet resizes, caption overlay, per-shot QA retries)
4. **VX Monster → Store → Strict QA → Verify All Store Assets (Full Gate)**
5. Agent/MCP: open each PNG under `Docs/StoreAssets/Screenshots/` — confirm per-shot content (see criteria in `Docs/StoreAssets/QA/qa-report.md`)
6. Mark visual PASS via QA report or recapture failing shots (**Recapture/** menus)
7. **VX Monster → Store → Promo → Record Promo From Lobby (Play Mode Now)** → upload MP4 to YouTube
8. When `qa-report.md` shows **23/23 PASS** (20 screenshots + icon + feature + promo): **Strict QA → Build UPLOAD Bundle**

Output folders:

| Asset | Path | Dimensions |
|-------|------|------------|
| Phone | `Docs/StoreAssets/Screenshots/phone_01_lobby.png` … `phone_08_meta.png` | 1080×1920 |
| 7" tablet landscape | `Docs/StoreAssets/Screenshots/Tablet7/tablet7_01`–`04.png` | 1920×1080 |
| 7" tablet portrait | `Tablet7/tablet7_portrait_01`–`02.png` | 1080×1920 |
| 10" tablet landscape | `Tablet10/tablet10_01`–`04.png` | 2560×1440 |
| 10" tablet portrait | `Tablet10/tablet10_portrait_01`–`02.png` | 1080×1920 |
| App icon | `Docs/StoreAssets/play_store_icon_512.png` | 512×512 |
| Feature graphic | `Docs/StoreAssets/play_feature_graphic_1024x500.png` | 1024×500 |
| Promo (YouTube) | `Docs/StoreAssets/promo/vx-monster-promo-30s.mp4` | 1920×1080, 25–35s |

**ASO phone upload order** (from `UPLOAD/` after bundle build):

1. `phone_01_survive_endless_waves.png` (lobby)
2. `phone_02_fight_massive_hordes.png` (gameplay)
3. `phone_03_unlock_8_heroes.png` (characters)
4. `phone_04_build_deadly_combos.png` (abilities)
5. `phone_05_talents_shop_more.png` (meta)
6. `phone_06_daily_endless_runs.png` (modes)
7. `phone_07_conquer_6_worlds.png` (stage)
8. `phone_08_power_up_talents.png` (upgrades)

Single-shot recapture: **VX Monster → Store → Capture Named/** or **Recapture/** menus.

**Promo video:** Unity Recorder 5.1.x does not compile on Unity 6000.5 yet. Use **VX Monster → Store → Promo → Record Promo From Lobby** after installing a Unity-6-compatible Recorder version, or record manually and place at `Docs/StoreAssets/promo/vx-monster-promo-30s.mp4`.

**Known gameplay capture note:** If `phone_05_gameplay.png` shows the weapon-select panel instead of combat, run **Recapture → 05 Gameplay Only** again (the pipeline dismisses the panel and waits for ≥2 enemies before capture).

QA report: `Docs/StoreAssets/QA/qa-report.md` + `qa-report.json`

### Day 4 — Release AAB

1. Bump **Version Code** (must be higher than any uploaded build)
2. Unity → **Build App Bundle** with **upload keystore** (not debug)
3. Play Console → **Testing → Closed testing** — upload AAB, final QA with license testers
4. When QA passes → promote same release to **Production**

### Day 5 — Production rollout

1. **Production → Create new release** → add tested AAB
2. **Countries/regions** → your launch countries
3. **Start rollout to Production** (staged rollout 10% → 100% recommended)
4. Monitor Crashlytics + Play Console vitals for 48 hours

---

## Store listing (paste into Play Console)

### App name
```
VX Monster
```

### Short description (≤80 chars)
```
Survive endless monster waves. Unlock talents, relics, and daily challenges.
```

### Full description
```
VX Monster is a fast vampire-survivors style action game from VitthalX Studio.

Fight through campaign stages packed with bats, plants, bosses, and chaotic swarms. Build a loadout of abilities, grab powerful relics, and push your personal best in Daily and Endless modes.

FEATURES
• Campaign stages with bosses and talent rewards
• Ability upgrades, rerolls, and banishes mid-run
• Relics that change how each run feels
• Daily challenge + streak tracking
• Endless mode for high-score loops
• Talent tree and character roster progression
• Optional Remove Ads and gold packs

CONTROLS
Move with the virtual stick. Abilities fire automatically — focus on positioning, pickups, and surviving the next wave.

PRIVACY
Ads use Google AdMob with consent where required. Purchases go through Google Play. Privacy policy: https://www.vitthalxstudios.com/privacy.html

Contact: vitthalgupta007@gmail.com
```

### Privacy policy URL
```
https://www.vitthalxstudios.com/privacy.html
```

### Category
Game → Action (or Arcade)

---

## Play Console — required declarations

| Section | Answer |
|---------|--------|
| Financial features | **No** |
| Health app | **No** |
| Ads | **Yes** |
| Target audience | **13+** or **16+** (not Designed for Families) |
| News app | **No** |
| App access | All functionality available without login |

### Data safety (summary)

| Question | Answer |
|----------|--------|
| Collects / shares data? | **Yes** (Advertising ID; analytics/diagnostics via Firebase) |
| Encrypted in transit? | **Yes** |
| Deletion request? | **Yes** (contact email; uninstall clears local data) |
| Shared with third parties? | **Yes** — Google (AdMob, Play, Firebase) |
| Sold? | **No** |

---

## IAP products (must match code exactly)

Play Console: **Monetize with Play → Products → One-time products** · Purchase option: `default`

| Product ID | Type | INR | In-game |
|------------|------|-----|---------|
| `com.vitthalxstudios.monster.remove_ads` | Non-consumable | **₹250** | Remove ads + free revive |
| `com.vitthalxstudios.monster.starter_bundle` | Non-consumable | **₹300** | Remove ads + 1,000 gold (once) |
| `com.vitthalxstudios.monster.gold_small` | Consumable | **₹100** | +500 gold |
| `com.vitthalxstudios.monster.gold_medium` | Consumable | **₹199** | +1,500 gold |
| `com.vitthalxstudios.monster.gold_large` | Consumable | **₹300** | +5,000 gold |

**Starter Bundle** automatically grants **Remove Ads** in code (`PurchaseFulfillment.FulfillStarterBundle`).

Pre-launch IAP QA: [IAP_OneTimePurchase_Checklist.md](IAP_OneTimePurchase_Checklist.md)  
After launch: remove reliance on license testers; real users purchase through Production track.

---

## Build & upload (production AAB)

1. Unity → **File → Build Settings → Android**
2. **Build App Bundle (Google Play)**
3. **Publishing Settings** → custom keystore `vx-monster-upload.keystore` / alias `vxmonster`
4. Increment **Version Code** every upload
5. Upload to **Closed testing** first → QA → **Promote to Production**

**Do not** upload debug-signed APK/AAB to Production.

---

## Testing vs production (important)

| | Pre-launch QA | Live on Play Store |
|--|---------------|-------------------|
| **Track** | Closed / internal testing | **Production** |
| **Firebase RTDB** | Locked mode | Locked mode (never test mode) |
| **IAP** | License testers in Play Console | Real purchases |
| **Analytics** | DebugView on QA devices | Production dashboard |
| **Ads** | Test ad units / test devices OK for QA | Production AdMob units only |

---

## What you must do manually

- Play Console declarations and rollout clicks
- Firebase locked-mode database + API key restrictions
- Keystore passwords (team vault)
- Physical device QA ([ProductionGates.md](ProductionGates.md))
- Production rollout monitoring
