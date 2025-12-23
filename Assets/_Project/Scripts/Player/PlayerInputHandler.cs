using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BEACON.Player
{
    /// <summary>
    /// Handles all player input using the new Input System.
    /// Decouples input reading from gameplay logic via events.
    /// All input is read in Update() for maximum responsiveness.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        // ============ INPUT EVENTS ============
        // Subscribe to these events from other components
        
        public event Action OnJumpPressed;
        public event Action OnJumpReleased;
        public event Action OnDashPressed;
        public event Action OnAttackPressed;
        public event Action OnParryPressed;
        public event Action OnParryReleased;
        public event Action OnSkillPressed;
        public event Action OnSkillReleased;
        public event Action OnInteractPressed;
        public event Action OnPausePressed;

        // ============ INPUT STATE ============
        // Readable properties for continuous input
        
        /// <summary>Current movement input (-1 to 1 on each axis)</summary>
        public Vector2 MoveInput { get; private set; }
        
        /// <summary>Is jump button currently held down</summary>
        public bool IsJumpHeld { get; private set; }
        
        /// <summary>Is parry button currently held down</summary>
        public bool IsParryHeld { get; private set; }
        
        /// <summary>Is attack button currently held down</summary>
        public bool IsAttackHeld { get; private set; }
        
        /// <summary>Is skill button currently held down</summary>
        public bool IsSkillHeld { get; private set; }

        /// <summary>Dash direction based on input or facing</summary>
        public Vector2 DashDirection { get; private set; }

        // ============ INPUT ACTIONS ============
        
        private PlayerInputActions inputActions;
        
        // ============ CONFIGURATION ============
        
        [Header("Input Settings")]
        [SerializeField, Tooltip("Deadzone for analog stick input")]
        private float analogDeadzone = 0.2f;

        [SerializeField, Tooltip("If true, snap diagonal input to cardinal directions for movement")]
        private bool snapToCardinal = false;

        // ============ INITIALIZATION ============

        private void Awake()
        {
            inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            inputActions.Enable();
            SubscribeToActions();
        }

        private void OnDisable()
        {
            UnsubscribeFromActions();
            inputActions.Disable();
        }

        private void SubscribeToActions()
        {
            // Movement is read continuously in Update, no callback needed
            
            // Jump
            inputActions.Gameplay.Jump.performed += OnJumpPerformed;
            inputActions.Gameplay.Jump.canceled += OnJumpCanceled;
            
            // Dash
            inputActions.Gameplay.Dash.performed += OnDashPerformed;
            
            // Attack
            inputActions.Gameplay.Attack.performed += OnAttackPerformed;
            inputActions.Gameplay.Attack.canceled += OnAttackCanceled;
            
            // Parry & Skill are handled manually in Update to bypass .inputactions file
            // inputActions.Gameplay.Parry.performed += OnParryPerformed;
            // inputActions.Gameplay.Parry.canceled += OnParryCanceled;
            // inputActions.Gameplay.Skill.performed += OnSkillPerformed;
            // inputActions.Gameplay.Skill.canceled += OnSkillCanceled;
            
            // Interact
            inputActions.Gameplay.Interact.performed += OnInteractPerformed;
            
            // Pause (UI action map)
            inputActions.UI.Pause.performed += OnPausePerformed;
        }

        private void UnsubscribeFromActions()
        {
            inputActions.Gameplay.Jump.performed -= OnJumpPerformed;
            inputActions.Gameplay.Jump.canceled -= OnJumpCanceled;
            inputActions.Gameplay.Dash.performed -= OnDashPerformed;
            inputActions.Gameplay.Attack.performed -= OnAttackPerformed;
            inputActions.Gameplay.Attack.canceled -= OnAttackCanceled;
            // inputActions.Gameplay.Parry.performed -= OnParryPerformed;
            // inputActions.Gameplay.Parry.canceled -= OnParryCanceled;
            // inputActions.Gameplay.Skill.performed -= OnSkillPerformed;
            // inputActions.Gameplay.Skill.canceled -= OnSkillCanceled;
            inputActions.Gameplay.Interact.performed -= OnInteractPerformed;
            inputActions.UI.Pause.performed -= OnPausePerformed;
        }

        // ============ UPDATE LOOP ============

        private void Update()
        {
            ReadMovementInput();
            UpdateDashDirection();
            HandleManualInputOverrides();
        }

        private void HandleManualInputOverrides()
        {
            var gamepad = Gamepad.current;
            var keyboard = Keyboard.current;

            // Reset Held States (will be set if button is down)
            IsSkillHeld = false;
            IsParryHeld = false;

            // --- GAMEPAD (RB = Special, LB = Parry) ---
            if (gamepad != null)
            {
                // RB -> Skill
                if (gamepad.rightShoulder.wasPressedThisFrame) 
                {
                    Debug.Log("[Input] RB Pressed (Special)");
                    OnSkillPressed?.Invoke();
                }
                if (gamepad.rightShoulder.wasReleasedThisFrame) OnSkillReleased?.Invoke();
                if (gamepad.rightShoulder.isPressed) IsSkillHeld = true;

                // LB -> Parry
                if (gamepad.leftShoulder.wasPressedThisFrame) 
                {
                    Debug.Log("[Input] LB Pressed (Parry)");
                    OnParryPressed?.Invoke();
                }
                if (gamepad.leftShoulder.wasReleasedThisFrame) OnParryReleased?.Invoke();
                if (gamepad.leftShoulder.isPressed) IsParryHeld = true;
            }

            // --- KEYBOARD (K = Special, L = Parry) ---
            if (keyboard != null)
            {
                // K -> Skill
                if (keyboard.kKey.wasPressedThisFrame) OnSkillPressed?.Invoke();
                if (keyboard.kKey.wasReleasedThisFrame) OnSkillReleased?.Invoke();
                if (keyboard.kKey.isPressed) IsSkillHeld = true;

                // L -> Parry
                if (keyboard.lKey.wasPressedThisFrame) OnParryPressed?.Invoke();
                if (keyboard.lKey.wasReleasedThisFrame) OnParryReleased?.Invoke();
                if (keyboard.lKey.isPressed) IsParryHeld = true;
            }
        }

        private void ReadMovementInput()
        {
            Vector2 rawInput = inputActions.Gameplay.Move.ReadValue<Vector2>();
            
            // Apply deadzone
            if (rawInput.magnitude < analogDeadzone)
            {
                MoveInput = Vector2.zero;
                return;
            }

            // Optionally snap to cardinal directions
            if (snapToCardinal)
            {
                if (Mathf.Abs(rawInput.x) > Mathf.Abs(rawInput.y))
                {
                    rawInput.y = 0f;
                }
                else
                {
                    rawInput.x = 0f;
                }
            }

            MoveInput = rawInput;
        }

        private void UpdateDashDirection()
        {
            // Dash direction is the normalized move input
            // If no input, dash in facing direction (handled by PlayerDash)
            if (MoveInput.magnitude > analogDeadzone)
            {
                DashDirection = MoveInput.normalized;
            }
            else
            {
                DashDirection = Vector2.zero; // Will use facing direction
            }
        }

        // ============ INPUT CALLBACKS ============

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            IsJumpHeld = true;
            OnJumpPressed?.Invoke();
        }

        private void OnJumpCanceled(InputAction.CallbackContext ctx)
        {
            IsJumpHeld = false;
            OnJumpReleased?.Invoke();
        }

        private void OnDashPerformed(InputAction.CallbackContext ctx)
        {
            OnDashPressed?.Invoke();
        }

        private void OnAttackPerformed(InputAction.CallbackContext ctx)
        {
            Debug.Log($"[PlayerInputHandler] Attack Input Received! (Phase: {ctx.phase})");
            IsAttackHeld = true;
            OnAttackPressed?.Invoke();
        }

        private void OnAttackCanceled(InputAction.CallbackContext ctx)
        {
            IsAttackHeld = false;
        }

        private void OnParryPerformed(InputAction.CallbackContext ctx)
        {
            IsParryHeld = true;
            OnParryPressed?.Invoke();
        }

        private void OnParryCanceled(InputAction.CallbackContext ctx)
        {
            IsParryHeld = false;
            OnParryReleased?.Invoke();
        }

        private void OnSkillPerformed(InputAction.CallbackContext ctx)
        {
            IsSkillHeld = true;
            OnSkillPressed?.Invoke();
        }

        private void OnSkillCanceled(InputAction.CallbackContext ctx)
        {
            IsSkillHeld = false;
            OnSkillReleased?.Invoke();
        }

        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            OnInteractPressed?.Invoke();
        }

        private void OnPausePerformed(InputAction.CallbackContext ctx)
        {
            OnPausePressed?.Invoke();
        }

        // ============ PUBLIC UTILITY METHODS ============

        /// <summary>
        /// Enables or disables gameplay input (useful for cutscenes, menus, etc.)
        /// </summary>
        public void SetGameplayInputEnabled(bool enabled)
        {
            if (enabled)
            {
                inputActions.Gameplay.Enable();
            }
            else
            {
                inputActions.Gameplay.Disable();
                // Reset held states
                IsJumpHeld = false;
                IsParryHeld = false;
                IsAttackHeld = false;
                MoveInput = Vector2.zero;
            }
        }

        /// <summary>
        /// Returns true if there's significant horizontal input
        /// </summary>
        public bool HasHorizontalInput()
        {
            return Mathf.Abs(MoveInput.x) > analogDeadzone;
        }

        /// <summary>
        /// Returns true if there's significant vertical input
        /// </summary>
        public bool HasVerticalInput()
        {
            return Mathf.Abs(MoveInput.y) > analogDeadzone;
        }

        /// <summary>
        /// Returns -1, 0, or 1 for horizontal input direction
        /// </summary>
        public int GetHorizontalInputSign()
        {
            if (MoveInput.x > analogDeadzone) return 1;
            if (MoveInput.x < -analogDeadzone) return -1;
            return 0;
        }

        /// <summary>
        /// Returns -1, 0, or 1 for vertical input direction
        /// </summary>
        public int GetVerticalInputSign()
        {
            if (MoveInput.y > analogDeadzone) return 1;
            if (MoveInput.y < -analogDeadzone) return -1;
            return 0;
        }
    }
}
