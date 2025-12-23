using UnityEngine;

namespace BEACON.Player.States
{
    /// <summary>
    /// State for when the player is in the air (jumping or falling).
    /// Handles air control, variable jump height, and coyote time.
    /// </summary>
    public class AirborneState : PlayerStateBase
    {
        private bool hasReleasedJump;
        private bool canCoyoteJump;

        public AirborneState(PlayerController player, PlayerStateMachine stateMachine) 
            : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            hasReleasedJump = !Input.IsJumpHeld;
            canCoyoteJump = player.IsCoyoteTimeActive;
        }

        public override void Execute()
        {
            // Check for dash
            if (CheckCommonTransitions())
            {
                return;
            }

            // Track jump release for variable height
            if (!Input.IsJumpHeld)
            {
                hasReleasedJump = true;
            }

            // Check for coyote jump
            if (player.JumpRequested && canCoyoteJump && player.IsCoyoteTimeActive)
            {
                player.ExecuteJump();
                canCoyoteJump = false;
                return;
            }

            // Buffer jump input for when we land
            if (player.JumpRequested && !Ground.IsGrounded)
            {
                player.BufferJump();
            }

            // Check for wall slide (if touching wall and falling)
            if (Wall.IsTouchingWall && Rb.linearVelocity.y < 0)
            {
                // Check if player is pressing towards the wall
                bool pressingTowardsWall = 
                    (Wall.IsTouchingWallRight && Input.MoveInput.x > 0.1f) ||
                    (Wall.IsTouchingWallLeft && Input.MoveInput.x < -0.1f);

                if (pressingTowardsWall)
                {
                    stateMachine.ChangeState<WallSlideState>();
                    return;
                }
            }

            // Check for landing
            if (Ground.IsGrounded && Rb.linearVelocity.y <= 0)
            {
                stateMachine.ChangeState<GroundedState>();
                return;
            }

            // Update facing
            UpdateFacing();
        }

        public override void FixedExecute()
        {
            // Apply horizontal movement with reduced air control
            ApplyHorizontalMovement();

            // Apply gravity modifiers (fall faster, jump cut, apex hang)
            ApplyGravityModifiers();

            // Apply jump cut if released early while rising
            if (hasReleasedJump && Rb.linearVelocity.y > 0)
            {
                Rb.linearVelocity = new Vector2(
                    Rb.linearVelocity.x, 
                    Rb.linearVelocity.y * Data.jumpCutMultiplier
                );
                hasReleasedJump = false; // Only apply once
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            // Reset gravity
            Rb.gravityScale = Data.defaultGravityScale;
        }
    }
}
