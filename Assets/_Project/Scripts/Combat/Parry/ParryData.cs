using UnityEngine;

namespace BEACON.Combat.Parry
{
    [CreateAssetMenu(fileName = "NewParryData", menuName = "BEACON/Combat/Parry Data")]
    public class ParryData : ScriptableObject
    {
        [Header("Timing Windows (Seconds)")]
        [Tooltip("Window for Perfect Parry (Frame Perfect)")]
        public float perfectWindow = 0.1f;
        
        [Tooltip("Window for Good Parry (Standard)")]
        public float goodWindow = 0.2f;

        [Header("Stun Duration")]
        public float perfectStunDuration = 2.5f;
        public float goodStunDuration = 1.2f;

        [Header("Resonance Logic")]
        public float perfectResonanceGain = 30f;
        public float goodResonanceGain = 15f;
        public float failedResonancePenalty = 5f;

        [Header("Game Feel")]
        public float perfectHitStop = 0.35f;
        public float goodHitStop = 0.15f;
        public float failedVulnerabilityTime = 0.4f;
    }
}
