using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.Core.UI
{
    /// <summary>
    /// Mirrors CharactersSave.SelectedCharacterId onto the lobby Characters-button portrait.
    /// Uses CharactersDatabase lookup by save id — no hard-coded class names.
    /// </summary>
    [DefaultExecutionOrder(120)]
    public class VXLobbySelectedCharacterIcon : MonoBehaviour
    {
        [SerializeField] CharactersDatabase charactersDatabase;
        [SerializeField] Image targetImage;

        CharactersSave charactersSave;

        void Start()
        {
            if (GameController.SaveManager == null) return;

            charactersSave = GameController.SaveManager.GetSave<CharactersSave>("Characters");
            if (charactersDatabase == null)
                charactersDatabase = ResolveDatabase();

            if (targetImage == null)
                targetImage = ResolveTargetImage();

            if (charactersSave != null)
                charactersSave.onSelectedCharacterChanged += Refresh;

            Refresh();
        }

        void OnDestroy()
        {
            if (charactersSave != null)
                charactersSave.onSelectedCharacterChanged -= Refresh;
        }

        void Refresh()
        {
            if (targetImage == null || charactersDatabase == null || charactersSave == null) return;

            var data = charactersDatabase.GetCharacterData(charactersSave.SelectedCharacterId);
            if (data == null && charactersDatabase.CharactersCount > 0)
                data = charactersDatabase.GetCharacterData(0);

            if (data?.Icon == null) return;

            targetImage.sprite = data.Icon;
            targetImage.preserveAspect = true;
        }

        static CharactersDatabase ResolveDatabase()
        {
            var windows = FindObjectsByType<CharactersWindowBehavior>(FindObjectsInactive.Include);
            var field = typeof(CharactersWindowBehavior).GetField("database",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) return null;

            foreach (var w in windows)
            {
                if (field.GetValue(w) is CharactersDatabase db)
                    return db;
            }

            return null;
        }

        Image ResolveTargetImage()
        {
            var lobby = GetComponentInParent<LobbyWindowBehavior>()
                        ?? FindAnyObjectByType<LobbyWindowBehavior>();
            if (lobby == null) return null;

            foreach (var t in lobby.GetComponentsInChildren<Transform>(true))
            {
                if (t.name != "Character Icon") continue;
                if (t.TryGetComponent<Image>(out var img)) return img;
            }

            return null;
        }
    }
}
