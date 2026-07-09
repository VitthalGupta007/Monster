# VX Monster — Production Gap Report (post HIGH checklist)

## HIGH checklist — DONE this pass

| Item | Status | Evidence |
|------|--------|----------|
| Stage tiles/props | Done (tiles + field assets) | Unique `back_stage_3..6.png`, `Stage N Field Data.asset`, BG prefabs; Stages 3–6 no longer share Stage 2 field GUID |
| Wave timing | Done | Stage 3×1.08, 4×0.92, 5×1.15, 6×0.85 on `m_Start` clips |
| Character portraits | Done (lobby icons) | Unique `ui_char_{rogue,necromancer,ranger,sage,paladin,berserker}.png` bound on Character Data |
| Relic icons | Done | Unique `ui_relic_*.png` bound on all 9 RelicData assets |
| Website mojibake | Done | `privacy.html` / `terms.html` — 0 `â€` matches |

## Honest remaining limits (not claimed done)

| Limit | Why |
|-------|-----|
| In-run character bodies | Prefabs still Wizard/Mage — only **lobby portraits** are unique |
| Prop variety | Only Bush + Stone prop prefabs exist; stages vary which/how many, not new prop art |
| Border tiles | Endless mode ignores side/corner prefabs; only background chunk + props matter |
| Style gate | Human should spot-check AI portraits/relics/tiles in Unity |

## Still CRITICAL (Play ship)

1. Firebase `google-services.json` + `UNITY_FIREBASE`
2. Signed AAB upload + Play Console payments/IAP products
3. Website APK file missing
4. Device ProductionGates

## Still MEDIUM

- GPGS / iOS stubs
- EditMode test gaps
- Relic HUD empty-slot polish
- Timeline track display names still Stage-2 labels
