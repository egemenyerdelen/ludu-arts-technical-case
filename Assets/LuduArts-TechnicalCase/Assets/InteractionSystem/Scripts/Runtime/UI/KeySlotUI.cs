using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.UI
{
    /// <summary>
    /// UI component for a single key slot in the inventory.
    /// </summary>
    public class KeySlotUI : MonoBehaviour
    {
        #region Fields

        // Private constant fields
        private const float k_PulseScaleMultiplier = 1.3f;

        // Serialized private instance fields
        [SerializeField] private Image m_IconImage;
        [SerializeField] private TextMeshProUGUI m_CountText;
        [SerializeField] private TextMeshProUGUI m_NameText;

        // Non-serialized private instance fields
        private KeyType m_KeyType;
        private int m_Count;
        private SO_KeyItem m_KeyData;
        private Vector3 m_OriginalScale;
        private Coroutine m_AnimationCoroutine;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the key type of this slot.
        /// </summary>
        public KeyType KeyType => m_KeyType;

        /// <summary>
        /// Gets the count of keys in this slot.
        /// </summary>
        public int Count => m_Count;

        #endregion

        #region Methods

        /// <summary>
        /// Sets up the key slot with data.
        /// </summary>
        /// <param name="keyType">The key type.</param>
        /// <param name="count">The number of keys.</param>
        /// <param name="keyData">Optional ScriptableObject data.</param>
        public void Setup(KeyType keyType, int count, SO_KeyItem keyData)
        {
            m_KeyType = keyType;
            m_Count = count;
            m_KeyData = keyData;
            m_OriginalScale = transform.localScale;

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the count display.
        /// </summary>
        /// <param name="newCount">The new count.</param>
        public void UpdateCount(int newCount)
        {
            m_Count = newCount;
            UpdateCountDisplay();
        }

        /// <summary>
        /// Plays an animation when a key is added.
        /// </summary>
        /// <param name="duration">Duration of the animation.</param>
        public void PlayAddAnimation(float duration)
        {
            if (m_AnimationCoroutine != null)
            {
                StopCoroutine(m_AnimationCoroutine);
            }

            m_AnimationCoroutine = StartCoroutine(AddAnimationCoroutine(duration));
        }

        #endregion

        #region Private Methods

        private void UpdateDisplay()
        {
            UpdateIconDisplay();
            UpdateCountDisplay();
            UpdateNameDisplay();
        }

        private void UpdateIconDisplay()
        {
            if (m_IconImage == null)
            {
                return;
            }

            if (m_KeyData != null && m_KeyData.Icon != null)
            {
                m_IconImage.sprite = m_KeyData.Icon;
                m_IconImage.color = Color.white;
            }
            else
            {
                m_IconImage.color = GetKeyColor();
            }
        }

        private void UpdateCountDisplay()
        {
            if (m_CountText == null)
            {
                return;
            }

            m_CountText.text = m_Count > 1 ? $"x{m_Count}" : "";
        }

        private void UpdateNameDisplay()
        {
            if (m_NameText == null)
            {
                return;
            }

            if (m_KeyData != null)
            {
                m_NameText.text = m_KeyData.DisplayName;
            }
            else
            {
                m_NameText.text = $"{m_KeyType} Key";
            }
        }

        private Color GetKeyColor()
        {
            if (m_KeyData != null)
            {
                return m_KeyData.KeyColor;
            }

            switch (m_KeyType)
            {
                case KeyType.Gold:
                    return new Color(1f, 0.84f, 0f);
                case KeyType.Silver:
                    return new Color(0.75f, 0.75f, 0.75f);
                case KeyType.Bronze:
                    return new Color(0.8f, 0.5f, 0.2f);
                default:
                    return Color.white;
            }
        }

        private System.Collections.IEnumerator AddAnimationCoroutine(float duration)
        {
            var elapsed = 0f;
            var halfDuration = duration * 0.5f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(m_OriginalScale, m_OriginalScale * k_PulseScaleMultiplier, t);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(m_OriginalScale * k_PulseScaleMultiplier, m_OriginalScale, t);
                yield return null;
            }

            transform.localScale = m_OriginalScale;
            m_AnimationCoroutine = null;
        }

        #endregion
    }
}
