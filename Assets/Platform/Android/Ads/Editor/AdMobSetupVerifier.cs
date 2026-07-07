#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using VXMonster.Platform.Ads;

namespace VXMonster.Platform.Ads.Editor
{
    public static class AdMobSetupVerifier
    {
        private const string Define = "GOOGLE_MOBILE_ADS_AVAILABLE";
        private const string PackageId = "com.google.ads.mobile";
        private const string ExpectedAndroidAppId = "ca-app-pub-8542754238593082~9088842908";

        [MenuItem("Tools/VX Monster/Verify AdMob Setup")]
        public static void RunVerification()
        {
            var report = BuildReport();
            Debug.Log(report);
            EditorUtility.DisplayDialog("AdMob Setup Verification", report, "OK");
        }

        public static string BuildReport()
        {
            var lines = new System.Collections.Generic.List<string>
            {
                "=== VX Monster AdMob Verification ===",
                string.Empty
            };

            AppendPackageChecks(lines);
            AppendDefineChecks(lines);
            AppendSettingsChecks(lines);
            AppendManifestChecks(lines);
            AppendGradleChecks(lines);
            AppendRuntimeServiceChecks(lines);
            AppendRecommendations(lines);

            return string.Join("\n", lines);
        }

        private static void AppendPackageChecks(System.Collections.Generic.List<string> lines)
        {
            var lockPath = Path.Combine("Packages", "packages-lock.json");
            var lockExists = File.Exists(lockPath);
            var packageResolved = lockExists && File.ReadAllText(lockPath).Contains($"\"{PackageId}\":");

            lines.Add("[Package]");
            lines.Add(lockExists ? "  packages-lock.json: found" : "  packages-lock.json: MISSING");
            lines.Add(packageResolved
                ? $"  {PackageId}: resolved"
                : $"  {PackageId}: NOT resolved — run Unity Package Manager resolve");

            var cacheDir = Path.Combine("Library", "PackageCache");
            var cached = Directory.Exists(cacheDir)
                && Directory.GetDirectories(cacheDir, $"{PackageId}@*").Length > 0;
            lines.Add(cached ? "  Package cache: present" : "  Package cache: missing (open project in Unity once)");
            lines.Add(string.Empty);
        }

        private static void AppendDefineChecks(System.Collections.Generic.List<string> lines)
        {
            lines.Add("[Scripting Defines]");
            foreach (var group in new[] { NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone })
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(group)
                    .Split(';')
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();

                var hasDefine = defines.Contains(Define);
                lines.Add(hasDefine
                    ? $"  {group.TargetName}: {Define} enabled"
                    : $"  {group.TargetName}: {Define} MISSING");
            }

            lines.Add(string.Empty);
        }

        private static void AppendSettingsChecks(System.Collections.Generic.List<string> lines)
        {
            lines.Add("[Google Mobile Ads Settings]");
            var settingsPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
            if (!File.Exists(settingsPath))
            {
                lines.Add("  GoogleMobileAdsSettings.asset: MISSING");
                lines.Add(string.Empty);
                return;
            }

            var text = File.ReadAllText(settingsPath);
            var androidId = ReadYamlValue(text, "adMobAndroidAppId");
            var iosId = ReadYamlValue(text, "adMobIOSAppId");

            lines.Add(string.IsNullOrWhiteSpace(androidId)
                ? "  Android App ID: EMPTY (should match AdMobConfig / manifest)"
                : $"  Android App ID: {androidId}");
            lines.Add(string.IsNullOrWhiteSpace(iosId)
                ? "  iOS App ID: EMPTY (required for iPhone builds)"
                : $"  iOS App ID: {iosId}");
            lines.Add(string.Empty);
        }

        private static void AppendManifestChecks(System.Collections.Generic.List<string> lines)
        {
            lines.Add("[Android Manifest]");
            var manifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
            if (!File.Exists(manifestPath))
            {
                lines.Add("  AndroidManifest.xml: MISSING");
                lines.Add(string.Empty);
                return;
            }

            var manifest = File.ReadAllText(manifestPath);
            lines.Add(manifest.Contains("android.permission.INTERNET")
                ? "  INTERNET permission: present"
                : "  INTERNET permission: MISSING");
            lines.Add(manifest.Contains("com.google.android.gms.ads.APPLICATION_ID")
                ? "  AdMob APPLICATION_ID meta-data: present"
                : "  AdMob APPLICATION_ID meta-data: MISSING");

            if (manifest.Contains(ExpectedAndroidAppId))
            {
                lines.Add($"  App ID matches config: {ExpectedAndroidAppId}");
            }
            else
            {
                lines.Add("  App ID in manifest does NOT match AdMobConfig default");
            }

            lines.Add(string.Empty);
        }

        private static void AppendGradleChecks(System.Collections.Generic.List<string> lines)
        {
            lines.Add("[Gradle Dependencies]");
            var gradlePath = "Assets/Plugins/Android/mainTemplate.gradle";
            if (!File.Exists(gradlePath))
            {
                lines.Add("  mainTemplate.gradle: MISSING");
                lines.Add(string.Empty);
                return;
            }

            var gradle = File.ReadAllText(gradlePath);
            lines.Add(gradle.Contains("play-services-ads")
                ? "  play-services-ads: present"
                : "  play-services-ads: MISSING");
            lines.Add(gradle.Contains("user-messaging-platform")
                ? "  user-messaging-platform (UMP): present"
                : "  user-messaging-platform: MISSING");
            lines.Add(string.Empty);
        }

        private static void AppendRuntimeServiceChecks(System.Collections.Generic.List<string> lines)
        {
            lines.Add("[Runtime Service Selection]");
#if UNITY_EDITOR
            lines.Add("  Unity Editor Play Mode: MockAdService (real ads do NOT load in Editor)");
#else
            lines.Add("  Device build: AdMobService when GOOGLE_MOBILE_ADS_AVAILABLE is set");
#endif

#if DEVELOPMENT_BUILD
            lines.Add("  This build: DEVELOPMENT_BUILD -> Google TEST ad unit IDs");
#else
            lines.Add("  This build: RELEASE -> PRODUCTION ad unit IDs from AdMobConfig");
            lines.Add("  If production units are new/unapproved, ads may fail to load.");
#endif

            lines.Add(string.Empty);
            lines.Add("[On Device — filter logcat / Xcode for:]");
            lines.Add("  [VX Ads] AdMob SDK initialized.");
            lines.Add("  [VX Ads] Rewarded ad loaded.");
            lines.Add("  [VX Ads] Rewarded load failed: ...");
            lines.Add(string.Empty);
        }

        private static void AppendRecommendations(System.Collections.Generic.List<string> lines)
        {
            lines.Add("[Notes]");
            lines.Add("  • Editor Play Mode cannot load real AdMob ads.");
            lines.Add("  • Build to Android/iOS device and check [VX Ads] logs.");
            lines.Add("  • For testing, enable Development Build in Build Settings.");
            lines.Add("  • No GDPR/UMP consent flow is implemented yet.");
        }

        private static string ReadYamlValue(string yaml, string key)
        {
            foreach (var rawLine in yaml.Split('\n'))
            {
                var line = rawLine.Trim();
                if (!line.StartsWith(key + ":", System.StringComparison.Ordinal)) continue;
                return line.Substring(key.Length + 1).Trim();
            }

            return string.Empty;
        }
    }
}
#endif
