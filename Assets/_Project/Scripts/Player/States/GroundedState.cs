using UnityEngine;

namespace BEACON.Player.States
{
    /// <summary>
    /// State for when the player is on the ground (idle or running).
    /// Handles horizontal movement and transition to jump/fall.
    /// </summary>
    public class GroundedState : PlayerStateBase
    {
        public GroundedState(PlayerController player, PlayerStateMachine stateMachine) 
            : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            // Reset air dash count when landing
            player.ResetAirDashes();
            
            // Reset gravity to default
            Rb.gravityScale = Data.defaultGravityScale;

            // Consume jump buffer if it was pressed before landing
            if (player.HasBufferedJump)
            {
                player.ExecuteJump();
                stateMachine.ChangeState<AirborneState>();
            }
        }

        public override void Execute()
        {
            // Check for dash first (highest priority action)
            if (CheckCommonTransitions())
            {
                return;
            }

            // Check for jump input
            if (player.JumpRequested)
            {
                player.ExecuteJump();
                stateMachine.ChangeState<AirborneState>();
                return;
            }

            // Check if we walked off a ledge
            if (!Ground.IsGrounded)
            {
                player.StartCoyoteTime();
                stateMachine.ChangeState<AirborneState>();
                return;
            }

            // Update facing direction
            UpdateFacing();
        }

        public override void FixedExecute()
        {
            ApplyHorizontalMovement();
            
            // Handle slope movement to prevent sliding
            if (Ground.IsOnSlope && Mathf.Abs(Input.MoveInput.x) < 0.01f)
            {
                // Zero out velocity when standing still on slope
                Rb.linearVelocity = Vector2.zero;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
