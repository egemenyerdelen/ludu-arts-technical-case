using System;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can be toggled between on/off states.
    /// Implement this for switches, levers, doors, etc.
    /// </summary>
    public interface IToggleable
    {
        #region Properties

        /// <summary>
        /// Gets whether the object is currently in the "on" state.
        /// </summary>
        bool IsOn { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when the toggle state changes.
        /// Parameter is the new state (true = on, false = off).
        /// </summary>
        event Action<bool> OnStateChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Toggles the object to the opposite state.
        /// </summary>
        void Toggle();

        /// <summary>
        /// Sets the object to a specific state.
        /// </summary>
        /// <param name="isOn">The desired state.</param>
        void SetState(bool isOn);

        #endregion
    }
}
