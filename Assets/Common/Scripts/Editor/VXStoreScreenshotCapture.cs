#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VXMonster.Core;
using VXMonster.Core.Abilities;
using VXMonster.Core.Abilities.UI;
using VXMonster.Core.UI;
using VXMonster.Core.Upgrades.UI;
using VXMonster.Gameplay;
using VXMonster.UI;

namespace VXMonster.EditorTools
{
    /// <summary>
    /// Live Play Mode phone captures for Play Store (1080×1920). No composited shots.
    /// </summary>
    public static class VXStoreScreenshotCapture
    {
        const string OutputFolder = "Docs/StoreAssets/Screenshots";
        const int Width = 1080;
        const int Height = 1920;
        const string WoodenWandDataPath = "Assets/Common/Scriptables/Abilities/Active Abilities/Wooden Wand Data.asset";

        static readonly List<(Action action, float runAt)> Pending = new();
        static bool _hooked;

        [InitializeOnLoadMethod]
        static void Hook()
        {
            if (_hooked) return;
            _hooked = true;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (Pending.Count == 0) return;
            var now = (float)EditorApplication.timeSinceStartup;
            for (var i = Pending.Count - 1; i >= 0; i--)
            {
                if (now < Pending[i].runAt) continue;
                var action = Pending[i].action;
                Pending.RemoveAt(i);
                try { action?.Invoke(); }
                catch (Exception ex) { Debug.LogError($"[VX Store] Scheduled step failed: {ex}"); }
            }
        }

        static void Schedule(Action action, float delaySeconds) =>
            Pending.Add((action, (float)EditorApplication.timeSinceStartup + delaySeconds));

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

        [MenuItem("VX Monster/Store/Capture Named/03 Characters")]
        public static void Capture03Characters() => CaptureNamed("phone_03_characters");

        [MenuItem("VX Monster/Store/Capture Named/04 Stage")]
        public static void Capture04Stage() => CaptureNamed("phone_04_stage");

        [MenuItem("VX Monster/Store/Capture Named/05 Gameplay")]
        public static void Capture05Gameplay() => CaptureNamed("phone_05_gameplay");

        [MenuItem("VX Monster/Store/Capture Named/06 Abilities")]
        public static void Capture06Abilities() => CaptureNamed("phone_06_abilities");

        [MenuItem("VX Monster/Store/Capture Named/07 Upgrades")]
        public static void Capture07Upgrades() => CaptureNamed("phone_07_upgrades");

        [MenuItem("VX Monster/Store/Capture Named/08 Meta")]
        public static void Capture08Meta() => CaptureNamed("phone_08_meta");

