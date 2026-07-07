using OctoberStudio.Abilities;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.Timeline.Bossfight;
using OctoberStudio.UI;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio
{
    public class StageController : MonoBehaviour
    {
        private static StageController instance;

        [SerializeField] StagesDatabase database;
        [SerializeField] PlayableDirector director;
        [SerializeField] EnemiesSpawner spawner;
        [SerializeField] StageFieldManager fieldManager;
        [SerializeField] ExperienceManager experienceManager;
        [SerializeField] DropManager dropManager;
        [SerializeField] AbilityManager abilityManager;
        [SerializeField] PoolsManager poolsManager;
        [SerializeField] WorldSpaceTextManager worldSpaceTextManager;
        [SerializeField] CameraManager cameraManager;

        public static EnemiesSpawner EnemiesSpawner => instance.spawner;
        public static ExperienceManager ExperienceManager => instance.experienceManager;
        public static AbilityManager AbilityManager => instance.abilityManager;
        public static StageFieldManager FieldManager => instance.fieldManager;
        public static PlayableDirector Director => instance.director;
        public static PoolsManager PoolsManager => instance.poolsManager;
        public static WorldSpaceTextManager WorldSpaceTextManager => instance.worldSpaceTextManager;
        public static CameraManager CameraController => instance.cameraManager;
        public static DropManager DropManager => instance.dropManager;

        [Header("UI")]
        [SerializeField] GameScreenBehavior gameScreen;
        [SerializeField] StageFailedScreen stageFailedScreen;
        [SerializeField] StageCompleteScreen stageCompletedScreen;

        [Header("Testing")]
        [SerializeField] PresetData testingPreset;

        public static GameScreenBehavior GameScreen => instance.gameScreen;

        public static StageData Stage { get; private set; }

        public static bool IsLoaded => instance != null;

        private StageSave stageSave;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
            Stage = null;
        }

        private void Awake()
        {
            instance = this;

            stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
        }

        private void Start()
        {
            Stage = database.GetStage(stageSave.SelectedStageId);

            director.playableAsset = Stage.Timeline;

            spawner.Init(director);
            experienceManager.Init(testingPreset);
            dropManager.Init();
            fieldManager.Init(Stage, director);
            abilityManager.Init(testingPreset, PlayerBehavior.Player.Data);
            cameraManager.Init(Stage);

            PlayerBehavior.Player.onPlayerDied += OnGameFailed;

            director.stopped += TimelineStopped;
            if (testingPreset != null) {
                director.time = testingPreset.StartTime; 
            } else
            {
                var time = stageSave.Time;

                var bossClips = director.GetClips<BossTrack, Boss>();

                for(int i = 0; i < bossClips.Count; i++)
                {
                    var bossClip = bossClips[i];

                    if(time >= bossClip.start && time <= bossClip.end)
                    {
                        time = (float) bossClip.start;
                        break;
                    }
                }

                director.time = time;
            }

            director.Play();

            if (Stage.UseCustomMusic)
            {
                GameController.ChangeMusic(Stage.MusicName);
            }
        }

        private void TimelineStopped(PlayableDirector director)
        {
            if (gameObject.activeSelf)
            {
                stageSave.CompleteStage(Stage);

                stageSave.IsPlaying = false;
                GameController.SaveManager.Save(true);

                gameScreen.Hide();
                stageCompletedScreen.Show();
                Time.timeScale = 0;
            }
        }

        private void OnGameFailed()
        {
            Time.timeScale = 0;

            stageSave.IsPlaying = false;
            GameController.SaveManager.Save(true);

            gameScreen.Hide();
            stageFailedScreen.Show();
        }

        public static void ResurrectPlayer()
        {
            EnemiesSpawner.DealDamageToAllEnemies(PlayerBehavior.Player.Damage * 1000);

            GameScreen.Show();
            PlayerBehavior.Player.Revive();
            Time.timeScale = 1;
        }

        public static void ReturnToMainMenu()
        {
            GameController.LoadMainMenu();
        }

        private void OnDisable()
        {
            director.stopped -= TimelineStopped;
        }
    }
}