# Firebase Setup (Phase 6)

Analytics uses `MockAnalyticsService` until Firebase is installed.

## Steps

1. Create Firebase project at https://console.firebase.google.com
2. Add Android app `com.vitthalxstudios.monster`
3. Download `google-services.json` → `Assets/Plugins/Android/google-services.json`
4. Install Firebase Unity SDK (Analytics + Crashlytics) via EDM4U
5. Open Unity — Firebase generates `google-services-desktop.json` and the `FirebaseApp.androidlib` / `FirebaseCrashlytics.androidlib` folders locally
6. `FirebaseDefineSync` adds `UNITY_FIREBASE` scripting define
7. `PlatformBootstrapper` switches to `FirebaseAnalyticsService`

## Security

- **Do not commit** `google-services.json` or any Firebase-generated config files. They are gitignored.
- If a Google API key was ever pushed to GitHub, **rotate it** in [Google Cloud Console → APIs & Services → Credentials](https://console.cloud.google.com/apis/credentials), then download a fresh `google-services.json`.
- Restrict the Android API key to your app package name and release/debug SHA-1 fingerprints.

## Events logged

| Event | When |
|-------|------|
| `run_start` | Stage load begins |
| `run_end` | Stage complete or fail |
| `iap_purchase` | IAP fulfillment |
| `ad_impression` | Interstitial/rewarded shown |
| `boss_killed` | Boss death + talent points |
| `talent_unlock` | Talent tree node purchased |
| `integrity_check` | Play Integrity token request (Android) |

Verify in Firebase DebugView on device.
