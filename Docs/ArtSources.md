# VX Monster — Art Sources

| Pack | License | URL/Path | Date | Used for |
|------|---------|----------|------|----------|
| Slurpcanon 2D Platformer Cartoon Pack | Free commercial, no credit required | https://slurpcanon.itch.io/2d-platformer-cartoon-pack | 2026-07-12 | Reference / incoming (`Assets/_IncomingArt/2DPlatformerCartoonPack.zip`) |
| Engvee Free Sideview Sprites Collection | Free commercial, credit appreciated | https://engvee.itch.io/free-sideview-sprites-collection | 2026-07-12 | Class combat bodies (Phase 6 target) |

## Unity AI generation (editor pipeline)

Used during Milestone A/B when pack crops were insufficient. Provenance JSON per asset under `GeneratedAssets/<guid>/`.

| Surface | Assets | Gate |
|---------|--------|------|
| Lobby logo, backdrop, panels, buttons | `ui_logo.png`, `ui_menu_background_color_3.png`, `ui_window_background.png`, buttons | Phase 2 gates PASS |
| Shop portraits | `ui_char_{class}.png` (8) | Phase 3 — distinct GUIDs bound |
| Stage chunk tiles | `back_stage_3..6.png` | Phase 7.2 gates PASS |
| Stage lobby previews | `ui_stage_{3..6}_preview.png` | Phase 7.4 gates PASS |
| Relic icons | `ui_relic_*.png` (9) | Bound; Phase 9 HUD gate pending |

**Style gate:** Pack-native look where possible; minimal recolor. AI fills gaps — not a forced purple/gold global palette (see locked plan decisions).
