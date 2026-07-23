#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VXMonster.Core;

namespace VXMonster.EditorTools
{
    /// <summary>
    /// Records a ~30s 1920×1080 promo via Unity Recorder (when installed), then runs strict video QA.
    /// </summary>
    public static class VXStorePromoVideoCapture
    {
        const string OutputRel = "Docs/StoreAssets/promo/vx-monster-promo-30s";
        const float TargetSeconds = 30f;
        const int FrameRate = 30;

        static object _controller;
        static bool _recording;

        [MenuItem("VX Monster/Store/Promo/Record 30s Promo (Play Mode)")]
        public static void RecordPromoMenu()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode required", "Enter Play Mode first.", "OK");
                return;
            }

            BeginPromoRecording();
        }

        [MenuItem("VX Monster/Store/Promo/Record Promo From Lobby (Play Mode Now)")]
        public static void RecordPromoFromLobbyNow()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
                VXStoreScreenshotCapture.Schedule(RecordPromoFromLobbyBody, 4f);
                return;
            }

            RecordPromoFromLobbyBody();
        }

        static void RecordPromoFromLobbyBody()
        {
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            VXStoreScreenshotCapture.TrySetGameViewSize(1920, 1080, "VX Promo 1920x1080");
            VXStoreScreenshotCapture.UnlockAllStages();
            VXStoreScreenshotCapture.SelectStage(0);
            VXStoreScreenshotCapture.StartEndlessGame(0);
            VXStoreScreenshotCapture.Schedule(BeginPromoRecording, 3f);
        }

        static void BeginPromoRecording()
        {
            if (!IsRecorderAvailable())
            {
                Debug.LogError("[VX Store Promo] Unity Recorder package not installed. Add com.unity.recorder via Package Manager.");
                return;
            }

            if (_recording)
            {
                Debug.LogWarning("[VX Store Promo] Already recording.");
                return;
            }

            var outDir = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "Docs/StoreAssets/promo");
            Directory.CreateDirectory(outDir);
            var outBase = Path.Combine(outDir, "vx-monster-promo-30s");

            try
            {
                var recorderAsm = AppDomain.CurrentDomain.GetAssemblies();
                Type controllerSettingsType = null;
                Type movieSettingsType = null;
                Type gameViewInputType = null;
                Type controllerType = null;

                foreach (var asm in recorderAsm)
                {
                    if (!asm.GetName().Name.Contains("Recorder")) continue;
                    controllerSettingsType ??= asm.GetType("UnityEditor.Recorder.RecorderControllerSettings");
                    movieSettingsType ??= asm.GetType("UnityEditor.Recorder.MovieRecorderSettings");
                    gameViewInputType ??= asm.GetType("UnityEditor.Recorder.Input.GameViewInputSettings");
                    controllerType ??= asm.GetType("UnityEditor.Recorder.RecorderController");
                }

                if (controllerSettingsType == null || movieSettingsType == null || controllerType == null)
                {
                    Debug.LogError("[VX Store Promo] Recorder types not found.");
                    return;
                }

                var controllerSettings = ScriptableObject.CreateInstance(controllerSettingsType);
                var movie = ScriptableObject.CreateInstance(movieSettingsType);
                movie.GetType().GetProperty("Enabled")?.SetValue(movie, true);
                movie.GetType().GetProperty("OutputFile")?.SetValue(movie, outBase);

                if (gameViewInputType != null)
                {
                    var input = Activator.CreateInstance(gameViewInputType);
                    gameViewInputType.GetProperty("OutputWidth")?.SetValue(input, 1920);
                    gameViewInputType.GetProperty("OutputHeight")?.SetValue(input, 1080);
                    movie.GetType().GetProperty("ImageInputSettings")?.SetValue(movie, input);
                }

                controllerSettingsType.GetMethod("AddRecorderSettings")?.Invoke(controllerSettings, new[] { movie });
                controllerSettingsType.GetMethod("SetRecordModeToManual")?.Invoke(controllerSettings, null);
                controllerSettingsType.GetProperty("FrameRate")?.SetValue(controllerSettings, (float)FrameRate);

                _controller = Activator.CreateInstance(controllerType, controllerSettings);
                controllerType.GetMethod("PrepareRecording")?.Invoke(_controller, null);
                controllerType.GetMethod("StartRecording")?.Invoke(_controller, null);
                _recording = true;
                EditorApplication.isPaused = false;
                Time.timeScale = 1f;
                Debug.Log("[VX Store Promo] Recording started — 30s gameplay promo.");
                VXStoreScreenshotCapture.Schedule(StopPromoRecording, TargetSeconds + 1.5f);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VX Store Promo] Failed to start recording: {ex.Message}");
            }
        }

        static void StopPromoRecording()
        {
            if (!_recording || _controller == null) return;
            try
            {
                _controller.GetType().GetMethod("StopRecording")?.Invoke(_controller, null);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VX Store Promo] StopRecording: {ex.Message}");
            }

            _recording = false;
            Debug.Log("[VX Store Promo] Recording stopped — waiting for MP4 write.");
            VXStoreScreenshotCapture.Schedule(VerifyPromoOutput, 2f);
        }

        static void VerifyPromoOutput()
        {
            var mp4 = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, OutputRel + ".mp4");
            if (!File.Exists(mp4))
            {
                Debug.LogError("[VX Store Promo] MP4 not found after record.");
                StoreAssetStrictQA.RecordPromoAttempt(false, "MP4 missing after record");
                return;
            }

            var info = new FileInfo(mp4);
            if (info.Length < 500_000)
            {
                StoreAssetStrictQA.RecordPromoAttempt(false, $"MP4 too small ({info.Length} bytes)");
                return;
            }

            StoreAssetStrictQA.RecordPromoAttempt(true, $"Recorded {info.Length / 1024} KB — run ffprobe for duration/resolution");
            Debug.Log($"[VX Store Promo] READY_FOR_VISUAL_QA={mp4}");
        }

        static bool IsRecorderAvailable()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                if (asm.GetName().Name == "Unity.Recorder.Editor") return true;
            return false;
        }
    }
}
#endif
