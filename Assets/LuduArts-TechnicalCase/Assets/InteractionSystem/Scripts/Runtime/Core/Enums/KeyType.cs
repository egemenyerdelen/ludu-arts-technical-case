namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums
{
    /// <summary>
    /// Defines the types of keys available in the game.
    /// Each key type can only unlock doors that require the same type.
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// No key type - used for unlocked doors or default state.
        /// </summary>
        None = 0,

        /// <summary>
        /// Gold key - typically for important/main doors.
        /// </summary>
        Gold = 1,

        /// <summary>
        /// Silver key - typically for secondary doors.
        /// </summary>
        Silver = 2,

        /// <summary>
        /// Bronze key - typically for common doors.
        /// </summary>
        Bronze = 3
    }
}
