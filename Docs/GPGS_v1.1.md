# VX Monster — Google Play Games v1.1 (Phase 9)

v1.0 uses **local personal bests only**. GPGS leaderboard submit calls route to `MockPlayGamesService`.

## v1.1 scope

| Feature | ID constant | Source |
|---------|-------------|--------|
| Daily Challenge | `GPGSIds.LeaderboardDailyChallenge` | `DailyChallengeSave.BestDailyScore` |
| Endless Waves | `GPGSIds.LeaderboardEndlessWaves` | `LifetimeStatsSave.EndlessLoopsBest` |
| Lifetime Kills | `GPGSIds.LeaderboardLifetimeKills` | `LifetimeStatsSave.TotalEnemiesKilled` |
| First Combo achievement | `GPGSIds.AchievementFirstCombo` | `CodexSave.HasDiscoveredFirstCombo` |

## Integration steps (when ready)

1. Install Google Play Games plugin for Unity
2. Configure Play Console → Play Games Services → link app
3. Create leaderboards/achievements matching `GPGSIds.cs`
4. Replace `MockPlayGamesService` with `GooglePlayGamesService` in `PlatformBootstrapper`
5. Add `GOOGLE_PLAY_GAMES_AVAILABLE` scripting define
6. Rename lobby "Daily Challenge" local best label to show cloud rank when signed in

## Code hook points

- `PlatformServices.SubmitDailyScore` — called from `StageController` on daily complete/fail
- `PlatformServices.SubmitEndlessScore` — called on endless death
- `PlatformServices.SubmitLifetimeKills` — called on stage complete

`RealPlayGamesService.cs` stub exists for v1.1 wiring.
