namespace InteractionSystem.Runtime.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can save and load their state.
    /// Implement this to persist object states across game sessions.
    /// </summary>
    public interface ISaveable
    {
        #region Properties

        /// <summary>
        /// Gets the unique identifier for this saveable object.
        /// Must be unique across all saveable objects in the scene.
        /// </summary>
        string UniqueId { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current state data to be saved.
        /// </summary>
        /// <returns>Object containing the state data to save.</returns>
        object GetSaveData();

        /// <summary>
        /// Loads and applies the saved state data.
        /// </summary>
        /// <param name="data">The previously saved state data.</param>
        void LoadSaveData(object data);

        #endregion
    }
}
