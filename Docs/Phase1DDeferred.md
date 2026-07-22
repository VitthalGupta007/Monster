# Phase 1D — Android Platform (launch track)

**Status:** Active — production Play Store release in progress (July 2026).

Platform work is **no longer deferred**. Use [PlayAlphaReady.md](PlayAlphaReady.md) as the master launch checklist.

---

## Production checklist

### Keystore

- [x] Upload keystore on disk (`vx-monster-upload.keystore`, alias `vxmonster`)
- [ ] Passwords stored in team vault — never commit
- [ ] Unity **Publishing Settings** → custom keystore for **release** builds

### Play Console

- [ ] App ID: `com.vitthalxstudios.monster`
- [ ] IAP products active (see [IAP_OneTimePurchase_Checklist.md](IAP_OneTimePurchase_Checklist.md))
- [ ] Store listing + assets ([PlayAlphaReady.md](PlayAlphaReady.md))
- [ ] Privacy policy: `https://www.vitthalxstudios.com/privacy.html`
- [ ] **Production** rollout (not permanent closed testing only)

### Ads & consent

- [ ] Production AdMob units in `AdMobConfig`
- [ ] UMP messages published ([ProductionGates.md](ProductionGates.md) Gate 1)
- [ ] Rewarded revive + interstitial tested on device

### Firebase (production)

- [ ] Realtime Database — **locked mode** ([FirebaseSetup.md](FirebaseSetup.md))
- [ ] `google-services.json` with `firebase_url`
- [ ] `UNITY_FIREBASE` define
- [ ] API key restricted to release SHA-1
- [ ] Crashlytics + Analytics verified

### QA

- [ ] [ProductionGates.md](ProductionGates.md) signed off

---

## iOS

**Deferred.** No Phase 1–2 time on `Assets/Platform/iOS` stubs until Android production is stable.

---

## GPGS

After production launch stabilizes → [GPGS_v1.1.md](GPGS_v1.1.md).
