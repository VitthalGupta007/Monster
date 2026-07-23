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
    /// Play Store captures with settle waits, caption overlay, staggered tablet resizes, and strict QA hooks (v2).
    /// </summary>
    public static class VXStoreScreenshotCapture
    {
        public const string OutputFolder = "Docs/StoreAssets/Screenshots";
        const string WoodenWandDataPath = "Assets/Common/Scriptables/Abilities/Active Abilities/Wooden Wand Data.asset";
        const float SettleSeconds = 1.0f;

        public readonly struct CaptureProfile
        {
            public readonly int Width;
            public readonly int Height;
            public readonly string ViewLabel;

            public CaptureProfile(int width, int height, string viewLabel)
            {
                Width = width;
                Height = height;
                ViewLabel = viewLabel;
            }
        }

        public static readonly CaptureProfile PhoneProfile = new(1080, 1920, "VX Phone 1080x1920");
        public static readonly CaptureProfile Tablet7Landscape = new(1920, 1080, "VX Tablet7 1920x1080");
        public static readonly CaptureProfile Tablet10Landscape = new(2560, 1440, "VX Tablet10 2560x1440");
        public static readonly CaptureProfile PortraitProfile = new(1080, 1920, "VX Portrait 1080x1920");
        public static readonly CaptureProfile PromoLandscape = new(1920, 1080, "VX Promo 1920x1080");

        static CaptureProfile _activeProfile = PhoneProfile;
        static int Width => _activeProfile.Width;
        static int Height => _activeProfile.Height;

        static readonly List<(Action action, float runAt)> Pending = new();
        static readonly List<GameObject> _hiddenLobbyLabels = new();
        static bool _hooked;
        static bool _includeTabletsInCapture;

        const string PendingRecapture05Key = "VXStore.PendingRecapture05";

        [InitializeOnLoadMethod]
        static void Hook()
        {
            if (_hooked) return;
            _hooked = true;
            EditorApplication.update += OnUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.delayCall += () =>
                Menu.SetChecked("VX Monster/Store/Captions Enabled", StoreCaptureCaptionOverlay.CaptionsEnabled);
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            if (!SessionState.GetBool(PendingRecapture05Key, false)) return;
            SessionState.SetBool(PendingRecapture05Key, false);
            Debug.Log("[VX Store] Entered Play Mode — starting pending Recapture 05.");
            Schedule(() => Recapture05OnlyAfterPlay(0), 2.5f);
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

        public static void Schedule(Action action, float delaySeconds) =>
            Pending.Add((action, (float)EditorApplication.timeSinceStartup + delaySeconds));

        static void WaitUntilReady(Func<bool> ready, Action onReady, int attempt, int maxAttempts, float interval)
        {
            if (ready())
            {
                onReady();
                return;
            }

            if (attempt >= maxAttempts)
            {
                Debug.LogWarning("[VX Store] WaitUntilReady timed out — continuing anyway.");
                onReady();
                return;
            }

            Schedule(() => WaitUntilReady(ready, onReady, attempt + 1, maxAttempts, interval), interval);
        }

        static void SettleGameView(CaptureProfile profile, Action onSettled)
        {
            _activeProfile = profile;
            // Do not force timeScale=1 here — gameplay capture freezes the horde on purpose.
            Application.runInBackground = true;
            EditorApplication.isPaused = false;
            TrySetGameViewSize(profile.Width, profile.Height, profile.ViewLabel);
            FocusGameView();
            Canvas.ForceUpdateCanvases();
            // Do NOT call EditorApplication.Step() — it leaves Play Mode paused and freezes combat.
            Schedule(onSettled, SettleSeconds);
        }

        static void ResumeStageSimulation()
        {
            Application.runInBackground = true;
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;

            if (!EditorApplication.isPlaying) return;

#if UNITY_2022_2_OR_NEWER
            var directors = UnityEngine.Object.FindObjectsByType<UnityEngine.Playables.PlayableDirector>(FindObjectsInactive.Include);
#else
            var directors = UnityEngine.Object.FindObjectsOfType<UnityEngine.Playables.PlayableDirector>(true);
#endif
            if (directors == null) return;
            foreach (var d in directors)
            {
                if (d == null) continue;
                if (d.state != UnityEngine.Playables.PlayState.Playing)
                    d.Play();
            }
        }

        [MenuItem("VX Monster/Store/Set Game View/Phone 1080x1920")]
        public static void SetPhoneGameView() => SetGameViewProfile(PhoneProfile);

        [MenuItem("VX Monster/Store/Set Game View/Tablet 7\" Landscape 1920x1080")]
        public static void SetTablet7LandscapeGameView() => SetGameViewProfile(Tablet7Landscape);

        [MenuItem("VX Monster/Store/Set Game View/Tablet 10\" Landscape 2560x1440")]
        public static void SetTablet10LandscapeGameView() => SetGameViewProfile(Tablet10Landscape);

        [MenuItem("VX Monster/Store/Set Game View/Promo 1920x1080")]
        public static void SetPromoGameView() => SetGameViewProfile(PromoLandscape);

        static void SetGameViewProfile(CaptureProfile profile)
        {
            _activeProfile = profile;
            TrySetGameViewSize(profile.Width, profile.Height, profile.ViewLabel);
            Debug.Log($"[VX Store] Game View set to {profile.Width}x{profile.Height}");
        }

        [MenuItem("VX Monster/Store/Capture Phone Screenshot (Play Mode)")]
        public static void CapturePhoneScreenshot()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode required", "Enter Play Mode first.", "OK");
                return;
            }

            CaptureWithVerify($"phone_live_{DateTime.Now:HHmmss}", PhoneProfile, _ => { });
        }

        [MenuItem("VX Monster/Store/Capture Named/01 Lobby")]
        public static void Capture01Lobby() => CaptureWithVerify("phone_01_lobby", PhoneProfile, _ => { });

        [MenuItem("VX Monster/Store/Capture Named/02 Modes")]
        public static void Capture02Modes() => CaptureWithVerify("phone_02_modes", PhoneProfile, _ => { });

        [MenuItem("VX Monster/Store/Capture Named/03 Characters")]
        public static void Capture03Characters() => CaptureWithVerify("phone_03_characters", PhoneProfile, _ => { });

        [MenuItem("VX Monster/Store/Capture Named/04 Stage")]
        public static void Capture04Stage() => CaptureWithVerify("phone_04_stage", PhoneProfile, _ => { });

        [MenuItem("VX Monster/Store/Capture Named/05 Gameplay")]
        public static void Capture05Gameplay() => CaptureWithVerify("phone_05_gameplay", PhoneProfile, _ => { });

        [MenuItem("VX Monster/Store/Capture Named/06 Abilities")]
        public static void Capture06Abilities() => CaptureWithVerify("phone_06_abilities", PhoneProfile, _ => { });

        [MenuItem("VX Monster/Store/Capture Named/07 Upgrades")]
        public static void Capture07Upgrades() => CaptureWithVerify("phone_07_upgrades", PhoneProfile, _ => { });

        [MenuItem("VX Monster/Store/Capture Named/08 Meta")]
        public static void Capture08Meta() => CaptureWithVerify("phone_08_meta", PhoneProfile, _ => { });

        [MenuItem("VX Monster/Store/Capture All Store Set (Play Mode Now)")]
        public static void CaptureAllStoreSetNow() => BeginStoreCapture(includeTablets: false);

        [MenuItem("VX Monster/Store/Capture All Store Set + Tablets (Play Mode Now)")]
        public static void CaptureAllStoreSetWithTabletsNow() => BeginStoreCapture(includeTablets: true);

        [MenuItem("VX Monster/Store/Recapture/04 Stage Only (Play Mode Now)")]
        public static void Recapture04Only()
        {
            if (!EnsurePlayMode()) return;
            PrepareForMainMenu(() =>
            {
                HideLobbyScoreLabels();
                SelectStage(2);
                WaitUntilReady(() => IsLobbyStageReady(2), () =>
                {
                    PrepareLobbyForCapture(2);
                    CaptureWithVerify("phone_04_stage", PhoneProfile, ok =>
                    {
                        RestoreLobbyScoreLabels();
                        if (!ok) Debug.LogError("[VX Store] phone_04_stage failed after max attempts.");
                    });
                }, 0, 24, 0.5f);
            });
        }

        [MenuItem("VX Monster/Store/Recapture/All Phone 01-08 (Play Mode Now)")]
        public static void RecaptureAllPhoneNow() => BeginStoreCapture(includeTablets: false);

        static void BeginStoreCapture(bool includeTablets)
        {
            if (!EnsurePlayMode()) return;

            _includeTabletsInCapture = includeTablets;
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            DismissContinuePopupIfNeeded();
            UnlockAllStages();
            HideLobbyScoreLabels();

            SelectStage(0);
            WaitUntilReady(() => IsLobbyStageReady(0), () =>
            {
                PrepareLobbyForCapture(0);
                CapturePhoneAndTabletsStaggered("phone_01_lobby", "01", "01", () =>
                    Schedule(Capture02ThenContinue, 0.5f));
            }, 0, 24, 0.5f);
        }

        static void Capture02ThenContinue()
        {
            SelectStage(1);
            WaitUntilReady(() => IsLobbyStageReady(1), () =>
            {
                PrepareLobbyForCapture(1);
                CapturePhoneAndTabletsStaggered("phone_02_modes", "02", null, () =>
                    Schedule(Capture03ThenContinue, 0.5f));
            }, 0, 24, 0.5f);
        }

        static void Capture03ThenContinue()
        {
            var characters = FindUi<CharactersWindowBehavior>();
            if (characters == null)
            {
                Debug.LogWarning("[VX Store] Characters window missing — skipping 03.");
                Schedule(Capture04ThenContinue, 0.3f);
                return;
            }

            characters.Open();
            WaitUntilReady(() => characters.gameObject.activeInHierarchy, () =>
            {
                PrepareCharactersWindowForCapture(characters);
                CaptureWithVerify("phone_03_characters", PhoneProfile, ok =>
                {
                    characters.Close();
                    if (ok) Schedule(Capture04ThenContinue, 0.5f);
                    else Debug.LogError("[VX Store] phone_03 failed QA.");
                });
            }, 0, 24, 0.5f);
        }

        static void Capture04ThenContinue()
        {
            SelectStage(2);
            WaitUntilReady(() => IsLobbyStageReady(2), () =>
            {
                PrepareLobbyForCapture(2);
                CaptureWithVerify("phone_04_stage", PhoneProfile, ok =>
                {
                    if (ok) Schedule(Capture05ThenContinue, 0.5f);
                    else Debug.LogError("[VX Store] phone_04 failed QA.");
                });
            }, 0, 24, 0.5f);
        }

        static void Capture05ThenContinue()
        {
            SelectStage(0);
            StartEndlessGame(0);
            Schedule(() => WaitCombatThenCapture05(0), 2.5f);
        }

        static void WaitCombatThenCapture05(int attempt)
        {
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;

            if (!StageController.IsLoaded)
            {
                if (attempt < 60) Schedule(() => WaitCombatThenCapture05(attempt + 1), 0.5f);
                else Debug.LogError("[VX Store] Combat never loaded.");
                return;
            }

            // Weapon-select freezes timeScale=0; dismiss first, then wait for enemies.
            SuppressAbilityUiAndEnsureWeapon();
            Schedule(() => PrepareGameplayCombatForCapture(() =>
            {
                Debug.Log($"[VX Store] Gameplay enemies={CountEnemies()}; capturing phone_05.");
                CapturePhoneAndTabletsStaggered("phone_05_gameplay", "03", "02", () =>
                    Schedule(Capture06ThenContinue, 0.5f));
            }), 1.5f);
        }

        static void PrepareGameplayCombatForCapture(Action onReady, int attempt = 0)
        {
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            MakePlayerInvincibleForCapture();
            SetPlayerCombatEnabled(false);

            if (IsStageFailedVisible())
            {
                TryClickRetry();
                if (attempt < 100) Schedule(() => PrepareGameplayCombatForCapture(onReady, attempt + 1), 0.75f);
                else Debug.LogError("[VX Store] Stuck on StageFailed for phone_05.");
                return;
            }

            if (!StageController.IsLoaded)
            {
                if (attempt < 100) Schedule(() => PrepareGameplayCombatForCapture(onReady, attempt + 1), 0.5f);
                else Debug.LogError("[VX Store] Combat scene never loaded for phone_05.");
                return;
            }

            if (IsAbilityPanelBlocking())
            {
                SuppressAbilityUiAndEnsureWeapon();
                if (attempt < 100) Schedule(() => PrepareGameplayCombatForCapture(onReady, attempt + 1), 0.75f);
                else Debug.LogError("[VX Store] Ability panel never dismissed for phone_05.");
                return;
            }

            // Need a visible horde for store QA — wait until the field is busy.
            if (CountEnemies() < 12)
            {
                if (attempt % 10 == 0)
                    Debug.Log($"[VX Store] Waiting for horde… enemies={CountEnemies()} attempt={attempt}");
                if (attempt < 150) Schedule(() => PrepareGameplayCombatForCapture(onReady, attempt + 1), 0.35f);
                else Debug.LogError("[VX Store] Not enough enemies for phone_05.");
                return;
            }

            // Freeze the moment so SettleGameView resize doesn't clear the horde / advance to death.
            Time.timeScale = 0f;
            Debug.Log($"[VX Store] Horde ready enemies={CountEnemies()}; freezing for capture.");
            onReady();
        }

        static void SetPlayerCombatEnabled(bool enabled)
        {
            if (!StageController.IsLoaded || StageController.AbilityManager == null) return;
            StageController.AbilityManager.enabled = enabled;
        }

        static bool IsStageFailedVisible()
        {
            var failed = UnityEngine.Object.FindAnyObjectByType<StageFailedScreen>(FindObjectsInactive.Exclude);
            return failed != null && failed.gameObject.activeInHierarchy;
        }

        static void TryClickRetry()
        {
            foreach (var b in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude))
            {
                if (b == null) continue;
                var label = b.GetComponentInChildren<TMP_Text>(true);
                if (label == null || label.text == null) continue;
                if (label.text.IndexOf("RETRY", StringComparison.OrdinalIgnoreCase) < 0) continue;
                b.onClick.Invoke();
                Debug.Log("[VX Store] Clicked RETRY on StageFailed.");
                return;
            }
        }

        static void MakePlayerInvincibleForCapture()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerBehavior>(FindObjectsInactive.Exclude);
            if (player == null) return;
            var field = typeof(PlayerBehavior).GetField("invincible",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            field?.SetValue(player, true);
        }

        static bool IsAbilityPanelBlocking()
        {
            var abilities = FindUi<AbilitiesWindowBehavior>();
            return abilities != null && abilities.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Same approach as VXRebrandGateRunner: inject/select weapon via AbilityManager, then Hide().
        /// Button onClick alone fails while timeScale=0 / EventSystem may not fire in editor capture.
        /// </summary>
        static void SuppressAbilityUiAndEnsureWeapon()
        {
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;

            if (StageController.IsLoaded && StageController.AbilityManager != null)
            {
#if UNITY_2022_2_OR_NEWER
                var cards = UnityEngine.Object.FindObjectsByType<AbilityCardBehavior>(FindObjectsInactive.Include);
#else
                var cards = UnityEngine.Object.FindObjectsOfType<AbilityCardBehavior>(true);
#endif
                AbilityData chosen = null;
                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        if (card != null && card.gameObject.activeInHierarchy && card.Data != null)
                        {
                            chosen = card.Data;
                            break;
                        }
                    }
                }

                if (chosen == null)
                {
                    chosen = AssetDatabase.LoadAssetAtPath<AbilityData>(WoodenWandDataPath);
                }

                if (chosen != null && !StageController.AbilityManager.IsAbilityAquired(chosen.AbilityType))
                {
                    StageController.AbilityManager.AddAbility(chosen);
                    Debug.Log($"[VX Store] Injected starting weapon: {chosen.Title}");
                }
            }

