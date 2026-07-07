# VX Monster

Android-first survivor roguelite built in Unity. Fight through timed stage waves, level up abilities, collect relics, and push Daily Challenge and Endless modes.

**Package:** `com.vitthalxstudios.monster`  
**Unity:** 6000.5.2f1  
**Target:** Android SDK 35 (min SDK 26)

## Requirements

- [Unity 6000.5.2f1](https://unity.com/releases/editor/whats-new/6000.5.2f1)
- Android Build Support (SDK / NDK / OpenJDK)
- Git LFS not required for this repo

Open the project folder in Unity Hub and load `Assets/Common/Scenes/Loading Screen.unity` as the entry scene (see **Build Settings**).

## Scenes

| Scene | Path |
|-------|------|
| Loading Screen | `Assets/Common/Scenes/Loading Screen.unity` |
| Main Menu | `Assets/Common/Scenes/Main Menu.unity` |
| Game | `Assets/Common/Scenes/Game.unity` |

## Project layout

```
Assets/
  Common/          Gameplay, UI, stages, scriptables
  Platform/        Android ads, IAP, bootstrap, Play Games stubs
Docs/              UI style guide and prefab hookup checklist
Packages/          Unity package manifest
ProjectSettings/   Player and build settings
```

## Game features (v1.0)

- **Campaign** — 6 biomes (stages 3–6 use placeholder timelines; swap in unique Timelines when ready)
- **Daily Challenge** — UTC-seeded modifiers, local best score
- **Endless** — looping stages with soft-scaling enemy stats
- **Difficulty** — Easy / Normal / Hard / Nightmare (lobby selector)
- **Talent tree** — points from boss kills; reroll, passive slot, and gold bonuses
- **Relics, banish, rerolls** — mid-run build variety
- **Codex** — discovered relics, evolutions, combos
- **Meta progression** — gold currency, upgrades, stage unlocks

## Monetization

| System | Location |
|--------|----------|
| AdMob (banner, interstitial, rewarded, app open) | `Assets/Platform/Android/Ads/` |
| UMP consent | `GoogleMobileAdsConsentController.cs` |
| Remove Ads + gold IAP | `Assets/Platform/Android/IAP/` |
| Entitlements (Remove Ads flag) | `EntitlementsSave.cs` |

**Remove Ads** disables all ad formats. Players still receive **one free revive per run** (same slot as the rewarded ad revive).

IAP product IDs are defined in `IAPProductIds.cs`. In the Editor, `MockIapService` completes purchases immediately. Live Google Play billing requires Play Console setup and matching in-app products.

Ad unit IDs live in `AdMobConfig` (ScriptableObject). Do not commit production keystores or Play service account keys.

## Documentation

| Doc | Purpose |
|-----|---------|
| [Docs/PrefabHookup.md](Docs/PrefabHookup.md) | Wire UI prefabs to new scripts (shop, talent tree, codex, legal, results, combo HUD) |
| [Docs/UIStyleGuide.md](Docs/UIStyleGuide.md) | Colors, typography, and screen conventions |

## Development notes

### Prefab wiring

Several features ship as C# scripts first; Unity prefab references must be assigned manually. Follow **PrefabHookup.md** after pulling script changes.

### Testing Remove Ads without IAP

Set `RemoveAdsPurchased = true` on the `VX Entitlements` save, or buy Remove Ads through the Mock Shop in the Editor.

### Packages

Key dependencies (see `Packages/manifest.json`):

- `com.google.ads.mobile` — AdMob
- `com.unity.purchasing` — Google Play billing (optional define `UNITY_PURCHASING` when resolved)
- URP, Input System, Timeline

### Android release build

1. Create or assign a release keystore in **Player Settings → Publishing Settings**
2. Build **App Bundle** (AAB) for Play Console
3. Configure AdMob UMP in the AdMob dashboard
4. Create IAP products matching `IAPProductIds.cs`
5. Host a privacy policy URL for Play Console (in-app legal text is in `LegalTexts.cs`)

## Store checklist (not in repo)

- Google Play Developer account ($25)
- Release keystore backup
- Firebase / Crashlytics (planned post-v1.0)
- Google Play Games leaderboards (deferred to v1.1)

## License

Proprietary — VitthalX Studios. All rights reserved unless otherwise noted in asset attributions.
