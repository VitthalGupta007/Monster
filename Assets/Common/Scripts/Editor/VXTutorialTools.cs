#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VXMonster.Core;
using VXMonster.Save;
using VXMonster.Tutorial;
using VXMonster.UI;

namespace VXMonster.EditorTools
{
    public static class VXTutorialTools
    {
        const string SequenceFolder = "Assets/Common/Scriptables/Tutorial";

        [MenuItem("VX Monster/Tutorial/Create Default Sequences")]
        public static void CreateDefaultSequences()
        {
            if (!AssetDatabase.IsValidFolder(SequenceFolder))
            {
                AssetDatabase.CreateFolder("Assets/Common/Scriptables", "Tutorial");
            }

            var lobby = ScriptableObject.CreateInstance<TutorialSequenceDefinition>();
            lobby.sequenceId = "LobbyIntro";
            lobby.steps = new[]
            {
                new TutorialStepData { stepId = TutorialCopy.LobbyStepWelcomeId, title = TutorialCopy.LobbyWelcomeTitle, body = TutorialCopy.LobbyWelcomeBody },
                new TutorialStepData { stepId = TutorialCopy.LobbyStepModesId, title = TutorialCopy.LobbyModesTitle, body = TutorialCopy.LobbyModesBody }
            };
            AssetDatabase.CreateAsset(lobby, SequenceFolder + "/LobbyIntro.asset");

            var howToPlay = ScriptableObject.CreateInstance<TutorialSequenceDefinition>();
            howToPlay.sequenceId = "HowToPlay";
            howToPlay.steps = new[]
            {
                new TutorialStepData { stepId = "howtoplay", title = TutorialCopy.HowToPlayTitle, body = TutorialCopy.HowToPlayBody }
            };
            AssetDatabase.CreateAsset(howToPlay, SequenceFolder + "/HowToPlay.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("[VX] Created LobbyIntro and HowToPlay tutorial sequences.");
        }

        [MenuItem("VX Monster/Tutorial/Reset Lobby Tutorial Save")]
        public static void ResetLobbyTutorialSave()
        {
            if (GameController.SaveManager == null)
            {
                Debug.LogWarning("[VX] SaveManager is not available. Enter Play Mode first.");
                return;
            }

            var save = GameController.SaveManager.GetSave<TutorialSave>(TutorialSave.SaveHash);
            save.ResetLobby();
            GameController.SaveManager.Save(false);
            Debug.Log("[VX] Lobby tutorial save reset.");
        }

        [MenuItem("VX Monster/Tutorial/Show Lobby Tutorial Now")]
        public static void ShowLobbyTutorialNow()
        {
            var overlay = Object.FindAnyObjectByType<TutorialOverlayBehavior>(FindObjectsInactive.Include);
            if (overlay == null)
            {
                Debug.LogWarning("[VX] TutorialOverlayBehavior not found.");
                return;
            }

            overlay.ReplayLobby();
        }
    }
}
#endif
