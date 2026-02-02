using System;
using System.Collections.Generic;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base;
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
    public class Chest : HoldInteractable, ISaveable
    {
        #region Fields

        // Private constant fields
        private const string k_AnimatorOpenParameter = "IsOpen";
        private const string k_AnimatorProgressParameter = "OpenProgress";

        // Serialized private instance fields
        [Header("Chest Settings")]
        [SerializeField] private Transform m_LidPivot;
        [SerializeField] private float m_OpenAngle = -110f;
        [SerializeField] private float m_OpenAnimationDuration = 0.5f;

        [Header("Contents")]
        [SerializeField] private List<ChestContent> m_Contents = new List<ChestContent>();
        [SerializeField] private bool m_GiveContentsOnOpen = true;

        [Header("Animation")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private bool m_UseAnimator = true;

        [Header("Audio")]
        [SerializeField] private AudioClip m_OpeningSound;
        [SerializeField] private AudioClip m_OpenedSound;
        [SerializeField] private AudioClip m_ItemCollectedSound;

        [Header("VFX")]
        [SerializeField] private ParticleSystem m_OpenVFX;
        [SerializeField] private Light m_GlowLight;
        [SerializeField] private float m_GlowIntensity = 2f;

        [Header("Save System")]
        [SerializeField] private string m_UniqueId;

        // Non-serialized private instance fields
        private AudioSource m_AudioSource;
        private Quaternion m_ClosedRotation;
        private Quaternion m_OpenRotation;
        private bool m_IsOpened;
        private float m_CurrentOpenProgress;
        private bool m_ContentsCollected;

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
        public string UniqueId => m_UniqueId;

        /// <summary>
        /// Gets whether the chest has been opened.
        /// </summary>
        public bool IsOpened => m_IsOpened;

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
                // Can interact if not opened, or if opened but contents not collected
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

        #endregion

        #region Methods

        /// <summary>
        /// Adds content to the chest.
        /// </summary>
        /// <param name="content">The content to add.</param>
        public void AddContent(ChestContent content)
        {
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
        public void CollectContents()
        {
            if (m_ContentsCollected || m_Contents.Count == 0)
            {
                return;
            }

            var inventory = FindPlayerInventory();
            
            foreach (var content in m_Contents)
            {
                CollectContent(content, inventory);
            }

            m_ContentsCollected = true;
            PlaySound(m_ItemCollectedSound);
            OnContentsCollected?.Invoke(new List<ChestContent>(m_Contents));

            Debug.Log($"[Chest] Collected {m_Contents.Count} items from chest.", this);
        }

        /// <inheritdoc/>
        public object GetSaveData()
        {
            return new ChestSaveData
            {
                IsOpened = m_IsOpened,
                ContentsCollected = m_ContentsCollected
            };
        }

        /// <inheritdoc/>
        public void LoadSaveData(object data)
        {
            if (data is ChestSaveData saveData)
            {
                m_IsOpened = saveData.IsOpened;
                m_ContentsCollected = saveData.ContentsCollected;

                // Apply visual state
                if (m_IsOpened)
                {
                    SetLidRotation(1f);
                    DisableGlow();
                }
            }
            else
            {
                Debug.LogError("[Chest] Invalid save data type.", this);
            }
        }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void OnHoldBegin()
        {
            PlaySound(m_OpeningSound);
            OnChestOpening?.Invoke();
        }

        /// <inheritdoc/>
        protected override void OnHoldUpdate(float progress)
        {
            m_CurrentOpenProgress = progress;
            
            // Update lid rotation based on progress
            SetLidRotation(progress);

            // Update animator
            if (m_UseAnimator && m_Animator != null)
            {
                m_Animator.SetFloat(k_AnimatorProgressParameter, progress);
            }

            // Update glow intensity
            if (m_GlowLight != null)
            {
                m_GlowLight.intensity = Mathf.Lerp(0f, m_GlowIntensity, progress);
            }
        }

        /// <inheritdoc/>
        protected override void PerformHoldInteraction()
        {
            m_IsOpened = true;
            m_CurrentOpenProgress = 1f;

            // Ensure lid is fully open
            SetLidRotation(1f);

            // Update animator
            if (m_UseAnimator && m_Animator != null)
            {
                m_Animator.SetBool(k_AnimatorOpenParameter, true);
            }

            // Play effects
            PlaySound(m_OpenedSound);
            PlayOpenVFX();

            // Give contents if configured
            if (m_GiveContentsOnOpen)
            {
                CollectContents();
            }

            OnChestOpened?.Invoke();
            Debug.Log("[Chest] Chest opened!", this);
        }

        /// <inheritdoc/>
        protected override void OnHoldCancelledInternal()
        {
            // Reset lid if cancelled
            StartCoroutine(AnimateLidClose());
        }

        /// <inheritdoc/>
        protected override void OnInteractInternal()
        {
            // If chest is already open but contents not collected, collect them
            if (m_IsOpened && !m_ContentsCollected)
            {
                CollectContents();
            }
        }

        #endregion

        #region Private Methods

        private void InitializeLidRotations()
        {
            if (m_LidPivot == null)
            {
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
                elapsed += Time.deltaTime;
                var t = 1f - (elapsed / duration);
                SetLidRotation(Mathf.Lerp(0f, startProgress, t));
                yield return null;
            }

            SetLidRotation(0f);
            m_CurrentOpenProgress = 0f;

            // Reset glow
            if (m_GlowLight != null)
            {
                m_GlowLight.intensity = 0f;
            }
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

        private void CollectContent(ChestContent content, PlayerInventory inventory)
        {
            switch (content.ContentType)
            {
                case ChestContentType.Key:
                    if (inventory != null && content.KeyItem != null)
                    {
                        inventory.AddKey(content.KeyItem.KeyType, content.KeyItem);
                    }
                    break;

                case ChestContentType.Generic:
                    // Handle generic items
                    Debug.Log($"[Chest] Collected generic item: {content.ItemName}", this);
                    break;
            }
        }

        private PlayerInventory FindPlayerInventory()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                return player.GetComponent<PlayerInventory>();
            }

            return FindAnyObjectByType<PlayerInventory>();
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
            public ChestContentType ContentType;
            public string ItemName;
            public int Quantity = 1;
            public SO_KeyItem KeyItem;
            public Sprite Icon;
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

        #endregion
    }
}
