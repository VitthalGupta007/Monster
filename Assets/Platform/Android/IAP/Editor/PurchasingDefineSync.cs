#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace VXMonster.Platform.IAP.Editor
{
    [InitializeOnLoad]
    public static class PurchasingDefineSync
    {
        private const string Define = "UNITY_PURCHASING";
        private const string PackageId = "com.unity.purchasing";
        private static readonly string LockPath = Path.Combine("Packages", "packages-lock.json");

        static PurchasingDefineSync()
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
