using UnityEngine;

namespace OctoberStudio.Pool
{
    public class PoolObject : Pool<GameObject>
    {
        private GameObject prefab;

        public PoolObject(string name, GameObject prefab, int startingSize) : base(name, startingSize)
        {
            this.prefab = prefab;

            for (int i = 0; i <= startingSize; i++)
            {
                AddNewEntity();
            }
        }

        public PoolObject(GameObject prefab, int startingSize) : base(prefab.name, startingSize)
        {
            this.prefab = prefab;

            for (int i = 0; i <= startingSize; i++)
            {
                AddNewEntity();
            }
        }

        protected override GameObject CreateEntity()
        {
            var entity = Object.Instantiate(prefab);

            return entity;
        }

        protected override void InitEntity(GameObject entity)
        {
            entity.SetActive(false);
        }

        protected override bool ValidateEntity(GameObject entity)
        {
            return !entity.activeSelf;
        }

        public override GameObject GetEntity()
        {
            var entity = base.GetEntity();

            entity.SetActive(true);

            return entity;
        }

        public T GetEntity<T>() where T : Component
        {
            var entity = base.GetEntity();

            entity.SetActive(true);

            return entity.GetComponent<T>();
        }

        protected override void DisableEntity(GameObject entity)
        {
            entity.SetActive(false);
        }

        protected override void DestroyEntity(GameObject entity)
        {
            Object.Destroy(entity);
        }
    }
}