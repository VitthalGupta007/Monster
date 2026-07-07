namespace VXMonster.Platform.Analytics
{
    public interface IAnalyticsService
    {
        bool IsReady { get; }
        void Initialize();
        void LogEvent(string eventName, params (string key, object value)[] parameters);
    }
}
