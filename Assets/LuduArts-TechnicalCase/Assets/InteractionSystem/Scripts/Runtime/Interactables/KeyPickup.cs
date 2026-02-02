using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    /// <summary>
    /// Pickupable key that adds itself to the player's inventory.
    /// Supports different key types for color-coded locks.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class KeyPickup : InstantInteractable
    {
        #region Fields

        // Serialized private instance fields
        [Header("Key Settings")]
        [SerializeField] private KeyType m_KeyType = KeyType.Gold;
        [SerializeField] private SO_KeyItem m_KeyItemData;

        [Header("Visual")]
        [SerializeField] private MeshRenderer m_KeyRenderer;
        [SerializeField] private float m_RotationSpeed = 50f;
        [SerializeField] private float m_BobSpeed = 2f;
        [SerializeField] private float m_BobAmount = 0.1f;

        [Header("Audio")]
        [SerializeField] private AudioClip m_PickupSound;

        [Header("VFX")]
        [SerializeField] private ParticleSystem m_PickupVFX;

        // Non-serialized private instance fields
        private AudioSource m_AudioSource;
        private Vector3 m_StartPosition;
        private bool m_IsCollected;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when the key is picked up.
        /// </summary>
        public event Action<KeyType> OnKeyPickedUp;

        /// <summary>
        /// Static event fired when any key is picked up.
        /// </summary>
        public static event Action<KeyType, KeyPickup> OnAnyKeyPickedUp;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of this key.
        /// </summary>
        public KeyType KeyType => m_KeyType;

        /// <summary>
        /// Gets the ScriptableObject data for this key.
        /// </summary>
        public SO_KeyItem KeyItemData => m_KeyItemData;

        /// <inheritdoc/>
        public override string InteractionPrompt
        {
            get
            {
                if (m_KeyItemData != null)
                {
                    return $"Pick up {m_KeyItemData.DisplayName}";
                }
                return $"Pick up {m_KeyType} Key";
            }
        }

        /// <inheritdoc/>
        public override bool CanInteract => base.CanInteract && !m_IsCollected;

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();
            
            m_AudioSource = GetComponent<AudioSource>();
            m_StartPosition = transform.position;

            if (m_AudioSource == null)
            {
                Debug.LogError("[KeyPickup] AudioSource component is missing.", this);
            }

            ApplyKeyColor();
        }

        private void Update()
        {
            if (m_IsCollected)
            {
                return;
            }

            AnimateKey();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // Apply color in editor
            ApplyKeyColor();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void PerformInstantInteraction()
        {
            if (m_IsCollected)
            {
                Debug.LogWarning("[KeyPickup] Key has already been collected.", this);
                return;
            }

            m_IsCollected = true;

            // Try to add to player inventory
            var inventory = FindPlayerInventory();
            
            if (inventory != null)
            {
                var added = inventory.AddKey(m_KeyType, m_KeyItemData);
                
                if (!added)
                {
                    Debug.LogWarning("[KeyPickup] Failed to add key to inventory.", this);
                    m_IsCollected = false;
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[KeyPickup] No PlayerInventory found in scene.", this);
            }

            // Play pickup effects
            PlayPickupEffects();

            // Fire events
            OnKeyPickedUp?.Invoke(m_KeyType);
            OnAnyKeyPickedUp?.Invoke(m_KeyType, this);

            Debug.Log($"[KeyPickup] {m_KeyType} key collected!", this);

            // Destroy after a short delay to allow sound to play
            Destroy(gameObject, m_PickupSound != null ? m_PickupSound.length : 0.1f);
        }

        #endregion

        #region Private Methods

        private void AnimateKey()
        {
            // Rotate
            transform.Rotate(Vector3.up, m_RotationSpeed * Time.deltaTime, Space.World);

            // Bob up and down
            var yOffset = Mathf.Sin(Time.time * m_BobSpeed) * m_BobAmount;
            transform.position = m_StartPosition + new Vector3(0f, yOffset, 0f);
        }

        private void ApplyKeyColor()
        {
            if (m_KeyRenderer == null)
            {
                return;
            }

            var keyColor = GetKeyColor();

            // Apply in editor or at runtime
            if (Application.isPlaying)
            {
                m_KeyRenderer.material.color = keyColor;
            }
            else
            {
                // In editor, use sharedMaterial property block
                var propertyBlock = new MaterialPropertyBlock();
                m_KeyRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", keyColor);
                propertyBlock.SetColor("_BaseColor", keyColor); // For URP/HDRP
                m_KeyRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private Color GetKeyColor()
        {
            // Use ScriptableObject color if available
            if (m_KeyItemData != null)
            {
                return m_KeyItemData.KeyColor;
            }

            // Default colors
            switch (m_KeyType)
            {
                case KeyType.Gold:
                    return new Color(1f, 0.84f, 0f); // Gold
                case KeyType.Silver:
                    return new Color(0.75f, 0.75f, 0.75f); // Silver
                case KeyType.Bronze:
                    return new Color(0.8f, 0.5f, 0.2f); // Bronze
                default:
                    return Color.gray;
            }
        }

        private void PlayPickupEffects()
        {
            // Play sound
            if (m_PickupSound != null && m_AudioSource != null)
            {
                // Play at position so sound continues after object is destroyed
                AudioSource.PlayClipAtPoint(m_PickupSound, transform.position);
            }

            // Play VFX
            if (m_PickupVFX != null)
            {
                m_PickupVFX.transform.SetParent(null);
                m_PickupVFX.Play();
                Destroy(m_PickupVFX.gameObject, m_PickupVFX.main.duration);
            }
        }

        private PlayerInventory FindPlayerInventory()
        {
            // Try to find via tag first
            var player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                var inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    return inventory;
                }
            }

            // Fallback to FindObjectOfType
            return FindAnyObjectByType<PlayerInventory>();
        }

        #endregion
    }
}
