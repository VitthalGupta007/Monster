# VX Monster — Biome Brief (Phase 4)

v1.0 ships **6 unique biomes**. Stages 1–2 are complete. Stages 3–6 are scaffolded with recolored spotlight tints and **placeholder Stage 2 timeline** until you tune in Unity.

## Biome roster

| Stage | Name | Theme | Spotlight tint | Unlock gold |
|-------|------|-------|----------------|-------------|
| 1 | VX WILDS | Grass / forest | Green | Free |
| 2 | VX SHADOW REALM | Stone / cave | Cool blue | 500 |
| 3 | VX FROST WASTES | Ice / tundra | Ice blue | 750 |
| 4 | VX EMBER DEPTHS | Lava / volcanic | Orange-red | 1000 |
| 5 | VX TOXIC SWAMP | Poison / bog | Toxic green | 1250 |
| 6 | VX VOID NEXUS | Cosmic / void | Purple | 1500 |

## Art approach (fast path — approved)

1. **Reuse** Stage 1 grass tiles + Stage 2 stone tiles
2. **Recolor** via spotlight + material tint per stage (`spotlightColor` on `StageData`)
3. **Recombine** existing props from both stages
4. **Boss**: reuse one of 6 existing boss prefabs per stage (assign in Timeline)
5. **Preview sprite**: duplicate `ui_stage_1_preview` / `ui_stage_2_preview` with tint overlay (stages 3–6 currently share stage 2 preview)

## Your Unity tasks per new biome

1. Duplicate `Stage 2.playable` → `Stage N.playable`
2. Adjust wave timing and enemy mix in Timeline window
3. Assign unique `stageFieldData` if prop layout should differ
4. Export `ui_stage_N_preview.png` for lobby diamond
5. Playtest HP/damage feel at Normal difficulty

## Post-launch (biomes 7–12)

Planned as content updates — same pipeline as above with new art families when budget allows.
