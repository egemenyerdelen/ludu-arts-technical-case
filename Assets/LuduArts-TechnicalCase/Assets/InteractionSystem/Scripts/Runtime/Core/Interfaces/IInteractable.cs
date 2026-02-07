using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces
{
    /// <summary>
    /// Main interface for all interactable objects in the game world.
    /// Implement this interface to make any object interactable by the player.
    /// </summary>
    public interface IInteractable
    {
        #region Properties

        /// <summary>
        /// Gets the type of interaction this object supports.
        /// </summary>
        InteractionType InteractionType { get; }

        /// <summary>
        /// Gets the prompt text to display when player can interact.
        /// Example: "Press E to Open", "Hold E to Search"
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// Gets whether the object can currently be interacted with.
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// Gets the duration in seconds required to complete a hold interaction.
        /// Only relevant for InteractionType.Hold.
        /// </summary>
        float HoldDuration { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the player starts looking at/focusing on this object.
        /// Use for highlighting, UI prompts, etc.
        /// </summary>
        void OnFocusEnter();

        /// <summary>
        /// Called when the player stops looking at/focusing on this object.
        /// Use to remove highlighting, hide UI prompts, etc.
        /// </summary>
        void OnFocusExit();

        /// <summary>
        /// Called when the player performs an instant or toggle interaction.
        /// For Hold interactions, use OnHoldComplete instead.
        /// </summary>
        void OnInteract(InteractionDetector interactionDetector);

        /// <summary>
        /// Called when the player begins holding the interaction input.
        /// Only called for InteractionType.Hold objects.
        /// </summary>
        void OnHoldStart(InteractionDetector interactionDetector);

        /// <summary>
        /// Called every frame while holding, with normalized progress (0-1).
        /// Only called for InteractionType.Hold objects.
        /// </summary>
        /// <param name="progress">Normalized progress from 0 to 1.</param>
        void OnHoldProgress(float progress);

        /// <summary>
        /// Called when the hold interaction is successfully completed.
        /// Only called for InteractionType.Hold objects.
        /// </summary>
        void OnHoldComplete(InteractionDetector interactionDetector);

        /// <summary>
        /// Called when the hold interaction is cancelled before completion.
        /// Only called for InteractionType.Hold objects.
        /// </summary>
        void OnHoldCancel(InteractionDetector interactionDetector);

        #endregion
    }
}