        /// <summary>Full live store set — Main Menu Play Mode required.</summary>
        [MenuItem("VX Monster/Store/Capture All Store Set (Play Mode Now)")]
        public static void CaptureAllStoreSetNow()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Store] Enter Play Mode from Main Menu first.");
                return;
            }

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            TrySetGameViewSize(Width, Height);
            FocusGameView();
            DismissContinuePopupIfNeeded();
            UnlockAllStages();

            // 01 Lobby (Stage 1)
            SelectStage(0);
            Schedule(() =>
            {
                CaptureNamed("phone_01_lobby");
                Schedule(Capture02ThenContinue, 1.5f);
            }, 1.0f);
        }

        /// <summary>Gameplay + post-run shots only (use when 01–04 already exist).</summary>
        [MenuItem("VX Monster/Store/Capture Remaining 05-08 (Play Mode Now)")]
        public static void CaptureRemaining0508Now()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Store] Enter Play Mode from Main Menu first.");
                return;
            }

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            TrySetGameViewSize(Width, Height);
            FocusGameView();
            DismissContinuePopupIfNeeded();
            UnlockAllStages();
            Capture05ThenContinue();
        }

        /// <summary>Lobby post-run shots only (upgrades + meta menu).</summary>
        [MenuItem("VX Monster/Store/Capture Remaining 07-08 (Play Mode Now)")]
        public static void CaptureRemaining0708Now()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Store] Enter Play Mode from Main Menu first.");
                return;
            }

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            TrySetGameViewSize(Width, Height);
            FocusGameView();

            if (SceneManager.GetActiveScene().name != "Main Menu")
                SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);

            Schedule(() => WaitLobbyThenCapture07(0), 2.5f);
        }

        static void Capture02ThenContinue()
        {
            // Modes band is on lobby — Stage 2 for visual variety
            SelectStage(1);
            CaptureNamed("phone_02_modes");
            Schedule(Capture03ThenContinue, 1.5f);
        }

        static void Capture03ThenContinue()
        {
            var characters = FindUi<CharactersWindowBehavior>();
            if (characters != null)
            {
                characters.Open();
                Schedule(() =>
                {
                    CaptureNamed("phone_03_characters");
                    characters.Close();
                    Schedule(Capture04ThenContinue, 1.2f);
                }, 1.2f);
            }
            else
            {
                Debug.LogWarning("[VX Store] Characters window missing — skipping 03.");
                Schedule(Capture04ThenContinue, 0.3f);
            }
        }

        static void Capture04ThenContinue()
        {
            SelectStage(2); // Stage 3 front
            Schedule(() =>
            {
                CaptureNamed("phone_04_stage");
                Schedule(Capture05ThenContinue, 1.2f);
            }, 1.0f);
        }

        static void Capture05ThenContinue()
        {
            // Direct Endless → Game load (same pattern as GateRunner; avoids Loading Screen stall)
            SelectStage(0);
            StartEndlessGame(0);
            Schedule(() => WaitCombatThenCapture05(0), 2.5f);
        }

        static void StartEndlessGame(int stageIndex)
        {
            GameSessionManager.Instance?.ConfigureEndless(DifficultyTier.Normal);

            if (GameController.SaveManager == null) return;
            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            save.SetSelectedStageId(stageIndex);
            save.IsPlaying = true;
            save.ResetStageData = true;
            save.Time = 0f;
            save.XP = 0f;
            save.XPLEVEL = 0;
            save.EnemiesKilled = 0;

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

        static void WaitCombatThenCapture05(int attempt)
        {
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;

            if (!StageController.IsLoaded)
            {
                if (attempt < 60)
                {
                    Schedule(() => WaitCombatThenCapture05(attempt + 1), 0.5f);
                    return;
                }

                Debug.LogError("[VX Store] Combat never loaded.");
                return;
            }

            EnsureStartingWeapon();
            Schedule(() => WaitForEnemiesThenCapture05(0), 2f);
        }

        static void WaitForEnemiesThenCapture05(int attempt)
        {
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;

#if UNITY_2022_2_OR_NEWER
            var enemies = UnityEngine.Object.FindObjectsByType<EnemyBehavior>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            var enemies = UnityEngine.Object.FindObjectsOfType<EnemyBehavior>();
#endif
            var count = enemies != null ? enemies.Length : 0;
            if (count < 2 && attempt < 50)
            {
                Schedule(() => WaitForEnemiesThenCapture05(attempt + 1), 0.5f);
                return;
            }

            Debug.Log($"[VX Store] Gameplay enemies visible={count}; capturing phone_05.");
            CaptureNamed("phone_05_gameplay");
            Schedule(Capture06ThenContinue, 1.5f);
        }

        // Keep legacy body used by Recapture05Only
        static void FinishRecapture05AfterEnemies(int attempt)
        {
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
#if UNITY_2022_2_OR_NEWER
            var enemies = UnityEngine.Object.FindObjectsByType<EnemyBehavior>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            var enemies = UnityEngine.Object.FindObjectsOfType<EnemyBehavior>();
#endif
            var count = enemies != null ? enemies.Length : 0;
            if (count < 2 && attempt < 50)
            {
                Schedule(() => FinishRecapture05AfterEnemies(attempt + 1), 0.5f);
                return;
            }

            Debug.Log($"[VX Store] Recapture 05 enemies={count}");
            CaptureNamed("phone_05_gameplay");
        }

        static void Capture06ThenContinue()
        {
            var abilities = FindUi<AbilitiesWindowBehavior>();
            if (abilities != null)
            {
                abilities.Show(true);
                Schedule(() =>
                {
                    CaptureNamed("phone_06_abilities");
                    abilities.Hide();
                    Schedule(Capture07ThenContinue, 1.2f);
                }, 1.2f);
            }
            else
            {
                Debug.LogWarning("[VX Store] Abilities window missing — skipping 06.");
                Schedule(Capture07ThenContinue, 0.3f);
            }
        }

        static void Capture07ThenContinue()
        {
            // Return to Main Menu for upgrades + meta
            if (SceneManager.GetActiveScene().name != "Main Menu")
                SceneManager.LoadScene("Main Menu");

            Schedule(() => WaitLobbyThenCapture07(0), 2.0f);
        }

        static void WaitLobbyThenCapture07(int attempt)
        {
            DismissContinuePopupIfNeeded();
            var upgrades = FindUi<UpgradesWindowBehavior>();
            if (upgrades == null)
            {
                if (attempt < 20)
                {
                    Schedule(() => WaitLobbyThenCapture07(attempt + 1), 0.5f);
                    return;
                }

                Debug.LogError("[VX Store] Upgrades window missing after return.");
                Schedule(Capture08MetaSheet, 0.5f);
                return;
            }

            upgrades.Open();
            Schedule(() =>
            {
                CaptureNamed("phone_07_upgrades");
                Schedule(Capture08MetaSheet, 1.0f);
            }, 1.2f);
        }

        static void Capture08MetaSheet()
        {
            FindUi<UpgradesWindowBehavior>()?.Close();
            FindUi<CharactersWindowBehavior>()?.Close();
            SelectStage(0);

            Schedule(() =>
            {
                // Drop legacy Sign In row if present; keep LEADERBOARDS (menu is dynamic).
                HideLegacyPlayGamesSignInRow();
                ClickButtonNamed("VX Menu Button");

                Schedule(() =>
                {
                    var sheet = GameObject.Find("VX Meta Menu Sheet");
                    if (sheet != null)
                        sheet.SetActive(true);
                    else
                    {
#if UNITY_2022_2_OR_NEWER
                        var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
                        var transforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
#endif
                        foreach (var t in transforms)
                        {
                            if (t != null && t.name == "VX Meta Menu Sheet")
                            {
                                t.gameObject.SetActive(true);
                                break;
                            }
                        }
                    }

                    HideLegacyPlayGamesSignInRow();
                    CaptureNamed("phone_08_meta");
                    Debug.Log("[VX Store] Store set complete (01–08).");
                }, 1.0f);
            }, 0.6f);
        }

        static void HideLegacyPlayGamesSignInRow()
        {
#if UNITY_2022_2_OR_NEWER
            var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var transforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
#endif
            foreach (var t in transforms)
            {
                if (t == null) continue;
                if (t.name == "Play Games Sign In Row")
                    t.gameObject.SetActive(false);
            }
        }

        [MenuItem("VX Monster/Store/Recapture/03 Characters Only (Play Mode Now)")]
        public static void Recapture03Only()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Store] Play Mode required.");
                return;
            }

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            TrySetGameViewSize(Width, Height);
            DismissContinuePopupIfNeeded();
            if (SceneManager.GetActiveScene().name != "Main Menu")
                SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);

            Schedule(() =>
            {
                DismissContinuePopupIfNeeded();
                var characters = FindUi<CharactersWindowBehavior>();
                if (characters == null)
                {
                    Debug.LogError("[VX Store] Characters window missing.");
                    return;
                }

                characters.Open();
                Schedule(() => CaptureNamed("phone_03_characters"), 1.2f);
            }, 2.0f);
        }

        [MenuItem("VX Monster/Store/Recapture/05 Gameplay Only (Play Mode Now)")]
        public static void Recapture05Only()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Store] Play Mode required.");
                return;
            }

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            TrySetGameViewSize(Width, Height);
            DismissContinuePopupIfNeeded();
            UnlockAllStages();
            StartEndlessGame(0);
            Schedule(() =>
            {
                if (!StageController.IsLoaded)
                {
                    Schedule(() => Recapture05OnlyBody(0), 1f);
                    return;
                }

                Recapture05OnlyBody(0);
            }, 3f);
        }

        static void Recapture05OnlyBody(int attempt)
        {
            if (!StageController.IsLoaded)
            {
                if (attempt < 40)
                {
                    Schedule(() => Recapture05OnlyBody(attempt + 1), 0.5f);
                    return;
                }

                Debug.LogError("[VX Store] Combat never loaded for 05 recapture.");
                return;
            }

            EnsureStartingWeapon();
            Schedule(() => FinishRecapture05AfterEnemies(0), 2f);
        }

        [MenuItem("VX Monster/Store/Recapture/08 Meta Only (Play Mode Now)")]
        public static void Recapture08Only()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Store] Play Mode required.");
                return;
            }

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            TrySetGameViewSize(Width, Height);
            if (SceneManager.GetActiveScene().name != "Main Menu")
                SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
            Schedule(Capture08MetaSheet, 2.5f);
        }

        static void EnsureStartingWeapon()
        {
            if (!StageController.IsLoaded || StageController.AbilityManager == null) return;

#if UNITY_2022_2_OR_NEWER
            var cards = UnityEngine.Object.FindObjectsByType<AbilityCardBehavior>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var cards = UnityEngine.Object.FindObjectsOfType<AbilityCardBehavior>(true);
#endif
            if (cards != null && cards.Length > 0 && cards[0].Data != null)
            {
                StageController.AbilityManager.AddAbility(cards[0].Data);
                FindUi<AbilitiesWindowBehavior>()?.Hide();
                return;
            }

            var wand = AssetDatabase.LoadAssetAtPath<AbilityData>(WoodenWandDataPath);
            if (wand != null)
            {
                StageController.AbilityManager.AddAbility(wand);
                FindUi<AbilitiesWindowBehavior>()?.Hide();
            }
        }

        static void SelectStage(int stageIndex)
        {
            if (GameController.SaveManager == null) return;
            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            save.SetSelectedStageId(stageIndex);
            FindUi<LobbyWindowBehavior>()?.InitStage(stageIndex);
        }

        static void UnlockAllStages()
        {
            var db = AssetDatabase.LoadAssetAtPath<StagesDatabase>("Assets/Common/Scriptables/Stages/Stages Database.asset");
            if (db == null || GameController.SaveManager == null) return;
            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            for (var i = 0; i < db.StagesCount; i++)
                save.UnlockStage(db.GetStage(i));
        }

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

            for (var i = 0; i < 12; i++)
                EditorApplication.Step();

            var path = Path.Combine(GetAbsoluteOutputFolder(), fileNameNoExt + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            ScreenCapture.CaptureScreenshot(path);

            for (var i = 0; i < 8; i++)
                EditorApplication.Step();

            Debug.Log($"[VX Store] CAPTURE_PATH={path}");
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

                var index = -1;
                var total = (int)getTotalCount!.Invoke(currentGroup, null)!;
                for (var i = 0; i < total; i++)
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

        static void DismissContinuePopupIfNeeded()
        {
            try
            {
#if UNITY_2022_2_OR_NEWER
                var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
                var buttons = UnityEngine.Object.FindObjectsOfType<Button>(true);
#endif
                foreach (var button in buttons)
                {
                    if (button == null || !button.isActiveAndEnabled) continue;
                    var label = button.GetComponentInChildren<TMP_Text>(true);
                    if (label == null) continue;
                    if (label.text.IndexOf("Cancel", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    try { button.onClick.Invoke(); }
                    catch (Exception ex) { Debug.LogWarning($"[VX Store] Cancel click skipped: {ex.Message}"); }
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VX Store] Dismiss continue skipped: {ex.Message}");
            }
        }

        static void ClickButtonNamed(string objectName)
        {
#if UNITY_2022_2_OR_NEWER
            var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var buttons = UnityEngine.Object.FindObjectsOfType<Button>(true);
#endif
            foreach (var button in buttons)
            {
                if (button != null && button.gameObject.name == objectName)
                {
                    button.onClick.Invoke();
                    return;
                }
            }
        }

        static T FindUi<T>() where T : Component
        {
#if UNITY_2022_2_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
            return UnityEngine.Object.FindObjectOfType<T>(true);
#endif
        }

        static string GetAbsoluteOutputFolder()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, OutputFolder);
        }
    }
}
#endif
