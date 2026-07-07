using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Pool
{
    public class PoolsManager : MonoBehaviour
    {
        [SerializeField] List<PoolData> preloadedPools;

        private Dictionary<int, PoolObject> pools;

        private void Awake()
        {
            pools = new Dictionary<int, PoolObject>();

            for (int i = 0; i < preloadedPools.Count; i++)
            {
                var data = preloadedPools[i];

                int hash = data.name.GetHashCode();

                var pool = new PoolObject(data.name, data.prefab, data.size);
                pools[hash] = pool;
            }
        }

        public PoolObject GetPool(string name)
        {
            return GetPool(name.GetHashCode());
        }

        public PoolObject GetPool(int hash)
        {
            if (pools.ContainsKey(hash))
            {
                return pools[hash];
            }

            return null;
        }

        public GameObject GetEntity(string name)
        {
            return GetEntity(name.GetHashCode());
        }

        public GameObject GetEntity(int hash)
        {
            var pool = GetPool(hash);

            if (pool != null) return pool.GetEntity();

            return null;
        }

        public T GetEntity<T>(string name) where T : Component
        {
            return GetEntity<T>(name.GetHashCode());
        }

        public T GetEntity<T>(int hash) where T : Component
        {
            var pool = GetPool(hash);

            if (pool != null) return pool.GetEntity<T>();

            return null;
        }

        [System.Serializable]
        private class PoolData
        {
            public string name;
            public GameObject prefab;
            public int size;
        }
    }
}