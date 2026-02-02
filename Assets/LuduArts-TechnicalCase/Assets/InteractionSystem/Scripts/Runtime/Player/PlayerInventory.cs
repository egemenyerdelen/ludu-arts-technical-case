using System;
using System.Collections.Generic;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player
{
    /// <summary>
    /// Simple inventory system for storing keys and checking requirements.
    /// </summary>
    public class PlayerInventory : MonoBehaviour, ISaveable
    {
        #region Fields

        // Serialized private instance fields
        [Header("Settings")]
        [SerializeField] private bool m_AutoTryUnlock = true;

        [Header("Save System")]
        [SerializeField] private string m_UniqueId = "PlayerInventory";

        // Non-serialized private instance fields
        private Dictionary<KeyType, int> m_Keys = new Dictionary<KeyType, int>();
        private Dictionary<KeyType, SO_KeyItem> m_KeyData = new Dictionary<KeyType, SO_KeyItem>();
        private List<InventoryItem> m_Items = new List<InventoryItem>();

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a key is added to the inventory.
        /// </summary>
        public event Action<KeyType, SO_KeyItem> OnKeyAdded;

        /// <summary>
        /// Event fired when a key is removed from the inventory.
        /// </summary>
        public event Action<KeyType> OnKeyRemoved;

        /// <summary>
        /// Event fired when a key is used.
        /// </summary>
        public event Action<KeyType> OnKeyUsed;

        /// <summary>
        /// Event fired when the inventory changes.
        /// </summary>
        public event Action OnInventoryChanged;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public string UniqueId => m_UniqueId;

        /// <summary>
        /// Gets the count of unique key types in inventory.
        /// </summary>
        public int KeyCount => m_Keys.Count;

        /// <summary>
        /// Gets all items in the inventory.
        /// </summary>
        public IReadOnlyList<InventoryItem> Items => m_Items;

        /// <summary>
        /// Gets whether auto-unlock is enabled.
        /// </summary>
        public bool AutoTryUnlock => m_AutoTryUnlock;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            InitializeKeyDictionary();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a key to the inventory.
        /// </summary>
        /// <param name="keyType">The type of key to add.</param>
        /// <param name="keyData">Optional ScriptableObject data for the key.</param>
        /// <returns>True if the key was added successfully.</returns>
        public bool AddKey(KeyType keyType, SO_KeyItem keyData = null)
        {
            if (keyType == KeyType.None)
            {
                Debug.LogWarning("[PlayerInventory] Cannot add KeyType.None to inventory.", this);
                return false;
            }

            if (m_Keys.ContainsKey(keyType))
            {
                m_Keys[keyType]++;
            }
            else
            {
                m_Keys[keyType] = 1;
            }

            // Store key data if provided
            if (keyData != null && !m_KeyData.ContainsKey(keyType))
            {
                m_KeyData[keyType] = keyData;
            }

            Debug.Log($"[PlayerInventory] Added {keyType} key. Count: {m_Keys[keyType]}", this);

            OnKeyAdded?.Invoke(keyType, keyData);
            OnInventoryChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// Removes a key from the inventory.
        /// </summary>
        /// <param name="keyType">The type of key to remove.</param>
        /// <returns>True if the key was removed successfully.</returns>
        public bool RemoveKey(KeyType keyType)
        {
            if (!HasKey(keyType))
            {
                Debug.LogWarning($"[PlayerInventory] No {keyType} key in inventory to remove.", this);
                return false;
            }

            m_Keys[keyType]--;

            if (m_Keys[keyType] <= 0)
            {
                m_Keys.Remove(keyType);
                m_KeyData.Remove(keyType);
            }

            Debug.Log($"[PlayerInventory] Removed {keyType} key.", this);

            OnKeyRemoved?.Invoke(keyType);
            OnInventoryChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// Uses a key from the inventory (removes it).
        /// </summary>
        /// <param name="keyType">The type of key to use.</param>
        /// <returns>True if the key was used successfully.</returns>
        public bool UseKey(KeyType keyType)
        {
            if (!HasKey(keyType))
            {
                return false;
            }

            var removed = RemoveKey(keyType);
            
            if (removed)
            {
                OnKeyUsed?.Invoke(keyType);
            }

            return removed;
        }

        /// <summary>
        /// Checks if the inventory contains a specific key type.
        /// </summary>
        /// <param name="keyType">The type of key to check for.</param>
        /// <returns>True if the key is in the inventory.</returns>
        public bool HasKey(KeyType keyType)
        {
            return m_Keys.ContainsKey(keyType) && m_Keys[keyType] > 0;
        }

        /// <summary>
        /// Gets the count of a specific key type.
        /// </summary>
        /// <param name="keyType">The type of key to count.</param>
        /// <returns>The number of keys of that type.</returns>
        public int GetKeyCount(KeyType keyType)
        {
            return m_Keys.ContainsKey(keyType) ? m_Keys[keyType] : 0;
        }

        /// <summary>
        /// Gets all key types currently in the inventory.
        /// </summary>
        /// <returns>List of key types.</returns>
        public List<KeyType> GetAllKeyTypes()
        {
            return new List<KeyType>(m_Keys.Keys);
        }

        /// <summary>
        /// Gets the ScriptableObject data for a key type.
        /// </summary>
        /// <param name="keyType">The type of key.</param>
        /// <returns>The key data, or null if not found.</returns>
        public SO_KeyItem GetKeyData(KeyType keyType)
        {
            return m_KeyData.ContainsKey(keyType) ? m_KeyData[keyType] : null;
        }

        /// <summary>
        /// Tries to unlock a lockable object using keys from the inventory.
        /// </summary>
        /// <param name="lockable">The lockable object to try to unlock.</param>
        /// <param name="consumeKey">Whether to consume the key on success.</param>
        /// <returns>True if successfully unlocked.</returns>
        public bool TryUnlockWithKey(ILockable lockable, bool consumeKey = true)
        {
            if (lockable == null)
            {
                Debug.LogError("[PlayerInventory] Cannot unlock null lockable.", this);
                return false;
            }

            if (!lockable.IsLocked)
            {
                return true; // Already unlocked
            }

            var requiredKey = lockable.RequiredKeyType;

            if (requiredKey == KeyType.None)
            {
                return lockable.TryUnlock(KeyType.None);
            }

            if (!HasKey(requiredKey))
            {
                Debug.Log($"[PlayerInventory] Missing required key: {requiredKey}", this);
                return false;
            }

            var unlocked = lockable.TryUnlock(requiredKey);

            if (unlocked && consumeKey)
            {
                UseKey(requiredKey);
            }

            return unlocked;
        }

        /// <summary>
        /// Clears all keys from the inventory.
        /// </summary>
        public void ClearKeys()
        {
            m_Keys.Clear();
            m_KeyData.Clear();
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Clears the entire inventory.
        /// </summary>
        public void ClearAll()
        {
            ClearKeys();
            m_Items.Clear();
            OnInventoryChanged?.Invoke();
        }

        /// <inheritdoc/>
        public object GetSaveData()
        {
            var keyList = new List<KeySaveEntry>();
            
            foreach (var kvp in m_Keys)
            {
                keyList.Add(new KeySaveEntry
                {
                    KeyType = kvp.Key,
                    Count = kvp.Value
                });
            }

            return new InventorySaveData
            {
                Keys = keyList
            };
        }

        /// <inheritdoc/>
        public void LoadSaveData(object data)
        {
            if (data is InventorySaveData saveData)
            {
                ClearKeys();

                foreach (var entry in saveData.Keys)
                {
                    m_Keys[entry.KeyType] = entry.Count;
                }

                OnInventoryChanged?.Invoke();
            }
            else
            {
                Debug.LogError("[PlayerInventory] Invalid save data type.", this);
            }
        }

        #endregion

        #region Private Methods

        private void InitializeKeyDictionary()
        {
            // Pre-initialize dictionary for all key types except None
            foreach (KeyType keyType in Enum.GetValues(typeof(KeyType)))
            {
                if (keyType != KeyType.None && !m_Keys.ContainsKey(keyType))
                {
                    m_Keys[keyType] = 0;
                }
            }

            // Remove zero-count entries for cleaner tracking
            var keysToRemove = new List<KeyType>();
            foreach (var kvp in m_Keys)
            {
                if (kvp.Value <= 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                m_Keys.Remove(key);
            }
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Generic inventory item.
        /// </summary>
        [Serializable]
        public class InventoryItem
        {
            public string ItemId;
            public string DisplayName;
            public int Quantity;
            public Sprite Icon;
        }

        /// <summary>
        /// Save data for a single key entry.
        /// </summary>
        [Serializable]
        private struct KeySaveEntry
        {
            public KeyType KeyType;
            public int Count;
        }

        /// <summary>
        /// Save data structure for the inventory.
        /// </summary>
        [Serializable]
        private class InventorySaveData
        {
            public List<KeySaveEntry> Keys = new List<KeySaveEntry>();
        }

        #endregion
    }
}
