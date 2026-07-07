using VXMonster.Core.Save;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VXMonster.Core
{
    public class CharactersSave : ISave
    {
        [Obsolete("Switching to string id from index in the database")]
        [SerializeField] protected int[] boughtCharacterIds;
        [Obsolete("Switching to string id from index in the database")]
        [SerializeField] protected int selectedCharacterId;

        [Obsolete("Switching to string id from index in the database")]
        protected List<int> BoughtCharacterIds { get; set; }

        public UnityAction onSelectedCharacterChanged;

        [SerializeField] protected string[] unlockedCharacters;
        [SerializeField] protected string selectedCharacter;

        public string SelectedCharacterId => selectedCharacter;

        protected List<string> UnlockedCharacters { get; set; }

        public virtual void Init()
        {
            if (unlockedCharacters == null)
            {
                unlockedCharacters = Array.Empty<string>();

                selectedCharacter = null;
            }
            UnlockedCharacters = new List<string>(unlockedCharacters);
        }

        [Obsolete("Use HasCharacterBeenBought(string)")]
        public virtual bool HasCharacterBeenBought(int id)
        {
            return false;
        }

        public virtual bool HasCharacterBeenBought(string id)
        {
            if (UnlockedCharacters == null) Init();

            return UnlockedCharacters.Contains(id);
        }

        [Obsolete("Use AddBoughtCharacter(string)")]
        public virtual void AddBoughtCharacter(int id)
        {

        }

        public virtual void AddBoughtCharacter(string id)
        {
            if (UnlockedCharacters == null) Init();

            UnlockedCharacters.Add(id);
        }

        [Obsolete("Use SetSelectedCharacterId(string)")]
        public virtual void SetSelectedCharacterId(int id)
        {

        }

        public virtual void SetSelectedCharacterId(string id)
        {
            if (UnlockedCharacters == null) Init();

            selectedCharacter = id;

            onSelectedCharacterChanged?.Invoke();
        }

        public virtual void Flush()
        {
            if (UnlockedCharacters == null) Init();

            unlockedCharacters = UnlockedCharacters.ToArray();
        }
    }
}