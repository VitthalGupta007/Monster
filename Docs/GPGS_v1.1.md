# VX Monster — Google Play Games (live)

Play Games sign-in, cloud Saved Games, leaderboards, and achievements are wired for **Android device builds**.

## IDs (from Play Console Android Setup)

Generated in [`Assets/GPGSIds.cs`](../Assets/GPGSIds.cs). Aliases in [`Assets/Platform/Android/PlayGames/GPGSIds.cs`](../Assets/Platform/Android/PlayGames/GPGSIds.cs).

| Feature | Constant | Play Console ID |
|---------|----------|-----------------|
| Daily Challenge leaderboard | `LeaderboardDailyChallenge` | `CgkI07ucnrYEEAIQAQ` |
| Endless Waves leaderboard | `LeaderboardEndlessWaves` | `CgkI07ucnrYEEAIQAg` |
| Lifetime Kills leaderboard | `LeaderboardLifetimeKills` | `CgkI07ucnrYEEAIQAw` |
| First Combo achievement | `AchievementFirstCombo` | `CgkI07ucnrYEEAIQBA` |
| Daily Complete achievement | `AchievementDailyComplete` | `CgkI07ucnrYEEAIQBQ` |

## Architecture

- **Device Android:** `GooglePlayGamesService` (`GOOGLE_PLAY_GAMES_AVAILABLE`)
- **Editor / other:** `MockPlayGamesService`
- Cloud save slot: `vx_monster_save` ↔ local `game_save` binary file
- GPGS initializes **before** ads (consent no longer blocks sign-in)

## Code hook points

| Event | Caller |
|-------|--------|
| Daily score submit + Daily Complete achievement | `StageController` on daily victory |
| Endless score submit | `StageController` on endless fail |
| Lifetime kills submit | `StageController` on stage complete |
| First Combo achievement | `ComboResolver` on first combo |
| Cloud push after local save | `SaveManager.OnSaveCompleted` → `PlatformServices.TryPushCloudSave` |
| Lobby sign-in / leaderboards | `PlayGamesLobbyBehavior` in meta menu |

## Device test checklist

1. Install internal-testing build on a device with a Play Games profile.
2. Launch → automatic sign-in prompt (or use **SIGN IN TO PLAY** in lobby MENU).
3. Play a run → local save writes → cloud upload on save.
4. Clear app data → reinstall → sign in → cloud restore.
5. Open **LEADERBOARDS** from lobby menu after a scored run.
6. Trigger first combo → achievement unlock in Play Games.

## Data Safety

Declare Play Games account / game progress collected and shared with Google. See [`Website/delete-data.html`](../Website/delete-data.html).
