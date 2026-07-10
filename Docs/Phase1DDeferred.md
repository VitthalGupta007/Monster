# Phase 1D — Android Platform (DEFERRED)

**Status:** Blocked until Play Console $25 account verification completes.

Do **not** block Phase 1A–1C on this section. Ship retention content first; platform hardening after console access is confirmed.

## When unblocked — checklist

### Keystore

- [ ] Create upload keystore (NOT debug)
- [ ] Store passwords in team vault — never commit keystore or passwords
- [ ] `Project Settings → Player → Android → Publishing Settings` → custom keystore

### Play Console

- [ ] App ID: `com.vitthalxstudios.monster` (draft exists)
- [ ] Create IAP products matching [IAPProductIds.cs](../Assets/Platform/Android/IAP/IAPProductIds.cs) (full IDs):
  - `com.vitthalxstudios.monster.remove_ads`
  - `com.vitthalxstudios.monster.gold_small`
  - `com.vitthalxstudios.monster.gold_medium`
  - `com.vitthalxstudios.monster.gold_large`
  - `com.vitthalxstudios.monster.starter_bundle`
- [ ] Closed testing track + tester emails
- [ ] Store listing: short/long description, screenshots, feature graphic — copy in [PlayAlphaReady.md](PlayAlphaReady.md)
- [ ] Privacy policy URL: `https://www.vitthalxstudios.com/privacy.html`

### Ads & consent

- [ ] AdMob app + ad unit IDs in `AdMobConfig`
- [ ] UMP consent flow on physical device ([ProductionGates.md](ProductionGates.md))
- [ ] Test rewarded (revive) + interstitial (exit run)

### Firebase

- [ ] Download `google-services.json` → `Assets/Plugins/Android/`
- [ ] Enable `UNITY_FIREBASE` via [FirebaseSetup.md](FirebaseSetup.md)
- [ ] Verify `AnalyticsEvents` in logcat

### BillDesk / payments (if applicable)

- Play Store URL for APK field: `https://play.google.com/store/apps/details?id=com.vitthalxstudios.monster`
- Business site: `vitthalxstudios.com`

## iOS

**Deferred indefinitely.** Do not spend Phase 1–2 time on `Assets/Platform/iOS` stubs.

## GPGS

After local daily/endless personal bests feel good → [GPGS_v1.1.md](GPGS_v1.1.md).
