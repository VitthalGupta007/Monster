using System;
using UnityEngine;
using System.Collections.Generic;

namespace OctoberStudio.Save
{
    [Serializable]
    public class SaveDatabase
    {
        [SerializeField] public string versionId;

        [SerializeField] SaveCell[] saveCells;
        private List<SaveCell> saveCellsList;

        public void Init()
        {
            if (saveCells == null) saveCells = new SaveCell[0];

            foreach(var cell in saveCells)
            {
                cell.IsReassembled = false;
            }

            saveCellsList = new List<SaveCell>(saveCells);
        }

        public void Flush()
        {
            saveCells = saveCellsList.ToArray();

            for (int i = 0; i < saveCellsList.Count; i++)
            {
                var save = saveCellsList[i];

                save.Flush();
            }
        }

        public T GetSave<T>(int hash) where T : ISave, new()
        {
            foreach(var cell in saveCellsList) 
            { 
                if(cell.Hash == hash)
                {
                    if (!cell.IsReassembled) cell.Reconstruct<T>();

                    var save = (T)cell.Save;

                    if(save == null)
                    {
                        save = new T();
                        cell.SetSave(save);
                    }
                    return save;
                }
            }

            var newCell = new SaveCell(hash, new T());
            saveCellsList.Add(newCell);

            return (T)newCell.Save;
        }

        public T GetSave<T>(string uniqueName) where T : ISave, new()
        {
            return GetSave<T>(uniqueName.GetHashCode());
        }
    }
}