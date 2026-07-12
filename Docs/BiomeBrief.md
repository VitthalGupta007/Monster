# VX Monster — Biome Brief (Phase 7 complete)

v1.0 ships **6 biomes**. Stages 1–2 are fully authored. **Stages 3–6** now have unique lobby previews, **distinct chunk tiles**, **per-stage field data**, **biome-tinted props**, and remapped enemy/boss mixes from the existing roster.

## Biome roster

| Stage | Name | Theme | Spotlight (run) | Unlock gold | Preview | Enemy mix | Field data | Props |
|-------|------|-------|-----------------|-------------|---------|-----------|------------|-------|
| 1 | VX WILDS | Grass / forest | Center tint + vignette | Free | Unique | Unique | `Stage 1 Field Data` | Bush |
| 2 | VX SHADOW REALM | Stone / cave | Center tint + vignette | 500 | Unique | Unique | `Stage 2 Field Data 1` | Stone |
| 3 | VX FROST WASTES | Ice / tundra | Center tint (`a≈0.28`) + vignette | 750 | Unique | Remapped | `Stage 3 Field Data` | Frost Crystal |
| 4 | VX EMBER DEPTHS | Lava / volcanic | Vignette only (`spotlightColor.a=0`) | 1000 | Unique | Remapped | `Stage 4 Field Data` | Ember Rock |
| 5 | VX TOXIC SWAMP | Poison / bog | Vignette only (`spotlightColor.a=0`) | 1250 | Unique | Remapped | `Stage 5 Field Data` | Marsh Growth |
| 6 | VX VOID NEXUS | Cosmic / void | Vignette only (`spotlightColor.a=0`) | 1500 | Unique | Remapped | `Stage 6 Field Data` | Void Crystal |

**Spotlight note:** Stages 4–6 disable the center spotlight sprite (`CameraManager` when `SpotlightColor.a ≤ 0.001`) to avoid orange/blue center wash on AI tiles. Biome mood comes from tile art + softer `spotlightShadowColor` vignette.

**Tile note:** `back_stage_3..6.png` are distinct 1024×1024 seamless chunks (Unity AI generation; provenance in `GeneratedAssets/`). Border prefabs on field data still reference Stage 2 assets — **ignored in endless mode**.

## Phase 7 gate status

| Step | Status | Evidence |
|------|--------|----------|
| 7.1 Field bind 3–6 | PASS | `VXRebrandGate` field audit |
| 7.2 Distinct tiles | PASS | `gate_7_2_stage_{3..6}_*.png` |
| 7.3 Props in-run | PASS | `EndlessFieldBehavior` initial-chunk spawn + biome props |
| 7.4 Lobby previews | PASS | `gate_7_4_lobby_stage_{3..6}.png` |
| 7.5 Balance | PASS | Editor audit + `gate_7_5_stage_{1,3,6}_balance.png` |

## Still outstanding (Phase 8+)

1. **Biome enemy art** — `Enemies Database.asset` still stock sprites; Phase 8
2. **Hand-authored timeline wave timing** — density multipliers applied; not fully re-authored per biome
3. **Unique prop sprites** — props are tinted Bush/Stone clones, not bespoke biome art
4. **In-run class bodies** — Phase 6; combat still Wizard/Mage prefabs for 6 classes
5. **Border tile art** for stages 3–6 (low priority — endless ignores borders)

## Editor menus

- `VX Monster/Content/Bind Stage Field Data 3-6 Only`
- `VX Monster/Content/Bind Stage Preview Icons`
- `VX Monster/Content/Differentiate Stages 3-6 Enemy Mix`
- `VX Monster/Content/Setup Stages 3-6 Timelines`
- `VX Monster/Rebrand/Setup Biome Props 3-6`
- `VX Monster/Rebrand/Run All Audits`

**Do not run** `VX Monster/Content/Setup All` during visual rebrand (disabled in editor).

## Post-launch (biomes 7–12)

Planned as content updates — same pipeline with new art families when budget allows.
