using VXMonster.Core;
using VXMonster.Core.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using VXMonster.Gameplay;
using VXMonster.Platform;
using VXMonster.Platform.Analytics;
using VXMonster.Platform.Ads;
using VXMonster.Platform.IAP;
using VXMonster.Platform.Integrity;
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
            var go = new GameObject("VX Monster Bootstrapper");
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

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && GOOGLE_MOBILE_ADS_AVAILABLE
            adService = new AdMobService(adMobConfig);
#else
            adService = new MockAdService();
#endif

#if UNITY_FIREBASE
            AnalyticsEvents.Bind(new FirebaseAnalyticsService());
#else
            AnalyticsEvents.Bind(new MockAnalyticsService());
#endif
            AnalyticsEvents.Initialize();

            GoogleMobileAdsConsentController.GatherConsent(_ =>
            {
                if (GameController.SaveManager != null)
                {
                    PlatformServices.BindEntitlements(
                        GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements"));
                }

#if UNITY_PURCHASING
                var iapService = new UnityPurchasingService();
#else
                var iapService = new MockIapService();
#endif
#if UNITY_ANDROID && !UNITY_EDITOR && GOOGLE_PLAY_GAMES_AVAILABLE
                var playGamesService = new GooglePlayGamesService();
#else
                var playGamesService = new MockPlayGamesService();
#endif
                PlatformServices.Initialize(adMobConfig, adService, playGamesService, iapService);
                PlayIntegrityBootstrap.RequestCheckOnce();
            });

            TryShowLobbyTutorial();
        }

        private static void TryShowLobbyTutorial()
        {
            var tutorial = FindAnyObjectByType<VXMonster.UI.TutorialOverlayBehavior>();
            tutorial?.TryShowFirstRun();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (GameController.SaveManager != null)
            {
                GameController.SaveManager.OnSaveCompleted -= PlatformServices.TryPushCloudSave;
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryShowLobbyTutorial();
            PlatformServices.RefreshBannerForActiveScene();
        }

        private void Start()
        {
            BindPersistentSaves();

            if (GameController.SaveManager != null)
            {
                PlatformServices.BindEntitlements(GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements"));
                GameController.SaveManager.OnSaveCompleted += PlatformServices.TryPushCloudSave;
            }

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
                GameSessionManager.Instance.BindSaves(runSession, lifetime, daily, codex, talent);
            }
        }

        public static void RefreshSaveBindings()
        {
            instance?.BindPersistentSaves();
        }
    }
}