#if UNITY_2022_2_OR_NEWER
            var windows = UnityEngine.Object.FindObjectsByType<AbilitiesWindowBehavior>(FindObjectsInactive.Include);
#else
            var windows = UnityEngine.Object.FindObjectsOfType<AbilitiesWindowBehavior>(true);
#endif
            if (windows != null)
            {
                foreach (var window in windows)
                {
                    if (window == null) continue;
                    try { window.Hide(); }
                    catch (Exception ex) { Debug.LogWarning($"[VX Store] Ability Hide skipped: {ex.Message}"); }
                    // Force-close immediately — Hide() animates and would block capture waits.
                    window.gameObject.SetActive(false);
                }
            }

            Time.timeScale = 1f;
            ResumeStageSimulation();
        }

        static void Capture06ThenContinue()
        {
            var abilities = FindUi<AbilitiesWindowBehavior>();
            if (abilities == null)
            {
                Schedule(Capture07ThenContinue, 0.3f);
                return;
            }

            abilities.Show(true);
            WaitUntilReady(() => abilities.gameObject.activeInHierarchy && CountAbilityCards() >= 2, () =>
            {
                CaptureWithVerify("phone_06_abilities", PhoneProfile, ok =>
                {
                    abilities.Hide();
                    if (ok) Schedule(Capture07ThenContinue, 0.5f);
                });
            }, 0, 24, 0.5f);
        }

        static void Capture07ThenContinue()
        {
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
                if (attempt < 20) Schedule(() => WaitLobbyThenCapture07(attempt + 1), 0.5f);
                else Schedule(Capture08MetaSheet, 0.5f);
                return;
            }

            upgrades.Open();
            WaitUntilReady(() => upgrades.gameObject.activeInHierarchy, () =>
            {
                PrepareUpgradesForCapture(upgrades);
                CaptureWithVerify("phone_07_upgrades", PhoneProfile, ok =>
                {
                    if (ok) Schedule(Capture08MetaSheet, 0.5f);
                });
            }, 0, 24, 0.5f);
        }

        static void Capture08MetaSheet()
        {
            FindUi<UpgradesWindowBehavior>()?.Close();
            FindUi<CharactersWindowBehavior>()?.Close();
            SelectStage(0);

            Schedule(() =>
            {
                HideLegacyPlayGamesSignInRow();
                ClickButtonNamed("VX Menu Button");

                WaitUntilReady(() => IsMetaMenuReady(), () =>
                {
                    HideLegacyPlayGamesSignInRow();
                    CapturePhoneAndTabletsStaggered("phone_08_meta", "04", null, () =>
                    {
                        RestoreLobbyScoreLabels();
                        Debug.Log(_includeTabletsInCapture
                            ? "[VX Store] Store set complete (phone + tablets). Run Strict QA visual pass."
                            : "[VX Store] Store set complete (phone). Run Strict QA visual pass.");
                    });
                }, 0, 24, 0.5f);
            }, 0.6f);
        }

        static void CapturePhoneAndTabletsStaggered(string phoneFile, string tabletIndex, string portraitIndex, Action onComplete)
        {
            CaptureWithVerify(phoneFile, PhoneProfile, phoneOk =>
            {
                if (!phoneOk)
                {
                    onComplete?.Invoke();
                    return;
                }

                if (!_includeTabletsInCapture)
                {
                    onComplete?.Invoke();
                    return;
                }

                CaptureWithVerify($"Tablet7/tablet7_{tabletIndex}", Tablet7Landscape, t7Ok =>
                {
                    if (!t7Ok) { onComplete?.Invoke(); return; }
                    CaptureWithVerify($"Tablet10/tablet10_{tabletIndex}", Tablet10Landscape, t10Ok =>
                    {
                        if (!t10Ok || string.IsNullOrEmpty(portraitIndex))
                        {
                            onComplete?.Invoke();
                            return;
                        }

                        CaptureWithVerify($"Tablet7/tablet7_portrait_{portraitIndex}", PortraitProfile, pOk =>
                        {
                            if (pOk)
                            {
                                var src = Path.Combine(GetAbsoluteOutputFolder(), $"Tablet7/tablet7_portrait_{portraitIndex}.png");
                                var dst = Path.Combine(GetAbsoluteOutputFolder(), $"Tablet10/tablet10_portrait_{portraitIndex}.png");
                                Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
                                File.Copy(src, dst, overwrite: true);
                                StoreAssetStrictQA.RunAutomatedOnCapture(dst, StoreCaptureCaptionOverlay.CaptionsEnabled);
                            }

                            onComplete?.Invoke();
                        });
                    });
                });
            });
        }

        static void CaptureWithVerify(string fileNameNoExt, CaptureProfile profile, Action<bool> onComplete, int attempt = 0)
        {
            if (!EditorApplication.isPlaying)
            {
                onComplete(false);
                return;
            }

            var rel = fileNameNoExt.Replace('\\', '/');
            if (attempt >= StoreAssetStrictQA.MaxAttempts)
            {
                Debug.LogError($"[VX Store QA] FAIL {rel} after {StoreAssetStrictQA.MaxAttempts} attempts.");
                StoreCaptureCaptionOverlay.Hide();
                onComplete(false);
                return;
            }

            var caption = StoreAssetStrictQA.GetCaptionForFile(fileNameNoExt);
            SettleGameView(profile, () =>
            {
                // Keep combat frozen only for the capture frame so enemies stay in shot.
                MakePlayerInvincibleForCapture();
                // Gameplay shots prepare with timeScale=0; keep frozen through the screenshot.
                var freezeForShot = fileNameNoExt.Contains("phone_05")
                    || fileNameNoExt.Contains("tablet7_03")
                    || fileNameNoExt.Contains("tablet10_03")
                    || fileNameNoExt.Contains("portrait_02");
                if (freezeForShot)
                    Time.timeScale = 0f;
                else
                    Time.timeScale = 1f;

                StoreCaptureCaptionOverlay.Show(caption, profile.Width, profile.Height);
                Canvas.ForceUpdateCanvases();

                var path = GetCaptureAbsolutePath(fileNameNoExt);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                // Delete stale PNG so we never QA an old frame that lacks the caption.
                if (File.Exists(path))
                {
                    try { File.Delete(path); }
                    catch (Exception ex) { Debug.LogWarning($"[VX Store] Could not delete stale PNG: {ex.Message}"); }
                }

                Application.runInBackground = true;
                EditorApplication.isPaused = false;
                FocusGameView();
                // CaptureScreenshot is end-of-frame async — leave caption up until the file lands.
                ScreenCapture.CaptureScreenshot(path);
                Debug.Log($"[VX Store] Capture queued ({rel}) attempt {attempt + 1}: {path}");

                Schedule(() => WaitForPngThenVerify(path, fileNameNoExt, profile, onComplete, attempt, 0), 0.35f);
            });
        }

        static void WaitForPngThenVerify(
            string path, string fileNameNoExt, CaptureProfile profile, Action<bool> onComplete, int attempt, int waitTick)
        {
            var rel = fileNameNoExt.Replace('\\', '/');
            if (!File.Exists(path) || new FileInfo(path).Length < 1024)
            {
                if (waitTick < 20)
                {
                    Schedule(() => WaitForPngThenVerify(path, fileNameNoExt, profile, onComplete, attempt, waitTick + 1), 0.25f);
                    return;
                }

                StoreCaptureCaptionOverlay.Hide();
                Debug.LogWarning($"[VX Store QA] PNG missing/empty, retry {rel} attempt {attempt + 1}");
                CaptureWithVerify(fileNameNoExt, profile, onComplete, attempt + 1);
                return;
            }

            // Caption stayed visible through the async capture; hide only after the file exists.
            StoreCaptureCaptionOverlay.Hide();
            var freezeForShot = fileNameNoExt.Contains("phone_05")
                || fileNameNoExt.Contains("tablet7_03")
                || fileNameNoExt.Contains("tablet10_03")
                || fileNameNoExt.Contains("portrait_02");
            Time.timeScale = freezeForShot ? 0f : 1f;
            EditorApplication.isPaused = false;

            if (!StoreAssetStrictQA.RunAutomatedOnCapture(path, StoreCaptureCaptionOverlay.CaptionsEnabled))
            {
                Debug.LogWarning($"[VX Store QA] AUTO-FAIL {rel} attempt {attempt + 1} — retrying capture.");
                CaptureWithVerify(fileNameNoExt, profile, onComplete, attempt + 1);
                return;
            }

            Debug.Log($"[VX Store] CAPTURE_PATH={path}");
            onComplete(true);
        }

        static string GetCaptureAbsolutePath(string fileNameNoExt) =>
            Path.Combine(GetAbsoluteOutputFolder(), fileNameNoExt.Replace('\\', '/') + ".png");

        static void PrepareLobbyForCapture(int stageIndex)
        {
            _activeProfile = PhoneProfile;
            TrySetGameViewSize(PhoneProfile.Width, PhoneProfile.Height, PhoneProfile.ViewLabel);
            FocusGameView();
            SelectStage(stageIndex);
            Canvas.ForceUpdateCanvases();
        }

        static void PrepareCharactersWindowForCapture(CharactersWindowBehavior characters)
        {
            PrepareLobbyForCapture(GameController.SaveManager?.GetSave<StageSave>("Stage")?.SelectedStageId ?? 0);
            var scroll = characters.GetComponentInChildren<ScrollRect>(true);
            if (scroll != null)
            {
                scroll.verticalNormalizedPosition = 1f;
                scroll.horizontalNormalizedPosition = 0f;
            }

            Canvas.ForceUpdateCanvases();
            if (scroll?.content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            Canvas.ForceUpdateCanvases();
        }

        static void PrepareUpgradesForCapture(UpgradesWindowBehavior upgrades)
        {
            _activeProfile = PhoneProfile;
            TrySetGameViewSize(PhoneProfile.Width, PhoneProfile.Height, PhoneProfile.ViewLabel);
            FocusGameView();
            var scroll = upgrades.GetComponentInChildren<ScrollRect>(true);
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
        }

        static bool IsLobbyStageReady(int stageIndex)
        {
            var lobby = FindUi<LobbyWindowBehavior>();
            if (lobby == null || !lobby.gameObject.activeInHierarchy) return false;
            var db = AssetDatabase.LoadAssetAtPath<StagesDatabase>("Assets/Common/Scriptables/Stages/Stages Database.asset");
            if (db == null || stageIndex >= db.StagesCount) return true;
            return true;
        }

        static bool IsMetaMenuReady()
        {
            var sheet = GameObject.Find("VX Meta Menu Sheet");
            return sheet != null && sheet.activeInHierarchy;
        }

        static int CountEnemies()
        {
#if UNITY_2022_2_OR_NEWER
            var enemies = UnityEngine.Object.FindObjectsByType<EnemyBehavior>(FindObjectsInactive.Exclude);
#else
            var enemies = UnityEngine.Object.FindObjectsOfType<EnemyBehavior>();
#endif
            return enemies != null ? enemies.Length : 0;
        }

        static int CountAbilityCards()
        {
#if UNITY_2022_2_OR_NEWER
            var cards = UnityEngine.Object.FindObjectsByType<AbilityCardBehavior>(FindObjectsInactive.Exclude);
#else
            var cards = UnityEngine.Object.FindObjectsOfType<AbilityCardBehavior>();
#endif
            return cards != null ? cards.Length : 0;
        }

        static void HideLobbyScoreLabels()
        {
            RestoreLobbyScoreLabels();
#if UNITY_2022_2_OR_NEWER
            var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
#else
            var transforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
#endif
            foreach (var t in transforms)
            {
                if (t == null) continue;
                if (t.name != "Daily Best Label" && t.name != "Endless Best Label") continue;
                if (!t.gameObject.activeSelf) continue;
                t.gameObject.SetActive(false);
                _hiddenLobbyLabels.Add(t.gameObject);
            }
        }

        static void RestoreLobbyScoreLabels()
        {
            foreach (var go in _hiddenLobbyLabels)
                if (go != null) go.SetActive(true);
            _hiddenLobbyLabels.Clear();
        }

        [MenuItem("VX Monster/Store/Capture Remaining 05-08 (Play Mode Now)")]
        public static void CaptureRemaining0508Now()
        {
            if (!EnsurePlayMode()) return;
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            DismissContinuePopupIfNeeded();
            UnlockAllStages();
            Capture05ThenContinue();
        }

        [MenuItem("VX Monster/Store/Capture Remaining 07-08 (Play Mode Now)")]
        public static void CaptureRemaining0708Now()
        {
            if (!EnsurePlayMode()) return;
            PrepareForMainMenu(() => Schedule(() => WaitLobbyThenCapture07(0), 1f));
        }

        [MenuItem("VX Monster/Store/Recapture/All Tablets Only (Play Mode Now)")]
        public static void RecaptureAllTabletsOnly()
        {
            if (!EnsurePlayMode()) return;
            _includeTabletsInCapture = true;
            HideLobbyScoreLabels();
            PrepareForMainMenu(() =>
            {
                SelectStage(0);
                WaitUntilReady(() => IsLobbyStageReady(0), () =>
                {
                    PrepareLobbyForCapture(0);
                    CapturePhoneAndTabletsStaggered("phone_01_lobby", "01", "01", () =>
                        Schedule(RecaptureTablets02, 0.5f));
                }, 0, 24, 0.5f);
            });
        }

        static void RecaptureTablets02()
        {
            SelectStage(1);
            WaitUntilReady(() => IsLobbyStageReady(1), () =>
            {
                PrepareLobbyForCapture(1);
                CapturePhoneAndTabletsStaggered("phone_02_modes", "02", null, () =>
                    Schedule(RecaptureTablets05, 0.5f));
            }, 0, 24, 0.5f);
        }

        static void RecaptureTablets05()
        {
            StartEndlessGame(0);
            Schedule(() => RecaptureTablets05Body(0), 3f);
        }

        static void RecaptureTablets05Body(int attempt)
        {
            if (!StageController.IsLoaded)
            {
                if (attempt < 40) Schedule(() => RecaptureTablets05Body(attempt + 1), 0.5f);
                return;
            }

            PrepareGameplayCombatForCapture(() =>
                CapturePhoneAndTabletsStaggered("phone_05_gameplay", "03", "02", () =>
                    Schedule(RecaptureTablets08, 0.5f)));
        }

        static void RecaptureTablets08()
        {
            PrepareForMainMenu(() => Schedule(Capture08MetaSheet, 1f));
        }

        [MenuItem("VX Monster/Store/Verify All Screenshots (Dimensions)")]
        public static void VerifyAllScreenshotsDimensionsMenu() =>
            StoreAssetStrictQA.VerifyAllScreenshotsMenu();

        [MenuItem("VX Monster/Store/Recapture/03 Characters Only (Play Mode Now)")]
        public static void Recapture03Only()
        {
            if (!EnsurePlayMode()) return;
            PrepareForMainMenu(() =>
            {
                var characters = FindUi<CharactersWindowBehavior>();
                if (characters == null) return;
                characters.Open();
                WaitUntilReady(() => characters.gameObject.activeInHierarchy, () =>
                {
                    PrepareCharactersWindowForCapture(characters);
                    CaptureWithVerify("phone_03_characters", PhoneProfile, _ => characters.Close());
                }, 0, 24, 0.5f);
            });
        }

        [MenuItem("VX Monster/Store/Recapture/05 Gameplay Only (Play Mode Now)")]
        public static void Recapture05Only()
        {
            if (!EditorApplication.isPlaying)
            {
                // Schedule cannot survive domain reload — use SessionState + playModeStateChanged.
                SessionState.SetBool(PendingRecapture05Key, true);
                EditorApplication.isPlaying = true;
                return;
            }

            Recapture05OnlyAfterPlay(0);
        }

        static void Recapture05OnlyAfterPlay(int attempt)
        {
            if (!EditorApplication.isPlaying)
            {
                if (attempt < 20) Schedule(() => Recapture05OnlyAfterPlay(attempt + 1), 0.5f);
                return;
            }

            UnlockAllStages();
            StartEndlessGame(0);
            Schedule(() => Recapture05OnlyBody(0), 3f);
        }

        static void Recapture05OnlyBody(int attempt)
        {
            if (!StageController.IsLoaded)
            {
                if (attempt < 40) Schedule(() => Recapture05OnlyBody(attempt + 1), 0.5f);
                return;
            }

            PrepareGameplayCombatForCapture(() =>
            {
                _includeTabletsInCapture = true;
                CapturePhoneAndTabletsStaggered("phone_05_gameplay", "03", "02", () =>
                {
                    Time.timeScale = 1f;
                    Debug.Log("[VX Store] Recapture 05 chain complete.");
                });
            });
        }

        [MenuItem("VX Monster/Store/Recapture/08 Meta Only (Play Mode Now)")]
        public static void Recapture08Only()
        {
            if (!EnsurePlayMode()) return;
            PrepareForMainMenu(() => Schedule(Capture08MetaSheet, 1f));
        }

        static bool EnsurePlayMode()
        {
            if (EditorApplication.isPlaying) return true;
            Debug.LogError("[VX Store] Play Mode required.");
            return false;
        }

        static void PrepareForMainMenu(Action then)
        {
            EditorApplication.isPaused = false;
            Time.timeScale = 1f;
            _activeProfile = PhoneProfile;
            TrySetGameViewSize(PhoneProfile.Width, PhoneProfile.Height, PhoneProfile.ViewLabel);
            DismissContinuePopupIfNeeded();
            if (SceneManager.GetActiveScene().name != "Main Menu")
                SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
            Schedule(then, 2f);
        }

        public static void StartEndlessGame(int stageIndex)
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

        static void EnsureStartingWeapon() => SuppressAbilityUiAndEnsureWeapon();

        public static void SelectStage(int stageIndex)
        {
            if (GameController.SaveManager == null) return;
            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            save.SetSelectedStageId(stageIndex);
            FindUi<LobbyWindowBehavior>()?.InitStage(stageIndex);
        }

        public static void UnlockAllStages()
        {
            var db = AssetDatabase.LoadAssetAtPath<StagesDatabase>("Assets/Common/Scriptables/Stages/Stages Database.asset");
            if (db == null || GameController.SaveManager == null) return;
            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            for (var i = 0; i < db.StagesCount; i++)
                save.UnlockStage(db.GetStage(i));
        }

        static string CaptureNamedImmediate(string fileNameNoExt, CaptureProfile profile)
        {
            _activeProfile = profile;
            ResumeStageSimulation();
            TrySetGameViewSize(profile.Width, profile.Height, profile.ViewLabel);
            FocusGameView();
            Canvas.ForceUpdateCanvases();
            var path = GetCaptureAbsolutePath(fileNameNoExt);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            ScreenCapture.CaptureScreenshot(path);
            return path;
        }

        static void HideLegacyPlayGamesSignInRow()
        {
#if UNITY_2022_2_OR_NEWER
            var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
#else
            var transforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
#endif
            foreach (var t in transforms)
            {
                if (t != null && t.name == "Play Games Sign In Row")
                    t.gameObject.SetActive(false);
            }
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
            catch (Exception e) { Debug.LogWarning($"[VX Store] Focus Game View failed: {e.Message}"); }
        }

        public static bool TrySetGameViewSize(int width, int height, string viewLabel)
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
                    if (w == width && h == height) { index = i; break; }
                }

                if (index < 0)
                {
                    var fixedRes = Enum.Parse(sizeTypeEnum!, "FixedResolution");
                    var ctor = sizeType!.GetConstructor(new[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) });
                    var newSize = ctor!.Invoke(new object[] { fixedRes, width, height, viewLabel });
                    groupType.GetMethod("AddCustomSize")!.Invoke(currentGroup, new[] { newSize });
                    index = (int)getTotalCount.Invoke(currentGroup, null)! - 1;
                }

                var selectedSizeProp = gameViewType!.GetProperty("selectedSizeIndex",
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
                var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
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
                    catch (Exception ex) { Debug.LogWarning($"[VX Store] Cancel skipped: {ex.Message}"); }
                    return;
                }
            }
            catch (Exception ex) { Debug.LogWarning($"[VX Store] Dismiss continue skipped: {ex.Message}"); }
        }

        static void ClickButtonNamed(string objectName)
        {
#if UNITY_2022_2_OR_NEWER
            var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
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
            return UnityEngine.Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
#else
            return UnityEngine.Object.FindObjectOfType<T>(true);
#endif
        }

        public static string GetAbsoluteOutputFolder()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, OutputFolder);
        }
    }
}
#endif
