#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace VXMonster.Platform.Analytics.Editor
{
    [InitializeOnLoad]
    public static class FirebaseDefineSync
    {
        private const string Define = "UNITY_FIREBASE";
        private const string PackageMarker = "Firebase.Analytics";
        private static readonly string PluginsPath = Path.Combine("Assets", "Plugins", "Android", "google-services.json");

        static FirebaseDefineSync()
        {
            EditorApplication.delayCall += SyncDefine;
        }

        private static void SyncDefine()
        {
            var firebasePresent = File.Exists(PluginsPath);
            var groups = new[] { NamedBuildTarget.Standalone, NamedBuildTarget.Android, NamedBuildTarget.iOS };

            foreach (var group in groups)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(group)
                    .Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
                var hasDefine = defines.Contains(Define);

                if (firebasePresent && !hasDefine)
                {
                    defines.Add(Define);
                    PlayerSettings.SetScriptingDefineSymbols(group, string.Join(";", defines));
                }
                else if (!firebasePresent && hasDefine)
                {
                    defines.Remove(Define);
                    PlayerSettings.SetScriptingDefineSymbols(group, string.Join(";", defines));
                }
            }
        }
    }
}
#endif
