using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core;

namespace VXMonster.UI
{
    public class LoadingScreenBehavior : MonoBehaviour
    {
        [SerializeField] Slider progressBar;
        [SerializeField] TMP_Text statusLabel;
        [SerializeField] TMP_Text percentLabel;

        private void OnEnable()
        {
            LoadingProgressReporter.OnProgressChanged += OnProgress;
            LoadingProgressReporter.OnStatusChanged += OnStatus;
            OnProgress(LoadingProgressReporter.Progress);
            OnStatus(LoadingProgressReporter.Status);
        }

        private void OnDisable()
        {
            LoadingProgressReporter.OnProgressChanged -= OnProgress;
            LoadingProgressReporter.OnStatusChanged -= OnStatus;
        }

        private void OnProgress(float value)
        {
            if (progressBar != null) progressBar.value = value;
            if (percentLabel != null) percentLabel.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void OnStatus(string status)
        {
            if (statusLabel != null) statusLabel.text = status;
        }
    }

    public static class LoadingProgressReporter
    {
        public static float Progress { get; private set; }
        public static string Status { get; private set; } = "Loading...";

        public static event System.Action<float> OnProgressChanged;
        public static event System.Action<string> OnStatusChanged;

        public static void Report(float progress, string status = null)
        {
            Progress = Mathf.Clamp01(progress);
            if (!string.IsNullOrEmpty(status)) Status = status;
            OnProgressChanged?.Invoke(Progress);
            if (!string.IsNullOrEmpty(status)) OnStatusChanged?.Invoke(Status);
        }

        public static void Reset()
        {
            Report(0f, "Loading...");
        }
    }
}
