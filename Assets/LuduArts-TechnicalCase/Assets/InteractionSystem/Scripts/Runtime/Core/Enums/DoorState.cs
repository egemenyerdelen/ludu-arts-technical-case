namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums
{
    /// <summary>
    /// Defines the possible states of a door.
    /// </summary>
    public enum DoorState
    {
        /// <summary>
        /// Door is closed.
        /// </summary>
        Closed = 0,

        /// <summary>
        /// Door is open.
        /// </summary>
        Open = 1,

        /// <summary>
        /// Door is locked and requires a key to unlock.
        /// </summary>
        Locked = 2
    }
}
