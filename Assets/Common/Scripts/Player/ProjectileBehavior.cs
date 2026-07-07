using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class ProjectileBehavior : MonoBehaviour
    {
        public List<Effect> Effects { get; private set; }

        public float DamageMultiplier { get; set; }
        public bool KickBack { get; set; }

        public virtual void Init()
        {
            Effects = new List<Effect>();
        }
    }
}