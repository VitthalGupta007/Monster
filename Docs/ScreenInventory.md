# VX Monster — Screen Inventory (Phase 0)

Complete UI audit for prefab wiring. Scripts live under `Assets/Common/Scripts/` unless noted.

## Existing prefabs (23 under `Assets/Common/Prefabs/UI/`)

| # | Screen | Prefab | Script | Status |
|---|--------|--------|--------|--------|
| 1 | Main menu shell | `Screens/Main Menu Screen.prefab` | `MainMenuScreenBehavior.cs` | Good — add hub buttons for Talent/Codex/Shop |
| 2 | Lobby / stage select | `Upgrades/Lobby Window.prefab` | `LobbyWindowBehavior.cs` | Good — VX modes via `VXLobbyModePanel` |
| 3 | Upgrades | `Upgrades/Upgrades Window.prefab` | `UpgradesWindowBehavior.cs` | Good — 8 tiers per stat (expanded) |
| 4 | Characters | `Upgrades/Characters Window.prefab` | `CharactersWindowBehavior.cs` | Good |
| 5 | Settings | `Upgrades/Settings Window.prefab` | `SettingsWindowBehavior.cs` | Privacy + Terms wired (assign buttons) |
| 6 | Game HUD | `Screens/Game Screen.prefab` | `GameScreenBehavior.cs` | Add relic strip, combo HUD, difficulty badge |
| 7 | Abilities level-up | `Abilities/Abilities Popup.prefab` | `AbilitiesWindowBehavior.cs` | Wire reroll/banish buttons |
| 8 | Chest | `Chest/Chest Window.prefab` | `ChestWindowBehavior.cs` | Good |
| 9 | Pause | `Pause Window.prefab` | `PauseWindowBehavior.cs` | Good |
| 10 | Stage complete | `Screens/Stage Complete Screen.prefab` | `StageCompleteScreen.cs` | Add `statsText` |
| 11 | Stage failed | `Screens/Stage Failed Screen.prefab` | `StageFailedScreen.cs` | Add `statsText`, `retryButton` |
| 12 | Loading | `Scenes/Loading Screen.unity` | `LoadingScreenBehavior.cs` | Progress bar hookup |

## New screens (agent scripts — you wire prefabs)

| # | Screen | Script | Key refs |
|---|--------|--------|----------|
| 13 | Difficulty modal | `DifficultyModalWindowBehavior.cs` | 4 tier buttons, preview labels, close |
| 14 | Talent tree | `TalentTreeWindowBehavior.cs` | `pointsLabel`, `nodesContainer`, `nodeButtonPrefab` |
| 15 | Codex | `CodexWindowBehavior.cs` | `bodyText`, `backButton` |
| 16 | Shop / IAP | `ShopWindowBehavior.cs` | product rows, restore, status |
| 17 | Legal | `LegalTextWindowBehavior.cs` | scroll body, back |
| 18 | Tutorial overlays | `TutorialOverlayBehavior.cs` | step panels, highlight rects |
| 19 | Relic HUD (in-run) | `RelicHudBehavior.cs` | 3 slot icons + labels |
| 20 | Combo HUD | `ComboHudBehavior.cs` | `comboLabel` |
| 21 | Daily modifier chips | `DailyModifierPreviewBehavior.cs` | modifier label list |
| 22 | Local PB strip | `LocalPersonalBestBehavior.cs` | daily/endless best labels |

## Runtime-only (no prefab yet)

| Item | Script | Notes |
|------|--------|-------|
| VX mode buttons | `VXLobbyModePanel.cs` | Daily / Practice / Endless + opens difficulty modal |
| Run results text | `RunResultsFormatter.cs` | Used by complete/fail screens |

## Navigation map

```
Main Menu
  └─ Lobby Window
       ├─ Play (Campaign) ──► Difficulty modal ──► Game
       ├─ Daily Challenge ──► Game (modifiers shown)
       ├─ Practice ──► Game
       ├─ Endless ──► Difficulty modal ──► Game
       ├─ Upgrades / Characters / Settings
       ├─ Talent Tree (new button → modal)
       ├─ Codex (new button → modal)
       └─ Shop (new button → modal)
Game
  ├─ HUD: relics, combos, difficulty badge
  ├─ Abilities popup: reroll, banish
  └─ Pause
End
  ├─ Stage Complete → stats + Continue
  └─ Stage Failed → stats + Retry + Revive + Exit
```

See also: [UIStyleGuide.md](UIStyleGuide.md), [PrefabHookup.md](PrefabHookup.md).
