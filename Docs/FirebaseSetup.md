# Firebase Setup (Phase 6)

Analytics uses `MockAnalyticsService` until Firebase is installed.

## Steps

1. Create Firebase project at https://console.firebase.google.com
2. Add Android app `com.vitthalxstudios.monster`
3. Download `google-services.json` → `Assets/Plugins/Android/google-services.json`
4. Install Firebase Unity SDK (Analytics + Crashlytics) via EDM4U
5. Open Unity — `FirebaseDefineSync` adds `UNITY_FIREBASE` scripting define
6. `PlatformBootstrapper` switches to `FirebaseAnalyticsService`

## Events logged

| Event | When |
|-------|------|
| `run_start` | Stage load begins |
| `run_end` | Stage complete or fail |
| `iap_purchase` | IAP fulfillment |
| `ad_impression` | Interstitial/rewarded shown |
| `boss_killed` | Boss death + talent points |
| `talent_unlock` | Talent tree node purchased |

Verify in Firebase DebugView on device.
