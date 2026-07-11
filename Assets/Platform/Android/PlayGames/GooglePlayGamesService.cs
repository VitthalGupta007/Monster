#if GOOGLE_PLAY_GAMES_AVAILABLE && UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.IO;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;
using VXMonster.Core;
using VXMonster.Core.Save;

namespace VXMonster.Platform.PlayGames
{
    public class GooglePlayGamesService : IPlayGamesService
    {
        private const string CloudSaveName = "vx_monster_save";

        private bool cloudBusy;

        public bool IsAuthenticated { get; private set; }
        public bool IsReady { get; private set; }

        public void Initialize(Action<bool> onComplete)
        {
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                IsAuthenticated = status == SignInStatus.Success;
                IsReady = true;

                if (IsAuthenticated)
                {
                    SyncCloudSave(_ => onComplete?.Invoke(true));
                }
                else
                {
                    Debug.Log("[GPGS] Automatic sign-in did not succeed; manual sign-in available from lobby.");
                    onComplete?.Invoke(false);
                }
            });
        }

        public void SignIn(Action<bool> onComplete)
        {
            PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
            {
                IsAuthenticated = status == SignInStatus.Success;
                if (IsAuthenticated)
                {
                    SyncCloudSave(_ => onComplete?.Invoke(true));
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
        }

        public void SubmitScore(string leaderboardId, long score, Action<bool> onComplete = null)
        {
            if (!IsAuthenticated)
            {
                onComplete?.Invoke(false);
                return;
            }

            PlayGamesPlatform.Instance.ReportScore(score, leaderboardId, onComplete);
        }

        public void UnlockAchievement(string achievementId, Action<bool> onComplete = null)
        {
            if (!IsAuthenticated)
            {
                onComplete?.Invoke(false);
                return;
            }

            PlayGamesPlatform.Instance.UnlockAchievement(achievementId, onComplete);
        }

        public void ShowLeaderboard(string leaderboardId = null)
        {
            if (!IsAuthenticated) return;

            if (string.IsNullOrEmpty(leaderboardId))
            {
                PlayGamesPlatform.Instance.ShowLeaderboardUI();
            }
            else
            {
                PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardId);
            }
        }

        public void ShowAchievements()
        {
            if (!IsAuthenticated) return;
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        }

        public void SyncCloudSave(Action<bool> onComplete = null)
        {
            if (!IsAuthenticated)
            {
                onComplete?.Invoke(false);
                return;
            }

            if (cloudBusy)
            {
                onComplete?.Invoke(false);
                return;
            }

            cloudBusy = true;
            var client = PlayGamesPlatform.Instance.SavedGame;
            client.OpenWithAutomaticConflictResolution(
                CloudSaveName,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                (status, metadata) =>
                {
                    if (status != SavedGameRequestStatus.Success || metadata == null)
                    {
                        cloudBusy = false;
                        Debug.LogWarning($"[GPGS] Cloud open failed ({status}); uploading local save if present.");
                        PushCloudSave(onComplete);
                        return;
                    }

                    client.ReadBinaryData(metadata, (readStatus, bytes) =>
                    {
                        try
                        {
                            var localPath = GetLocalSavePath();
                            var localExists = File.Exists(localPath);
                            var localTime = localExists ? File.GetLastWriteTimeUtc(localPath) : DateTime.MinValue;
                            var cloudTime = metadata.LastModifiedTimestamp.ToUniversalTime();
                            var cloudHasData = readStatus == SavedGameRequestStatus.Success && bytes != null && bytes.Length > 0;

                            if (cloudHasData && cloudTime > localTime)
                            {
                                WriteLocalSave(bytes);
                                GameController.SaveManager?.ReloadFromDisk();
                                VXMonster.Platform.Bootstrap.PlatformBootstrapper.RefreshSaveBindings();
                                onComplete?.Invoke(true);
                            }
                            else if (localExists)
                            {
                                PushCloudSave(onComplete);
                            }
                            else
                            {
                                onComplete?.Invoke(true);
                            }
                        }
                        finally
                        {
                            cloudBusy = false;
                        }
                    });
                });
        }

        public void PushCloudSave(Action<bool> onComplete = null)
        {
            if (!IsAuthenticated)
            {
                onComplete?.Invoke(false);
                return;
            }

            var localPath = GetLocalSavePath();
            if (!File.Exists(localPath))
            {
                onComplete?.Invoke(true);
                return;
            }

            if (cloudBusy)
            {
                onComplete?.Invoke(false);
                return;
            }

            cloudBusy = true;
            byte[] payload;
            try
            {
                payload = File.ReadAllBytes(localPath);
            }
            catch (Exception ex)
            {
                cloudBusy = false;
                Debug.LogException(ex);
                onComplete?.Invoke(false);
                return;
            }

            var client = PlayGamesPlatform.Instance.SavedGame;
            client.OpenWithAutomaticConflictResolution(
                CloudSaveName,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                (status, metadata) =>
                {
                    if (status != SavedGameRequestStatus.Success || metadata == null)
                    {
                        cloudBusy = false;
                        Debug.LogWarning($"[GPGS] Cloud open for upload failed ({status}).");
                        onComplete?.Invoke(false);
                        return;
                    }

                    var update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("VX Monster progress")
                        .WithUpdatedPlayedTime(TimeSpan.FromSeconds(Time.realtimeSinceStartup))
                        .Build();

                    client.CommitUpdate(metadata, update, payload, (commitStatus, _) =>
                    {
                        cloudBusy = false;
                        var ok = commitStatus == SavedGameRequestStatus.Success;
                        if (!ok)
                        {
                            Debug.LogWarning($"[GPGS] Cloud upload failed ({commitStatus}).");
                        }

                        onComplete?.Invoke(ok);
                    });
                });
        }

        private static string GetLocalSavePath()
        {
            return SerializationHelper.persistentDataPath + SaveManager.SAVE_FILE_NAME;
        }

        private static void WriteLocalSave(byte[] bytes)
        {
            var path = GetLocalSavePath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, bytes);
        }
    }
}
#endif
