using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base
{
    /// <summary>
    /// Base class for toggle interaction objects.
    /// Use this for doors, switches, levers, and any on/off interactions.
    /// </summary>
    public abstract class ToggleInteractable : InteractableBase, IToggleable
    {
        #region Fields

        // Serialized private instance fields
        [Header("Toggle Interaction Settings")]
        [SerializeField] private bool m_StartOn;
        [SerializeField] private string m_OnPrompt = "Turn Off";
        [SerializeField] private string m_OffPrompt = "Turn On";

        // Non-serialized private instance fields
        private bool m_IsOn;

        #endregion

        #region Events

        /// <inheritdoc/>
        public event Action<bool> OnStateChanged;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public sealed override InteractionType InteractionType => InteractionType.Toggle;

        /// <inheritdoc/>
        public bool IsOn => m_IsOn;

        /// <inheritdoc/>
        public override string InteractionPrompt => m_IsOn ? m_OnPrompt : m_OffPrompt;

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();
            
            // Initialize to starting state without triggering events
            m_IsOn = m_StartOn;
        }

        protected virtual void Start()
        {
            // Apply initial state
            ApplyState(m_IsOn, false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Toggle()
        {
            SetState(!m_IsOn);
        }

        /// <inheritdoc/>
        public void SetState(bool isOn)
        {
            if (m_IsOn == isOn)
            {
                return;
            }

            var previousState = m_IsOn;
            m_IsOn = isOn;

            ApplyState(m_IsOn, true);
            OnStateChanged?.Invoke(m_IsOn);
            OnToggleStateChanged(previousState, m_IsOn);
        }

        /// <summary>
        /// Sets the prompt texts for on and off states.
        /// </summary>
        /// <param name="onPrompt">Prompt shown when object is on.</param>
        /// <param name="offPrompt">Prompt shown when object is off.</param>
        public void SetPrompts(string onPrompt, string offPrompt)
        {
            if (!string.IsNullOrEmpty(onPrompt))
            {
                m_OnPrompt = onPrompt;
            }

            if (!string.IsNullOrEmpty(offPrompt))
            {
                m_OffPrompt = offPrompt;
            }
        }

        /// <inheritdoc/>
        protected override void OnInteractInternal()
        {
            Toggle();
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Called when the toggle state changes. Override for custom behavior.
        /// </summary>
        /// <param name="previousState">The previous state.</param>
        /// <param name="newState">The new state.</param>
        protected virtual void OnToggleStateChanged(bool previousState, bool newState) { }

        /// <summary>
        /// Apply the current state visually and functionally.
        /// </summary>
        /// <param name="isOn">The state to apply.</param>
        /// <param name="animate">Whether to animate the state change.</param>
        protected abstract void ApplyState(bool isOn, bool animate);

        #endregion

        #region Hold Interaction Overrides (Not Used)

        /// <summary>
        /// Not used for toggle interactions.
        /// </summary>
        protected sealed override void OnHoldStartInternal() { }

        /// <summary>
        /// Not used for toggle interactions.
        /// </summary>
        protected sealed override void OnHoldProgressInternal(float progress) { }

        /// <summary>
        /// Not used for toggle interactions.
        /// </summary>
        protected sealed override void OnHoldCompleteInternal() { }

        /// <summary>
        /// Not used for toggle interactions.
        /// </summary>
        protected sealed override void OnHoldCancelInternal() { }

        #endregion
    }
}
