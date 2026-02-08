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
        [SerializeField] private PlayerKeyInventory m_PlayerKeyInventory;
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
            if (m_PlayerKeyInventory == null)
            {
                Debug.LogWarning("[InventoryUI] Cannot refresh - no PlayerInventory assigned.", this);
                return;
            }

            RefreshKeyDisplay();
        }

        /// <summary>
        /// Sets the player inventory reference.
        /// </summary>
        /// <param name="keyInventory">The player inventory to display.</param>
        public void SetInventory(PlayerKeyInventory keyInventory)
        {
            if (keyInventory == null)
            {
                Debug.LogError("[InventoryUI] Cannot set null inventory.", this);
                return;
            }

            if (m_PlayerKeyInventory != null)
            {
                UnsubscribeFromEvents();
            }

            m_PlayerKeyInventory = keyInventory;
            SubscribeToEvents();
            RefreshDisplay();
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            if (m_IsInitialized)
            {
                return;
            }

            if (m_PlayerKeyInventory == null)
            {
                m_PlayerKeyInventory = FindAnyObjectByType<PlayerKeyInventory>();

                if (m_PlayerKeyInventory == null)
                {
                    Debug.LogWarning("[InventoryUI] No PlayerInventory found.", this);
                }
            }

            if (m_KeySlotPrefab == null)
            {
                Debug.LogWarning("[InventoryUI] Key slot prefab not assigned. Fallback UI will be used.", this);
            }

            m_IsInitialized = true;
        }

        private void SubscribeToEvents()
        {
            if (m_PlayerKeyInventory == null)
            {
                return;
            }

            m_PlayerKeyInventory.OnKeyAdded += HandleKeyAdded;
            m_PlayerKeyInventory.OnKeyRemoved += HandleKeyRemoved;
            m_PlayerKeyInventory.OnInventoryChanged += HandleKeyInventoryChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (m_PlayerKeyInventory == null)
            {
                return;
            }

            m_PlayerKeyInventory.OnKeyAdded -= HandleKeyAdded;
            m_PlayerKeyInventory.OnKeyRemoved -= HandleKeyRemoved;
            m_PlayerKeyInventory.OnInventoryChanged -= HandleKeyInventoryChanged;
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

        private void HandleKeyInventoryChanged()
        {
            if (m_AutoRefresh)
            {
                RefreshDisplay();
            }
        }

        private void RefreshKeyDisplay()
        {
            if (m_KeyContainer == null || m_PlayerKeyInventory == null)
            {
                return;
            }

            ClearKeySlots();

            var keyTypes = m_PlayerKeyInventory.GetAllKeyTypes();
            var displayCount = 0;

            foreach (var keyType in keyTypes)
            {
                if (displayCount >= m_MaxVisibleKeys)
                {
                    break;
                }

                var count = m_PlayerKeyInventory.GetKeyCount(keyType);

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
                CreateSimpleKeySlot(keyType, count);
                return;
            }

            var slotObj = Instantiate(m_KeySlotPrefab, m_KeyContainer);
            var slot = slotObj.GetComponent<KeySlotUI>();

            if (slot == null)
            {
                slot = slotObj.AddComponent<KeySlotUI>();
            }

            var keyData = m_PlayerKeyInventory.GetKeyData(keyType);
            slot.Setup(keyType, count, keyData);

            m_KeySlots.Add(slot);
        }

        private void CreateSimpleKeySlot(KeyType keyType, int count)
        {
            var slotObj = new GameObject($"KeySlot_{keyType}");
            slotObj.transform.SetParent(m_KeyContainer);
            slotObj.transform.localScale = Vector3.one;

            var text = slotObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{keyType} Key x{count}";
            text.fontSize = 14;
            text.color = GetKeyColor(keyType);
            text.alignment = TextAlignmentOptions.Left;

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
}
