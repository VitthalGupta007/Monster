#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Core.Abilities.UI;
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
        static float _waitUntil;
        static Action _pendingAction;
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
        public static void Gate73Stage3Now() => CaptureStageEndlessGate(2, "gate_7_3_stage_3_props");

        [MenuItem("VX Monster/Rebrand/Gate 7.3/Stage 4 Props (Play Mode Now)")]
        public static void Gate73Stage4Now() => CaptureStageEndlessGate(3, "gate_7_3_stage_4_props");

        [MenuItem("VX Monster/Rebrand/Gate 7.3/Stage 5 Props (Play Mode Now)")]
        public static void Gate73Stage5Now() => CaptureStageEndlessGate(4, "gate_7_3_stage_5_props");

        [MenuItem("VX Monster/Rebrand/Gate 7.3/Stage 6 Props (Play Mode Now)")]
        public static void Gate73Stage6Now() => CaptureStageEndlessGate(5, "gate_7_3_stage_6_props");

        [MenuItem("VX Monster/Rebrand/Gate 7.5/Stage 1 Balance (Play Mode Now)")]
        public static void Gate75Stage1Now() => CaptureStageEndlessGate(0, "gate_7_5_stage_1_balance");

        [MenuItem("VX Monster/Rebrand/Gate 7.5/Stage 3 Balance (Play Mode Now)")]
        public static void Gate75Stage3Now() => CaptureStageEndlessGate(2, "gate_7_5_stage_3_balance");

        [MenuItem("VX Monster/Rebrand/Gate 7.5/Stage 6 Balance (Play Mode Now)")]
        public static void Gate75Stage6Now() => CaptureStageEndlessGate(5, "gate_7_5_stage_6_balance");

        static void CaptureStageEndlessGate(int stageIndex, string gateLabel)
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
                Debug.LogError("[VX Gate] LobbyWindowBehavior not found.");
                return;
            }

            lobby.StartEndlessRun(DifficultyTier.Normal);
            Schedule(() => TrySelectFirstWeapon(gateLabel), 3.5f);
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

        static void TrySelectFirstWeapon(string gateLabel = "gate_4_3_combat_walk")
        {
#if UNITY_2022_2_OR_NEWER
            var cards = UnityEngine.Object.FindObjectsByType<AbilityCardBehavior>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var cards = UnityEngine.Object.FindObjectsOfType<AbilityCardBehavior>(true);
#endif
            if (cards == null || cards.Length == 0)
            {
                Schedule(() => TrySelectFirstWeapon(gateLabel), 0.5f);
                return;
            }

            var ability = cards[0].Data;
            if (ability != null && StageController.AbilityManager != null)
            {
                StageController.AbilityManager.AddAbility(ability);
                FindUiBehavior<AbilitiesWindowBehavior>()?.Hide();
            }

            Schedule(() => CaptureGate(gateLabel), 3f);
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
            if (_pendingAction != null) return;

            if (_pendingGate.Contains("shop"))
                Schedule(OpenShopAndCapture, 2.5f);
            else if (_pendingGate.Contains("combat"))
                Schedule(StartCombatAndCapture, 2.5f);
            else
                Schedule(CaptureLobby, 2f);
        }

        static void OnEditorUpdate()
        {
            if (_pendingAction == null) return;
            if (!EditorApplication.isPlaying) return;
            if (EditorApplication.timeSinceStartup < _waitUntil) return;

            var action = _pendingAction;
            _pendingAction = null;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VX Gate] Capture failed: {ex}");
            }
        }

        static void Schedule(Action action, float delaySeconds)
        {
            _pendingAction = action;
            _waitUntil = (float)EditorApplication.timeSinceStartup + delaySeconds;
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
            var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

            Debug.Log($"[VX Gate] CAPTURE_PATH={path}");
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
    }
}
#endif
