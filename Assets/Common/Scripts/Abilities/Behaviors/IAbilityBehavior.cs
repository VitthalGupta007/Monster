using OctoberStudio.Abilities;

namespace OctoberStudio
{
    public interface IAbilityBehavior
    {
        AbilityType AbilityType { get; }
        AbilityData AbilityData { get; }
        void Init(AbilityData data, int stageId);
        void Clear();
    }
}