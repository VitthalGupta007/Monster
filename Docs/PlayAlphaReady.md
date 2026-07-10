# VX Monster — Alpha / Closed Testing Ready Pack

Use this for Google Play **closed testing (alpha)**. Last prepared: 10 July 2026.

## Already verified in project (agent)

| Item | Status |
|------|--------|
| Package ID | `com.vitthalxstudios.monster` |
| Version name / code | `1.0.0` / `3` |
| Min / target SDK | 26 / 35 |
| Upload keystore on disk | `vx-monster-upload.keystore` (alias `vxmonster`) — passwords not in repo |
| AdMob app + unit IDs | Set in `AdMobConfig.cs` |
| IAP product IDs in code | See table below |
| Privacy policy live | https://www.vitthalxstudios.com/privacy.html |
| In-app legal text | `LegalTexts.cs` |
| Lobby HUD label overlap (small phones) | Top-anchored under logo |
| EditMode tests excluded from player | Moved to `Assets/Tests/Editor` (fixes IL2CPP / nunit build fail) |
| Firebase `google-services.json` | **Missing** (gitignored) — analytics optional for first alpha |
| Play Console forms | **You** must complete (agent cannot access your account) |

---

## Paste into Play Console — Store listing

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
Move with the virtual stick (or keyboard in editor). Abilities fire automatically — focus on positioning, pickups, and surviving the next wave.

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

## Play Console answers (your 5 errors + common dashboard)

### 1) Store listing
Paste short + full description above → **Save**.

### 2) Countries / regions
Release → **Countries/regions** → select countries (or all available) → Save.

### 3) Financial features
**Go to Financial features** → for this game answer **No** (no banking, wallets, crypto trading, lending, etc.). IAP gold/Remove Ads alone is not a “financial feature” in Play’s sense.

### 4) Health declaration
**Go to declaration** → this is a normal entertainment game → **not** a health/fitness/medical app. Answer accordingly and submit.

### 5) Dashboard leftovers (finish every red item)
Typical remaining items:
- **App content → Privacy policy** → paste URL above
- **App content → Ads** → Yes, app contains ads
- **App content → Content rating** → start questionnaire (Violence: fantasy mild; no real gambling)
- **App content → Target audience** → 13+ or 16+ (not Designed for Families / not primarily children)
- **App content → News app** → No
- **App content → Data safety** → see section below
- **App access** → All functionality available without special login (or “no restrictions”)

---

## Data safety (suggested answers)

Be honest; adjust if your Firebase/AdMob setup differs.

| Question | Suggested |
|----------|-----------|
| Collects / shares user data? | **Yes** (ads / advertising ID; analytics if Firebase enabled) |
| Encrypted in transit? | **Yes** |
| Users can request deletion? | **Yes** (contact email; local data cleared by uninstall) |
| Data collected | Device or other IDs (Advertising ID); App activity / diagnostics if analytics on |
| Collected for | Advertising / analytics / app functionality |
| Shared with third parties? | **Yes** — Google (AdMob / Play services) |
| Sold? | **No** |
| Required vs optional | Ads-related data optional where consent applies; local save required for gameplay |

---

## IAP products to create (Monetize → Products → In-app products)

Product IDs **must match code exactly**:

| Product ID | Type | Name (suggestion) | Notes |
|------------|------|-------------------|--------|
| `com.vitthalxstudios.monster.remove_ads` | Non-consumable | Remove Ads | Disables ads + free revive/run |
| `com.vitthalxstudios.monster.starter_bundle` | Non-consumable | Starter Bundle | Remove Ads + 1000 gold |
| `com.vitthalxstudios.monster.gold_small` | Consumable | Gold Small | 500 gold |
| `com.vitthalxstudios.monster.gold_medium` | Consumable | Gold Medium | 1500 gold |
| `com.vitthalxstudios.monster.gold_large` | Consumable | Gold Large | 5000 gold |

Activate products. They only work after the app is on a testing track with license testers.

---

## Build & upload (you)

1. Unity → **File → Build Settings → Android → App Bundle (AAB)**
2. Confirm Publishing Settings use `vx-monster-upload.keystore` / alias `vxmonster` (enter passwords)
3. Build AAB
4. Play Console → **Testing → Closed testing** (or Internal) → Create release → upload AAB
5. Add **tester emails** (or Google Group)
6. Add **countries**
7. Review and **Start rollout**
8. Testers open the opt-in link → install from Play Store

Suggested release name: `1.0.0 (3) — closed alpha`

---

## Optional before / during alpha

| Item | Priority |
|------|----------|
| Add license testers under Setup → License testing | High if testing IAP |
| Device smoke test (`Docs/ProductionGates.md` Gate 3) | High |
| UMP consent on EEA test device (Gate 1) | High if you show ads in EEA |
| Firebase `google-services.json` + `UNITY_FIREBASE` | Medium (analytics) |
| Feature graphic 1024×500 + phone screenshots | Required for listing polish / often for closed track |

---

## What the agent cannot do

- Log into your Play Console / click declarations
- Enter keystore passwords or upload the AAB for you
- Create AdMob/Firebase console resources you don’t already have
- Run physical-device QA
