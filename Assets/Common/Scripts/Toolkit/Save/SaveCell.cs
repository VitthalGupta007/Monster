using UnityEngine;

namespace OctoberStudio.Save
{
    [System.Serializable]
    public class SaveCell
    {
        [SerializeField] int hash;
        [SerializeField] string json;
        [System.NonSerialized] ISave save;

        public int Hash => hash;
        public bool IsReassembled { get; set; }
        public ISave Save => save;

        public SaveCell(int hash, ISave save)
        {
            this.hash = hash;
            this.save = save;
            IsReassembled = true;
        }

        public void SetSave(ISave save)
        {
            this.save = save;
        }

        public void Flush()
        {
            if (save != null) save.Flush();
            if (IsReassembled) json = JsonUtility.ToJson(save);
        }

        public void Reconstruct<T>() where T : ISave
        {
            save = JsonUtility.FromJson<T>(json);
            IsReassembled = true;
        }
    }
}