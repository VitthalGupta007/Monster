using System;

namespace VXMonster.Platform.Integrity
{
    public interface IPlayIntegrityService
    {
        void RequestCheck(Action<IntegrityCheckResult> onComplete);
    }
}
