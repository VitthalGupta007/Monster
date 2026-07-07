using System.Collections.Generic;
using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class CodexSave : ISave
    {
        [SerializeField] protected List<string> discoveredEvolutions = new List<string>();
        [SerializeField] protected List<string> discoveredRelics = new List<string>();
        [SerializeField] protected bool discoveredFirstCombo;

        public bool DiscoverEvolution(string evolutionId)
        {
            if (discoveredEvolutions.Contains(evolutionId)) return false;
            discoveredEvolutions.Add(evolutionId);
            return true;
        }

        public bool DiscoverRelic(string relicId)
        {
            if (discoveredRelics.Contains(relicId)) return false;
            discoveredRelics.Add(relicId);
            return true;
        }

        public bool MarkFirstCombo()
        {
            if (discoveredFirstCombo) return false;
            discoveredFirstCombo = true;
            return true;
        }

        public IReadOnlyList<string> DiscoveredRelicIds => discoveredRelics;
        public IReadOnlyList<string> DiscoveredEvolutionIds => discoveredEvolutions;
        public bool HasDiscoveredFirstCombo => discoveredFirstCombo;

        public void Flush() { }
    }
}
