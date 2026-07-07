using UnityEngine;
using VXMonster.Core;

namespace VXMonster.Gameplay
{
    public class MidRunSaveController : MonoBehaviour
    {
        [SerializeField] float autoSaveIntervalSeconds = 30f;

        private float timer;
        private ExperienceManager experienceManager;

        private void Start()
        {
            experienceManager = StageController.ExperienceManager;
            if (experienceManager != null)
            {
                experienceManager.onXpLevelChanged += OnLevelUp;
            }
        }

        private void OnDestroy()
        {
            if (experienceManager != null)
            {
                experienceManager.onXpLevelChanged -= OnLevelUp;
            }
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer < autoSaveIntervalSeconds) return;
            timer = 0f;
            PersistRun();
        }

        private void OnLevelUp(int _)
        {
            PersistRun();
        }

        private static void PersistRun()
        {
            if (GameController.SaveManager == null) return;
            var stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
            if (stageSave == null || !stageSave.IsPlaying) return;

            GameController.SaveManager.Save(true);
        }
    }
}
