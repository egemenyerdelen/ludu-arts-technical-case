using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can be locked and unlocked with keys.
    /// Implement this alongside IInteractable for lockable doors, chests, etc.
    /// </summary>
    public interface ILockable
    {
        #region Properties

        /// <summary>
        /// Gets whether the object is currently locked.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Gets the type of key required to unlock this object.
        /// Returns KeyType.None if the object doesn't require a key.
        /// </summary>
        KeyType RequiredKeyType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Attempts to unlock the object with the specified key type.
        /// </summary>
        /// <param name="keyType">The type of key to try.</param>
        /// <returns>True if successfully unlocked, false otherwise.</returns>
        bool TryUnlock(KeyType keyType);

        /// <summary>
        /// Locks the object. May require a key depending on implementation.
        /// </summary>
        void Lock();

        #endregion
    }
}
