using System.Collections.Generic;

namespace OctoberStudio.Pool
{
    public abstract class Pool<T>: AbstractPool
    {
        private List<T> entities;

        protected abstract T CreateEntity();
        protected abstract void InitEntity(T entity);
        protected abstract bool ValidateEntity(T entity);
        protected abstract void DisableEntity(T entity);
        protected abstract void DestroyEntity(T entity);

        public Pool(string name, int startingSize) : base(name)
        {
            if (startingSize < 1) startingSize = 1;

            entities = new List<T>(startingSize);
        }

        protected T AddNewEntity()
        {
            var entity = CreateEntity();
            InitEntity(entity);

            entities.Add(entity);

            return entity;
        }

        public virtual T GetEntity()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (CheckForNull(i))
                {
                    i--;
                    continue;
                }

                if (ValidateEntity(entities[i]))
                {
                    return entities[i];
                }
            }

            var newEntity = AddNewEntity();
            return newEntity;
        }

        private bool CheckForNull(int index)
        {
            if (entities[index] == null)
            {
                for (int j = index; j < entities.Count - 1; j++)
                {
                    entities[j] = entities[j + 1];
                }

                entities.RemoveAt(entities.Count - 1);

                return true;
            }

            return false;
        }

        public void DisableAllEntities()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (CheckForNull(i))
                {
                    i--;
                    continue;
                }

                DisableEntity(entities[i]);
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] != null)
                {
                    DestroyEntity(entities[i]);
                }
            }

            entities.Clear();
        }
    }
}