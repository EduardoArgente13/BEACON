namespace BEACON.Player.States
{
    /// <summary>
    /// Interface for all player states in the FSM.
    /// Provides standard lifecycle methods for state behavior.
    /// </summary>
    public interface IPlayerState
    {
        /// <summary>
        /// Called once when entering this state.
        /// Use for initialization and setting up state-specific values.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called every frame (in Update).
        /// Use for input handling and non-physics logic.
        /// </summary>
        void Execute();

        /// <summary>
        /// Called every fixed frame (in FixedUpdate).
        /// Use for physics-based movement and calculations.
        /// </summary>
        void FixedExecute();

        /// <summary>
        /// Called once when exiting this state.
        /// Use for cleanup and resetting values.
        /// </summary>
        void Exit();
    }
}
