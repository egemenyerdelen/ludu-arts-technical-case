using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.UI
{
    /// <summary>
    /// Central manager for interaction UI elements.
    /// Handles prompts, progress bars, and feedback displays.
    /// </summary>
    public class InteractionUIManager : MonoBehaviour
    {
        #region Fields

        // Serialized private instance fields
        [Header("UI References")]
        [SerializeField] private InteractionPromptUI m_PromptUI;
        [SerializeField] private HoldProgressUI m_HoldProgressUI;
        [SerializeField] private InventoryUI m_InventoryUI;

        [Header("Settings")]
        [SerializeField] private bool m_AutoFindUI = true;

        // Non-serialized private instance fields
        private bool m_IsInitialized;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the prompt UI component.
        /// </summary>
        public InteractionPromptUI PromptUI => m_PromptUI;

        /// <summary>
        /// Gets the hold progress UI component.
        /// </summary>
        public HoldProgressUI HoldProgressUI => m_HoldProgressUI;

        /// <summary>
        /// Gets the inventory UI component.
        /// </summary>
        public InventoryUI InventoryUI => m_InventoryUI;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Initialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the interaction prompt.
        /// </summary>
        /// <param name="promptText">The prompt text to display.</param>
        /// <param name="keyText">The key/button text.</param>
        /// <param name="interactionType">The type of interaction.</param>
        public void ShowPrompt(string promptText, string keyText, InteractionType interactionType)
        {
            if (m_PromptUI == null)
            {
                return;
            }

            m_PromptUI.Show(promptText, keyText, interactionType);
        }

        /// <summary>
        /// Hides the interaction prompt.
        /// </summary>
        public void HidePrompt()
        {
            if (m_PromptUI == null)
            {
                return;
            }

            m_PromptUI.Hide();
        }

        /// <summary>
        /// Shows a "cannot interact" message.
        /// </summary>
        /// <param name="reason">The reason interaction is not possible.</param>
        public void ShowCannotInteract(string reason)
        {
            if (m_PromptUI == null)
            {
                return;
            }

            m_PromptUI.ShowCannotInteract(reason);
        }

        /// <summary>
        /// Shows or hides the hold progress bar.
        /// </summary>
        /// <param name="show">Whether to show the progress bar.</param>
        public void ShowHoldProgress(bool show)
        {
            if (m_HoldProgressUI == null)
            {
                return;
            }

            if (show)
            {
                m_HoldProgressUI.Show();
            }
            else
            {
                m_HoldProgressUI.Hide();
            }
        }

        /// <summary>
        /// Hides the hold progress bar.
        /// </summary>
        public void HideHoldProgress()
        {
            ShowHoldProgress(false);
        }

        /// <summary>
        /// Updates the hold progress bar.
        /// </summary>
        /// <param name="progress">Progress value from 0 to 1.</param>
        public void UpdateHoldProgress(float progress)
        {
            if (m_HoldProgressUI == null)
            {
                return;
            }

            m_HoldProgressUI.SetProgress(progress);
        }

        /// <summary>
        /// Refreshes the inventory UI.
        /// </summary>
        public void RefreshInventory()
        {
            if (m_InventoryUI == null)
            {
                return;
            }

            m_InventoryUI.RefreshDisplay();
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            if (m_IsInitialized)
            {
                return;
            }

            if (m_AutoFindUI)
            {
                FindUIComponents();
            }

            ValidateComponents();
            m_IsInitialized = true;
        }

        private void FindUIComponents()
        {
            if (m_PromptUI == null)
            {
                m_PromptUI = GetComponentInChildren<InteractionPromptUI>(true);
            }

            if (m_HoldProgressUI == null)
            {
                m_HoldProgressUI = GetComponentInChildren<HoldProgressUI>(true);
            }

            if (m_InventoryUI == null)
            {
                m_InventoryUI = GetComponentInChildren<InventoryUI>(true);
            }
        }

        private void ValidateComponents()
        {
            if (m_PromptUI == null)
            {
                Debug.LogWarning("[InteractionUIManager] PromptUI not found.", this);
            }

            if (m_HoldProgressUI == null)
            {
                Debug.LogWarning("[InteractionUIManager] HoldProgressUI not found.", this);
            }

            if (m_InventoryUI == null)
            {
                Debug.LogWarning("[InteractionUIManager] InventoryUI not found.", this);
            }
        }

        #endregion
    }
}
