using UnityEngine;

namespace BEACON.Player.States
{
    /// <summary>
    /// Base class for all player states.
    /// Provides common functionality and references to player components.
    /// </summary>
    public abstract class PlayerStateBase : IPlayerState
    {
        // ============ REFERENCES ============
        
        protected readonly PlayerController player;
        protected readonly PlayerStateMachine stateMachine;

        // ============ SHORTCUTS (for cleaner code in derived states) ============
        
        protected Rigidbody2D Rb => player.Rb;
        protected PlayerInputHandler Input => player.InputHandler;
        protected GroundChecker Ground => player.GroundChecker;
        protected WallChecker Wall => player.WallChecker;
        protected Data.PlayerMovementData Data => player.MovementData;

        // ============ STATE DATA ============
        
        /// <summary>Time when this state was entered</summary>
        protected float stateEnterTime;

        /// <summary>How long the state has been active</summary>
        protected float StateDuration => Time.time - stateEnterTime;

        // ============ CONSTRUCTOR ============

        protected PlayerStateBase(PlayerController player, PlayerStateMachine stateMachine)
        {
            this.player = player;
            this.stateMachine = stateMachine;
        }

        // ============ LIFECYCLE ============

        public virtual void Enter()
        {
            stateEnterTime = Time.time;
        }

        public abstract void Execute();

        public abstract void FixedExecute();

        public virtual void Exit()
        {
        }

        // ============ COMMON STATE CHECKS ============

        /// <summary>
        /// Checks common transitions that apply to multiple states.
        /// Returns true if a transition occurred.
        /// </summary>
        protected bool CheckCommonTransitions()
        {
            // Check for dash input (can interrupt most states)
            if (player.CanDash && player.DashRequested)
            {
                stateMachine.ChangeState<DashState>();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies horizontal movement with acceleration/deceleration.
        /// Call this in FixedExecute for physics-based movement.
        /// </summary>
        protected void ApplyHorizontalMovement()
        {
            // Respect wall jump lock or combat movement overrides
            if (player.IsWallJumpLocked || player.IsMovementOverridden) return;

            float targetSpeed = Input.MoveInput.x * Data.moveSpeed;
            
            // Get apex modifiers for hang time
            var (speedBonus, _) = Data.GetApexModifiers(Rb.linearVelocity.y);
            targetSpeed *= (1f + speedBonus);

            // Calculate speed difference
            float speedDiff = targetSpeed - Rb.linearVelocity.x;

            // Choose acceleration or deceleration
            float accelRate;
            if (Mathf.Abs(targetSpeed) > 0.01f)
            {
                accelRate = Data.GetAcceleration(Ground.IsGrounded);
            }
            else
            {
                accelRate = Data.GetDeceleration(Ground.IsGrounded);
            }

            // Apply movement force
            float movement = speedDiff * accelRate * Time.fixedDeltaTime;
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x + movement, Rb.linearVelocity.y);
        }

        /// <summary>
        /// Applies modified gravity based on fall state.
        /// </summary>
        protected void ApplyGravityModifiers()
        {
            if (Rb.linearVelocity.y < 0)
            {
                // Falling - apply fall gravity multiplier
                Rb.gravityScale = Data.defaultGravityScale * Data.fallGravityMultiplier;
            }
            else if (Rb.linearVelocity.y > 0 && !Input.IsJumpHeld)
            {
                // Rising but jump released - apply jump cut
                Rb.gravityScale = Data.defaultGravityScale * Data.fallGravityMultiplier;
            }
            else
            {
                // Normal or apex - check for apex modifier
                var (_, gravityMod) = Data.GetApexModifiers(Rb.linearVelocity.y);
                Rb.gravityScale = Data.defaultGravityScale * gravityMod;
            }

            // Clamp fall speed
            if (Rb.linearVelocity.y < -Data.maxFallSpeed)
            {
                Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, -Data.maxFallSpeed);
            }
        }

        /// <summary>
        /// Updates player facing direction based on input.
        /// </summary>
        protected void UpdateFacing()
        {
            if (Input.MoveInput.x > 0.1f && !player.IsFacingRight)
            {
                player.Flip();
            }
            else if (Input.MoveInput.x < -0.1f && player.IsFacingRight)
            {
                player.Flip();
            }
        }
    }
}
