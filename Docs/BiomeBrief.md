# VX Monster — Biome Brief (Phase 4)

v1.0 ships **6 biomes**. Stages 1–2 are fully authored. Stages 3–6 now have **unique lobby previews** and **distinct enemy/boss mixes** remapped from the existing enemy roster. They still share Stage 2 **field/prop layout** (tiles) — unique StageFieldData remains outstanding.

## Biome roster

| Stage | Name | Theme | Spotlight tint | Unlock gold | Preview | Enemy mix | Field data |
|-------|------|-------|----------------|-------------|---------|-----------|------------|
| 1 | VX WILDS | Grass / forest | Green | Free | Unique | Unique | Unique |
| 2 | VX SHADOW REALM | Stone / cave | Cool blue | 500 | Unique | Unique | Unique |
| 3 | VX FROST WASTES | Ice / tundra | Ice blue | 750 | Unique (AI) | Remapped | Shares Stage 2 |
| 4 | VX EMBER DEPTHS | Lava / volcanic | Orange-red | 1000 | Unique (AI) | Remapped | Shares Stage 2 |
| 5 | VX TOXIC SWAMP | Poison / bog | Toxic green | 1250 | Unique (AI) | Remapped | Shares Stage 2 |
| 6 | VX VOID NEXUS | Cosmic / void | Purple | 1500 | Unique (AI) | Remapped | Shares Stage 2 |

## Still outstanding for true unique biomes

1. Per-stage `StageFieldData` (tiles/props) — currently Stages 3–6 reuse Stage 2 stone field
2. Hand-authored Timeline wave timing (not only enemy-type remap)
3. New enemy art families (current remap uses existing EnemyType pool only)
4. Approve/regenerate AI lobby previews if style gate fails vs Stage 1–2

## Editor menus

- `VX Monster/Content/Bind Stage Preview Icons`
- `VX Monster/Content/Differentiate Stages 3-6 Enemy Mix`
- `VX Monster/Content/Setup Stages 3-6 Timelines` (no longer overwrites existing unique previews)

## Post-launch (biomes 7–12)

Planned as content updates — same pipeline with new art families when budget allows.
