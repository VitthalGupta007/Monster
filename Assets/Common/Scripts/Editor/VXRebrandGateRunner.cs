#if UNITY_EDITOR
using System;
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
using VXMonster.Core.Audio;
using VXMonster.Core.UI;
using VXMonster.Gameplay;

namespace VXMonster.EditorTools
{
    public static class VXRebrandGateRunner
    {
        const string GateFolder = "Docs/StoreAssets/Screenshots/gates";
        const int Width = 1080;
        const int Height = 1920;

        static string _pendingGate;
        static readonly System.Collections.Generic.List<ScheduledGateAction> _scheduledActions = new();

        struct ScheduledGateAction
        {
            public Action Action;
            public float RunAt;
        }

        static bool HasScheduledActions => _scheduledActions.Count > 0;
        static bool _initialized;

        [InitializeOnLoadMethod]
        static void Init()
        {
            if (_initialized) return;
            _initialized = true;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        [MenuItem("VX Monster/Rebrand/Gate 0/Lobby")]
        public static void Gate0Lobby() => RunGateSequence("gate_0_lobby", CaptureLobby);

        [MenuItem("VX Monster/Rebrand/Gate 0/Lobby (Play Mode Now)")]
        public static void Gate0LobbyNow()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Enter Play Mode first.");
                return;
            }

            CaptureLobby();
        }

