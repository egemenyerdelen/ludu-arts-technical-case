using System.Collections.Generic;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.UI
{
    /// <summary>
    /// UI component that displays the player's inventory.
    /// Shows collected keys and items.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        #region Fields

        // Serialized private instance fields
        [Header("References")]
        [SerializeField] private PlayerInventory m_PlayerInventory;
        [SerializeField] private Transform m_KeyContainer;
        [SerializeField] private Transform m_ItemContainer;
        [SerializeField] private GameObject m_KeySlotPrefab;
        [SerializeField] private GameObject m_ItemSlotPrefab;

        [Header("Display Settings")]
        [SerializeField] private bool m_AutoRefresh = true;
        [SerializeField] private bool m_ShowEmptySlots;
        [SerializeField] private int m_MaxVisibleKeys = 5;

        [Header("Animation")]
        [SerializeField] private bool m_AnimateOnAdd = true;
        [SerializeField] private float m_AddAnimationDuration = 0.3f;

        // Non-serialized private instance fields
        private List<KeySlotUI> m_KeySlots = new List<KeySlotUI>();
        private bool m_IsInitialized;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            RefreshDisplay();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes the entire inventory display.
        /// </summary>
        public void RefreshDisplay()
        {
            if (m_PlayerInventory == null)
            {
                return;
            }

            RefreshKeyDisplay();
        }

        /// <summary>
        /// Sets the player inventory reference.
        /// </summary>
        /// <param name="inventory">The player inventory to display.</param>
        public void SetInventory(PlayerInventory inventory)
        {
            if (m_PlayerInventory != null)
            {
                UnsubscribeFromEvents();
            }

            m_PlayerInventory = inventory;

            if (m_PlayerInventory != null)
            {
                SubscribeToEvents();
                RefreshDisplay();
            }
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            if (m_IsInitialized)
            {
                return;
            }

            // Find inventory if not assigned
            if (m_PlayerInventory == null)
            {
                m_PlayerInventory = FindAnyObjectByType<PlayerInventory>();
                
                if (m_PlayerInventory == null)
                {
                    Debug.LogWarning("[InventoryUI] No PlayerInventory found.", this);
                }
            }

            // Create default slot prefab if not assigned
            if (m_KeySlotPrefab == null)
            {
                Debug.LogWarning("[InventoryUI] Key slot prefab not assigned.", this);
            }

            m_IsInitialized = true;
        }

        private void SubscribeToEvents()
        {
            if (m_PlayerInventory == null)
            {
                return;
            }

            m_PlayerInventory.OnKeyAdded += HandleKeyAdded;
            m_PlayerInventory.OnKeyRemoved += HandleKeyRemoved;
            m_PlayerInventory.OnInventoryChanged += HandleInventoryChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (m_PlayerInventory == null)
            {
                return;
            }

            m_PlayerInventory.OnKeyAdded -= HandleKeyAdded;
            m_PlayerInventory.OnKeyRemoved -= HandleKeyRemoved;
            m_PlayerInventory.OnInventoryChanged -= HandleInventoryChanged;
        }

        private void HandleKeyAdded(KeyType keyType, SO_KeyItem keyData)
        {
            if (m_AutoRefresh)
            {
                RefreshKeyDisplay();
            }

            if (m_AnimateOnAdd)
            {
                AnimateKeySlot(keyType);
            }
        }

        private void HandleKeyRemoved(KeyType keyType)
        {
            if (m_AutoRefresh)
            {
                RefreshKeyDisplay();
            }
        }

        private void HandleInventoryChanged()
        {
            if (m_AutoRefresh)
            {
                RefreshDisplay();
            }
        }

        private void RefreshKeyDisplay()
        {
            if (m_KeyContainer == null || m_PlayerInventory == null)
            {
                return;
            }

            // Clear existing slots
            ClearKeySlots();

            // Get all key types
            var keyTypes = m_PlayerInventory.GetAllKeyTypes();

            // Create slots for each key
            var displayCount = 0;
            
            foreach (var keyType in keyTypes)
            {
                if (displayCount >= m_MaxVisibleKeys)
                {
                    break;
                }

                var count = m_PlayerInventory.GetKeyCount(keyType);
                
                if (count <= 0 && !m_ShowEmptySlots)
                {
                    continue;
                }

                CreateKeySlot(keyType, count);
                displayCount++;
            }
        }

        private void ClearKeySlots()
        {
            foreach (var slot in m_KeySlots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    Destroy(slot.gameObject);
                }
            }

            m_KeySlots.Clear();
        }

        private void CreateKeySlot(KeyType keyType, int count)
        {
            if (m_KeySlotPrefab == null || m_KeyContainer == null)
            {
                // Create a simple text slot as fallback
                CreateSimpleKeySlot(keyType, count);
                return;
            }

            var slotObj = Instantiate(m_KeySlotPrefab, m_KeyContainer);
            var slot = slotObj.GetComponent<KeySlotUI>();

            if (slot == null)
            {
                slot = slotObj.AddComponent<KeySlotUI>();
            }

            var keyData = m_PlayerInventory.GetKeyData(keyType);
            slot.Setup(keyType, count, keyData);
            
            m_KeySlots.Add(slot);
        }

        private void CreateSimpleKeySlot(KeyType keyType, int count)
        {
            // Create a simple UI element
            var slotObj = new GameObject($"KeySlot_{keyType}");
            slotObj.transform.SetParent(m_KeyContainer);
            slotObj.transform.localScale = Vector3.one;

            // Add text
            var text = slotObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{keyType} Key x{count}";
            text.fontSize = 14;
            text.color = GetKeyColor(keyType);
            text.alignment = TextAlignmentOptions.Left;

            // Add layout element
            var layout = slotObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 24;

            var slot = slotObj.AddComponent<KeySlotUI>();
            slot.Setup(keyType, count, null);
            
            m_KeySlots.Add(slot);
        }

        private Color GetKeyColor(KeyType keyType)
        {
            switch (keyType)
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

        private void AnimateKeySlot(KeyType keyType)
        {
            foreach (var slot in m_KeySlots)
            {
                if (slot != null && slot.KeyType == keyType)
                {
                    slot.PlayAddAnimation(m_AddAnimationDuration);
                    break;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// UI component for a single key slot in the inventory.
    /// </summary>
    public class KeySlotUI : MonoBehaviour
    {
        #region Fields

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
                // Use color tint for default icon
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
                transform.localScale = Vector3.Lerp(m_OriginalScale, m_OriginalScale * 1.3f, t);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(m_OriginalScale * 1.3f, m_OriginalScale, t);
                yield return null;
            }

            transform.localScale = m_OriginalScale;
            m_AnimationCoroutine = null;
        }

        #endregion
    }
}
