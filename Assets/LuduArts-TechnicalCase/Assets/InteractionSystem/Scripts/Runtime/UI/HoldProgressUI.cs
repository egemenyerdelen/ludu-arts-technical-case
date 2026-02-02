using UnityEngine;
using UnityEngine.UI;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.UI
{
    /// <summary>
    /// UI component that displays hold interaction progress.
    /// Supports both circular (radial) and linear progress bar styles.
    /// </summary>
    public class HoldProgressUI : MonoBehaviour
    {
        #region Fields

        // Private constant fields
        private const float k_DefaultFadeSpeed = 10f;

        // Serialized private instance fields
        [Header("UI References")]
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private Image m_ProgressFill;
        [SerializeField] private Image m_BackgroundImage;

        [Header("Progress Settings")]
        [SerializeField] private ProgressStyle m_ProgressStyle = ProgressStyle.Circular;
        [SerializeField] private bool m_SmoothProgress = true;
        [SerializeField] private float m_SmoothSpeed = 10f;

        [Header("Animation")]
        [SerializeField] private bool m_UseAnimation = true;
        [SerializeField] private float m_FadeSpeed = 10f;
        [SerializeField] private bool m_PulseOnComplete = true;
        [SerializeField] private float m_PulseScale = 1.2f;
        [SerializeField] private float m_PulseSpeed = 5f;

        [Header("Colors")]
        [SerializeField] private Color m_StartColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Color m_EndColor = new Color(0.2f, 1f, 0.2f, 1f);
        [SerializeField] private Gradient m_ProgressGradient;

        // Non-serialized private instance fields
        private float m_TargetProgress;
        private float m_CurrentProgress;
        private float m_TargetAlpha;
        private bool m_IsVisible;
        private bool m_IsComplete;
        private Vector3 m_OriginalScale;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the progress bar is currently visible.
        /// </summary>
        public bool IsVisible => m_IsVisible;

        /// <summary>
        /// Gets the current progress value (0-1).
        /// </summary>
        public float CurrentProgress => m_CurrentProgress;

        /// <summary>
        /// Gets whether the progress is complete.
        /// </summary>
        public bool IsComplete => m_IsComplete;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ValidateComponents();
            m_OriginalScale = transform.localScale;
            
            // Start hidden
            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
            }

            m_TargetAlpha = 0f;
            m_IsVisible = false;
            m_CurrentProgress = 0f;
            m_TargetProgress = 0f;

            SetupProgressFill();
        }

        private void Update()
        {
            UpdateFade();
            UpdateProgress();
            UpdatePulse();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the progress bar.
        /// </summary>
        public void Show()
        {
            m_IsVisible = true;
            m_TargetAlpha = 1f;
            m_IsComplete = false;

            // Reset progress
            m_CurrentProgress = 0f;
            m_TargetProgress = 0f;
            UpdateProgressVisual(0f);

            if (!m_UseAnimation && m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Hides the progress bar.
        /// </summary>
        public void Hide()
        {
            m_IsVisible = false;
            m_TargetAlpha = 0f;
            m_IsComplete = false;

            // Reset scale
            transform.localScale = m_OriginalScale;

            if (!m_UseAnimation && m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Sets the progress value.
        /// </summary>
        /// <param name="progress">Progress value from 0 to 1.</param>
        public void SetProgress(float progress)
        {
            m_TargetProgress = Mathf.Clamp01(progress);

            if (!m_SmoothProgress)
            {
                m_CurrentProgress = m_TargetProgress;
                UpdateProgressVisual(m_CurrentProgress);
            }

            // Check for completion
            if (m_TargetProgress >= 1f && !m_IsComplete)
            {
                m_IsComplete = true;
                OnProgressComplete();
            }
        }

        /// <summary>
        /// Resets the progress bar to zero.
        /// </summary>
        public void ResetProgress()
        {
            m_TargetProgress = 0f;
            m_CurrentProgress = 0f;
            m_IsComplete = false;
            transform.localScale = m_OriginalScale;
            UpdateProgressVisual(0f);
        }

        /// <summary>
        /// Sets the progress style.
        /// </summary>
        /// <param name="style">The progress style to use.</param>
        public void SetProgressStyle(ProgressStyle style)
        {
            m_ProgressStyle = style;
            SetupProgressFill();
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

            if (m_ProgressFill == null)
            {
                Debug.LogWarning("[HoldProgressUI] Progress fill image not assigned.", this);
            }

            // Setup default gradient if not set
            if (m_ProgressGradient == null || m_ProgressGradient.colorKeys.Length == 0)
            {
                m_ProgressGradient = new Gradient();
                var colorKeys = new GradientColorKey[2];
                colorKeys[0].color = m_StartColor;
                colorKeys[0].time = 0f;
                colorKeys[1].color = m_EndColor;
                colorKeys[1].time = 1f;

                var alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0].alpha = 1f;
                alphaKeys[0].time = 0f;
                alphaKeys[1].alpha = 1f;
                alphaKeys[1].time = 1f;

                m_ProgressGradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        private void SetupProgressFill()
        {
            if (m_ProgressFill == null)
            {
                return;
            }

            switch (m_ProgressStyle)
            {
                case ProgressStyle.Circular:
                    m_ProgressFill.type = Image.Type.Filled;
                    m_ProgressFill.fillMethod = Image.FillMethod.Radial360;
                    m_ProgressFill.fillOrigin = (int)Image.Origin360.Top;
                    m_ProgressFill.fillClockwise = true;
                    break;

                case ProgressStyle.Linear:
                    m_ProgressFill.type = Image.Type.Filled;
                    m_ProgressFill.fillMethod = Image.FillMethod.Horizontal;
                    m_ProgressFill.fillOrigin = (int)Image.Origin360.Left;
                    break;

                case ProgressStyle.Vertical:
                    m_ProgressFill.type = Image.Type.Filled;
                    m_ProgressFill.fillMethod = Image.FillMethod.Vertical;
                    m_ProgressFill.fillOrigin = (int)Image.Origin360.Bottom;
                    break;
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

        private void UpdateProgress()
        {
            if (!m_SmoothProgress)
            {
                return;
            }

            if (!Mathf.Approximately(m_CurrentProgress, m_TargetProgress))
            {
                m_CurrentProgress = Mathf.MoveTowards(m_CurrentProgress, m_TargetProgress, m_SmoothSpeed * Time.deltaTime);
                UpdateProgressVisual(m_CurrentProgress);
            }
        }

        private void UpdateProgressVisual(float progress)
        {
            if (m_ProgressFill == null)
            {
                return;
            }

            m_ProgressFill.fillAmount = progress;

            // Update color based on gradient
            if (m_ProgressGradient != null)
            {
                m_ProgressFill.color = m_ProgressGradient.Evaluate(progress);
            }
        }

        private void UpdatePulse()
        {
            if (!m_PulseOnComplete || !m_IsComplete)
            {
                return;
            }

            // Pulse animation
            var pulse = 1f + Mathf.Sin(Time.time * m_PulseSpeed) * (m_PulseScale - 1f) * 0.5f;
            transform.localScale = m_OriginalScale * pulse;
        }

        private void OnProgressComplete()
        {
            // Could trigger completion effects here
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Progress bar display styles.
        /// </summary>
        public enum ProgressStyle
        {
            /// <summary>
            /// Circular/radial fill.
            /// </summary>
            Circular,

            /// <summary>
            /// Horizontal linear fill.
            /// </summary>
            Linear,

            /// <summary>
            /// Vertical linear fill.
            /// </summary>
            Vertical
        }

        #endregion
    }
}
