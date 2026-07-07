# VX Monster — Prefab Hookup Checklist

Wire these in Unity after pulling latest scripts. Optional fields can stay empty; features degrade gracefully.

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

## Legal modal (`LegalTextWindowBehavior`)

| Field | Object |
|-------|--------|
| `bodyText` | Scroll content TMP |
| `scrollRect` | Scroll view |
| `backButton` | Close |

## Talent Tree (new modal)

1. Duplicate an existing modal (e.g. Settings).
2. Add `TalentTreeWindowBehavior`.
3. Assign `pointsLabel`, `nodesContainer` (Vertical Layout), `nodeButtonPrefab` (Button + TMP child).

Open from lobby with a button calling `TalentTreeWindowBehavior.Open()`.

## Codex (new modal)

1. Add `CodexWindowBehavior` on a panel with a TMP body field.
2. Assign `bodyText`, `backButton`.

## Shop (new modal)

1. Add `ShopWindowBehavior`.
2. For each IAP row, fill `ShopProductRow`:
   - `productId` — use constants from `IAPProductIds`
   - `buyButton`, `priceLabel`, `titleLabel`
3. Assign `restoreButton`, `backButton`, `statusLabel`.

**Dev testing:** Mock IAP grants immediately in Editor. For Remove Ads without purchase, set `EntitlementsSave.RemoveAdsPurchased = true` in save JSON.

**Live testing:** Create matching products in Play Console after $25 registration. Product IDs must match `IAPProductIds.cs`.

## Run results

| Screen | Field | Notes |
|--------|-------|-------|
| Stage Complete | `statsText` | Optional TMP; shows mode, kills, time, combos |
| Stage Failed | `statsText` | Same formatter, includes endless loop on death |

## Combo HUD (in-game)

1. On Game Screen canvas top bar, add TMP label.
2. Add `ComboHudBehavior`, assign `comboLabel`.

## Stages 3–6 (content)

Assets exist with recolored spotlight tints and Stage 2 timeline/field as placeholder. **Your task:** duplicate timelines per biome and assign unique field art when ready.

## Package install

Open Unity once so it resolves `com.unity.purchasing` from `manifest.json`. Editor script `PurchasingDefineSync` adds `UNITY_PURCHASING` when the package is present.
