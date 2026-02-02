using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.UI
{
    /// <summary>
    /// UI component that displays interaction prompts to the player.
    /// Shows dynamic text based on the interactable object.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        #region Fields

        // Private constant fields
        private const string k_DefaultPromptFormat = "Press [{0}] to {1}";
        private const string k_HoldPromptFormat = "Hold [{0}] to {1}";
        private const float k_DefaultFadeSpeed = 10f;

        // Serialized private instance fields
        [Header("UI References")]
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private TextMeshProUGUI m_PromptText;
        [SerializeField] private TextMeshProUGUI m_KeyText;
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private Image m_IconImage;

        [Header("Settings")]
        [SerializeField] private bool m_UseAnimation = true;
        [SerializeField] private float m_FadeSpeed = 10f;
        [SerializeField] private string m_PromptFormat = "Press [{0}] to {1}";
        [SerializeField] private string m_HoldPromptFormat = "Hold [{0}] to {1}";

        [Header("Colors")]
        [SerializeField] private Color m_NormalColor = Color.white;
        [SerializeField] private Color m_CannotInteractColor = new Color(1f, 0.5f, 0.5f);
        [SerializeField] private Color m_HoldColor = new Color(1f, 0.9f, 0.5f);

        [Header("Icons")]
        [SerializeField] private Sprite m_InstantIcon;
        [SerializeField] private Sprite m_HoldIcon;
        [SerializeField] private Sprite m_ToggleIcon;
        [SerializeField] private Sprite m_LockedIcon;

        // Non-serialized private instance fields
        private float m_TargetAlpha;
        private bool m_IsVisible;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the prompt is currently visible.
        /// </summary>
        public bool IsVisible => m_IsVisible;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ValidateComponents();
            
            // Start hidden
            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
            }
            
            m_TargetAlpha = 0f;
            m_IsVisible = false;
        }

        private void Update()
        {
            UpdateFade();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the interaction prompt.
        /// </summary>
        /// <param name="promptText">The action text (e.g., "Open Door").</param>
        /// <param name="keyText">The key text (e.g., "E").</param>
        /// <param name="interactionType">The type of interaction.</param>
        public void Show(string promptText, string keyText, InteractionType interactionType)
        {
            m_IsVisible = true;
            m_TargetAlpha = 1f;

            // Format prompt based on interaction type
            var format = interactionType == InteractionType.Hold ? m_HoldPromptFormat : m_PromptFormat;
            var formattedPrompt = string.Format(format, keyText, promptText);

            // Update text
            if (m_PromptText != null)
            {
                m_PromptText.text = formattedPrompt;
                m_PromptText.color = interactionType == InteractionType.Hold ? m_HoldColor : m_NormalColor;
            }

            if (m_KeyText != null)
            {
                m_KeyText.text = keyText;
            }

            // Update icon
            UpdateIcon(interactionType);

            // Immediate show if animation disabled
            if (!m_UseAnimation && m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Shows a "cannot interact" message.
        /// </summary>
        /// <param name="reason">The reason for inability to interact.</param>
        public void ShowCannotInteract(string reason)
        {
            m_IsVisible = true;
            m_TargetAlpha = 1f;

            if (m_PromptText != null)
            {
                m_PromptText.text = reason;
                m_PromptText.color = m_CannotInteractColor;
            }

            if (m_KeyText != null)
            {
                m_KeyText.text = "";
            }

            // Show locked icon
            if (m_IconImage != null && m_LockedIcon != null)
            {
                m_IconImage.sprite = m_LockedIcon;
                m_IconImage.enabled = true;
            }

            if (!m_UseAnimation && m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Hides the interaction prompt.
        /// </summary>
        public void Hide()
        {
            m_IsVisible = false;
            m_TargetAlpha = 0f;

            if (!m_UseAnimation && m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Sets the prompt format string.
        /// </summary>
        /// <param name="format">Format string with {0} for key and {1} for action.</param>
        public void SetPromptFormat(string format)
        {
            if (!string.IsNullOrEmpty(format))
            {
                m_PromptFormat = format;
            }
        }

        #endregion

        #region Private Methods

        private void ValidateComponents()
        {
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
                
                if (m_CanvasGroup == null)
                {
                    m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (m_PromptText == null)
            {
                m_PromptText = GetComponentInChildren<TextMeshProUGUI>();
                
                if (m_PromptText == null)
                {
                    Debug.LogWarning("[InteractionPromptUI] No TextMeshProUGUI found.", this);
                }
            }
        }

        private void UpdateFade()
        {
            if (m_CanvasGroup == null || !m_UseAnimation)
            {
                return;
            }

            var currentAlpha = m_CanvasGroup.alpha;
            
            if (!Mathf.Approximately(currentAlpha, m_TargetAlpha))
            {
                m_CanvasGroup.alpha = Mathf.MoveTowards(currentAlpha, m_TargetAlpha, m_FadeSpeed * Time.deltaTime);
            }
        }

        private void UpdateIcon(InteractionType interactionType)
        {
            if (m_IconImage == null)
            {
                return;
            }

            Sprite icon = null;

            switch (interactionType)
            {
                case InteractionType.Instant:
                    icon = m_InstantIcon;
                    break;
                case InteractionType.Hold:
                    icon = m_HoldIcon;
                    break;
                case InteractionType.Toggle:
                    icon = m_ToggleIcon;
                    break;
            }

            if (icon != null)
            {
                m_IconImage.sprite = icon;
                m_IconImage.enabled = true;
            }
            else
            {
                m_IconImage.enabled = false;
            }
        }

        #endregion
    }
}
