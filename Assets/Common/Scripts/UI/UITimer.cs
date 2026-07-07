using OctoberStudio.Easing;
using System;
using TMPro;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class UITimer : MonoBehaviour
    {
        [SerializeField] protected TMP_Text timerText;

        protected IEasingCoroutine alphaCoroutine;

        protected StageSave stageSave;

        protected int lastSeconds = -1;

        private void Awake()
        {
            stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
        }

        private void Update()
        {
            var timespan = TimeSpan.FromSeconds(StageController.Director.time);
            if(timespan.Seconds != lastSeconds)
            {
                lastSeconds = timespan.Seconds;

                timerText.text = string.Format("{0:mm\\:ss}", timespan);

                stageSave.Time = (float)StageController.Director.time;
            }
        }

        public void Show()
        {
            alphaCoroutine.StopIfExists();

            gameObject.SetActive(true);
            alphaCoroutine = timerText.DoAlpha(1, 0.3f);
        }

        public void Hide()
        {
            alphaCoroutine.StopIfExists();

            timerText.DoAlpha(0, 0.3f).SetOnFinish(() => gameObject.SetActive(false));
        }
    }
}