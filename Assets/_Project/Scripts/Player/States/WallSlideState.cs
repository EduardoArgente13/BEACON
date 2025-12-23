using UnityEngine;

namespace BEACON.Player.States
{
    /// <summary>
    /// State for when the player is sliding down a wall.
    /// Handles wall slide friction and wall jump.
    /// </summary>
    public class WallSlideState : PlayerStateBase
    {
        private int wallDirection;

        public WallSlideState(PlayerController player, PlayerStateMachine stateMachine) 
            : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            wallDirection = Wall.WallDirection;
            
            // Reset gravity for controlled slide
            Rb.gravityScale = Data.defaultGravityScale * 0.5f;
        }

        public override void Execute()
        {
            // Check for wall jump
            if (player.JumpRequested)
            {
                ExecuteWallJump();
                stateMachine.ChangeState<AirborneState>();
                return;
            }

            // Check for dash
            if (CheckCommonTransitions())
            {
                return;
            }

            // Check if we left the wall
            if (!Wall.IsTouchingWall)
            {
                stateMachine.ChangeState<AirborneState>();
                return;
            }

            // Check if player stopped pressing towards wall
            bool stillPressingWall = 
                (wallDirection > 0 && Input.MoveInput.x > 0.1f) ||
                (wallDirection < 0 && Input.MoveInput.x < -0.1f);

            if (!stillPressingWall)
            {
                stateMachine.ChangeState<AirborneState>();
                return;
            }

            // Check if we landed
            if (Ground.IsGrounded)
            {
                stateMachine.ChangeState<GroundedState>();
                return;
            }
        }

        public override void FixedExecute()
        {
            // Forces sliding if stuck due to friction (velocity close to 0)
            if (Rb.linearVelocity.y > -0.5f)
            {
                Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, -1f);
            }

            // Clamp slide speed
            if (Rb.linearVelocity.y < -Data.wallSlideSpeed)
            {
                Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, -Data.wallSlideSpeed);
            }
        }

        private void ExecuteWallJump()
        {
            // Jump away from wall
            Vector2 jumpDirection = Wall.GetWallJumpDirection();
            
            Rb.linearVelocity = new Vector2(
                jumpDirection.x * Data.wallJumpForceX,
                Data.wallJumpForceY
            );

            // Reset air dashes
            player.ResetAirDashes();
            
            // Start wall jump lock (reduced control for a moment)
            player.StartWallJumpLock();

            // Face away from wall
            if (jumpDirection.x > 0 && !player.IsFacingRight)
            {
                player.Flip();
            }
            else if (jumpDirection.x < 0 && player.IsFacingRight)
            {
                player.Flip();
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
