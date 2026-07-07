namespace VXMonster.Platform.iOS
{
    /// <summary>
    /// Placeholder for iOS ATT / privacy prompt integration (Phase 8).
    /// </summary>
    public static class IOSAttConsentStub
    {
        public static bool CanRequestAds => true;

        public static void RequestTrackingAuthorization(System.Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }
}
