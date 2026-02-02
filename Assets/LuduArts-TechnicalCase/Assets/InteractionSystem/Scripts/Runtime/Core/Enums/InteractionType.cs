namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums
{
    /// <summary>
    /// Defines the type of interaction an interactable object supports.
    /// </summary>
    public enum InteractionType
    {
        /// <summary>
        /// Single press interaction - activates immediately on input.
        /// Example: Picking up items, pressing buttons.
        /// </summary>
        Instant = 0,

        /// <summary>
        /// Hold interaction - requires holding input for a duration.
        /// Example: Opening chests.
        /// </summary>
        Hold = 1,

        /// <summary>
        /// Toggle interaction - switches between on/off states.
        /// Example: Doors, switches.
        /// </summary>
        Toggle = 2
    }
}
