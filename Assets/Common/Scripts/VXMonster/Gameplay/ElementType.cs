using System;

namespace VXMonster.Gameplay
{
    [Flags]
    public enum ElementType
    {
        None = 0,
        Burn = 1 << 0,
        Chill = 1 << 1,
        Shock = 1 << 2
    }
}
