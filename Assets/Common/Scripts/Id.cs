using UnityEngine;

namespace VXMonster.Core
{
    [System.Serializable]
    public class Id
    {
        [SerializeField] protected string value;
        [SerializeField] protected bool canEdit = false;

        public static implicit operator string(Id id)
        {
            return id.value;
        }

        public static implicit operator Id(string id)
        {
            return new Id { value = id, canEdit = false };
        }
    }
}