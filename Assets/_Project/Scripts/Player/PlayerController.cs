using System;
using UnityEngine;
using BEACON.Player.Data;
using BEACON.Player.States;

namespace BEACON.Player
{
    /// <summary>
    /// Main player controller that orchestrates all movement systems.
    /// Acts as the central hub connecting input, states, and physics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(GroundChecker))]
    public class PlayerController : MonoBehaviour
    {
        // ============ EVENTS FOR EXTERNAL SYSTEMS ============
        
        /// <summary>Fired when dash starts. Arg: dash direction</summary>
        public Action<Vector2> OnDashStarted;
        
        /// <summary>Fired when dash ends</summary>
        public Action OnDashEnded;
        
        /// <summary>Fired when player lands on ground</summary>
        public Action OnLanded;
        
        /// <summary>Fired when player jumps</summary>
        public Action OnJumped;
        
        /// <summary>Fired when player gets hurt. Args: damage, knockback direction</summary>
        public Action<float, Vector2> OnHurt;
        
        /// <summary>Fired when facing direction changes. Arg: is facing right</summary>
        public Action<bool> OnFlipped;

        /// <summary>Fired on successful Perfect Dodge</summary>
        public Action OnPerfectDodge;

        // ============ COMPONENTS ============
        
        [Header("Required Components")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private GroundChecker groundChecker;
        [SerializeField] private WallChecker wallChecker;
        
        [Header("Data")]
        [SerializeField] private PlayerMovementData movementData;

        // ============ PUBLIC ACCESSORS ============
        
        public Rigidbody2D Rb => rb;
        public PlayerInputHandler InputHandler => inputHandler;
        public GroundChecker GroundChecker => groundChecker;
        public WallChecker WallChecker => wallChecker;
        public PlayerMovementData MovementData => movementData;
        public PlayerStateMachine StateMachine => stateMachine;

        // ============ STATE ============
        
        private PlayerStateMachine stateMachine;
        
        // Facing
        private bool isFacingRight = true;
        public bool IsFacingRight => isFacingRight;
        
        // Jump state
        private bool jumpRequested;
        private float jumpBufferTimer;
        private float coyoteTimer;
        private bool usedCoyoteJump;
        
        // Dash state
        private bool dashRequested;
        private float dashCooldownTimer;
        private int airDashesRemaining;
        private bool isInvincible;
        private float iFramesTimer;
        
        // Wall jump
        private float wallJumpLockTimer;
        
        // Combat overrides
        private float movementOverrideTimer;
        public bool IsMovementOverridden => movementOverrideTimer > 0;

        // ============ PUBLIC STATE QUERIES ============
        
        public bool JumpRequested => jumpRequested;
        public bool DashRequested => dashRequested;
        public bool HasBufferedJump => jumpBufferTimer > 0;
        public bool IsCoyoteTimeActive => coyoteTimer > 0 && !usedCoyoteJump;
        public bool IsInvincible => isInvincible;
        public bool IsWallJumpLocked => wallJumpLockTimer > 0;
        
        public bool CanDash
        {
            get
            {
                if (dashCooldownTimer > 0) return false;
                if (!groundChecker.IsGrounded && airDashesRemaining <= 0) return false;
                return true;
            }
        }

        // ============ INITIALIZATION ============

        private void Awake()
        {
            // Get components - using Unity null check (== null) instead of ??= 
            // because serialized "None" references aren't true C# nulls
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
            if (groundChecker == null) groundChecker = GetComponent<GroundChecker>();
            if (wallChecker == null) wallChecker = GetComponentInChildren<WallChecker>();

            // Validate
            if (movementData == null)
            {
                Debug.LogError("[PlayerController] MovementData not assigned!");
            }

            // Configure Rigidbody
            ConfigureRigidbody();

            // Initialize state machine
            InitializeStateMachine();

            // Initialize dash count
            airDashesRemaining = movementData.airDashesAllowed;
        }

        private void OnEnable()
        {
            // Subscribe to input events
            inputHandler.OnJumpPressed += HandleJumpPressed;
            inputHandler.OnJumpReleased += HandleJumpReleased;
            inputHandler.OnDashPressed += HandleDashPressed;
        }

        private void OnDisable()
        {
            // Unsubscribe from input events
            inputHandler.OnJumpPressed -= HandleJumpPressed;
            inputHandler.OnJumpReleased -= HandleJumpReleased;
            inputHandler.OnDashPressed -= HandleDashPressed;
        }

        private void ConfigureRigidbody()
        {
            rb.gravityScale = movementData.defaultGravityScale;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void InitializeStateMachine()
        {
            stateMachine = new PlayerStateMachine();

            // Create and register states
            var groundedState = new GroundedState(this, stateMachine);
            var airborneState = new AirborneState(this, stateMachine);
            var dashState = new DashState(this, stateMachine);
            var wallSlideState = new WallSlideState(this, stateMachine);

            stateMachine.RegisterStates(groundedState, airborneState, dashState, wallSlideState);

            // Start in grounded state
            stateMachine.ChangeState<GroundedState>();
        }

        // ============ UPDATE LOOP ============

        private void Update()
        {
            UpdateTimers();
            stateMachine.Update();
            
            // Reset single-frame flags
            ClearFrameFlags();
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        private void UpdateTimers()
        {
            // Jump buffer countdown
            if (jumpBufferTimer > 0)
            {
                jumpBufferTimer -= Time.deltaTime;
            }

            // Coyote time countdown
            if (coyoteTimer > 0)
            {
                coyoteTimer -= Time.deltaTime;
            }

            // Dash cooldown
            if (dashCooldownTimer > 0)
            {
                dashCooldownTimer -= Time.deltaTime;
            }

            // I-frames
            if (iFramesTimer > 0)
            {
                iFramesTimer -= Time.deltaTime;
                if (iFramesTimer <= 0)
                {
                    isInvincible = false;
                }
            }

            // Wall jump lock
            if (wallJumpLockTimer > 0)
            {
                wallJumpLockTimer -= Time.deltaTime;
            }

            // Combat movement override
            if (movementOverrideTimer > 0)
            {
                movementOverrideTimer -= Time.deltaTime;
            }
        }

        private void ClearFrameFlags()
        {
            jumpRequested = false;
            dashRequested = false;
        }

        // ============ INPUT HANDLERS ============

        private void HandleJumpPressed()
        {
            jumpRequested = true;
            BufferJump();
        }

        private void HandleJumpReleased()
        {
            // Jump cut is handled in AirborneState
        }

        private void HandleDashPressed()
        {
            dashRequested = true;
        }

        // ============ PUBLIC METHODS FOR STATES ============

        /// <summary>
        /// Executes a jump. Called by states when jump should occur.
        /// </summary>
        public void ExecuteJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, movementData.jumpForce);
            jumpBufferTimer = 0;
            usedCoyoteJump = true;
            coyoteTimer = 0;

            OnJumped?.Invoke();
        }

        /// <summary>
        /// Starts coyote time countdown.
        /// </summary>
        public void StartCoyoteTime()
        {
            coyoteTimer = movementData.coyoteTime;
            usedCoyoteJump = false;
        }

        /// <summary>
        /// Buffers a jump input for execution on landing.
        /// </summary>
        public void BufferJump()
        {
            jumpBufferTimer = movementData.jumpBufferTime;
        }

        /// <summary>
        /// Resets air dash count. Called when landing.
        /// </summary>
        public void ResetAirDashes()
        {
            airDashesRemaining = movementData.airDashesAllowed;
        }

        /// <summary>
        /// Consumes one air dash. Called during air dash.
        /// </summary>
        public void ConsumeAirDash()
        {
            airDashesRemaining = Mathf.Max(0, airDashesRemaining - 1);
        }

        /// <summary>
        /// Starts dash cooldown timer.
        /// </summary>
        public void StartDashCooldown()
        {
            dashCooldownTimer = movementData.dashCooldown;
        }

        /// <summary>
        /// Starts i-frames for the specified duration.
        /// </summary>
        public void StartIFrames(float duration)
        {
            isInvincible = true;
            iFramesTimer = duration;
        }

        /// <summary>
        /// Starts wall jump input lock.
        /// </summary>
        public void StartWallJumpLock()
        {
            wallJumpLockTimer = movementData.wallJumpLockTime;
        }

        /// <summary>
        /// Flips the player's facing direction.
        /// </summary>
        public void Flip()
        {
            isFacingRight = !isFacingRight;
            
            // Flip the sprite by scaling
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1 : -1);
            transform.localScale = scale;

            OnFlipped?.Invoke(isFacingRight);
        }

