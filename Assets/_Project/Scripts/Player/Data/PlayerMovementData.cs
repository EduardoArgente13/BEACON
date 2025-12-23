using UnityEngine;

namespace BEACON.Player.Data
{
    /// <summary>
    /// ScriptableObject containing all player movement parameters.
    /// All values are tweakable in the Inspector for rapid iteration.
    /// Reference values are based on Hollow Knight/Celeste feel.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "BEACON/Player/Movement Data")]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("=== HORIZONTAL MOVEMENT ===")]
        
        [Tooltip("Maximum horizontal movement speed")]
        [Range(1f, 20f)]
        public float moveSpeed = 8f;

        [Tooltip("How fast the player reaches max speed on ground")]
        [Range(10f, 100f)]
        public float groundAcceleration = 50f;

        [Tooltip("How fast the player stops on ground")]
        [Range(10f, 100f)]
        public float groundDeceleration = 50f;

        [Tooltip("How fast the player reaches max speed in air (less = floatier)")]
        [Range(5f, 100f)]
        public float airAcceleration = 35f;

        [Tooltip("How fast the player stops in air")]
        [Range(5f, 50f)]
        public float airDeceleration = 20f;

        [Header("=== JUMP SETTINGS ===")]
        
        [Tooltip("Initial jump velocity")]
        [Range(10f, 30f)]
        public float jumpForce = 18f;

        [Tooltip("Multiplier applied to Y velocity when jump is released early (0.5 = half height)")]
        [Range(0.1f, 1f)]
        public float jumpCutMultiplier = 0.5f;

        [Tooltip("Gravity multiplier when falling (higher = faster fall)")]
        [Range(1f, 3f)]
        public float fallGravityMultiplier = 1.8f;

        [Tooltip("Maximum falling speed (terminal velocity)")]
        [Range(10f, 50f)]
        public float maxFallSpeed = 25f;

        [Tooltip("Velocity threshold below which apex modifiers apply")]
        [Range(0.5f, 5f)]
        public float apexThreshold = 2.5f;

        [Tooltip("Extra horizontal acceleration at jump apex (0 = none, 1 = double)")]
        [Range(0f, 1f)]
        public float apexSpeedBonus = 0.5f;

        [Tooltip("Gravity reduction at apex (0 = no reduction, 1 = zero gravity at apex)")]
        [Range(0f, 1f)]
        public float apexGravityReduction = 0.4f;

        [Header("=== JUMP ASSIST ===")]
        
        [Tooltip("Time after leaving ground where jump is still allowed (Coyote Time)")]
        [Range(0.05f, 0.3f)]
        public float coyoteTime = 0.12f;

        [Tooltip("Time before landing where jump input is buffered")]
        [Range(0.05f, 0.3f)]
        public float jumpBufferTime = 0.1f;

        [Header("=== DASH SETTINGS ===")]
        
        [Tooltip("Dash movement speed")]
        [Range(15f, 40f)]
        public float dashSpeed = 25f;

        [Tooltip("Total dash duration in seconds")]
        [Range(0.1f, 0.4f)]
        public float dashDuration = 0.3f; // Increased from 0.15 for easier testing

        [Tooltip("Cooldown between dashes")]
        [Range(0.1f, 2f)]
        public float dashCooldown = 0.4f;

        [Tooltip("Duration of invincibility frames during dash")]
        [Range(0.05f, 0.3f)]
        public float dashIFramesDuration = 0.3f; // Matched to duration

        [Tooltip("Number of air dashes allowed before touching ground")]
        [Range(0, 3)]
        public int airDashesAllowed = 1;

        [Tooltip("End dash velocity multiplier (0 = stop instantly, 1 = keep full speed)")]
        [Range(0f, 1f)]
        public float dashEndSpeedMultiplier = 0.5f;

        [Header("=== WALL MECHANICS (Optional) ===")]
        
        [Tooltip("Sliding speed on walls")]
        [Range(1f, 10f)]
        public float wallSlideSpeed = 3f;

        [Tooltip("Horizontal force when wall jumping")]
        [Range(5f, 20f)]
        public float wallJumpForceX = 12f;

        [Tooltip("Vertical force when wall jumping")]
        [Range(10f, 25f)]
        public float wallJumpForceY = 16f;

        [Tooltip("Time after wall jump where horizontal input is reduced")]
        [Range(0.1f, 0.5f)]
        public float wallJumpLockTime = 0.2f;

        [Header("=== GROUND DETECTION ===")]
        
        [Tooltip("Length of ground detection raycasts")]
        [Range(0.05f, 0.5f)]
        public float groundCheckDistance = 0.1f;

        [Tooltip("Horizontal offset for side ground raycasts")]
        [Range(0.1f, 1f)]
        public float groundCheckWidth = 0.4f;

        [Tooltip("Maximum slope angle the player can walk on")]
        [Range(20f, 60f)]
        public float maxSlopeAngle = 45f;

        [Header("=== PHYSICS SETTINGS ===")]
        
        [Tooltip("Default gravity scale for Rigidbody2D")]
        [Range(1f, 5f)]
        public float defaultGravityScale = 3f;

        [Tooltip("Gravity scale while holding jump (for variable height)")]
        [Range(0.5f, 3f)]
        public float holdJumpGravityScale = 2f;

        [Header("=== RESONANCE DASH SCALING ===")]
        [Tooltip("Extra speed multiplier at Max Resonance (e.g. 0.2 = 20% faster)")]
        [Range(0f, 0.5f)]
        public float dashResonanceSpeedBonus = 0.2f;

        [Tooltip("Extra i-frames at Max Resonance (Added to base duration)")]
        [Range(0f, 0.3f)]
        public float dashResonanceIFramesBonus = 0.15f;

        [Header("=== PERFECT DODGE ===")]
        [Tooltip("Time window from dash start where Perfect Dodge is possible")]
        [Range(0.05f, 0.3f)]
        public float perfectDodgeWindow = 0.15f;
        
        [Tooltip("Slow motion duration on success")]
        [Range(0.1f, 1f)]
        public float perfectDodgeSlowMoDuration = 0.2f;

        [Tooltip("Resonance gained on Perfect Dodge")]
        [Range(0, 50)]
        public float perfectDodgeResonanceGain = 10f;

        // ============ HELPER METHODS ============
        
        /// <summary>
        /// Returns current acceleration based on grounded state
        /// </summary>
        public float GetAcceleration(bool isGrounded)
        {
            return isGrounded ? groundAcceleration : airAcceleration;
        }

        /// <summary>
        /// Returns current deceleration based on grounded state
        /// </summary>
        public float GetDeceleration(bool isGrounded)
        {
            return isGrounded ? groundDeceleration : airDeceleration;
        }

        /// <summary>
        /// Calculates apex modifiers based on current Y velocity
        /// </summary>
        /// <param name="yVelocity">Current vertical velocity</param>
        /// <returns>Tuple of (speedBonus, gravityMultiplier)</returns>
        public (float speedBonus, float gravityMultiplier) GetApexModifiers(float yVelocity)
        {
            if (Mathf.Abs(yVelocity) < apexThreshold)
            {
                float t = 1f - (Mathf.Abs(yVelocity) / apexThreshold);
                float bonus = Mathf.Lerp(0f, apexSpeedBonus, t);
                float gravity = Mathf.Lerp(1f, 1f - apexGravityReduction, t);
                return (bonus, gravity);
            }
            return (0f, 1f);
        }
    }
}
