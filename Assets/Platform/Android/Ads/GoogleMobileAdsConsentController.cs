#if GOOGLE_MOBILE_ADS_AVAILABLE
using System;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace VXMonster.Platform.Ads
{
    public static class GoogleMobileAdsConsentController
    {
        public static bool CanRequestAds => ConsentInformation.CanRequestAds();

        public static bool PrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        public static void GatherConsent(Action<string> onComplete)
        {
            var request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
            };

            ConsentInformation.Update(request, (FormError updateError) =>
            {
                if (updateError != null)
                {
                    onComplete?.Invoke(updateError.Message);
                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                {
                    onComplete?.Invoke(formError != null ? formError.Message : null);
                });
            });
        }

        public static void ShowPrivacyOptionsForm(Action<string> onComplete = null)
        {
            ConsentForm.ShowPrivacyOptionsForm((FormError error) =>
            {
                onComplete?.Invoke(error != null ? error.Message : null);
            });
        }
    }
}
#else
using System;

namespace VXMonster.Platform.Ads
{
    public static class GoogleMobileAdsConsentController
    {
        public static bool CanRequestAds => true;
        public static bool PrivacyOptionsRequired => false;

        public static void GatherConsent(Action<string> onComplete)
        {
            onComplete?.Invoke(null);
        }

        public static void ShowPrivacyOptionsForm(Action<string> onComplete = null)
        {
            onComplete?.Invoke(null);
        }
    }
}
#endif
