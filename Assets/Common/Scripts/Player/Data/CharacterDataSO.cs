using VXMonster.Core.Currency;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace VXMonster.Core
{
    [CreateAssetMenu(menuName = "VX Monster/Character/Data", fileName = "Character Data")]
    public class CharacterDataSO : ScriptableObject
    {
        [SerializeField] protected Id id;
        public string Id => id;

        [SerializeField] protected string characterName;
        public string Name => characterName;

        [SerializeField] protected Price price;
        public Price Price => price;

        [SerializeField] protected Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] protected GameObject prefab;
        public GameObject Prefab => prefab;

        [Space]
        [SerializeField] protected bool hasStartingAbility = false;
        public bool HasStartingAbility => hasStartingAbility;

        [SerializeField] protected AbilityType startingAbility;
        public AbilityType StartingAbility => startingAbility;

        [Space]
        [SerializeField, Min(1)] protected float baseHP;
        public float BaseHP => baseHP;

        [SerializeField, Min(1f)] protected float baseDamage;
        public float BaseDamage => baseDamage;
    
        // Keeping Character Data for backward compatability
        public CharacterData GetCharacterData()
        {
            return new CharacterData(id, characterName, price, icon, prefab, hasStartingAbility, startingAbility, baseHP, baseDamage);
        }

#if UNITY_EDITOR

        public virtual void CloneFromSerializedProperty(SerializedProperty characterProperty, string currencyId)
        {
            var nameProperty = characterProperty.FindPropertyRelative("name");
            var costProperty = characterProperty.FindPropertyRelative("cost");
            var iconProperty = characterProperty.FindPropertyRelative("icon");
            var prefabProperty = characterProperty.FindPropertyRelative("prefab");
            var hasStartingAbilityProperty = characterProperty.FindPropertyRelative("hasStartingAbility");
            var startingAbilityProperty = characterProperty.FindPropertyRelative("startingAbility");
            var baseHPProperty = characterProperty.FindPropertyRelative("prebaseHPfab");
            var baseDamageProperty = characterProperty.FindPropertyRelative("baseDamage");

            characterName = nameProperty != null ? nameProperty.stringValue : $"Character";
            price = costProperty != null ? new Price(currencyId, costProperty.intValue) : new Price(currencyId, 0);
            icon = iconProperty != null ? iconProperty.objectReferenceValue as Sprite : null;
            prefab = prefabProperty != null ? prefabProperty.objectReferenceValue as GameObject : null;
            hasStartingAbility = hasStartingAbilityProperty != null ? hasStartingAbilityProperty.boolValue : false;
            startingAbility = startingAbilityProperty != null ? (AbilityType)startingAbilityProperty.intValue : 0;
            baseHP = baseHPProperty != null ? baseHPProperty.floatValue : 100f;
            baseDamage = baseDamageProperty != null ? baseDamageProperty.floatValue : 1f;
        }

#endif

    }
}