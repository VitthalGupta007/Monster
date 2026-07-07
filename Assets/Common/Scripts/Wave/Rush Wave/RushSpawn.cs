using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class RushSpawn
    {
        [SerializeField] protected DataType dataType;
        [SerializeField] protected SpawnData spawnData;
        [SerializeField] protected SpawnData additionalSpawnData;

        public DataType Type => dataType;
        public float Angle => spawnData.Angle;
        public float AdditionalAngle => additionalSpawnData.Angle;

        public float Position => spawnData.Position;
        public float Width => spawnData.Width;

        public virtual void GetSpawnData(out Vector2 direction, out float position)
        {
            direction = Vector2.down;
            position = 0f;

            switch (dataType)
            {
                case DataType.Simple:
                    GetSingleSpawnData(out direction, out position);
                    break;
                case DataType.RandomBetweenTwo:
                    GetMultiSpawnData(out direction, out position);
                    break;
            }
        }

        protected virtual void GetSingleSpawnData(out Vector2 direction, out float position)
        {
            direction = spawnData.Rotation * Vector2.right;
            direction.y *= -1f;
            position = spawnData.RandomPosition;
        }

        protected virtual void GetMultiSpawnData(out Vector2 direction, out float position)
        {
            if(Random.value > 0.5f)
            {
                direction = spawnData.Rotation * Vector2.right;
                direction.y *= -1f;
                position = spawnData.RandomPosition;
            } else
            {
                direction = additionalSpawnData.Rotation * Vector2.right;
                direction.y *= -1f;
                position = additionalSpawnData.RandomPosition;
            }
        }

        [System.Serializable]
        public enum DataType
        {
            Simple = 0,
            RandomBetweenTwo = 1,
        }

        [System.Serializable]
        public class SpawnData
        {
            [SerializeField] protected float angle = 0f;
            [SerializeField] protected float width = 0.3f;
            [SerializeField] protected float position = 0f;

            public float Angle => angle;
            public float Width => width;
            public float Position => position;

            public Quaternion Rotation => Quaternion.Euler(0, 0, angle);
            public float RandomPosition => position + Random.Range(-width / 2f, width / 2f);
        }
    }
}