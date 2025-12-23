using UnityEngine;

namespace BEACON.Player.States
{
    /// <summary>
    /// State for when the player is dashing.
    /// Player is invincible and moves rapidly in a direction.
    /// </summary>
    public class DashState : PlayerStateBase
    {
        private Vector2 dashDirection;
        private float dashEndTime;
        private bool wasGroundedOnEnter;
        private float appliedDashSpeed;

        public DashState(PlayerController player, PlayerStateMachine stateMachine) 
            : base(player, stateMachine)
        {
        }

        private float dashStartTime;
        private BEACON.Core.Stats.ResonanceController resonanceController;

        public override void Enter()
        {
            base.Enter();
            
            // Cache Resonance Controller (could be done in Constructor/Init for perf, but safe here)
            if (resonanceController == null) resonanceController = player.GetComponent<BEACON.Core.Stats.ResonanceController>();

            wasGroundedOnEnter = Ground.IsGrounded;
            dashStartTime = Time.time;
            
            // === SCALING LOGIC ===
            float currentResonance = resonanceController != null ? resonanceController.CurrentResonance : 0f;
            float maxResonance = resonanceController != null ? resonanceController.MaxResonance : 100f;
            float resonancePercent = Mathf.Clamp01(currentResonance / maxResonance);

            // Calculate bonuses
            float speedBonus = Data.dashSpeed * (Data.dashResonanceSpeedBonus * resonancePercent);
            float iFrameBonus = Data.dashResonanceIFramesBonus * resonancePercent;

            appliedDashSpeed = Data.dashSpeed + speedBonus;
            float finalIFrames = Data.dashIFramesDuration + iFrameBonus;

            // Debug Feedback for Scaling
            if (resonancePercent > 0.5f)
            {
                // TODO: Trigger "Powered Dash" VFX
                // Debug.Log($"[Dash] Powered Up! Speed: {finalDashSpeed} (+{speedBonus:F1}), iFrames: {finalIFrames} (+{iFrameBonus:F2})");
            }

            // Calculate dash direction
            dashDirection = GetDashDirection();
            
            // Calculate end time
            dashEndTime = Time.time + Data.dashDuration;
            
            // Disable gravity during dash
            Rb.gravityScale = 0f;
            
            // Set velocity
            Rb.linearVelocity = dashDirection * appliedDashSpeed;
            
            // Start i-frames
            player.StartIFrames(finalIFrames);
            
            // Register Perfect Dodge Callback
            player.PerfectDodgeCheck = TryTriggerPerfectDodge;
            
            // Consume air dash if in air
            if (!wasGroundedOnEnter)
            {
                player.ConsumeAirDash();
            }

            // Start dash cooldown
            player.StartDashCooldown();

            // Notify for VFX/SFX
            player.OnDashStarted?.Invoke(dashDirection);
        }

        private bool TryTriggerPerfectDodge()
        {
            float timeSinceDash = Time.time - dashStartTime;
            // Debug.Log($"[Dash] PD Check. Time: {timeSinceDash:F3}s / Window: {Data.perfectDodgeWindow}");

            // Check Window
            if (timeSinceDash < Data.perfectDodgeWindow)
            {
                // SUCCESS!
                Debug.Log($"<color=cyan>[Dash] PERFECT DODGE! (Time: {timeSinceDash:F3}s)</color>");
                
                // 1. Rewards
                if (resonanceController != null)
                {
                    resonanceController.AddResonance(Data.perfectDodgeResonanceGain);
                }

                // 2. Extend Invincibility (Safety)
                player.StartIFrames(0.5f);

                // 3. Game Feel (HitStop / SlowMo)
                BEACON.Combat.Feedback.HitStopManager.Instance?.TriggerHitStop(Data.perfectDodgeSlowMoDuration);
                
                // 4. Events
                player.OnPerfectDodge?.Invoke();

                return true; // Damage Negated AND Rewarded
            }
            
            Debug.Log($"[Dash] PD Failed (Too Late). Time: {timeSinceDash:F3}s");
            // Outside window, but still invincible? 
            // PlayerController checks isInvincible AFTER this returns false, so standard i-frames still apply.
            return false;
        }

        private Vector2 GetDashDirection()
        {
            Vector2 inputDir = Input.DashDirection;

            // If no directional input, dash in facing direction
            if (inputDir.magnitude < 0.1f)
            {
                return player.IsFacingRight ? Vector2.right : Vector2.left;
            }

            // Normalize and quantize to 8 directions
            inputDir = inputDir.normalized;
            
            // Quantize to 8 directions (45-degree increments)
            float angle = Mathf.Atan2(inputDir.y, inputDir.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / 45f) * 45f;
            float radians = angle * Mathf.Deg2Rad;
            
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        public override void Execute()
        {
            // Check if dash duration ended
            if (Time.time >= dashEndTime)
            {
                EndDash();
                return;
            }
        }

        public override void FixedExecute()
        {
            // Maintain dash velocity (override any external forces)
            Rb.linearVelocity = dashDirection * appliedDashSpeed;
        }

        private void EndDash()
        {
            // Apply end velocity multiplier
            float residualX = dashDirection.x * appliedDashSpeed * Data.dashEndSpeedMultiplier;
            Rb.linearVelocity = new Vector2(residualX, 0f); // Zero vertical velocity at end of dash

            // Notify for VFX/SFX
            player.OnDashEnded?.Invoke();

            // Transition to appropriate state
            if (Ground.IsGrounded)
            {
                stateMachine.ChangeState<GroundedState>();
            }
            else
            {
                stateMachine.ChangeState<AirborneState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            // Restore gravity
            Rb.gravityScale = Data.defaultGravityScale;

            // Cleanup
            player.PerfectDodgeCheck = null;
        }
    }
}
