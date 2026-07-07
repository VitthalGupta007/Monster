#if UNITY_FIREBASE
using Firebase;
using Firebase.Analytics;
using UnityEngine;

namespace VXMonster.Platform.Analytics
{
    public class FirebaseAnalyticsService : IAnalyticsService
    {
        public bool IsReady { get; private set; }

        public void Initialize()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    IsReady = true;
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                }
                else
                {
                    Debug.LogWarning($"[Firebase] Dependencies unavailable: {task.Result}");
                }
            });
        }

        public void LogEvent(string eventName, params (string key, object value)[] parameters)
        {
            if (!IsReady) return;

            if (parameters == null || parameters.Length == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var args = new Parameter[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var value = parameters[i].value;
                args[i] = value switch
                {
                    int intVal => new Parameter(parameters[i].key, intVal),
                    long longVal => new Parameter(parameters[i].key, longVal),
                    double doubleVal => new Parameter(parameters[i].key, doubleVal),
                    float floatVal => new Parameter(parameters[i].key, floatVal),
                    _ => new Parameter(parameters[i].key, value?.ToString() ?? string.Empty)
                };
            }

            FirebaseAnalytics.LogEvent(eventName, args);
        }
    }
}
#endif
