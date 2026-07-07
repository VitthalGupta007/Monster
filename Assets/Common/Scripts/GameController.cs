using VXMonster.Core.Currency;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using VXMonster.Platform;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VXMonster.Core
{
    using VXMonster.Core.Audio;
    using VXMonster.Core.Easing;
    using VXMonster.Core.Input;
    using Save;
    using Upgrades;
    using Vibration;

    public class GameController : MonoBehaviour
    {
        protected static readonly string MAIN_MENU_MUSIC_NAME = "Main Menu Music";

        private static GameController instance;

        [SerializeField] protected CurrenciesManager currenciesManager;
        public static CurrenciesManager CurrenciesManager => instance.currenciesManager;

        [SerializeField] protected UpgradesManager upgradesManager;
        public static UpgradesManager UpgradesManager => instance.upgradesManager;

        [SerializeField] protected ProjectSettings projectSettings;
        public static ProjectSettings ProjectSettings => instance.projectSettings;

        public static ISaveManager SaveManager { get; private set; }
        public static IAudioManager AudioManager { get; private set; }
        public static IVibrationManager VibrationManager { get; private set; }
        public static IInputManager InputManager { get; private set; }

        public static AudioSource Music { get; private set; }

        private static StageSave stageSave;

        // Indicates that the main menu is just loaded, and not exited from the game scene
        public static bool FirstTimeLoaded { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;

            SaveManager = null;
            AudioManager = null;
            VibrationManager = null;
            InputManager = null;
            Music = null;
            stageSave = null;
            FirstTimeLoaded = true;
        }

        protected virtual void Awake()
        {
            if (instance != null)
            {
                Destroy(this);

                FirstTimeLoaded = false;

                return;
            }

            instance = this;

            FirstTimeLoaded = true;

            currenciesManager.Init();

            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 120;
        }

        protected virtual void Start()
        {
            stageSave = SaveManager.GetSave<StageSave>("Stage");
            stageSave.Init();
            if (!stageSave.loadedBefore)
            {
                stageSave.loadedBefore = true;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            InputManager.InputAsset.UI.Click.performed += MusicStartWebGL;
#else

            EasingManager.DoAfter(0.2f, () => Music = AudioManager.PlayMusic(MAIN_MENU_MUSIC_NAME.GetHashCode()));
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        protected virtual void MusicStartWebGL(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            InputManager.InputAsset.UI.Click.performed -= MusicStartWebGL;

            Music = AudioManager.PlayMusic(MAIN_MENU_MUSIC_NAME.GetHashCode());
        }
#endif
        public static void ChangeMusic(string musicName)
        {
            if (Music != null)
            {
                var oldMusic = Music;
                oldMusic.DoVolume(0, 0.3f).SetOnFinish(() => oldMusic.Stop());
            }

            Music = AudioManager.PlayMusic(musicName.GetHashCode());

            if(Music != null)
            {
                var volume = Music.volume;
                Music.volume = 0;
                Music.DoVolume(volume, 0.3f);
            }
        }

        public static void ChangeMusic(SoundContainer music)
        {
            if (Music != null)
            {
                var oldMusic = Music;
                oldMusic.DoVolume(0, 0.3f).SetOnFinish(() => oldMusic.Stop());
            }

            Music = music.Play(true);

            if (Music != null)
            {
                var volume = Music.volume;
                Music.volume = 0;
                Music.DoVolume(volume, 0.3f);
            }
        }

        public static void RegisterInputManager(IInputManager inputManager)
        {
            InputManager = inputManager;
        }

        public static void RegisterSaveManager(ISaveManager saveManager)
        {
            SaveManager = saveManager;
        }

        public static void RegisterVibrationManager(IVibrationManager vibrationManager)
        {
            VibrationManager = vibrationManager;
        }

        public static void RegisterAudioManager(IAudioManager audioManager)
        {
            AudioManager = audioManager;
        }

        public static void LoadStage()
        {
            if (stageSave.ResetStageData) CurrenciesManager.EraseGameplayAmounts();

            instance.StartCoroutine(StageLoadingCoroutine());

            SaveManager.Save(false);
        }

        public static void LoadMainMenu()
        {
            CurrenciesManager.ApplyGameplayAmounts();

            if (instance != null) instance.StartCoroutine(MainMenuLoadingCoroutine());

            SaveManager.Save(false);
        }

        protected static string GetLoadingScreenSceneName()
        {
            if (ProjectSettings != null && SceneExists(ProjectSettings.LoadingSceneName))
            {
                return ProjectSettings.LoadingSceneName;
            }
            else if (SceneExists("Loading Screen"))
            {
                return "Loading Screen";
            }
            else
            {
                Debug.LogWarning("Loading screen scene not found. Please add a loading screen scene to the project settings or ensure it exists in the build settings.");
                return null;
            }
        }

        protected static string GetMainMenuSceneName()
        {
            if (ProjectSettings != null && SceneExists(ProjectSettings.MainMenuSceneName))
            {
                return ProjectSettings.MainMenuSceneName;
            }
            else if (SceneExists("Main Menu"))
            {
                return "Main Menu";
            }
            else
            {
                Debug.LogError("Main menu scene not found. Please add a main menu scene to the project settings or ensure it exists in the build settings.");
                return null;
            }
        }

        protected static string GetGameSceneName()
        {
            if (ProjectSettings != null && SceneExists(ProjectSettings.GameSceneName))
            {
                return ProjectSettings.GameSceneName;
            }
            else if (SceneExists("Game"))
            {
                return "Game";
            }
            else
            {
                Debug.LogError("Game scene not found. Please add a game scene to the project settings or ensure it exists in the build settings.");
                return null;
            }
        }

        protected static IEnumerator StageLoadingCoroutine()
        {
            var loadingSceneName = GetLoadingScreenSceneName();
            var mainMenuSceneName = GetMainMenuSceneName();
            var gameSceneName = GetGameSceneName();

            if (loadingSceneName != null)
            {
                yield return LoadAsyncScene(loadingSceneName, LoadSceneMode.Additive);
            }

            yield return UnloadAsyncScene(mainMenuSceneName);
            yield return LoadAsyncScene(gameSceneName, LoadSceneMode.Single);
        }

        protected static IEnumerator MainMenuLoadingCoroutine()
        {
            var loadingSceneName = GetLoadingScreenSceneName();
            var mainMenuSceneName = GetMainMenuSceneName();
            var gameSceneName = GetGameSceneName();

            if (loadingSceneName != null)
            {
                yield return LoadAsyncScene(loadingSceneName, LoadSceneMode.Additive);
            }

            yield return UnloadAsyncScene(gameSceneName);
            yield return LoadAsyncScene(mainMenuSceneName, LoadSceneMode.Single);

            if (StageController.Stage.UseCustomMusic)
            {
                ChangeMusic(MAIN_MENU_MUSIC_NAME);
            }
        }

        protected static bool SceneExists(string sceneName)
        {
#if UNITY_EDITOR
            return SceneExistsInAssets(sceneName);
#else
            return SceneExistsInBuildSettings(sceneName);
#endif
        }

        protected static bool SceneExistsInBuildSettings(string sceneName)
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < sceneCount; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName)
                    return true;
            }

            return false;
        }

#if UNITY_EDITOR

        public static bool SceneExistsInAssets(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            return guids.Any(guid =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return System.IO.Path.GetFileNameWithoutExtension(path) == sceneName;
            });
        }

#endif

        protected static IEnumerator UnloadAsyncScene(string sceneName)
        {
            var asyncLoad = SceneManager.UnloadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            //wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        protected static IEnumerator LoadAsyncScene(string sceneName, LoadSceneMode loadSceneMode)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            asyncLoad.allowSceneActivation = false;
            //wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                //scene has loaded as much as possible,
                // the last 10% can't be multi-threaded
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }
        }

        private void OnApplicationFocus(bool focus)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (focus) { 
                EasingManager.DoAfter(0.1f, () => { 
                    if (!Music.isPlaying)
                    {
                        Music = AudioManager.AudioDatabase.Music.Play(true);
                    }
                });
            } 
#endif
            if (focus && !StageController.IsLoaded)
            {
                PlatformServices.TryShowAppOpen(null);
            }
        }
    }
}