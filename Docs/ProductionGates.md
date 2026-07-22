# VX Monster — Production Gates (pre–Play Store release)

Run these on a **release-signed** build (upload keystore AAB via closed track, or release APK sideload).  
Complete **all gates** before **Production rollout**.

---

## Gate 1 — UMP consent (EEA / ads)

- [ ] Fresh install on device with **EEA test geography** (AdMob test device or VPN QA)
- [ ] Consent form appears before first ad request
- [ ] `GoogleMobileAdsConsentController.CanRequestAds()` is false until consent granted
- [ ] Settings → Privacy opens in-app policy or UMP form
- [ ] AdMob dashboard → **Privacy & messaging** has Europe (+ US states if needed) messages **published**

---

## Gate 2 — IAP (production products)

- [ ] All 5 product IDs in Play Console **Active** (match `IAPProductIds.cs`)
- [ ] License tester account completes purchase on **closed testing** build
- [ ] **Remove Ads** → all ad formats stop; shop shows **Owned**
- [ ] **Starter Bundle** → Remove Ads owned + 1,000 gold once
- [ ] Gold pack → gold increases; consumable can be bought again
- [ ] **Restore** → status message; Remove Ads / Starter Bundle sync after reinstall
- [ ] Death screen **Free Revive** when Remove Ads owned

Details: [IAP_OneTimePurchase_Checklist.md](IAP_OneTimePurchase_Checklist.md)

---

## Gate 3 — Core gameplay (2+ physical devices)

- [ ] Campaign Stage 1 complete without crash
- [ ] Boss kill → talent points in talent tree
- [ ] Reroll / banish in ability picker
- [ ] Lobby → MENU → Talent / Codex / Shop → Back to lobby
- [ ] Settings → Exit (Android) / Back; Privacy / Terms legal text
- [ ] Talent info popup → OK dismisses
- [ ] Shop shows product names, descriptions, prices (not blank / `—` after store ready)
- [ ] Fail / complete screens show stats
- [ ] Daily + Endless modes start and record bests
- [ ] Endless 10+ loops — acceptable FPS on 4GB RAM device

---

## Gate 4 — Performance

- [ ] High enemy count (Endless) — no silent spawn failure; FPS acceptable
- [ ] No ANR on cold start or returning from background

---

## Gate 5 — Mid-run persistence

- [ ] Play 2+ minutes → force-kill app → Continue prompt on lobby
- [ ] Abilities and stage time restored

---

## Gate 6 — Firebase (production)

- [ ] Realtime Database created in **locked mode** (not test mode) — [FirebaseSetup.md](FirebaseSetup.md)
- [ ] `google-services.json` with `firebase_url` in `Assets/Plugins/Android/`
- [ ] `UNITY_FIREBASE` define active
- [ ] Logcat: **no** recurring *Database URL not set* warning
- [ ] DebugView (QA device): `run_start`, `run_end`, `iap_purchase`, `ad_impression`
- [ ] Crashlytics receives test crash from **internal QA build only** (not in production UI)
- [ ] Android API key restricted to package + release SHA-1

---

## Gate 7 — Play Console readiness

- [ ] Store listing complete (text, screenshots, feature graphic)
- [ ] Content rating questionnaire submitted
- [ ] Data safety form submitted
- [ ] Privacy policy URL live
- [ ] Production release uses **upload keystore** AAB (not debug)
- [ ] Version code incremented vs any prior upload

Store copy: [PlayAlphaReady.md](PlayAlphaReady.md)

---

## Automated tests (Editor)

**Window → General → Test Runner → EditMode** (`Assets/Tests/Editor`):

- `TalentTreeSaveTests`
- `EntitlementsSaveTests`
- `DifficultyTierTests`
- `DailyStreakUtilityTests`

---

## Sign-off (fill before Production rollout)

| Gate | Tester | Date | Pass |
|------|--------|------|------|
| UMP | | | |
| IAP | | | |
| Gameplay | | | |
| Perf | | | |
| Save | | | |
| Firebase | | | |
| Play Console | | | |
