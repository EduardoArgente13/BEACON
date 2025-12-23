using UnityEngine;

namespace BEACON.Combat.Parry
{
    public enum ParryTiming
    {
        None,
        Perfect,
        Good,
        Failed
    }

    [System.Serializable]
    public struct ParryResult
    {
        public ParryTiming timing;
        public float resonanceGained;
        public float enemyStunDuration;
        public bool successfulReference; // Helper to check if any success happened

        public static ParryResult Failed => new ParryResult { timing = ParryTiming.Failed, successfulReference = false };
    }
}
