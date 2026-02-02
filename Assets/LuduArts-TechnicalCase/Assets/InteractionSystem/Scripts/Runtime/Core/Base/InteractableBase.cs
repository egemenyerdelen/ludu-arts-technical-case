using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Data;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base
{
    /// <summary>
    /// Abstract base class for all interactable objects.
    /// Provides common functionality and enforces the IInteractable contract.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        #region Fields

        // Private constant fields
        private const float k_DefaultHoldDuration = 0f;

        // Serialized private instance fields
        [Header("Interaction Settings")]
        [SerializeField] private string m_InteractionPrompt = "Interact";
        [SerializeField] private bool m_IsInteractable = true;

        [Header("Visual Feedback")]
        [SerializeField] private bool m_UseHighlight = true;
        [SerializeField] private Material m_HighlightMaterial;

        // Non-serialized private instance fields
        private bool m_IsFocused;
        private Renderer[] m_Renderers;
        private Material[] m_OriginalMaterials;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when the object is interacted with.
        /// </summary>
        public event Action<InteractionData> OnInteracted;

        /// <summary>
        /// Event fired when focus enters this object.
        /// </summary>
        public event Action OnFocusEntered;

        /// <summary>
        /// Event fired when focus exits this object.
        /// </summary>
        public event Action OnFocusExited;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public abstract InteractionType InteractionType { get; }

        /// <inheritdoc/>
        public virtual string InteractionPrompt => m_InteractionPrompt;

        /// <inheritdoc/>
        public virtual bool CanInteract => m_IsInteractable && enabled && gameObject.activeInHierarchy;

        /// <inheritdoc/>
        public virtual float HoldDuration => k_DefaultHoldDuration;

        /// <summary>
        /// Gets whether this object is currently being focused by the player.
        /// </summary>
        public bool IsFocused => m_IsFocused;

        /// <summary>
        /// Gets or sets whether highlighting is enabled for this object.
        /// </summary>
        protected bool UseHighlight
        {
            get => m_UseHighlight;
            set => m_UseHighlight = value;
        }

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            CacheRenderers();
        }

        protected virtual void OnEnable()
        {
            // Reset state when enabled
            m_IsFocused = false;
        }

        protected virtual void OnDisable()
        {
            // Clean up highlight if disabled while focused
            if (m_IsFocused)
            {
                RemoveHighlight();
                m_IsFocused = false;
            }
        }

        protected virtual void OnValidate()
        {
            // Ensure prompt is not empty
            if (string.IsNullOrEmpty(m_InteractionPrompt))
            {
                m_InteractionPrompt = "Interact";
                Debug.LogWarning($"[{GetType().Name}] Interaction prompt was empty, reset to default.", this);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the interaction prompt text.
        /// </summary>
        /// <param name="prompt">The new prompt text.</param>
        public void SetInteractionPrompt(string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                Debug.LogError($"[{GetType().Name}] Cannot set empty interaction prompt.", this);
                return;
            }

            m_InteractionPrompt = prompt;
        }

        /// <summary>
        /// Sets whether this object can be interacted with.
        /// </summary>
        /// <param name="canInteract">Whether interaction is enabled.</param>
        public void SetInteractable(bool canInteract)
        {
            m_IsInteractable = canInteract;
        }

        /// <summary>
        /// Raises the OnInteracted event with the given data.
        /// </summary>
        /// <param name="data">The interaction data.</param>
        protected void RaiseOnInteracted(InteractionData data)
        {
            OnInteracted?.Invoke(data);
        }

        /// <summary>
        /// Creates interaction data for the current interaction.
        /// </summary>
        /// <param name="interactor">The GameObject initiating the interaction.</param>
        /// <returns>The created InteractionData.</returns>
        protected InteractionData CreateInteractionData(GameObject interactor)
        {
            return new InteractionData(
                gameObject,
                InteractionType,
                interactor,
                transform.position
            );
        }

        private void CacheRenderers()
        {
            m_Renderers = GetComponentsInChildren<Renderer>();
            
            if (m_Renderers == null || m_Renderers.Length == 0)
            {
                Debug.LogWarning($"[{GetType().Name}] No renderers found for highlight system.", this);
                return;
            }

            m_OriginalMaterials = new Material[m_Renderers.Length];
            for (int i = 0; i < m_Renderers.Length; i++)
            {
                if (m_Renderers[i] != null)
                {
                    m_OriginalMaterials[i] = m_Renderers[i].material;
                }
            }
        }

        private void ApplyHighlight()
        {
            if (!m_UseHighlight || m_HighlightMaterial == null || m_Renderers == null)
            {
                return;
            }

            for (int i = 0; i < m_Renderers.Length; i++)
            {
                if (m_Renderers[i] != null)
                {
                    // Add highlight material as additional material
                    Material[] materials = m_Renderers[i].materials;
                    Material[] newMaterials = new Material[materials.Length + 1];
                    materials.CopyTo(newMaterials, 0);
                    newMaterials[materials.Length] = m_HighlightMaterial;
                    m_Renderers[i].materials = newMaterials;
                }
            }
        }

        private void RemoveHighlight()
        {
            if (!m_UseHighlight || m_Renderers == null || m_OriginalMaterials == null)
            {
                return;
            }

            for (int i = 0; i < m_Renderers.Length; i++)
            {
                if (m_Renderers[i] != null && m_OriginalMaterials[i] != null)
                {
                    m_Renderers[i].material = m_OriginalMaterials[i];
                }
            }
        }

        #endregion

        #region Interface Implementations

        /// <inheritdoc/>
        void IInteractable.OnFocusEnter()
        {
            if (m_IsFocused)
            {
                return;
            }

            m_IsFocused = true;
            ApplyHighlight();
            OnFocusEnterInternal();
            OnFocusEntered?.Invoke();
        }

        /// <inheritdoc/>
        void IInteractable.OnFocusExit()
        {
            if (!m_IsFocused)
            {
                return;
            }

            m_IsFocused = false;
            RemoveHighlight();
            OnFocusExitInternal();
            OnFocusExited?.Invoke();
        }

        /// <inheritdoc/>
        void IInteractable.OnInteract()
        {
            if (!CanInteract)
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot interact - object is not interactable.", this);
                return;
            }

            OnInteractInternal();
        }

        /// <inheritdoc/>
        void IInteractable.OnHoldStart()
        {
            if (!CanInteract)
            {
                return;
            }

            OnHoldStartInternal();
        }

        /// <inheritdoc/>
        void IInteractable.OnHoldProgress(float progress)
        {
            OnHoldProgressInternal(progress);
        }

        /// <inheritdoc/>
        void IInteractable.OnHoldComplete()
        {
            if (!CanInteract)
            {
                return;
            }

            OnHoldCompleteInternal();
        }

        /// <inheritdoc/>
        void IInteractable.OnHoldCancel()
        {
            OnHoldCancelInternal();
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Called when focus enters this object. Override for custom behavior.
        /// </summary>
        protected virtual void OnFocusEnterInternal() { }

        /// <summary>
        /// Called when focus exits this object. Override for custom behavior.
        /// </summary>
        protected virtual void OnFocusExitInternal() { }

        /// <summary>
        /// Called when the object is interacted with. Override for custom behavior.
        /// </summary>
        protected virtual void OnInteractInternal() { }

        /// <summary>
        /// Called when hold interaction starts. Override for custom behavior.
        /// </summary>
        protected virtual void OnHoldStartInternal() { }

        /// <summary>
        /// Called during hold interaction with progress. Override for custom behavior.
        /// </summary>
        /// <param name="progress">Normalized progress from 0 to 1.</param>
        protected virtual void OnHoldProgressInternal(float progress) { }

        /// <summary>
        /// Called when hold interaction completes. Override for custom behavior.
        /// </summary>
        protected virtual void OnHoldCompleteInternal() { }

        /// <summary>
        /// Called when hold interaction is cancelled. Override for custom behavior.
        /// </summary>
        protected virtual void OnHoldCancelInternal() { }

        #endregion
    }
}
