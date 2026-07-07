# VX Monster — UI Style Guide (Phase 0)

Use this when wiring prefabs in Unity. Agent delivers scripts; you assign references.

## Colors (match existing purple/gold survivor theme)

| Token | Hex | Usage |
|-------|-----|--------|
| PrimaryPurple | `#6B3FA0` | Main buttons, headers |
| PrimaryGreen | `#4CAF50` | Play, confirm, Daily Challenge |
| Gold | `#FFC107` | Currency, rewards |
| Danger | `#E53935` | Exit, fail accents |
| PanelDark | `#1A1228` @ 92% alpha | Modal backgrounds |
| TextPrimary | `#FFFFFF` | Body on dark panels |
| TextMuted | `#B0A8C0` | Secondary labels |

## Typography (TMP)

| Role | Size | Weight |
|------|------|--------|
| H1 | 48–56 | Bold |
| H2 | 36–40 | Bold |
| Body | 28–32 | Regular |
| Caption | 22–24 | Regular |
| Button | 30–34 | Bold |

Font: reuse **NeverMindRounded-Bold** / lobby labels from existing prefabs.

## Components

- **Primary button**: sliced `ui_button_purple` or green play sprite, min height 56dp, 8pt corner padding
- **Secondary button**: `ui_square_rounded_frame`, outline style
- **Modal**: `CanvasGroup` fade 0.3s unscaled; slide from bottom for ability panel pattern
- **Safe area**: keep `SafeAreaManager` on root canvas; 16pt horizontal inset on new screens

## Screens to build (prefab hookup)

Full inventory: [ScreenInventory.md](ScreenInventory.md)

| Screen | Script | Key `[SerializeField]` refs |
|--------|--------|------------------------------|
| Difficulty modal | `DifficultyModalWindowBehavior` | 4 tier buttons, preview, close |
| Talent Tree | `TalentTreeWindowBehavior` | `pointsLabel`, `nodesContainer`, `nodeButtonPrefab` |
| Codex | `CodexWindowBehavior` | `bodyText`, `backButton` |
| Shop | `ShopWindowBehavior` | product rows, `restoreButton`, `statusLabel` |
| Legal | `LegalTextWindowBehavior` | `bodyText`, `scrollRect`, `backButton` |
| Results | `StageCompleteScreen` / `StageFailedScreen` | `statsText`, `retryButton` (fail) |
| Combo HUD | `ComboHudBehavior` | `comboLabel` |
| Relic HUD | `RelicHudBehavior` | slot icons/labels |
| Difficulty badge | `DifficultyBadgeHudBehavior` | `badgeLabel` |
| Tutorial | `TutorialOverlayBehavior` | body, next, skip |
| Loading | `LoadingScreenBehavior` | progress bar, labels |
| Local PB | `LocalPersonalBestBehavior` | daily/endless labels |
| Daily modifiers | `DailyModifierPreviewBehavior` | `modifiersLabel` |

## Abilities popup — new optional fields

On `Abilities Popup` prefab, add two buttons and assign to `AbilitiesWindowBehavior`:

- `rerollButton` + `rerollLabel`
- `banishButton` + `banishLabel`

If left unassigned, reroll/banish still work via code paths when buttons are added later.

## Legal / privacy

- Settings: add buttons opening `LegalTextWindowBehavior` with `LegalTexts.PrivacyPolicyBody` / `TermsBody`
- Play Console still needs hosted URL at submit (copy same text)

## Motion

- Screen open: alpha 0→1 in 0.3s unscaled
- List items: stagger 0.1s (see ability cards)
- Currency change: brief scale pulse 1→1.08→1
