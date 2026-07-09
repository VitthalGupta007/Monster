# Unity AI Prompt Pack 02 — Prefab Hookup (Hub + HUD)

**Primary path:** Run **VX Monster → Wire All Prefab Hookup (Phase 1)** in Unity Editor (Cursor editor menu). Use this pack only if MCP/Editor menu is unavailable.

## Goal

Wire remaining Phase 1 UI: legal buttons, run-result stats/retry, lobby hub (Talent/Codex/Shop), modals, in-run HUD, tutorial, daily modifiers, personal bests.

## Preconditions

- Scripts pulled from repo (`MainMenuScreenBehavior`, `VXPrefabHookupWiring`, etc.)
- Font: `Assets/Common/Fonts/TMP Font Assets/NeverMindRounded-Bold/Shadow Outline NeverMindRounded-Bold.asset`
- Button sprite GUID: `4b9abba727e1c924cba2bcc0ee5bb8de`

## Task A — Editor menu (preferred)

1. Open Unity **6000.5.2f1**, project `Monster`
2. Menu: **VX Monster → Wire All Prefab Hookup (Phase 1)**
3. Save project

## Task B — Manual fallback (atomic)

### Settings legal (`Settings Window.prefab`)

Parent: root `Settings Window`

| Create | RectTransform | Field |
|--------|---------------|-------|
| Privacy Button | anchor center, y=-80, 320×72 | `SettingsWindowBehavior.privacyButton` |
| Terms Button | anchor center, y=-220 | `SettingsWindowBehavior.termsButton` |
| Legal Text Window (full-screen child) | stretch fill | `SettingsWindowBehavior.legalTextWindow` |

Legal window needs `LegalTextWindowBehavior`: `bodyText`, `scrollRect`, `backButton`.

### Main Menu modals (`Main Menu Screen.prefab`)

Add inactive children under `VX Modals`:

| Object | Script | Key fields |
|--------|--------|------------|
| VX Talent Tree | `TalentTreeWindowBehavior` | `pointsLabel`, `nodesContainer`, `nodeButtonPrefab`, `backButton` |
| VX Codex | `CodexWindowBehavior` | `bodyText`, `backButton` |
| VX Shop | `ShopWindowBehavior` | product rows, `restoreButton`, `backButton`, `statusLabel` |
| VX Difficulty Modal | `DifficultyModalWindowBehavior` | 4 tier buttons, `previewLabel`, `closeButton` |
| VX Tutorial Overlay | `TutorialOverlayBehavior` | `bodyText`, `nextButton`, `skipButton` |

Assign on `MainMenuScreenBehavior`: `talentTreeWindow`, `codexWindow`, `shopWindow`, `difficultyModal`.

### Lobby (`Lobby Window.prefab`)

| Create | Position | Field |
|--------|----------|-------|
| Talent / Codex / Shop buttons | y≈720, x=-220/0/220 | `LobbyWindowBehavior.talentButton` etc. |
| Daily/Endless/Streak labels | y≈640/600/560 | `LocalPersonalBestBehavior` |
| Daily modifiers label | y≈480 | `DailyModifierPreviewBehavior.modifiersLabel` |

### Run results

| Prefab | Fields |
|--------|--------|
| `Stage Failed Screen.prefab` | `statsText`, `retryButton` |
| `Stage Complete Screen.prefab` | `statsText` |

### Game HUD (`Game Screen.prefab` → `Safe Area` → `VX HUD`)

| Component | Fields |
|-----------|--------|
| `ComboHudBehavior` | `comboLabel` (top-left) |
| `DifficultyBadgeHudBehavior` | `badgeLabel` (top-right) |
| `RelicHudBehavior` | 3× `slotIcons` (bottom center) |

## Style tokens

- Panel: `#1A1228` @ 92% alpha
- Purple buttons: `#6B3FA0`
- Green confirm: `#4CAF50`
- Body TMP: 28–32, white

## Acceptance checks (Play Mode)

1. Main Menu → Talent / Codex / Shop open and **Back** returns to lobby
2. Settings → Privacy / Terms open scrollable legal text
3. Fail a run → stats visible, **Retry** restarts same mode
4. Complete stage → stats on victory screen
5. In run → combo label, difficulty badge, relic slots update
6. First launch → tutorial overlay; skip completes save
7. Lobby shows daily modifiers + personal best / streak labels

## Forbidden

- Do not create new gameplay systems
- Do not change C# unless a serialized field name mismatches this doc
