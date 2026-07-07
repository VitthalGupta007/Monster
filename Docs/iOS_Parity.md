# VX Monster — iOS Parity (Phase 8)

Android-first launch. iOS stubs live under `Assets/Platform/iOS/` for future parity.

## Bundle ID

`com.vitthalxstudios.monster` (already set in Project Settings)

## Platform services mapping

| Android | iOS stub |
|---------|----------|
| `AdMobService` | `IOSAdServiceStub` |
| `UnityPurchasingService` | Same IAP service (StoreKit via Unity Purchasing) |
| `GoogleMobileAdsConsentController` | `IOSAttConsentStub` (ATT prompt placeholder) |
| `MockPlayGamesService` | `IOSGameCenterStub` (Game Center v1.1) |

## Pre-launch checklist (when targeting iOS)

- [ ] Apple Developer account ($99/year)
- [ ] AdMob iOS app + ad units in `AdMobConfig`
- [ ] App Tracking Transparency string in `Info.plist`
- [ ] StoreKit products mirror `IAPProductIds.cs`
- [ ] Privacy nutrition labels + hosted policy URL
- [ ] TestFlight internal build

## Build

Use Unity iOS Build Support module. Platform folder uses `#if UNITY_IOS` guards; Android code unchanged.
