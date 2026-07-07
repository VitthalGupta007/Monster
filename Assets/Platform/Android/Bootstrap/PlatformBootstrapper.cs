using OctoberStudio;
using OctoberStudio.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using VXMonster.Gameplay;
using VXMonster.Platform;
using VXMonster.Platform.Ads;
using VXMonster.Platform.PlayGames;
using VXMonster.Save;

namespace VXMonster.Platform.Bootstrap
{
    [DefaultExecutionOrder(50)]
    public class PlatformBootstrapper : MonoBehaviour
    {
        [SerializeField] AdMobConfig adMobConfig;

        private static PlatformBootstrapper instance;
        private IAdService adService;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (FindAnyObjectByType<PlatformBootstrapper>() != null) return;
            var go = new GameObject("VX Platform Bootstrapper");
            go.AddComponent<PlatformBootstrapper>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            EnsureGameSessionManager();
            EnsureRelicsManager();

            if (adMobConfig == null)
            {
                adMobConfig = ScriptableObject.CreateInstance<AdMobConfig>();
            }

#if UNITY_ANDROID && !UNITY_EDITOR && GOOGLE_MOBILE_ADS_AVAILABLE
            adService = new AdMobService(adMobConfig);
#else
            adService = new MockAdService();
#endif

            PlatformServices.Initialize(adMobConfig, adService, new MockPlayGamesService());

            // AfterSceneLoad may run before this object exists; attach panel for the current scene too.
            TryAttachLobbyModePanel();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttachLobbyModePanel();
        }

        private static void TryAttachLobbyModePanel()
        {
            if (FindAnyObjectByType<VXMonster.UI.VXLobbyModePanel>() != null) return;

            var lobby = FindAnyObjectByType<OctoberStudio.UI.LobbyWindowBehavior>();
            if (lobby == null) return;

            lobby.gameObject.AddComponent<VXMonster.UI.VXLobbyModePanel>();
        }

        private void Start()
        {
            BindPersistentSaves();
            PlatformServices.ShowMainMenuBanner();
        }

        private static void EnsureGameSessionManager()
        {
            if (GameSessionManager.Instance != null) return;

            var go = new GameObject("Game Session Manager");
            go.AddComponent<GameSessionManager>();
            DontDestroyOnLoad(go);
        }

        private static void EnsureRelicsManager()
        {
            if (RelicsManager.Instance != null) return;

            var go = new GameObject("Relics Manager");
            go.AddComponent<RelicsManager>();
            DontDestroyOnLoad(go);
        }

        private void BindPersistentSaves()
        {
            if (GameController.SaveManager == null) return;

            var runSession = GameController.SaveManager.GetSave<RunSessionSave>("VX Run Session");
            var lifetime = GameController.SaveManager.GetSave<LifetimeStatsSave>("VX Lifetime Stats");
            var daily = GameController.SaveManager.GetSave<DailyChallengeSave>("VX Daily Challenge");
            var codex = GameController.SaveManager.GetSave<CodexSave>("VX Codex");
            var talent = GameController.SaveManager.GetSave<TalentTreeSave>("VX Talent Tree");

            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.BindSaves(runSession, lifetime, daily, codex);
            }
        }

        public static void RefreshSaveBindings()
        {
            instance?.BindPersistentSaves();
        }
    }
}
