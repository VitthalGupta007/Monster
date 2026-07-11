#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace VXMonster.Platform.PlayGames.Editor
{
    [InitializeOnLoad]
    public static class GooglePlayGamesDefineSync
    {
        private const string Define = "GOOGLE_PLAY_GAMES_AVAILABLE";
        private static readonly string PluginMarker = Path.Combine("Assets", "GooglePlayGames", "com.google.play.games");

        static GooglePlayGamesDefineSync()
        {
            EditorApplication.delayCall += SyncDefine;
        }

        private static void SyncDefine()
        {
            var pluginPresent = Directory.Exists(PluginMarker);
            var groups = new[] { NamedBuildTarget.Standalone, NamedBuildTarget.Android, NamedBuildTarget.iOS };

            foreach (var group in groups)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(group)
                    .Split(';')
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();

                var hasDefine = defines.Contains(Define);

                if (pluginPresent && !hasDefine)
                {
                    defines.Add(Define);
                    PlayerSettings.SetScriptingDefineSymbols(group, string.Join(";", defines));
                }
                else if (!pluginPresent && hasDefine)
                {
                    defines.Remove(Define);
                    PlayerSettings.SetScriptingDefineSymbols(group, string.Join(";", defines));
                }
            }
        }
    }
}
#endif
