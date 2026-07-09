# VX Monster — public legal site

Static pages for Play Console privacy URL and BillDesk website field.

## Host (fastest): Netlify Drop

1. Open https://app.netlify.com/drop  
2. Drag this entire `Website` folder onto the page  
3. Copy the HTTPS URL (example: `https://something.netlify.app`)  
4. Use:
   - **BillDesk website:** `https://something.netlify.app`
   - **Play Console privacy policy:** `https://something.netlify.app/privacy.html`

## Privacy policy sources used

Content was written to cover requirements described in:

- [Google Play User Data policy — Privacy Policy](https://support.google.com/googleplay/android-developer/answer/10144311)  
  (developer contact; data types; sharing parties; security; retention/deletion; clear “Privacy Policy” label; app/developer name; public non-PDF URL)
- [Google Play Data safety section](https://support.google.com/googleplay/android-developer/answer/10787469)  
  (privacy policy required; disclose third-party SDK practices)
- AdMob / Google ads disclosure expectations (name AdMob; advertising identifiers; purposes; third-party tech; opt-out links; link to Google partner data use)

This is **not legal advice**. Have a lawyer review before relying on it for regulated markets.

## Files

| File | URL path |
|------|----------|
| `index.html` | `/` |
| `download.html` | `/download.html` |
| `privacy.html` | `/privacy.html` |
| `terms.html` | `/terms.html` |
| `downloads/vx-monster.apk` | `/downloads/vx-monster.apk` |
| `styles.css` | `/styles.css` |

## Host APK for BillDesk

1. In Unity: **File → Build Settings → Android → Build** (APK).
2. Copy the APK to `Website/downloads/vx-monster.apk` (exact name).
3. Redeploy: drag the whole `Website` folder onto Netlify Drop / Deploys.
4. BillDesk fields:
   - Website: `vitthalxstudios.com`
   - APP Name: `VX Monster`
   - Mobile App APK URL: `https://www.vitthalxstudios.com/downloads/vx-monster.apk`

A debug/development APK is usually enough for BillDesk review. Use a release-signed APK if you already have a keystore.
