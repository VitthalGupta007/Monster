using VXMonster.Core.Currency;
using System;
using UnityEngine;

namespace VXMonster.Core
{
    [System.Serializable]
    public class CharacterData
    {
        [SerializeField] protected string id;
        public string Id => id;

        [SerializeField] protected string name;
        public string Name => name;

        [Obsolete("Migrating from cost to price")]
        [SerializeField] protected int cost;
        [Obsolete("Migrating from cost to price")]
        public int Cost => cost;

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

        public Price Price { get; protected set; }

        public CharacterData(string id, string name, Price price, Sprite icon, GameObject prefab, bool hasStartingAbility, AbilityType startingAbility, float baseHP, float baseDamage)
        {
            this.id = id;
            this.name = name;
            Price = price;
            this.icon = icon;
            this.prefab = prefab;
            this.hasStartingAbility = hasStartingAbility;
            this.startingAbility = startingAbility;
            this.baseHP = baseHP;
            this.baseDamage = baseDamage;
        }
    }
}