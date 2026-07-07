using UnityEditor;

namespace VXMonster.Core
{
    public static class DefaultSceneLoaderActions
    {
        private const string MenuPath = "Tools/VX Monster/Default Scene Loader Enabled";
        private const string PrefKeyEnabled = "DefaultSceneLoader.Enabled";
        private const string PrefKeySkipWarning = "DefaultSceneLoader.Skip Warning";

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(PrefKeyEnabled, true);
            set => EditorPrefs.SetBool(PrefKeyEnabled, value);
        }

        private static bool SkipWarning
        {
            get => EditorPrefs.GetBool(PrefKeySkipWarning, false);
            set => EditorPrefs.SetBool(PrefKeySkipWarning, value);
        }

        [MenuItem(MenuPath)]
        private static void ToggleOption()
        {
            if (Enabled && !SkipWarning)
            {
                var result = EditorUtility.DisplayDialogComplex(
                    "Warning",
                    "Disabling this option may break the game if used incorrectly.\n\n" +
                    "When you press Play without this option turned on, Unity will play the currently open scene, not the Main Menu scene.\n" +
                    "The Main Menu scene contains most of the systems necessary for the game to function.\n" +
                    "Use it only to test isolated features that do not depend on the full game bootstrap",
                    "Disable",
                    "Cancel",
                    "Disable & Don't Show Again"
                );

                switch (result)
                {
                    case 0: // Disable
                        break;

                    case 1: // Cancel
                        return;

                    case 2: // Enable & Don't Show Again
                        SkipWarning = true;
                        break;
                }
            }
            Enabled = !Enabled;
            Menu.SetChecked(MenuPath, Enabled);

            DefaultSceneLoader.OnProjectSettingsChanged();
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleOptionValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }
    }
}