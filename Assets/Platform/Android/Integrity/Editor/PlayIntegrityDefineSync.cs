#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace VXMonster.Platform.Integrity.Editor
{
    [InitializeOnLoad]
    public static class PlayIntegrityDefineSync
    {
        private const string Define = "PLAY_INTEGRITY_AVAILABLE";
        private static readonly string ManifestPath = Path.Combine("Packages", "manifest.json");

        static PlayIntegrityDefineSync()
        {
            EditorApplication.delayCall += SyncDefine;
        }

        private static void SyncDefine()
        {
            var packagePresent = File.Exists(ManifestPath)
                && File.ReadAllText(ManifestPath).Contains("com.google.play.integrity");

            var groups = new[] { NamedBuildTarget.Standalone, NamedBuildTarget.Android, NamedBuildTarget.iOS };

            foreach (var group in groups)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(group)
                    .Split(';')
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();

                var hasDefine = defines.Contains(Define);

                if (packagePresent && !hasDefine)
                {
                    defines.Add(Define);
                    PlayerSettings.SetScriptingDefineSymbols(group, string.Join(";", defines));
                }
                else if (!packagePresent && hasDefine)
                {
                    defines.Remove(Define);
                    PlayerSettings.SetScriptingDefineSymbols(group, string.Join(";", defines));
                }
            }
        }
    }
}
#endif