        [MenuItem("VX Monster/Rebrand/Gate 0/Shop (Play Mode Now)")]
        public static void Gate0ShopNow()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Enter Play Mode first.");
                return;
            }

            OpenShopAndCapture();
        }

        [MenuItem("VX Monster/Rebrand/Gate 4/Combat Walk (Play Mode Now)")]
        public static void Gate4CombatWalkNow()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Enter Play Mode first.");
                return;
            }

            StartCombatSelectWeaponAndCapture();
        }

        [MenuItem("VX Monster/Rebrand/Gate 7.3/Stage 3 Props (Play Mode Now)")]
        public static void Gate73Stage3Now() => BeginStageGateCapture(2, "gate_7_3_stage_3_props");

        [MenuItem("VX Monster/Rebrand/Gate 7.3/Stage 4 Props (Play Mode Now)")]
        public static void Gate73Stage4Now() => BeginStageGateCapture(3, "gate_7_3_stage_4_props");

        [MenuItem("VX Monster/Rebrand/Gate 7.3/Stage 5 Props (Play Mode Now)")]
        public static void Gate73Stage5Now() => BeginStageGateCapture(4, "gate_7_3_stage_5_props");

        [MenuItem("VX Monster/Rebrand/Gate 7.3/Stage 6 Props (Play Mode Now)")]
        public static void Gate73Stage6Now() => BeginStageGateCapture(5, "gate_7_3_stage_6_props");

        [MenuItem("VX Monster/Rebrand/Gate 7.5/Stage 1 Balance (Play Mode Now)")]
        public static void Gate75Stage1Now() => BeginStageGateCapture(0, "gate_7_5_stage_1_balance");

        [MenuItem("VX Monster/Rebrand/Gate 7.5/Stage 3 Balance (Play Mode Now)")]
        public static void Gate75Stage3Now() => BeginStageGateCapture(2, "gate_7_5_stage_3_balance");

        [MenuItem("VX Monster/Rebrand/Gate 7.5/Stage 6 Balance (Play Mode Now)")]
        public static void Gate75Stage6Now() => BeginStageGateCapture(5, "gate_7_5_stage_6_balance");

        [MenuItem("VX Monster/Rebrand/Gate 8.1/Stage 3 Enemies (Play Mode Now)")]
        public static void Gate81Stage3Now() => BeginStageGateCapture(2, "gate_8_1_stage_3_enemies", 10f);

        [MenuItem("VX Monster/Rebrand/Gate 8.1/Stage 4 Enemies (Play Mode Now)")]
        public static void Gate81Stage4Now() => BeginStageGateCapture(3, "gate_8_1_stage_4_enemies", 10f);

        [MenuItem("VX Monster/Rebrand/Gate 8.1/Stage 6 Enemies (Play Mode Now)")]
        public static void Gate81Stage6Now() => BeginStageGateCapture(5, "gate_8_1_stage_6_enemies", 10f);

        [MenuItem("VX Monster/Rebrand/Gate 9/Combat HUD (Play Mode Now)")]
        public static void Gate9HudNow() => BeginStageGateCapture(0, "gate_9_hud_combat", 4f);

        [MenuItem("VX Monster/Rebrand/Gate 7.8/Stage 1 Lobby Front (Play Mode Now)")]
        public static void Gate78Stage1LobbyNow() => CaptureLobbyStageFront(0, "gate_7_8_stage_1_front");

        [MenuItem("VX Monster/Rebrand/Gate 7.8/Stage 2 Lobby Front (Play Mode Now)")]
        public static void Gate78Stage2LobbyNow() => CaptureLobbyStageFront(1, "gate_7_8_stage_2_front");

        static void CaptureLobbyStageFront(int stageIndex, string gateLabel, int attempt = 0)
        {
            Init();
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Enter Play Mode from Main Menu first.");
                return;
            }

            EditorApplication.isPaused = false;
            DismissContinuePopupIfNeeded();
            UnlockAllStagesForGate();

            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            save.SetSelectedStageId(stageIndex);

            var lobby = FindUiBehavior<LobbyWindowBehavior>();
            if (lobby == null)
            {
                if (attempt < 20)
                {
                    Schedule(() => CaptureLobbyStageFront(stageIndex, gateLabel, attempt + 1), 0.5f);
                    return;
                }

                Debug.LogError("[VX Gate] LobbyWindowBehavior not found for stage front capture.");
                return;
            }

            lobby.InitStage(stageIndex);

            Schedule(() => CaptureGate(gateLabel), 1.2f);
        }

        [MenuItem("VX Monster/Rebrand/Enter Play Mode (Unpaused)")]
        public static void EnterPlayModeUnpaused()
        {
            EditorApplication.isPaused = false;
            if (!EditorApplication.isPlaying)
                EditorApplication.isPlaying = true;
        }

        [MenuItem("VX Monster/Rebrand/Capture Missing Gates (Play Mode Now)")]
        public static void CaptureMissingGatesNow()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Enter Play Mode from Main Menu first.");
                return;
            }

            Init();
            _captureQueueIndex = 0;
            Schedule(RunNextQueuedCapture, 1f);
        }

        static int _captureQueueIndex;

        static readonly (string menuPath, float waitSeconds)[] MissingGateQueue =
        {
            ("VX Monster/Rebrand/Gate 8.1/Stage 3 Enemies (Play Mode Now)", 70f),
            ("VX Monster/Rebrand/Gate 8.1/Stage 4 Enemies (Play Mode Now)", 70f),
            ("VX Monster/Rebrand/Gate 8.1/Stage 6 Enemies (Play Mode Now)", 70f),
            ("VX Monster/Rebrand/Gate 7.3/Stage 4 Props (Play Mode Now)", 55f),
            ("VX Monster/Rebrand/Gate 7.3/Stage 5 Props (Play Mode Now)", 55f),
            ("VX Monster/Rebrand/Gate 7.3/Stage 6 Props (Play Mode Now)", 55f),
        };

        static void RunNextQueuedCapture()
        {
            if (_captureQueueIndex >= MissingGateQueue.Length)
            {
                Debug.Log("[VX Gate] Missing gate capture queue complete.");
                return;
            }

            var (menuPath, waitSeconds) = MissingGateQueue[_captureQueueIndex];
            _captureQueueIndex++;

            if (!EditorApplication.ExecuteMenuItem(menuPath))
                Debug.LogError($"[VX Gate] Menu failed: {menuPath}");

            Schedule(() => WaitForLobbyBetweenCaptures(0), waitSeconds);
        }

        static void WaitForLobbyBetweenCaptures(int attempt)
        {
            var sceneName = GetActiveSceneName();
            if (sceneName == "Main Menu" && FindUiBehavior<LobbyWindowBehavior>() != null)
            {
                RunNextQueuedCapture();
                return;
            }

            if (attempt < 90)
            {
                EditorApplication.isPaused = false;
                Schedule(() => WaitForLobbyBetweenCaptures(attempt + 1), 1f);
                return;
            }

            Debug.LogWarning("[VX Gate] Lobby wait timed out — continuing queue.");
            RunNextQueuedCapture();
        }

        static void BeginStageGateCapture(int stageIndex, string gateLabel, float captureDelaySeconds = 3f)
        {
            Init();

            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Enter Play Mode first.");
                return;
            }

            EnsureGameReady(() => CaptureStageEndlessGate(stageIndex, gateLabel, captureDelaySeconds));
        }

        static void EnsureGameReady(Action whenReady, int attempt = 0)
        {
            if (!EditorApplication.isPlaying) return;

            EditorApplication.isPaused = false;
            DismissContinuePopupIfNeeded();

            if (GameController.SaveManager == null)
            {
                if (attempt < 120)
                {
                    Schedule(() => EnsureGameReady(whenReady, attempt + 1), 0.5f);
                    return;
                }

                Debug.LogError("[VX Gate] SaveManager never initialized.");
                return;
            }

            whenReady();
        }

        static void CaptureStageEndlessGate(int stageIndex, string gateLabel, float captureDelaySeconds = 3f, int lobbyAttempt = 0)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Enter Play Mode first.");
                return;
            }

            DismissContinuePopupIfNeeded();
            UnlockAllStagesForGate();

            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            save.SetSelectedStageId(stageIndex);

            var lobby = FindUiBehavior<LobbyWindowBehavior>();
            if (lobby == null)
            {
                if (lobbyAttempt < 40)
                {
                    var sceneName = GetActiveSceneName();
                    if (StageController.IsLoaded || sceneName == "Game")
                    {
                        _stageLoadInProgress = false;
                        CleanupStrayLoadingScenes();
                        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
                    }

                    Schedule(() => CaptureStageEndlessGate(stageIndex, gateLabel, captureDelaySeconds, lobbyAttempt + 1), 2.5f);
                    return;
                }

                Debug.LogError("[VX Gate] LobbyWindowBehavior not found.");
                return;
            }

            StartEndlessRunForGate(stageIndex);
            Schedule(() => WaitForStageThenCapture(gateLabel, captureDelaySeconds), 2f);
        }

        static bool _stageLoadInProgress;

        static void StartEndlessRunForGate(int stageIndex)
        {
            GameSessionManager.Instance?.ConfigureEndless(DifficultyTier.Normal);

            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            save.SetSelectedStageId(stageIndex);
            save.IsPlaying = true;
            save.ResetStageData = true;
            save.Time = 0f;
            save.XP = 0f;
            save.XPLEVEL = 0;
            save.EnemiesKilled = 0;

            if (GameController.AudioManager != null)
                GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            EditorApplication.isPaused = false;
            Time.timeScale = 1f;

            if (_stageLoadInProgress)
            {
                Debug.LogWarning("[VX Gate] Stage load already in progress — waiting.");
                return;
            }

            _stageLoadInProgress = true;
            CleanupStrayLoadingScenes();

            Debug.Log($"[VX Gate] Loading stage {stageIndex + 1} (scene={GetActiveSceneName()}).");
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

        static void CleanupStrayLoadingScenes()
        {
            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.name == "Loading Screen")
                    SceneManager.UnloadSceneAsync(scene);
            }
        }

        static void WaitForStageThenCapture(string gateLabel, float captureDelaySeconds, int attempt = 0)
        {
            EditorApplication.isPaused = false;

            if (StageController.IsLoaded)
            {
                _stageLoadInProgress = false;
                Schedule(() => TrySelectFirstWeapon(gateLabel, captureDelaySeconds), 1f);
                return;
            }

            if (attempt < 100)
            {
                if (attempt > 0 && attempt % 20 == 0)
                    Debug.Log($"[VX Gate] Waiting for StageController... attempt {attempt}, scene={GetActiveSceneName()}");

                Schedule(() => WaitForStageThenCapture(gateLabel, captureDelaySeconds, attempt + 1), 0.5f);
                return;
            }

            Debug.LogError("[VX Gate] StageController never loaded — cannot capture.");
            _stageLoadInProgress = false;
        }

        static void UnlockAllStagesForGate()
        {
            var lobby = FindUiBehavior<LobbyWindowBehavior>();
            if (lobby == null) return;

            var dbField = typeof(LobbyWindowBehavior).GetField("stagesDatabase",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var database = dbField?.GetValue(lobby) as StagesDatabase;
            if (database == null) return;

            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            for (var i = 0; i < database.StagesCount; i++)
                save.UnlockStage(database.GetStage(i));
        }

        static void StartCombatSelectWeaponAndCapture()
        {
            var lobby = FindUiBehavior<LobbyWindowBehavior>();
            if (lobby == null)
            {
                Debug.LogError("[VX Gate] LobbyWindowBehavior not found.");
                return;
            }

            lobby.OnPlayButtonClicked();
            Schedule(() => TrySelectFirstWeapon("gate_4_3_combat_walk"), 3.5f);
        }

        static void TrySelectFirstWeapon(string gateLabel = "gate_4_3_combat_walk", float captureDelaySeconds = 3f, int attempt = 0)
        {
            if (!StageController.IsLoaded)
            {
                if (attempt < 20)
                {
                    Schedule(() => TrySelectFirstWeapon(gateLabel, captureDelaySeconds, attempt + 1), 0.5f);
                    return;
                }

                Debug.LogError("[VX Gate] StageController not ready for weapon select.");
                return;
            }

#if UNITY_2022_2_OR_NEWER
            var cards = UnityEngine.Object.FindObjectsByType<AbilityCardBehavior>(FindObjectsInactive.Include);
#else
            var cards = UnityEngine.Object.FindObjectsOfType<AbilityCardBehavior>(true);
#endif
            if (cards == null || cards.Length == 0)
            {
                if (attempt < 30)
                {
                    Schedule(() => TrySelectFirstWeapon(gateLabel, captureDelaySeconds, attempt + 1), 0.5f);
                    return;
                }

                if (!TryInjectWoodenWand())
                {
                    Debug.LogError("[VX Gate] Could not select or inject starting weapon.");
                    return;
                }

                Schedule(() => CaptureGate(gateLabel), captureDelaySeconds);
                return;
            }

            var ability = cards[0].Data;
            if (ability != null && StageController.AbilityManager != null)
            {
                StageController.AbilityManager.AddAbility(ability);
                SuppressAbilityUi();
            }
            else if (!TryInjectWoodenWand())
            {
                Debug.LogError("[VX Gate] Ability card had null data and wand injection failed.");
                return;
            }

            Schedule(() => CaptureGate(gateLabel), captureDelaySeconds);
        }

        const string WoodenWandDataPath = "Assets/Common/Scriptables/Abilities/Active Abilities/Wooden Wand Data.asset";

        static bool TryInjectWoodenWand()
        {
            if (!StageController.IsLoaded || StageController.AbilityManager == null) return false;

            var wand = AssetDatabase.LoadAssetAtPath<AbilityData>(WoodenWandDataPath);
            if (wand == null) return false;

            StageController.AbilityManager.AddAbility(wand);
            SuppressAbilityUi();
            return true;
        }

        static void SuppressAbilityUi()
        {
            FindUiBehavior<AbilitiesWindowBehavior>()?.Hide();

#if UNITY_2022_2_OR_NEWER
            var windows = UnityEngine.Object.FindObjectsByType<AbilitiesWindowBehavior>(FindObjectsInactive.Include);
#else
            var windows = UnityEngine.Object.FindObjectsOfType<AbilitiesWindowBehavior>(true);
#endif
            foreach (var window in windows)
                window.Hide();
        }

        static void SelectFirstWeaponAndCaptureWalk()
        {
            TrySelectFirstWeapon();
        }

        [MenuItem("VX Monster/Rebrand/Gate 0/Combat (Play Mode Now)")]
        public static void Gate0CombatNow()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Enter Play Mode first.");
                return;
            }

            StartCombatAndCapture();
        }

        [MenuItem("VX Monster/Rebrand/Gate 0/Shop")]
        public static void Gate0Shop() => RunGateSequence("gate_0_shop", OpenShopAndCapture);

        [MenuItem("VX Monster/Rebrand/Gate 0/Combat")]
        public static void Gate0Combat() => RunGateSequence("gate_0_combat", StartCombatAndCapture);

        public static void RunGateSequence(string gateName, Action whenReady)
        {
            Init();
            _pendingGate = gateName;
            if (EditorApplication.isPlaying)
            {
                Schedule(whenReady, 1.5f);
                return;
            }

            Debug.LogWarning("[VX Gate] Not in Play Mode. Use MCP ManageEditor Play, then Gate 0/* (Play Mode Now) menus.");
            Schedule(whenReady, 3f);
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || string.IsNullOrEmpty(_pendingGate)) return;
            if (HasScheduledActions) return;

            if (_pendingGate.Contains("shop"))
                Schedule(OpenShopAndCapture, 2.5f);
            else if (_pendingGate.Contains("combat"))
                Schedule(StartCombatAndCapture, 2.5f);
            else
                Schedule(CaptureLobby, 2f);
        }

        static void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying || _scheduledActions.Count == 0) return;

            EditorApplication.isPaused = false;

            var now = (float)EditorApplication.timeSinceStartup;
            for (var i = 0; i < _scheduledActions.Count;)
            {
                if (_scheduledActions[i].RunAt > now)
                {
                    i++;
                    continue;
                }

                var action = _scheduledActions[i].Action;
                _scheduledActions.RemoveAt(i);
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[VX Gate] Capture failed: {ex}");
                }
            }
        }

        static void Schedule(Action action, float delaySeconds)
        {
            _scheduledActions.Add(new ScheduledGateAction
            {
                Action = action,
                RunAt = (float)EditorApplication.timeSinceStartup + delaySeconds,
            });
        }

        static void CaptureLobby()
        {
            CaptureGate("gate_0_lobby");
            _pendingGate = null;
        }

        static T FindUiBehavior<T>() where T : Component
        {
#if UNITY_2022_2_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
            return UnityEngine.Object.FindObjectOfType<T>(true);
#endif
        }

        static void OpenShopAndCapture()
        {
            var window = FindUiBehavior<CharactersWindowBehavior>();
            if (window != null)
            {
                window.Open();
                Schedule(() =>
                {
                    CaptureGate("gate_0_shop");
                    window.Close();
                    _pendingGate = null;
                }, 1.2f);
            }
            else
            {
                Debug.LogError("[VX Gate] CharactersWindowBehavior not found.");
                CaptureGate("gate_0_shop");
                _pendingGate = null;
            }
        }

        static void StartCombatAndCapture()
        {
            var lobby = FindUiBehavior<LobbyWindowBehavior>();
            if (lobby != null)
            {
                lobby.OnPlayButtonClicked();
                Schedule(() =>
                {
                    CaptureGate("gate_0_combat");
                    _pendingGate = null;
                }, 3f);
            }
            else
            {
                Debug.LogError("[VX Gate] LobbyWindowBehavior not found.");
                CaptureGate("gate_0_combat");
                _pendingGate = null;
            }
        }

        public static void CaptureGateLabel(string fileNameNoExt) => CaptureGate(fileNameNoExt);

        [MenuItem("VX Monster/Rebrand/Gate 2/1 Lobby Logo (Play Mode Now)")]
        public static void Gate21LobbyNow()
        {
            if (!EditorApplication.isPlaying) { Debug.LogError("[VX Gate] Play Mode required."); return; }
            DismissContinuePopupIfNeeded();
            Schedule(() =>
            {
                for (var i = 0; i < 30; i++) EditorApplication.Step();
                CaptureGate("gate_2_1_lobby_logo");
            }, 0.6f);
        }

        static void DismissContinuePopupIfNeeded()
        {
#if UNITY_2022_2_OR_NEWER
            var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
#else
            var buttons = UnityEngine.Object.FindObjectsOfType<Button>(true);
#endif
            foreach (var button in buttons)
            {
                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null && label.text.IndexOf("Cancel", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.onClick.Invoke();
                    return;
                }
            }
        }

        static void CaptureGate(string fileNameNoExt)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[VX Gate] Play Mode required.");
                return;
            }

            EditorApplication.isPaused = false;
            TrySetGameViewSize(Width, Height);
            FocusGameView();

            for (var i = 0; i < 15; i++)
                EditorApplication.Step();

            var path = Path.Combine(GetAbsoluteGateFolder(), fileNameNoExt + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            ScreenCapture.CaptureScreenshot(path);

            for (var i = 0; i < 10; i++)
                EditorApplication.Step();

            Schedule(() =>
            {
                if (File.Exists(path))
                {
                    Debug.Log($"[VX Gate] CAPTURE_PATH={path}");
                    if (StageController.IsLoaded)
                    {
                        _stageLoadInProgress = false;
                        CleanupStrayLoadingScenes();
                        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
                    }
                }
                else
                    Debug.LogError($"[VX Gate] Capture file missing: {path}");
            }, 0.75f);
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
                Debug.LogWarning($"[VX Gate] Focus Game View failed: {e.Message}");
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
                    var h = (int)sizeType!.GetProperty("height")!.GetValue(gvs, null)!;
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
                Debug.LogWarning($"[VX Gate] Could not set Game View size: {e.Message}");
                return false;
            }
        }

        static string GetAbsoluteGateFolder()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, GateFolder);
        }

        static string GetActiveSceneName()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return scene.IsValid() ? scene.name : null;
        }
    }
}
#endif
