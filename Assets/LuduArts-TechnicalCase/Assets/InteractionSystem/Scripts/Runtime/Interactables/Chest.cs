using System;
using System.Collections.Generic;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    /// <summary>
    /// Interactive chest/container that requires holding to open.
    /// Contains items that can be collected by the player.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Chest : HoldInteractable
    {
        #region Fields

        // Serialized private instance fields
        [Header("Chest Settings")]
        [SerializeField] private Transform m_LidPivot;
        [SerializeField] private float m_OpenAngle = -110f;
        [SerializeField] private float m_OpenAnimationDuration = 0.5f;
        [SerializeField] private ChestState m_State = ChestState.Closed;

        [Header("Contents")]
        [SerializeField] private List<ChestContent> m_Contents = new List<ChestContent>();
        [SerializeField] private bool m_GiveContentsOnOpen = true;

        [Header("Audio")]
        [SerializeField] private AudioClip m_OpeningSound;
        [SerializeField] private AudioClip m_OpenedSound;
        [SerializeField] private AudioClip m_ItemCollectedSound;

        [Header("VFX")]
        [SerializeField] private ParticleSystem m_OpenVFX;
        [SerializeField] private Light m_GlowLight;
        [SerializeField] private float m_GlowIntensity = 2f;
        [SerializeField] private AnimationCurve m_GlowCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Save System")]
        [SerializeField] private string m_UniqueId;

        // Non-serialized private instance fields
        private AudioSource m_AudioSource;
        private Quaternion m_ClosedRotation;
        private Quaternion m_OpenRotation;
        private bool m_IsOpened;
        private float m_CurrentOpenProgress;
        private bool m_ContentsCollected;
        private Coroutine m_CloseLidCoroutine;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when chest starts opening.
        /// </summary>
        public event Action OnChestOpening;

        /// <summary>
        /// Event fired when chest is fully opened.
        /// </summary>
        public event Action OnChestOpened;

        /// <summary>
        /// Event fired when contents are collected.
        /// </summary>
        public event Action<List<ChestContent>> OnContentsCollected;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override InteractionType InteractionType
        {
            get
            {
                // OPEN chest = Instant (just press E)
                if (m_IsOpened)
                    return InteractionType.Instant;
        
                // CLOSED chest = Hold (hold E to open)
                return InteractionType.Hold;
            }
        }

        /// <inheritdoc/>
        public string UniqueId => m_UniqueId;

        /// <summary>
        /// Gets whether the chest has been opened.
        /// </summary>
        public bool IsOpened
        {
            get => m_IsOpened;
            set => m_IsOpened = value;
        }

        /// <summary>
        /// Gets whether the contents have been collected.
        /// </summary>
        public bool ContentsCollected => m_ContentsCollected;

        /// <summary>
        /// Gets the chest contents.
        /// </summary>
        public IReadOnlyList<ChestContent> Contents => m_Contents;

        /// <inheritdoc/>
        public override string InteractionPrompt
        {
            get
            {
                if (m_IsOpened)
                {
                    return m_ContentsCollected ? "Empty" : "Collect Items";
                }
                return $"Hold to Open ({HoldDuration:F1}s)";
            }
        }

        /// <inheritdoc/>
        public override bool CanInteract
        {
            get
            {
                if (m_IsOpened)
                {
                    return !m_ContentsCollected && m_Contents.Count > 0;
                }
                return base.CanInteract;
            }
        }

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();

            m_AudioSource = GetComponent<AudioSource>();

            if (m_AudioSource == null)
            {
                Debug.LogError("[Chest] AudioSource component is missing.", this);
            }

            InitializeLidRotations();
            GenerateUniqueIdIfEmpty();
            SetupGlowLight();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (string.IsNullOrEmpty(m_UniqueId))
            {
                GenerateUniqueIdIfEmpty();
            }
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
    
            // Cleanup
            if (m_CloseLidCoroutine != null)
            {
                StopCoroutine(m_CloseLidCoroutine);
                m_CloseLidCoroutine = null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds content to the chest.
        /// </summary>
        /// <param name="content">The content to add.</param>
        public void AddContent(ChestContent content)
        {
            if (content == null)
            {
                Debug.LogError("[Chest] Cannot add null content.", this);
                return;
            }

            m_Contents.Add(content);
        }

        /// <summary>
        /// Clears all contents from the chest.
        /// </summary>
        public void ClearContents()
        {
            m_Contents.Clear();
        }

        /// <summary>
        /// Collects all contents from the chest.
        /// </summary>
        public void CollectContents(PlayerKeyInventory playerKeyInventory)
        {
            if (m_ContentsCollected || m_Contents.Count == 0)
            {
                return;
            }

            foreach (var content in m_Contents)
            {
                CollectContent(content, playerKeyInventory);
            }

            m_ContentsCollected = true;
            PlaySound(m_ItemCollectedSound);
            OnContentsCollected?.Invoke(new List<ChestContent>(m_Contents));

            Debug.Log($"[Chest] Collected {m_Contents.Count} items from chest.", this);
        }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void OnHoldBegin(InteractionDetector interactionDetector)
        {
            m_State = ChestState.Opening; // Clear state
            
            // Stop close animation if player starts holding again
            if (m_CloseLidCoroutine != null)
            {
                StopCoroutine(m_CloseLidCoroutine);
                m_CloseLidCoroutine = null;
            }
    
            PlaySound(m_OpeningSound);
            OnChestOpening?.Invoke();
        }

        /// <inheritdoc/>
        protected override void OnHoldUpdate(float progress)
        {
            // Only update if actually opening
            if (m_State != ChestState.Opening)
            {
                return;
            }
            
            m_CurrentOpenProgress = progress;
            SetLidRotation(progress);

            if (m_GlowLight != null)
            {
                var curvedProgress = m_GlowCurve.Evaluate(progress);
                m_GlowLight.intensity = curvedProgress * m_GlowIntensity;
            }
        }

        /// <inheritdoc/>


        /// <inheritdoc/>
        protected override void PerformHoldInteraction(InteractionDetector interactionDetector)
        {
            m_State = ChestState.Open;
            m_IsOpened = true;
            m_CurrentOpenProgress = 1f;

            SetLidRotation(1f);

            PlaySound(m_OpenedSound);
            PlayOpenVFX();

            // Only auto-collect if opened by a player (not a switch)
            if (m_GiveContentsOnOpen && interactionDetector != null)
            {
                var playerInventory = interactionDetector.GetComponent<PlayerKeyInventory>();
                if (playerInventory != null)
                {
                    CollectContents(playerInventory);
                }
                else
                {
                    Debug.LogWarning("[Chest] No PlayerKeyInventory found on interactor!", this);
                }
            }
            else if (interactionDetector == null)
            {
                Debug.Log("[Chest] Opened by external trigger (switch). Player can interact to collect items.", this);
            }

            OnChestOpened?.Invoke();
        }

        /// <inheritdoc/>
        protected override void OnHoldCancelledInternal()
        {
            m_State = ChestState.Closing;
    
            if (m_CloseLidCoroutine != null)
            {
                StopCoroutine(m_CloseLidCoroutine);
            }
    
            m_CloseLidCoroutine = StartCoroutine(AnimateLidClose());
        }

        /// <inheritdoc/>
        protected override void OnInteractInternal(InteractionDetector interactionDetector)
        {
            // Called when InteractionType is Instant
            if (m_IsOpened && !m_ContentsCollected)
            {
                if (interactionDetector != null)
                {
                    var playerInventory = interactionDetector.GetComponent<PlayerKeyInventory>();
                    CollectContents(playerInventory);
                }
            }
        }

        #endregion

        #region Private Methods

        private void InitializeLidRotations()
        {
            if (m_LidPivot == null)
            {
                Debug.LogWarning("[Chest] Lid pivot is not assigned. Lid animation will not work.", this);
                return;
            }

            m_ClosedRotation = m_LidPivot.localRotation;
            m_OpenRotation = m_ClosedRotation * Quaternion.Euler(m_OpenAngle, 0f, 0f);
        }

        private void SetLidRotation(float normalizedProgress)
        {
            if (m_LidPivot == null)
            {
                return;
            }

            m_LidPivot.localRotation = Quaternion.Slerp(m_ClosedRotation, m_OpenRotation, normalizedProgress);
        }

        private System.Collections.IEnumerator AnimateLidClose()
        {
            var startProgress = m_CurrentOpenProgress;
            var elapsed = 0f;
            var duration = m_OpenAnimationDuration * startProgress;
        
            while (elapsed < duration)
            {
                // Check if we should abort (player started holding again)
                if (m_State != ChestState.Closing)
                {
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                var t = 1f - (elapsed / duration);
                SetLidRotation(Mathf.Lerp(0f, startProgress, t));
                yield return null;
            }
        
            SetLidRotation(0f);
            m_CurrentOpenProgress = 0f;
            m_State = ChestState.Closed;
        
            if (m_GlowLight != null)
            {
                m_GlowLight.intensity = 0f;
            }
            
            m_CloseLidCoroutine = null;
        }

        private void SetupGlowLight()
        {
            if (m_GlowLight != null)
            {
                m_GlowLight.intensity = 0f;
            }
        }

        private void DisableGlow()
        {
            if (m_GlowLight != null)
            {
                m_GlowLight.enabled = false;
            }
        }

        private void PlayOpenVFX()
        {
            if (m_OpenVFX != null)
            {
                m_OpenVFX.Play();
            }
        }

        private void CollectContent(ChestContent content, PlayerKeyInventory keyInventory)
        {
            switch (content.ContentType)
            {
                case ChestContentType.Key:
                    if (keyInventory != null && content.KeyItem != null)
                    {
                        keyInventory.AddKey(content.KeyItem.KeyType, content.KeyItem);
                    }
                    else
                    {
                        Debug.LogWarning($"[Chest] Cannot collect key content: inventory={keyInventory != null}, keyItem={content.KeyItem != null}", this);
                    }
                    break;

                case ChestContentType.Generic:
                    Debug.Log($"[Chest] Collected generic item: {content.ItemName}", this);
                    break;

                case ChestContentType.Consumable:
                    Debug.Log($"[Chest] Collected consumable: {content.ItemName}", this);
                    break;

                case ChestContentType.Equipment:
                    Debug.Log($"[Chest] Collected equipment: {content.ItemName}", this);
                    break;

                default:
                    Debug.LogWarning($"[Chest] Unknown content type: {content.ContentType}", this);
                    break;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(clip);
            }
        }

        private void GenerateUniqueIdIfEmpty()
        {
            if (string.IsNullOrEmpty(m_UniqueId))
            {
                m_UniqueId = $"Chest_{gameObject.name}_{GetInstanceID()}";
            }
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents content inside a chest.
        /// </summary>
        [Serializable]
        public class ChestContent
        {
            [SerializeField] private ChestContentType m_ContentType;
            [SerializeField] private string m_ItemName;
            [SerializeField] private int m_Quantity = 1;
            [SerializeField] private SO_KeyItem m_KeyItem;
            [SerializeField] private Sprite m_Icon;

            /// <summary>
            /// Gets the content type.
            /// </summary>
            public ChestContentType ContentType => m_ContentType;

            /// <summary>
            /// Gets the item name.
            /// </summary>
            public string ItemName => m_ItemName;

            /// <summary>
            /// Gets the quantity.
            /// </summary>
            public int Quantity => m_Quantity;

            /// <summary>
            /// Gets the key item data (for Key content type).
            /// </summary>
            public SO_KeyItem KeyItem => m_KeyItem;

            /// <summary>
            /// Gets the icon sprite.
            /// </summary>
            public Sprite Icon => m_Icon;
        }

        /// <summary>
        /// Types of chest content.
        /// </summary>
        public enum ChestContentType
        {
            Generic,
            Key,
            Consumable,
            Equipment
        }

        /// <summary>
        /// Save data structure for chest state.
        /// </summary>
        [Serializable]
        private struct ChestSaveData
        {
            public bool IsOpened;
            public bool ContentsCollected;
        }
        
        /// <summary>
        /// Chest state enum for better case handling.
        /// </summary>
        private enum ChestState
        {
            Closed,
            Opening,    // Hold in progress
            Open,       // Fully opened
            Closing     // Cancel animation playing
        }

        #endregion
    }
}
