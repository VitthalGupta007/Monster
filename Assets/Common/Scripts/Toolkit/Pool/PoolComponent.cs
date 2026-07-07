using UnityEngine;

namespace OctoberStudio.Pool
{
    public class PoolComponent<T> : Pool<T> where T : Component
    {
        private GameObject prefab;
        private Transform parent;
        private bool dontDestroyOnLoad;

        public PoolComponent(string name, GameObject prefab, int startingSize, Transform parent = null, bool dontDestroyOnLoad = false) : base(name, startingSize)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.dontDestroyOnLoad = dontDestroyOnLoad;

            for (int i = 0; i <= startingSize; i++)
            {
                AddNewEntity();
            }
        }

        public PoolComponent(GameObject prefab, int startingSize, Transform parent = null, bool dontDestroyOnLoad = false) : base(prefab.name, startingSize)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.dontDestroyOnLoad = dontDestroyOnLoad;

            for (int i = 0; i <= startingSize; i++)
            {
                AddNewEntity();
            }
        }

        protected override T CreateEntity()
        {
            var entity = Object.Instantiate(prefab, parent).GetComponent<T>();
            if(dontDestroyOnLoad && parent == null)
            {
                Object.DontDestroyOnLoad(entity.gameObject);
            }
            return entity;
        }

        protected override void InitEntity(T entity)
        {
            entity.gameObject.SetActive(false);
        }

        protected override bool ValidateEntity(T entity)
        {
            return !entity.gameObject.activeSelf;
        }

        public override T GetEntity()
        {
            var entity = base.GetEntity();

            entity.gameObject.SetActive(true);

            return entity;
        }

        protected override void DisableEntity(T entity)
        {
            entity.gameObject.SetActive(false);
        }

        protected override void DestroyEntity(T entity)
        {
            if(entity != null) Object.Destroy(entity.gameObject);
        }
    }
}