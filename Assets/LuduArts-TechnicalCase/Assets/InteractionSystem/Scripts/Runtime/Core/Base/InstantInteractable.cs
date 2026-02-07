using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base
{
    /// <summary>
    /// Base class for instant interaction objects.
    /// Use this for pickups, buttons, and any single-press interactions.
    /// </summary>
    public abstract class InstantInteractable : InteractableBase
    {
        #region Fields

        // Serialized private instance fields
        [Header("Instant Interaction Settings")]
        [SerializeField] private bool m_DestroyOnInteract;
        [SerializeField] private float m_DestroyDelay;
        [SerializeField] private bool m_DisableOnInteract = true;

        // Non-serialized private instance fields
        private bool m_HasBeenInteracted;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public sealed override InteractionType InteractionType => InteractionType.Instant;

        /// <inheritdoc/>
        public override bool CanInteract => base.CanInteract && !m_HasBeenInteracted;

        /// <summary>
        /// Gets whether this object has already been interacted with.
        /// </summary>
        protected bool HasBeenInteracted => m_HasBeenInteracted;

        #endregion

        #region Methods

        /// <summary>
        /// Resets the interaction state, allowing the object to be interacted with again.
        /// </summary>
        public void ResetInteraction()
        {
            m_HasBeenInteracted = false;
        }

        /// <inheritdoc/>
        protected sealed override void OnInteractInternal(InteractionDetector interactionDetector)
        {
            if (m_HasBeenInteracted && m_DisableOnInteract)
            {
                Debug.LogWarning($"[{GetType().Name}] Object has already been interacted with.", this);
                return;
            }

            // Perform the instant interaction
            PerformInstantInteraction();

            // Mark as interacted if configured to disable
            if (m_DisableOnInteract)
            {
                m_HasBeenInteracted = true;
            }

            // Handle destruction if configured
            if (m_DestroyOnInteract)
            {
                if (m_DestroyDelay > 0f)
                {
                    Destroy(gameObject, m_DestroyDelay);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Override this method to implement the specific instant interaction behavior.
        /// Called when the player interacts with this object.
        /// </summary>
        protected abstract void PerformInstantInteraction();

        #endregion

        #region Hold Interaction Overrides (Not Used)

        /// <summary>
        /// Not used for instant interactions.
        /// </summary>
        protected sealed override void OnHoldStartInternal(InteractionDetector interactionDetector) { }

        /// <summary>
        /// Not used for instant interactions.
        /// </summary>
        protected sealed override void OnHoldProgressInternal(float progress) { }

        /// <summary>
        /// Not used for instant interactions.
        /// </summary>
        protected sealed override void OnHoldCompleteInternal(InteractionDetector interactionDetector) { }

        /// <summary>
        /// Not used for instant interactions.
        /// </summary>
        protected sealed override void OnHoldCancelInternal() { }

        #endregion
    }
}
