#if !GOOGLE_MOBILE_ADS_AVAILABLE
namespace VXMonster.Platform.Ads
{
    /// <summary>
    /// Fallback when the Google Mobile Ads UPM package is not resolved yet.
    /// </summary>
    public sealed class AdMobService : MockAdService
    {
    }
}
#endif
