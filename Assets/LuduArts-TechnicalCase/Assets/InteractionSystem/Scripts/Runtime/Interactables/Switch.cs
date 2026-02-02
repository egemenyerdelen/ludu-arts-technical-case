using System;
using System.Collections.Generic;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    /// <summary>
    /// Interactive switch/lever that can toggle and trigger connected objects.
    /// Supports chained interactions via events.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Switch : ToggleInteractable, ISaveable
    {
        #region Fields

        // Private constant fields
        private const string k_AnimatorOnParameter = "IsOn";

        // Serialized private instance fields
        [Header("Switch Settings")]
        [SerializeField] private SwitchType m_SwitchType = SwitchType.Toggle;

        [Header("Connected Objects")]
        [SerializeField] private List<GameObject> m_ConnectedObjects = new List<GameObject>();
        [SerializeField] private bool m_InvertConnectedState;

        [Header("Animation")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private bool m_UseAnimator = true;
        [SerializeField] private Transform m_LeverPivot;
        [SerializeField] private float m_LeverAngle = 45f;
        [SerializeField] private float m_AnimationSpeed = 5f;

        [Header("Audio")]
        [SerializeField] private AudioClip m_SwitchOnSound;
        [SerializeField] private AudioClip m_SwitchOffSound;

        [Header("Events")]
        [SerializeField] private UnityEvent m_OnSwitchOn;
        [SerializeField] private UnityEvent m_OnSwitchOff;
        [SerializeField] private UnityEvent<bool> m_OnSwitchStateChanged;

        [Header("Save System")]
        [SerializeField] private string m_UniqueId;

        // Non-serialized private instance fields
        private AudioSource m_AudioSource;
        private Quaternion m_OffRotation;
        private Quaternion m_OnRotation;
        private float m_CurrentLeverAngle;
        private float m_TargetLeverAngle;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when switch is turned on.
        /// </summary>
        public event Action OnSwitchedOn;

        /// <summary>
        /// Event fired when switch is turned off.
        /// </summary>
        public event Action OnSwitchedOff;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public string UniqueId => m_UniqueId;

        /// <summary>
        /// Gets the type of this switch.
        /// </summary>
        public SwitchType Type => m_SwitchType;

        /// <summary>
        /// Gets the connected objects list.
        /// </summary>
        public IReadOnlyList<GameObject> ConnectedObjects => m_ConnectedObjects;

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();
            
            m_AudioSource = GetComponent<AudioSource>();
            
            if (m_AudioSource == null)
            {
                Debug.LogError("[Switch] AudioSource component is missing.", this);
            }

            InitializeLeverRotations();
            GenerateUniqueIdIfEmpty();
        }

        protected override void Start()
        {
            base.Start();
            
            // Initialize lever position
            m_CurrentLeverAngle = IsOn ? m_LeverAngle : -m_LeverAngle;
            m_TargetLeverAngle = m_CurrentLeverAngle;
            ApplyLeverRotation();
        }

        private void Update()
        {
            UpdateLeverAnimation();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (string.IsNullOrEmpty(m_UniqueId))
            {
                GenerateUniqueIdIfEmpty();
            }

            // Clean null entries from connected objects
            m_ConnectedObjects.RemoveAll(obj => obj == null);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a connected object that will be triggered when this switch is toggled.
        /// </summary>
        /// <param name="target">The object to connect.</param>
        public void AddConnectedObject(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("[Switch] Cannot add null connected object.", this);
                return;
            }

            if (!m_ConnectedObjects.Contains(target))
            {
                m_ConnectedObjects.Add(target);
            }
        }

        /// <summary>
        /// Removes a connected object.
        /// </summary>
        /// <param name="target">The object to disconnect.</param>
        public void RemoveConnectedObject(GameObject target)
        {
            m_ConnectedObjects.Remove(target);
        }

        /// <inheritdoc/>
        public object GetSaveData()
        {
            return new SwitchSaveData
            {
                IsOn = IsOn
            };
        }

        /// <inheritdoc/>
        public void LoadSaveData(object data)
        {
            if (data is SwitchSaveData saveData)
            {
                // Apply state without animation
                SetState(saveData.IsOn);
            }
            else
            {
                Debug.LogError("[Switch] Invalid save data type.", this);
            }
        }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void ApplyState(bool isOn, bool animate)
        {
            // Set target lever angle
            m_TargetLeverAngle = isOn ? m_LeverAngle : -m_LeverAngle;

            if (!animate)
            {
                m_CurrentLeverAngle = m_TargetLeverAngle;
                ApplyLeverRotation();
            }

            // Update animator
            if (m_UseAnimator && m_Animator != null)
            {
                m_Animator.SetBool(k_AnimatorOnParameter, isOn);
            }

            // Play sound
            if (animate)
            {
                PlaySound(isOn ? m_SwitchOnSound : m_SwitchOffSound);
            }

            // Trigger connected objects
            TriggerConnectedObjects(isOn);

            // Fire Unity events
            if (isOn)
            {
                m_OnSwitchOn?.Invoke();
            }
            else
            {
                m_OnSwitchOff?.Invoke();
            }
            
            m_OnSwitchStateChanged?.Invoke(isOn);
        }

        /// <inheritdoc/>
        protected override void OnToggleStateChanged(bool previousState, bool newState)
        {
            Debug.Log($"[Switch] State changed: {(previousState ? "On" : "Off")} -> {(newState ? "On" : "Off")}", this);

            if (newState)
            {
                OnSwitchedOn?.Invoke();
            }
            else
            {
                OnSwitchedOff?.Invoke();
            }
        }

        #endregion

        #region Private Methods

        private void InitializeLeverRotations()
        {
            if (m_LeverPivot == null)
            {
                return;
            }

            m_OffRotation = m_LeverPivot.localRotation * Quaternion.Euler(-m_LeverAngle, 0f, 0f);
            m_OnRotation = m_LeverPivot.localRotation * Quaternion.Euler(m_LeverAngle, 0f, 0f);
        }

        private void UpdateLeverAnimation()
        {
            if (m_LeverPivot == null)
            {
                return;
            }

            // Smoothly interpolate to target angle
            if (!Mathf.Approximately(m_CurrentLeverAngle, m_TargetLeverAngle))
            {
                m_CurrentLeverAngle = Mathf.MoveTowards(
                    m_CurrentLeverAngle,
                    m_TargetLeverAngle,
                    m_AnimationSpeed * m_LeverAngle * 2f * Time.deltaTime
                );
                
                ApplyLeverRotation();
            }
        }

        private void ApplyLeverRotation()
        {
            if (m_LeverPivot == null)
            {
                return;
            }

            m_LeverPivot.localRotation = Quaternion.Euler(m_CurrentLeverAngle, 0f, 0f);
        }

        private void TriggerConnectedObjects(bool isOn)
        {
            var targetState = m_InvertConnectedState ? !isOn : isOn;

            foreach (var obj in m_ConnectedObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                // Try IToggleable first
                var toggleable = obj.GetComponent<IToggleable>();
                if (toggleable != null)
                {
                    toggleable.SetState(targetState);
                    continue;
                }

                // Try Door specifically
                var door = obj.GetComponent<Door>();
                if (door != null)
                {
                    if (targetState)
                    {
                        door.Open();
                    }
                    else
                    {
                        door.Close();
                    }
                    continue;
                }

                Debug.LogWarning($"[Switch] Connected object '{obj.name}' has no IToggleable or Door component.", this);
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
                m_UniqueId = $"Switch_{gameObject.name}_{GetInstanceID()}";
            }
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Types of switch behavior.
        /// </summary>
        public enum SwitchType
        {
            /// <summary>
            /// Standard toggle - stays in position after interaction.
            /// </summary>
            Toggle,

            /// <summary>
            /// Momentary - returns to off state when released.
            /// </summary>
            Momentary,

            /// <summary>
            /// One-shot - can only be activated once.
            /// </summary>
            OneShot
        }

        /// <summary>
        /// Save data structure for switch state.
        /// </summary>
        [Serializable]
        private struct SwitchSaveData
        {
            public bool IsOn;
        }

        #endregion
    }
}
