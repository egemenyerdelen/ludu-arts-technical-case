using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    /// <summary>
    /// Interactive door that can be opened, closed, locked, and unlocked.
    /// Supports animations, sounds, and can be triggered by switches.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Door : ToggleInteractable, ILockable, ISaveable
    {
        #region Fields

        // Private constant fields
        private const string k_AnimatorOpenParameter = "IsOpen";
        private const string k_AnimatorLockedParameter = "IsLocked";
        private const float k_DefaultAnimationDuration = 1f;

        // Serialized private instance fields
        [Header("Door Settings")]
        [SerializeField] private DoorState m_InitialState = DoorState.Closed;
        [SerializeField] private float m_OpenAngle = 90f;
        [SerializeField] private float m_AnimationDuration = 1f;
        [SerializeField] private Transform m_PivotPoint;

        [Header("Lock Settings")]
        [SerializeField] private bool m_IsLocked;
        [SerializeField] private KeyType m_RequiredKeyType = KeyType.None;
        [SerializeField] private bool m_ConsumeKeyOnUnlock = true;

        [Header("Animation")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private bool m_UseAnimator = true;

        [Header("Audio")]
        [SerializeField] private AudioClip m_OpenSound;
        [SerializeField] private AudioClip m_CloseSound;
        [SerializeField] private AudioClip m_LockedSound;
        [SerializeField] private AudioClip m_UnlockSound;

        [Header("Save System")]
        [SerializeField] private string m_UniqueId;

        // Non-serialized private instance fields
        private AudioSource m_AudioSource;
        private Quaternion m_ClosedRotation;
        private Quaternion m_OpenRotation;
        private Coroutine m_AnimationCoroutine;
        private bool m_IsAnimating;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when the door is unlocked.
        /// </summary>
        public event Action OnUnlocked;

        /// <summary>
        /// Event fired when the door is locked.
        /// </summary>
        public event Action OnLockedEvent;

        /// <summary>
        /// Event fired when an unlock attempt fails.
        /// </summary>
        public event Action<KeyType> OnUnlockFailed;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsLocked => m_IsLocked;

        /// <inheritdoc/>
        public KeyType RequiredKeyType => m_RequiredKeyType;

        /// <inheritdoc/>
        public string UniqueId => m_UniqueId;

        /// <summary>
        /// Gets whether the key is consumed on unlock.
        /// </summary>
        public bool ConsumeKeyOnUnlock => m_ConsumeKeyOnUnlock;

        /// <summary>
        /// Gets the current door state.
        /// </summary>
        public DoorState CurrentState
        {
            get
            {
                if (m_IsLocked)
                {
                    return DoorState.Locked;
                }
                return IsOn ? DoorState.Open : DoorState.Closed;
            }
        }

        /// <inheritdoc/>
        public override string InteractionPrompt
        {
            get
            {
                if (m_IsLocked)
                {
                    return GetLockedPrompt();
                }
                return base.InteractionPrompt;
            }
        }

        /// <inheritdoc/>
        public override bool CanInteract => base.CanInteract && !m_IsAnimating;

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();

            m_AudioSource = GetComponent<AudioSource>();

            if (m_AudioSource == null)
            {
                Debug.LogError("[Door] AudioSource component is missing.", this);
            }

            InitializeRotations();
            GenerateUniqueIdIfEmpty();
        }

        protected override void Start()
        {
            ApplyInitialState();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (m_AnimationDuration <= 0f)
            {
                m_AnimationDuration = k_DefaultAnimationDuration;
                Debug.LogWarning("[Door] Animation duration must be positive, reset to default.", this);
            }

            if (string.IsNullOrEmpty(m_UniqueId))
            {
                GenerateUniqueIdIfEmpty();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Unlocks the door without requiring a key.
        /// </summary>
        public void Unlock()
        {
            if (!m_IsLocked)
            {
                return;
            }

            m_IsLocked = false;
            PlaySound(m_UnlockSound);
            UpdateAnimatorLockState();
            OnUnlocked?.Invoke();
        }

        /// <summary>
        /// Opens the door externally (e.g., from a switch).
        /// </summary>
        public void Open()
        {
            if (m_IsLocked)
            {
                Debug.LogWarning("[Door] Cannot open - door is locked.", this);
                PlaySound(m_LockedSound);
                return;
            }

            if (!IsOn)
            {
                SetState(true);
            }
        }

        /// <summary>
        /// Closes the door externally (e.g., from a switch).
        /// </summary>
        public void Close()
        {
            if (IsOn)
            {
                SetState(false);
            }
        }

        #endregion

        #region Interface Implementations

        /// <inheritdoc/>
        bool ILockable.TryUnlock(KeyType keyType)
        {
            if (!m_IsLocked)
            {
                Debug.LogWarning("[Door] Door is not locked.", this);
                return false;
            }

            if (m_RequiredKeyType == KeyType.None || keyType == m_RequiredKeyType)
            {
                Unlock();
                return true;
            }

            Debug.Log($"[Door] Unlock failed. Required: {m_RequiredKeyType}, Provided: {keyType}", this);
            PlaySound(m_LockedSound);
            OnUnlockFailed?.Invoke(keyType);
            return false;
        }

        /// <inheritdoc/>
        void ILockable.Lock()
        {
            if (m_IsLocked)
            {
                return;
            }

            // Close door if open before locking
            if (IsOn)
            {
                SetState(false);
            }

            m_IsLocked = true;
            UpdateAnimatorLockState();
            OnLockedEvent?.Invoke();
        }

        /// <inheritdoc/>
        object ISaveable.GetSaveData()
        {
            return new DoorSaveData
            {
                IsOpen = IsOn,
                IsLocked = m_IsLocked
            };
        }

        /// <inheritdoc/>
        void ISaveable.LoadSaveData(object data)
        {
            if (data is DoorSaveData saveData)
            {
                m_IsLocked = saveData.IsLocked;
                ApplyState(saveData.IsOpen, false);
            }
            else
            {
                Debug.LogError("[Door] Invalid save data type.", this);
            }
        }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void OnInteractInternal()
        {
            if (m_IsLocked)
            {
                PlaySound(m_LockedSound);
                Debug.Log($"[Door] Door is locked. Requires {m_RequiredKeyType} key.", this);
                return;
            }

            base.OnInteractInternal();
        }

        /// <inheritdoc/>
        protected override void ApplyState(bool isOn, bool animate)
        {
            if (animate && m_AnimationDuration > 0f)
            {
                AnimateToState(isOn);
            }
            else
            {
                SetRotationImmediate(isOn);
            }

            if (animate)
            {
                PlaySound(isOn ? m_OpenSound : m_CloseSound);
            }

            if (m_UseAnimator && m_Animator != null)
            {
                m_Animator.SetBool(k_AnimatorOpenParameter, isOn);
            }
        }

        /// <inheritdoc/>
        protected override void OnToggleStateChanged(bool previousState, bool newState)
        {
            Debug.Log($"[Door] State changed: {(previousState ? "Open" : "Closed")} -> {(newState ? "Open" : "Closed")}", this);
        }

        #endregion

        #region Private Methods

        private void InitializeRotations()
        {
            var pivot = m_PivotPoint != null ? m_PivotPoint : transform;
            m_ClosedRotation = pivot.localRotation;
            m_OpenRotation = m_ClosedRotation * Quaternion.Euler(0f, m_OpenAngle, 0f);
        }

        private void ApplyInitialState()
        {
            switch (m_InitialState)
            {
                case DoorState.Closed:
                    m_IsLocked = false;
                    ApplyState(false, false);
                    break;

                case DoorState.Open:
                    m_IsLocked = false;
                    ApplyState(true, false);
                    break;

                case DoorState.Locked:
                    m_IsLocked = true;
                    ApplyState(false, false);
                    break;
            }

            UpdateAnimatorLockState();
        }

        private void AnimateToState(bool isOn)
        {
            if (m_AnimationCoroutine != null)
            {
                StopCoroutine(m_AnimationCoroutine);
            }

            m_AnimationCoroutine = StartCoroutine(AnimateRotation(isOn));
        }

        private System.Collections.IEnumerator AnimateRotation(bool isOn)
        {
            m_IsAnimating = true;

            var pivot = m_PivotPoint != null ? m_PivotPoint : transform;
            var startRotation = pivot.localRotation;
            var targetRotation = isOn ? m_OpenRotation : m_ClosedRotation;

            var elapsed = 0f;

            while (elapsed < m_AnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.SmoothStep(0f, 1f, elapsed / m_AnimationDuration);
                pivot.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            pivot.localRotation = targetRotation;
            m_IsAnimating = false;
            m_AnimationCoroutine = null;
        }

        private void SetRotationImmediate(bool isOn)
        {
            var pivot = m_PivotPoint != null ? m_PivotPoint : transform;
            pivot.localRotation = isOn ? m_OpenRotation : m_ClosedRotation;
        }

        private void UpdateAnimatorLockState()
        {
            if (m_UseAnimator && m_Animator != null)
            {
                m_Animator.SetBool(k_AnimatorLockedParameter, m_IsLocked);
            }
        }

        private string GetLockedPrompt()
        {
            if (m_RequiredKeyType == KeyType.None)
            {
                return "Locked";
            }

            return $"Locked - Requires {m_RequiredKeyType} Key";
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
                m_UniqueId = $"Door_{gameObject.name}_{GetInstanceID()}";
            }
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Save data structure for door state.
        /// </summary>
        [Serializable]
        private struct DoorSaveData
        {
            public bool IsOpen;
            public bool IsLocked;
        }

        #endregion
    }
}
