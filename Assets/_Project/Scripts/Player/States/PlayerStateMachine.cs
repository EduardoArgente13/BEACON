using System;
using System.Collections.Generic;
using UnityEngine;

namespace BEACON.Player.States
{
    /// <summary>
    /// Generic state machine for managing player states.
    /// Handles state transitions, history, and lifecycle.
    /// </summary>
    public class PlayerStateMachine
    {
        // ============ STATE MANAGEMENT ============
        
        private IPlayerState currentState;
        private IPlayerState previousState;
        private readonly Dictionary<Type, IPlayerState> states = new Dictionary<Type, IPlayerState>();
        
        // ============ EVENTS ============
        
        /// <summary>Fired when transitioning to a new state. Args: (previousState, newState)</summary>
        public event Action<IPlayerState, IPlayerState> OnStateChanged;

        // ============ PROPERTIES ============

        /// <summary>Currently active state</summary>
        public IPlayerState CurrentState => currentState;

        /// <summary>Previously active state</summary>
        public IPlayerState PreviousState => previousState;

        /// <summary>Type of the current state</summary>
        public Type CurrentStateType => currentState?.GetType();

        // ============ REGISTRATION ============

        /// <summary>
        /// Registers a state instance with the state machine.
        /// </summary>
        public void RegisterState<T>(T state) where T : IPlayerState
        {
            var type = typeof(T);
            if (states.ContainsKey(type))
            {
                Debug.LogWarning($"[StateMachine] State {type.Name} already registered. Overwriting.");
            }
            states[type] = state;
        }

        /// <summary>
        /// Registers multiple states at once.
        /// </summary>
        public void RegisterStates(params IPlayerState[] stateList)
        {
            foreach (var state in stateList)
            {
                states[state.GetType()] = state;
            }
        }

        // ============ STATE TRANSITIONS ============

        /// <summary>
        /// Transitions to a new state by type.
        /// </summary>
        public void ChangeState<T>() where T : IPlayerState
        {
            var type = typeof(T);
            if (!states.TryGetValue(type, out var newState))
            {
                Debug.LogError($"[StateMachine] State {type.Name} not registered!");
                return;
            }

            ChangeStateInternal(newState);
        }

        /// <summary>
        /// Transitions to a new state by instance.
        /// </summary>
        public void ChangeState(IPlayerState newState)
        {
            if (newState == null)
            {
                Debug.LogError("[StateMachine] Cannot change to null state!");
                return;
            }

            ChangeStateInternal(newState);
        }

        private void ChangeStateInternal(IPlayerState newState)
        {
            // Don't transition to the same state
            if (currentState == newState)
            {
                return;
            }

            // Exit current state
            previousState = currentState;
            currentState?.Exit();

            // Enter new state
            currentState = newState;
            currentState.Enter();

            // Fire event
            OnStateChanged?.Invoke(previousState, currentState);

            #if UNITY_EDITOR
            Debug.Log($"[StateMachine] {previousState?.GetType().Name ?? "None"} -> {currentState.GetType().Name}");
            #endif
        }

        /// <summary>
        /// Returns to the previous state.
        /// </summary>
        public void RevertToPreviousState()
        {
            if (previousState != null)
            {
                ChangeState(previousState);
            }
        }

        // ============ UPDATE LOOP ============

        /// <summary>
        /// Call this in MonoBehaviour.Update()
        /// </summary>
        public void Update()
        {
            currentState?.Execute();
        }

        /// <summary>
        /// Call this in MonoBehaviour.FixedUpdate()
        /// </summary>
        public void FixedUpdate()
        {
            currentState?.FixedExecute();
        }

        // ============ QUERIES ============

        /// <summary>
        /// Checks if the current state is of a specific type.
        /// </summary>
        public bool IsInState<T>() where T : IPlayerState
        {
            return currentState is T;
        }

        /// <summary>
        /// Gets a registered state by type.
        /// </summary>
        public T GetState<T>() where T : IPlayerState
        {
            if (states.TryGetValue(typeof(T), out var state))
            {
                return (T)state;
            }
            return default;
        }

        /// <summary>
        /// Checks if a state type is registered.
        /// </summary>
        public bool HasState<T>() where T : IPlayerState
        {
            return states.ContainsKey(typeof(T));
        }
    }
}
