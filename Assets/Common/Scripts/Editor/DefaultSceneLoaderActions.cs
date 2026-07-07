using UnityEditor;

namespace OctoberStudio
{
    public static class DefaultSceneLoaderActions
    {
        private const string MenuPath = "Tools/October/Default Scene Loader Enabled";
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
                    "Disabling this option may break the asset if used incorrectly.\n\n" +
                    "When you press Play without this option turned on, the Unity will play the currently open scene, not the Main Menu scene.\n" +
                    "The Main Menu scene contains most of the systems necessary for the game to function.\n" +
                    "Use it only to test things that do not relly on the 'The Archer - Roguelike' asset to work",
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