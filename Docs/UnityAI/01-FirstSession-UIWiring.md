# Unity AI / MCP Prompt Pack — First Session UI Wiring

Paste into Unity Assistant **Agent** mode if Cursor MCP is disconnected. Or approve Cursor MCP `Unity_RunCommand` when bridge is live.

## Task A — Abilities Popup reroll/banish

**Asset:** `Assets/Common/Prefabs/UI/Abilities/Abilities Popup.prefab`  
**Script:** `AbilitiesWindowBehavior`

### Create under root `Abilities Popup` (panelRect)

1. Child `Action Buttons` (RectTransform + HorizontalLayoutGroup)
   - Anchors: min/max `(0.5, 0)`, pivot `(0.5, 0)`
   - AnchoredPosition: `(0, 36)`
   - SizeDelta: `(860, 96)`
   - Spacing: `40`, Child Alignment: Middle Center
   - childControlWidth/Height: false

2. Child of Action Buttons: `Reroll Button` (Image + Button)
   - SizeDelta: `(360, 88)`
   - Sprite: same sliced purple panel sprite as Abilities Popup background (`guid 4b9abba727e1c924cba2bcc0ee5bb8de`)
   - Image color: `#6B3FA0` (PrimaryPurple)
   - Child TMP `Label`: text `REROLL`, font NeverMindRounded Shadow Outline Bold, size `32`, white, center

3. Child of Action Buttons: `Banish Button` (Image + Button)
   - Same size/sprite as Reroll
   - Image color: `#E53935` (Danger)
   - Child TMP `Label`: text `BANISH`, same font/size

### Assign on AbilitiesWindowBehavior

| Field | Object |
|-------|--------|
| `rerollButton` | Reroll Button |
| `banishButton` | Banish Button |
| `rerollLabel` | Reroll Button/Label TMP |
| `banishLabel` | Banish Button/Label TMP |

### Acceptance

Play Mode → level up → REROLL and BANISH visible under ability cards; REROLL spends a reroll; BANISH enters banish mode.

---

## Task B — Loading Screen progress UI

**Scene:** `Assets/Common/Scenes/Loading Screen.unity`  
**Canvas:** `Loaind Canvas`  
**Script:** `VXMonster.UI.LoadingScreenBehavior`

### Create under `Loaind Canvas`

1. Add component `LoadingScreenBehavior` on `Loaind Canvas` (or empty child `Loading UI Root`).

2. Child `Progress Bar` (Slider)
   - Anchors: min `(0.5, 0.5)`, max `(0.5, 0.5)`
   - AnchoredPosition: `(0, -120)`
   - SizeDelta: `(720, 36)`
   - Min 0, Max 1, Whole Numbers off
   - Fill color: `#FFC107` (Gold) or PrimaryPurple `#6B3FA0`
   - Background: dark panel `#1A1228` ~92% alpha

3. Reuse existing `Loading Text` TMP as `statusLabel` (or duplicate as Status Label at `(0, 40)`).

4. Child `Percent Label` (TMP)
   - AnchoredPosition: `(0, -170)`
   - SizeDelta: `(200, 48)`
   - Font: NeverMindRounded Shadow Outline Bold, size `28`, white, center
   - Text: `0%`

### Assign on LoadingScreenBehavior

| Field | Object |
|-------|--------|
| `progressBar` | Progress Bar Slider |
| `statusLabel` | Loading Text (or Status Label) |
| `percentLabel` | Percent Label |

### Acceptance

Enter Play from Loading Screen / load Main Menu or Game — bar and percent update via `LoadingProgressReporter` (not stuck at static LOADING only).
