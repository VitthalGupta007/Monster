using UnityEngine;

namespace VXMonster.Platform.Analytics
{
    public class MockAnalyticsService : IAnalyticsService
    {
        public bool IsReady => true;

        public void Initialize() { }

        public void LogEvent(string eventName, params (string key, object value)[] parameters)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (parameters == null || parameters.Length == 0)
            {
                Debug.Log($"[Analytics] {eventName}");
                return;
            }

            var detail = eventName;
            for (var i = 0; i < parameters.Length; i++)
            {
                detail += $" {parameters[i].key}={parameters[i].value}";
            }

            Debug.Log($"[Analytics] {detail}");
#endif
        }
    }
}
