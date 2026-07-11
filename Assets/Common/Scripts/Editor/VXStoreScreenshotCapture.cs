#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VXMonster.EditorTools
{
    /// <summary>
    /// Captures real Game View frames for Play Store phone screenshots (1080x1920).
    /// Uses EditorApplication.Step + ScreenCapture.CaptureScreenshot (reliable for Overlay UI).
    /// </summary>
    public static class VXStoreScreenshotCapture
    {
        const string OutputFolder = "Docs/StoreAssets/Screenshots";
        const int Width = 1080;
        const int Height = 1920;

        [MenuItem("VX Monster/Store/Set Game View 1080x1920")]
        public static void SetPhoneGameView()
        {
            TrySetGameViewSize(Width, Height);
            Debug.Log("[VX Store] Game View set to 1080x1920");
        }

        [MenuItem("VX Monster/Store/Capture Phone Screenshot (Play Mode)")]
        public static void CapturePhoneScreenshot()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Play Mode required",
                    "Enter Play Mode, show the screen you want, then run this again.",
                    "OK");
                return;
            }

            CaptureNamed($"phone_live_{DateTime.Now:HHmmss}");
        }

        [MenuItem("VX Monster/Store/Capture Named/01 Lobby")]
        public static void Capture01Lobby() => CaptureNamed("phone_01_lobby");

        [MenuItem("VX Monster/Store/Capture Named/02 Modes")]
        public static void Capture02Modes() => CaptureNamed("phone_02_modes");

        [MenuItem("VX Monster/Store/Capture Named/03 Gameplay")]
        public static void Capture03Gameplay() => CaptureNamed("phone_03_gameplay");

        [MenuItem("VX Monster/Store/Capture Named/04 Abilities")]
        public static void Capture04Abilities() => CaptureNamed("phone_04_abilities");

        static void CaptureNamed(string fileNameNoExt)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Store] Play Mode required for capture.");
                return;
            }

            EditorApplication.isPaused = false;
            TrySetGameViewSize(Width, Height);
            FocusGameView();

            for (int i = 0; i < 12; i++)
                EditorApplication.Step();

            var path = Path.Combine(GetAbsoluteOutputFolder(), fileNameNoExt + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            ScreenCapture.CaptureScreenshot(path);

            for (int i = 0; i < 8; i++)
                EditorApplication.Step();

            Debug.Log($"[VX Store] Queued capture {path}");
        }

        static void FocusGameView()
        {
            try
            {
                var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                var window = EditorWindow.GetWindow(gameViewType);
                window.Focus();
                window.Repaint();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VX Store] Focus Game View failed: {e.Message}");
            }
        }

        static bool TrySetGameViewSize(int width, int height)
        {
            try
            {
                var asm = typeof(EditorWindow).Assembly;
                var gameViewType = asm.GetType("UnityEditor.GameView");
                var window = EditorWindow.GetWindow(gameViewType);

                var sizesType = asm.GetType("UnityEditor.GameViewSizes");
                var singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var gameViewSizes = singletonType.GetProperty("instance")!.GetValue(null, null);
                var currentGroup = sizesType.GetProperty("currentGroup")!.GetValue(gameViewSizes, null);
                var groupType = currentGroup.GetType();

                var getTotalCount = groupType.GetMethod("GetTotalCount");
                var getGameViewSize = groupType.GetMethod("GetGameViewSize");
                var sizeType = asm.GetType("UnityEditor.GameViewSize");
                var sizeTypeEnum = asm.GetType("UnityEditor.GameViewSizeType");

                int index = -1;
                int total = (int)getTotalCount!.Invoke(currentGroup, null)!;
                for (int i = 0; i < total; i++)
                {
                    var gvs = getGameViewSize!.Invoke(currentGroup, new object[] { i });
                    var w = (int)sizeType!.GetProperty("width")!.GetValue(gvs, null)!;
                    var h = (int)sizeType.GetProperty("height")!.GetValue(gvs, null)!;
                    if (w == width && h == height)
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                {
                    var fixedRes = Enum.Parse(sizeTypeEnum!, "FixedResolution");
                    var ctor = sizeType!.GetConstructor(new[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) });
                    var newSize = ctor!.Invoke(new object[] { fixedRes, width, height, "VX Phone 1080x1920" });
                    groupType.GetMethod("AddCustomSize")!.Invoke(currentGroup, new[] { newSize });
                    index = (int)getTotalCount.Invoke(currentGroup, null)! - 1;
                }

                var selectedSizeProp = gameViewType!.GetProperty(
                    "selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                selectedSizeProp!.SetValue(window, index, null);
                window.Repaint();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VX Store] Could not set Game View size: {e.Message}");
                return false;
            }
        }

        static string GetAbsoluteOutputFolder()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, OutputFolder);
        }
    }
}
#endif
