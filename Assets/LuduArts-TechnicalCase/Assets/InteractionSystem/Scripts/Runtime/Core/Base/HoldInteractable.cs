using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base
{
    /// <summary>
    /// Base class for hold interaction objects.
    /// Use this for chests, locks, and any interaction requiring held input.
    /// </summary>
    public abstract class HoldInteractable : InteractableBase
    {
        #region Fields

        // Private constant fields
        private const float k_MinHoldDuration = 0.1f;
        private const float k_MaxHoldDuration = 10f;

        // Serialized private instance fields
        [Header("Hold Interaction Settings")]
        [SerializeField] private float m_HoldDuration = 2f;
        [SerializeField] private bool m_OneTimeUse = true;
        [SerializeField] private bool m_ResetProgressOnCancel = true;

        // Non-serialized private instance fields
        private bool m_IsHolding;
        private float m_CurrentProgress;
        private bool m_HasBeenCompleted;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when hold progress updates. Parameter is normalized progress (0-1).
        /// </summary>
        public event Action<float> OnHoldProgressChanged;

        /// <summary>
        /// Event fired when hold interaction starts.
        /// </summary>
        public event Action OnHoldStarted;

        /// <summary>
        /// Event fired when hold interaction is completed.
        /// </summary>
        public event Action OnHoldCompleted;

        /// <summary>
        /// Event fired when hold interaction is cancelled.
        /// </summary>
        public event Action OnHoldCancelled;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override InteractionType InteractionType => InteractionType.Hold;

        /// <inheritdoc/>
        public sealed override float HoldDuration => m_HoldDuration;

        /// <inheritdoc/>
        public override bool CanInteract => base.CanInteract && (!m_OneTimeUse || !m_HasBeenCompleted);

        /// <summary>
        /// Gets whether the hold interaction is currently in progress.
        /// </summary>
        public bool IsHolding => m_IsHolding;

        /// <summary>
        /// Gets the current hold progress (0-1).
        /// </summary>
        public float CurrentProgress => m_CurrentProgress;

        /// <summary>
        /// Gets whether this hold interaction has been completed.
        /// </summary>
        public bool HasBeenCompleted => m_HasBeenCompleted;

        #endregion

        #region Unity Methods

        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Clamp hold duration to valid range
            m_HoldDuration = Mathf.Clamp(m_HoldDuration, k_MinHoldDuration, k_MaxHoldDuration);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            // Cancel hold if disabled during hold
            if (m_IsHolding)
            {
                CancelHold();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the hold interaction state.
        /// </summary>
        public void ResetHoldInteraction()
        {
            m_HasBeenCompleted = false;
            m_CurrentProgress = 0f;
            m_IsHolding = false;
        }

        /// <summary>
        /// Sets the hold duration.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        public void SetHoldDuration(float duration)
        {
            m_HoldDuration = Mathf.Clamp(duration, k_MinHoldDuration, k_MaxHoldDuration);
        }

        private void CancelHold()
        {
            m_IsHolding = false;
            
            if (m_ResetProgressOnCancel)
            {
                m_CurrentProgress = 0f;
            }
        }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected sealed override void OnHoldStartInternal(InteractionDetector interactionDetector)
        {
            if (m_HasBeenCompleted && m_OneTimeUse)
            {
                Debug.LogWarning($"[{GetType().Name}] Hold interaction already completed.", this);
                return;
            }

            m_IsHolding = true;
            m_CurrentProgress = 0f;
            
            OnHoldBegin(interactionDetector);
            OnHoldStarted?.Invoke();
        }

        /// <inheritdoc/>
        protected sealed override void OnHoldProgressInternal(float progress)
        {
            if (!m_IsHolding)
            {
                return;
            }

            m_CurrentProgress = Mathf.Clamp01(progress);
            OnHoldUpdate(m_CurrentProgress);
            OnHoldProgressChanged?.Invoke(m_CurrentProgress);
        }

        /// <inheritdoc/>
        protected sealed override void OnHoldCompleteInternal(InteractionDetector interactionDetector)
        {
            if (!m_IsHolding)
            {
                return;
            }

            m_IsHolding = false;
            m_CurrentProgress = 1f;
            m_HasBeenCompleted = true;

            PerformHoldInteraction(interactionDetector);
            OnHoldCompleted?.Invoke();
        }

        /// <inheritdoc/>
        protected sealed override void OnHoldCancelInternal()
        {
            if (!m_IsHolding)
            {
                return;
            }

            CancelHold();
            OnHoldCancelledInternal();
            OnHoldCancelled?.Invoke();
        }

        /// <summary>
        /// Not used for hold interactions - use OnHoldCompleteInternal instead.
        /// </summary>
        protected override void OnInteractInternal(InteractionDetector interactionDetector)
        {
            // Hold interactions don't use instant interact
            Debug.LogWarning($"[{GetType().Name}] Hold interaction triggered via OnInteract. Use hold methods instead.", this);
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Called when the hold interaction begins. Override for custom behavior.
        /// </summary>
        protected virtual void OnHoldBegin(InteractionDetector interactor) { }

        /// <summary>
        /// Called each frame during hold with current progress. Override for custom behavior.
        /// </summary>
        /// <param name="progress">Normalized progress from 0 to 1.</param>
        protected virtual void OnHoldUpdate(float progress) { }

        /// <summary>
        /// Called when the hold interaction is cancelled. Override for custom behavior.
        /// </summary>
        protected virtual void OnHoldCancelledInternal() { }

        /// <summary>
        /// Override this method to implement the specific hold interaction behavior.
        /// Called when the hold duration is completed.
        /// </summary>
        protected abstract void PerformHoldInteraction(InteractionDetector interactor);

        #endregion
    }
}
