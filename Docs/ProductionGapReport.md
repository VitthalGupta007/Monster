# VX Monster — Production Gap Report

**Updated:** 22 July 2026 — Play Store production launch track

---

## Ready for production

| Area | Status |
|------|--------|
| Core gameplay loop | Campaign, Daily, Endless, talents, relics, codex |
| Lobby UI (menu, shop, settings, talent info) | Wired + recent fixes |
| IAP code (purchase, restore, starter bundle → remove ads) | Implemented |
| UMP + AdMob integration | In project |
| Legal (web + in-app) | Live URL + `LegalTexts.cs` |
| Upload keystore | On disk (not in repo) |
| Store listing copy | [PlayAlphaReady.md](PlayAlphaReady.md) |

---

## Must complete before Production rollout

1. **Firebase production config** — RTDB **locked mode**, fresh `google-services.json`, API key restrictions ([FirebaseSetup.md](FirebaseSetup.md))
2. **Play Console** — all app content declarations, IAP products active, production AAB
3. **ProductionGates** — device sign-off on release-signed build
4. **Version code** — bump above any prior upload

---

## Known limits (acceptable for v1.0)

| Limit | Notes |
|-------|-------|
| In-run character bodies | Lobby portraits unique; combat sprites shared Wizard base |
| Prop variety | Biome-tinted clones; bespoke art later |
| GPGS / iOS | Post-launch |
| Style gate | Human spot-check portraits/tiles in Unity |

---

## Doc index (production)

| Doc | Use |
|-----|-----|
| [PlayAlphaReady.md](PlayAlphaReady.md) | 5-day launch checklist + store copy |
| [FirebaseSetup.md](FirebaseSetup.md) | Locked-mode RTDB + production Firebase |
| [ProductionGates.md](ProductionGates.md) | Pre-release QA sign-off |
| [IAP_OneTimePurchase_Checklist.md](IAP_OneTimePurchase_Checklist.md) | IAP + restore testing |
