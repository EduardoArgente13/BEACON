using UnityEngine;

namespace BEACON.Combat
{
    /// <summary>
    /// Defines the properties of a single attack within a combo or ability.
    /// This struct acts as the DNA for every hit in the game.
    /// </summary>
    [System.Serializable]
    public struct AttackData
    {
        [Header("Animation")]
        public string animationName;
        
        [Header("Damage & Impact")]
        public float damage;
        public float knockbackForce;
        public Vector2 knockbackDirection; // Local direction relative to facing
        public float hitStopDuration;
        
        [Header("Timing (Frames)")]
        [Tooltip("Frame where hitbox becomes active")]
        public int activeFrameStart;
        
        [Tooltip("Frame where hitbox deactivates")]
        public int activeFrameEnd;
        
        [Header("Canceling")]
        public bool canCancelIntoDash;
        public bool canCancelIntoJump;
        public int earliestCancelFrame; // Frame number
        
        [Header("Hitbox Configuration")]
        public HitboxShape hitboxShape;
        public Vector2 hitboxOffset;
        public Vector2 hitboxSize; // For Box: x=width, y=height. For Circle: x=radius.
        
        [Header("Movement")]
        public bool providesMovement;
        public Vector2 movementVector; // Impulse applied when attacking
        public float movementDuration;
        
        [Header("Special Props")]
        public AttackType attackType;
        public float clashMultiplier; // 1.0 = normal match
        public float resonanceGain;   // Base resonance gained on hit (for Melee)
        public float resonanceCost;   // Resonance spent when using (for Super/Ranged)

        [Header("Ranged Props")]
        public GameObject projectilePrefab;
        public float projectileSpeed;
        public bool isCharged;
        public float minChargeTime;
        public int projectileCount; // Default should be 1
        public float spreadAngle;   // For shotgun-style attacks
        
        /// <summary>
        /// Returns duration in frames of the active window
        /// </summary>
        public int ActiveDurationFrames => activeFrameEnd - activeFrameStart;
    }
}
