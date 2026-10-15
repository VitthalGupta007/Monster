using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class LoginClaimSave : ISave
    {
        public const string SaveKey = "VX Login Claim";

        [SerializeField] protected string lastClaimUtcDate = string.Empty;
        [SerializeField] protected int loginStreak;

        public string LastClaimUtcDate
        {
            get => lastClaimUtcDate;
            set => lastClaimUtcDate = value;
        }

        public int LoginStreak
        {
            get => loginStreak;
            set => loginStreak = value;
        }

        public bool HasClaimedToday(string utcDateKey)
        {
            return lastClaimUtcDate == utcDateKey;
        }

        public void Flush() { }
    }
}
