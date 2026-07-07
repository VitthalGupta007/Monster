using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(menuName = "October/Enemies Database", fileName = "Enemies Database")]
    public class EnemiesDatabase : ScriptableObject
    {
        [SerializeField] List<EnemyData> enemies;

        public int EnemiesCount => enemies.Count;

        public EnemyData GetEnemyData(int index)
        {
            return enemies[index];
        }

        public EnemyData GetEnemyData(EnemyType type)
        {
            for(int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Type == type) return enemies[i];
            }

            return null;
        }

        public Dictionary<EnemyType, EnemyData> GetEnemyDataDictionary()
        {
            var dictionary = new Dictionary<EnemyType, EnemyData>();

            for(int i = 0; i < enemies.Count; i++)
            {
                if (!dictionary.ContainsKey(enemies[i].Type))
                {
                    dictionary.Add(enemies[i].Type, enemies[i]);
                }
            }

            return dictionary;
        }
    }

    [System.Serializable]
    public class EnemyData
    {
        [SerializeField] EnemyType type;
        [SerializeField] GameObject prefab;
        [SerializeField] Sprite icon;
        [SerializeField] List<EnemyDropData> enemyDrop;

        public EnemyType Type => type;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
        public List<EnemyDropData> EnemyDrop => enemyDrop;
    }

    [System.Serializable]
    public class EnemyDropData
    {
        [SerializeField] DropType dropType;
        [SerializeField, Range(0, 100)] float chance;

        public DropType DropType => dropType;
        public float Chance => chance;
    }

    public enum EnemyType
    {
        Pumpkin = 0,
        Bat = 1,
        Slime = 2,
        Vampire = 3,
        Plant = 4,
        Jellyfish = 5,
        Bug = 8,
        Wasp = 9,
        Hand = 10,
        Eye = 11,
        FireSlime = 12,
        PurpleJellyfish = 13,
        StagBeetle = 14,
        Shade = 15,
        ShadeJellyfish = 16,
        ShadeBat = 17,
        ShadeVampire = 18,
    }
}