        /// <summary>
        /// Sets facing direction without toggling.
        /// </summary>
        public void SetFacing(bool facingRight)
        {
            if (isFacingRight != facingRight)
            {
                Flip();
            }
        }

        // ============ EXTERNAL INTERACTIONS ============

        /// <summary>
        /// Applies knockback from external source (damage, parry, etc.)
        /// </summary>
        public void ApplyKnockback(Vector2 direction, float force, float overrideDuration = 0)
        {
            rb.linearVelocity = direction.normalized * force;
            if (overrideDuration > 0)
            {
                movementOverrideTimer = overrideDuration;
            }
        }

        /// <summary>
        /// Called when player takes damage.
        /// Returns true if damage was applied, false if invincible.
        /// </summary>
        public bool TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
        {
            string currentState = stateMachine?.CurrentStateType?.Name ?? "None";
            // Debug.Log($"[Player] TakeDamage. State: {currentState}, Invincible: {isInvincible}");

            // Check for Perfect Dodge opportunity first
            if (CheckPerfectDodge())
            {
                Debug.Log($"<color=green>[Player] Damage Negated by Perfect Dodge! (State: {currentState})</color>");
                return false; // Damage negated by Perfect Dodge
            }

            if (isInvincible)
            {
                Debug.Log($"[Player] Damage Negated by Invincibility. (State: {currentState})");
                return false;
            }

            Debug.Log($"<color=red>[Player] OUCH! Damage Taken. (State: {currentState})</color>");
            ApplyKnockback(knockbackDir, knockbackForce);
            OnHurt?.Invoke(damage, knockbackDir);
            
            // Brief i-frames after damage
            StartIFrames(0.5f);

            return true;
        }

        // Delegate to allow states to separate "Invincible" from "Perfect Dodge" logic
        public Func<bool> PerfectDodgeCheck;

        private bool CheckPerfectDodge()
        {
            if (PerfectDodgeCheck != null)
            {
                return PerfectDodgeCheck.Invoke();
            }
            return false;
        }

        // ============ DEBUG ============

        #if UNITY_EDITOR
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"State: {stateMachine.CurrentStateType?.Name ?? "None"}");
            GUILayout.Label($"Grounded: {groundChecker.IsGrounded}");
            GUILayout.Label($"Velocity: {rb.linearVelocity}");
            GUILayout.Label($"Air Dashes: {airDashesRemaining}");
            GUILayout.Label($"Coyote: {coyoteTimer:F2}");
            GUILayout.Label($"Jump Buffer: {jumpBufferTimer:F2}");
            GUILayout.Label($"Invincible: {isInvincible}");
            GUILayout.EndArea();
        }
        #endif
    }
}
