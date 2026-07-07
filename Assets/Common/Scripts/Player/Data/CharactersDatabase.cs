using System;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    [CreateAssetMenu(fileName = "Characters Database", menuName = "VX Monster/Character/Characters Database")]
    public class CharactersDatabase : ScriptableObject
    {
        [Obsolete("Use characterDatas")]
        [SerializeField, HideInInspector] protected List<CharacterData> characters = new List<CharacterData>();

        [Obsolete("Use CharactersCount")]
        public int CharactersCountOld => characters.Count;

        [SerializeField] protected List<CharacterDataSO> characterDatas;
        public int CharactersCount => characterDatas.Count;

        public virtual CharacterData GetCharacterDataOld(int index)
        {
#pragma warning disable CS0618 // Migration from obsolete
            return characters[index];
#pragma warning restore CS0618
        }

        public virtual CharacterData GetCharacterData(int index)
        {
            return characterDatas[index].GetCharacterData();
        }

        public virtual CharacterData GetCharacterData(string id)
        {
            for (int i = 0; i < characterDatas.Count; i++)
            {
                if (characterDatas[i].Id == id) return characterDatas[i].GetCharacterData();
            }
            return null;
        }

#if UNITY_EDITOR
        public virtual void AddCharacterDataSO(CharacterDataSO characterDataSO)
        {
            if(characterDatas == null) characterDatas = new List<CharacterDataSO>();
            characterDatas.Add(characterDataSO);
        }
#endif
    }
}