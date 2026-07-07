using UnityEngine;

namespace OctoberStudio.Currency
{
    [System.Serializable]
    public class CurrencyData
    {
        [SerializeField] string id;
        public string ID => id;

        [SerializeField] string name;
        public string Name => name;

        [SerializeField] Sprite icon;
        public Sprite Icon => icon;
    }
}