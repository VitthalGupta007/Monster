# VX Monster — Production Gates (Phase 0b)

Run these checks before Play Console submit. Agent cannot run Unity/device tests — **you** verify each gate.

## Gate 1 — UMP consent

- [ ] Fresh install on device with **EEA debug geography** (AdMob test device)
- [ ] Consent form appears before first ad request
- [ ] `GoogleMobileAdsConsentController.CanRequestAds()` is false until consent granted
- [ ] Settings → Privacy opens in-app policy text
- [ ] AdMob dashboard → Privacy & messaging has Europe (+ US state if needed) messages

## Gate 2 — IAP internal track (requires Play Console $25)

- [ ] `com.unity.purchasing` resolves in Unity
- [ ] Product IDs in Play Console match `IAPProductIds.cs`
- [ ] Internal testing track: purchase **Remove Ads**
- [ ] All ad formats stop (banner, interstitial, rewarded, app open)
- [ ] Death screen shows **Free Revive** once per run
- [ ] Gold pack purchase credits lobby gold
- [ ] Restore purchases works on second device (iOS) or reinstall test

## Gate 3 — Core gameplay (3+ devices)

- [ ] Campaign Stage 1 complete without crash
- [ ] Boss kill awards talent points (check talent tree)
- [ ] Reroll and banish work in ability picker
- [ ] Lobby → Talent / Codex / Shop open and return to lobby
- [ ] Settings → Privacy / Terms show in-app legal text
- [ ] Fail screen shows stats + Retry; complete screen shows stats
- [ ] In-run HUD: combo count, difficulty badge, relic slots
- [ ] Daily modifiers + personal best / streak visible on lobby
- [ ] Phoenix relic revives once per run
- [ ] Daily Challenge scored run records local best
- [ ] Endless loop 10+ maintains playable FPS

## Gate 4 — Performance (spawn caps)

- [ ] Endless run with 500+ enemies on screen — FPS acceptable on 4GB RAM phone
- [ ] No silent spawn stop (cap 3000) without despawn working

## Gate 5 — Mid-run persistence

- [ ] Start run, play 2+ minutes, force-kill app
- [ ] Continue prompt appears on lobby
- [ ] Abilities and stage time restored

## Gate 6 — Firebase (Phase 6)

- [ ] `google-services.json` in `Assets/Plugins/Android/`
- [ ] `run_start`, `run_end`, `iap_purchase`, `ad_impression` events in Firebase DebugView

## Automated tests (Editor)

Run **Window → General → Test Runner → EditMode** (tests live in `Assets/Tests/Editor`):

- `TalentTreeSaveTests` — unlock spend logic
- `EntitlementsSaveTests` — Remove Ads flag
- `DifficultyTierTests` — talent points per boss
- `DailyStreakUtilityTests` — meta talent IDs + streak save defaults

## Sign-off

| Gate | Tester | Date | Pass |
|------|--------|------|------|
| UMP | | | |
| IAP | | | |
| Gameplay | | | |
| Perf | | | |
| Save | | | |
| Firebase | | | |
