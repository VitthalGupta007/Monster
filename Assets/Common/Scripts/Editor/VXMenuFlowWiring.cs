#if UNITY_EDITOR
using UnityEditor;

namespace VXMonster.EditorTools
{
    /// <summary>
    /// Rebuilds Shop/Talent/Codex modals using the classic VXPrefabHookupWiring layout.
    /// MENU sheet is prefab-built in Lobby Window via Wire Lobby Hub.
    /// </summary>
    public static class VXMenuFlowWiring
    {
        [MenuItem("VX Monster/Rewire Menu Flow Modals (Classic)")]
        public static void RewireMenuFlowModalsMenu()
        {
            RewireMenuFlowModals();
        }

        public static void RewireMenuFlowModals()
        {
            VXPrefabHookupWiring.ForceRewireMainMenuModals();
        }
    }
}
#endif
