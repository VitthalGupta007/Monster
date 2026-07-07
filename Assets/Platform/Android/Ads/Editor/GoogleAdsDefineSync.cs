#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace VXMonster.Platform.Ads.Editor
{
    /// <summary>
    /// Enables GOOGLE_MOBILE_ADS_AVAILABLE when com.google.ads.mobile is resolved in packages-lock.json.
    /// </summary>
    [InitializeOnLoad]
    public static class GoogleAdsDefineSync
    {
        private const string Define = "GOOGLE_MOBILE_ADS_AVAILABLE";
        private const string PackageId = "com.google.ads.mobile";
        private static readonly string LockPath = Path.Combine("Packages", "packages-lock.json");

        static GoogleAdsDefineSync()
        {
            EditorApplication.delayCall += SyncDefine;
        }

        private static void SyncDefine()
        {
            var packageResolved = IsPackageResolved();
            var groups = new[]
            {
                NamedBuildTarget.Standalone,
                NamedBuildTarget.Android,
                NamedBuildTarget.iOS
            };

            foreach (var group in groups)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(group)
                    .Split(';')
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();

                var hasDefine = defines.Contains(Define);

                if (packageResolved && !hasDefine)
                {
                    defines.Add(Define);
                    PlayerSettings.SetScriptingDefineSymbols(group, string.Join(";", defines));
                }
                else if (!packageResolved && hasDefine)
                {
                    defines.Remove(Define);
                    PlayerSettings.SetScriptingDefineSymbols(group, string.Join(";", defines));
                }
            }
        }

        private static bool IsPackageResolved()
        {
            var fullPath = Path.GetFullPath(LockPath);
            if (!File.Exists(fullPath)) return false;

            var text = File.ReadAllText(fullPath);
            return text.Contains($"\"{PackageId}\":");
        }
    }
}
#endif
