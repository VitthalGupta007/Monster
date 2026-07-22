# Firebase — Production Setup (VX Monster)

VX Monster uses **Firebase Analytics** and **Firebase Crashlytics** only.  
It does **not** read or write **Realtime Database** or **Firestore** in code.

You still create Realtime Database once so `google-services.json` includes `firebase_url` (removes the Unity warning: *Database URL not set in the Firebase config*).

---

## 1. Firebase Console — Realtime Database (one-time)

You are on **Set up database → Security rules**. For Play Store production:

| Option | Use? | Why |
|--------|------|-----|
| **Start in locked mode** | **Yes — choose this** | Default deny all reads/writes. Safe for production because the game never connects to RTDB. |
| Start in test mode | **No** | Open to the public for 30 days — not acceptable for a live app. |

Steps:

1. [Firebase Console](https://console.firebase.google.com) → project **VX Monster**
2. **Build → Realtime Database → Create Database**
3. Pick region (e.g. `asia-southeast1` if most players are in India, or closest to you)
4. Select **Start in locked mode**
5. Click **Enable**

You do **not** need to write security rules or use the database in Unity.

---

## 2. Download production `google-services.json`

1. **Project settings** (gear next to Project Overview)
2. **Your apps** → Android app `com.vitthalxstudios.monster`
3. **Download google-services.json**
4. Copy to: `Assets/Plugins/Android/google-services.json`

Verify the file contains:

```json
"project_info": {
  "project_id": "...",
  "firebase_url": "https://YOUR-PROJECT-default-rtdb.firebaseio.com",
  ...
}
```

5. In Unity: **Assets → External Dependency Manager → Android Resolver → Force Resolve**
6. Confirm scripting define **`UNITY_FIREBASE`** is present (auto-added by `FirebaseDefineSync` when the JSON exists)

**Never commit** `google-services.json` — it is gitignored.

---

## 3. Lock down API keys (required before public release)

1. [Google Cloud Console → Credentials](https://console.cloud.google.com/apis/credentials)
2. Select the **Android API key** used by Firebase (`AIza...` from your JSON)
3. **Application restrictions** → Android apps
4. Add:
   - Package: `com.vitthalxstudios.monster`
   - **Release** SHA-1 (from your upload keystore)
   - **Debug** SHA-1 (only if you still sideload debug builds for QA)
5. **API restrictions** → Restrict key → allow only what Firebase needs (Firebase Installations, FCM if used, etc.)

If a key was ever pushed to GitHub, **rotate it** and download a fresh `google-services.json`.

---

## 4. Crashlytics — production

1. Firebase Console → **Build → Crashlytics**
2. Confirm Crashlytics is enabled for the Android app
3. After first production AAB is on a device, force a test crash in a **internal** build only, then confirm it appears in the dashboard
4. Do **not** ship a build with a test-crash button in production UI

---

## 5. Analytics — production vs debug

| Build | Analytics | How to verify |
|-------|-----------|---------------|
| **Release AAB** (Play Store) | Production data | Firebase → Analytics → Dashboard (24–48h delay) |
| **Debug / dev build** | DebugView only | Enable debug mode on device (see below) |

### Enable DebugView on a QA device (optional)

```bash
adb shell setprop debug.firebase.analytics.app com.vitthalxstudios.monster
```

Firebase Console → **Analytics → DebugView** — events appear in real time.

Remove debug mode before treating metrics as production:

```bash
adb shell setprop debug.firebase.analytics.app .none.
```

---

## 6. What the game logs

| Event | When |
|-------|------|
| `run_start` | Stage load begins |
| `run_end` | Stage complete or fail |
| `iap_purchase` | IAP fulfillment |
| `ad_impression` | Interstitial / rewarded shown |
| `boss_killed` | Boss death + talent points |
| `talent_unlock` | Talent tree node purchased |
| `integrity_check` | Play Integrity token request (Android) |

Without `google-services.json`, the project falls back to `MockAnalyticsService` (Editor / no define).

---

## 7. Pre-launch Firebase checklist

- [ ] Realtime Database created in **locked mode** (not test mode)
- [ ] Fresh `google-services.json` with `firebase_url` in `Assets/Plugins/Android/`
- [ ] EDM4U Force Resolve completed
- [ ] `UNITY_FIREBASE` define active for Android
- [ ] Android API key restricted to package + SHA-1
- [ ] Release AAB tested — no *Database URL not set* warning in logcat
- [ ] Crashlytics dashboard receives at least one test crash from internal QA build
- [ ] Analytics events visible in DebugView (QA) or dashboard (production, after delay)

See also: [ProductionGates.md](ProductionGates.md) Gate 6 · [PlayAlphaReady.md](PlayAlphaReady.md) production launch section
