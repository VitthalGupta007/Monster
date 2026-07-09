# Generator Art Brief — VX Monster

Use Unity **Generators** for icons and preview art. Every asset must pass the **style gate** against existing Mage/Wizard sprites and [UIStyleGuide.md](UIStyleGuide.md).

## Style gate (reject if failed)

- Purple/gold survivor palette — **not** flat neon, not photoreal
- Thick readable silhouettes at 64×64 and 128×128
- Match line weight and saturation of `Assets/Common/Sprites/UI/Abilities/ui_ab_*.png`
- Character icons must look like siblings of Wizard/Mage lobby portraits
- Stage previews: top-down biome mood, same framing as `ui_stage_1_preview.png` / `ui_stage_2_preview.png`

## Phase 1 assets

| Asset | Path | Prompt seed |
|-------|------|-------------|
| Relic icons (9) | Map in `VX Monster/Content/Setup Relics Database` | "purple gold game icon, {name}, flat survivor style, transparent PNG" |
| Stage 3 preview | `Assets/Common/Sprites/UI/Stage/ui_stage_3_preview.png` | "frost wastes, cool blue tint, top-down arena preview" |
| Stage 4 preview | `ui_stage_4_preview.png` | "ember caverns, orange-red glow" |
| Stage 5 preview | `ui_stage_5_preview.png` | "void rift, violet-black swirl" |
| Stage 6 preview | `ui_stage_6_preview.png` | "royal hive, gold-purple crown motif" |

After generation, run **VX Monster → Content → Setup All** to bind icons to `Relics Database` and stage assets.

## Phase 2 assets

| Asset | Count | Notes |
|-------|-------|-------|
| Character portraits | 6 new (ROGUE…SAGE) | Same bust framing as Wizard; distinct class read |
| Relic expansion | +6 optional | Synergy with combo/boss themes |
| Juice SFX | hit, level-up, relic pickup | Short, punchy, mobile-friendly |

## Review checklist

1. Place new sprite beside `ui_ab_star.png` in Scene view — same "family"?
2. Icon readable at 48dp on `#1A1228` panel?
3. No unintended text baked into image
4. Import: Sprite (2D), filter Point/Bilinear per existing icons, PPU match neighbors

## Editor automation

- Relic DB + placeholder icons: `VX Monster/Content/Setup Relics Database`
- Character roster (8): `VX Monster/Content/Setup Character Roster (8)`
- Stages 3–6 timelines: `VX Monster/Content/Setup Stages 3-6 Timelines`

Human still approves Generator output before replacing placeholders.
