# VX Monster — Prefab Hookup Checklist

Wire these in Unity after pulling latest scripts. **Fast path:** menu **VX Monster → Wire All Prefab Hookup (Phase 1)**. Backup prompt pack: [UnityAI/02-PrefabHookup-Hub.md](UnityAI/02-PrefabHookup-Hub.md).

Optional fields can stay empty; features degrade gracefully.

## Abilities Popup (`AbilitiesWindowBehavior`)

| Field | Object |
|-------|--------|
| `rerollButton` | New button, bottom row |
| `rerollLabel` | TMP on reroll button |
| `banishButton` | New button, bottom row |
| `banishLabel` | TMP on banish button |

## Settings (`SettingsWindowBehavior`)

| Field | Object |
|-------|--------|
| `privacyButton` | Opens legal modal |
| `termsButton` | Opens legal modal |
| `legalTextWindow` | Panel with `LegalTextWindowBehavior` |

## Difficulty modal (`DifficultyModalWindowBehavior`)

1. Create modal panel on lobby canvas (inactive by default).
2. Add 4 tier buttons + `previewLabel` + close.
3. `VXLobbyModePanel` opens this when present; otherwise cycles difficulty.

## Talent Tree / Codex / Shop

See [ScreenInventory.md](ScreenInventory.md) for full list.

## Run results

| Screen | Field |
|--------|-------|
| Stage Complete | `statsText` |
| Stage Failed | `statsText`, `retryButton` |

## In-run HUD (`Game Screen.prefab`)

| Script | Field |
|--------|-------|
| `ComboHudBehavior` | `comboLabel` |
| `RelicHudBehavior` | 3× `slotIcons`, optional `slotLabels` |
| `DifficultyBadgeHudBehavior` | `badgeLabel` |

## Loading screen (`Loading Screen.unity`)

Add `LoadingScreenBehavior` with `progressBar`, `statusLabel`, `percentLabel`.

## Tutorial (`TutorialOverlayBehavior`)

Full-screen overlay on Main Menu or Lobby; assign `bodyText`, `nextButton`, `skipButton`.

## Local personal bests

`LocalPersonalBestBehavior` auto-attaches to lobby at runtime. For custom layout, add manually with `dailyBestLabel` and `endlessBestLabel`.

## Daily modifiers

Add `DailyModifierPreviewBehavior` with `modifiersLabel` near Daily Challenge button.

## Stages 3–6

See [BiomeBrief.md](BiomeBrief.md). Editor menu: **VX Monster → Content → Setup Stages 3-6 Timelines**.

## Package install

Open Unity once for `com.unity.purchasing`. `PurchasingDefineSync` adds `UNITY_PURCHASING` when resolved.

## Firebase (Phase 6)

See [FirebaseSetup.md](FirebaseSetup.md).

## Production QA

See [ProductionGates.md](ProductionGates.md).
