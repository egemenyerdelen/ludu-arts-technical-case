using System;
using System.Collections;
using System.Collections.Generic;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player;
using UnityEngine;
using UnityEngine.Events;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    /// <summary>
    /// Interactive switch that can toggle objects and trigger connected actions.
    /// Supports three behavioral modes: Toggle, Momentary, and OneShot.
    /// Designed for clean architecture and performance.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Switch : ToggleInteractable
    {
        #region Fields

        // Animation constants
        private const float k_LeverAnimationSpeed = 5f;

        [Header("Switch Behavior")]
        [SerializeField] private SwitchType m_SwitchType = SwitchType.Toggle;
        [SerializeField] private float m_MomentaryResetDelay = 0.5f;

        [Header("Connected Objects")]
        [SerializeField] private List<ConnectedTarget> m_Targets = new List<ConnectedTarget>();

        [Header("Visual Animation")]
        [SerializeField] private Transform m_LeverPivot;
        [SerializeField] private float m_LeverRotationAngle = 45f;

        [Header("Audio")]
        [SerializeField] private AudioClip m_SwitchOnSound;
        [SerializeField] private AudioClip m_SwitchOffSound;
        [SerializeField] private AudioClip m_DisabledSound;

        [Header("Events")]
        [SerializeField] private UnityEvent m_OnSwitchOn;
        [SerializeField] private UnityEvent m_OnSwitchOff;

        // Runtime state
        private AudioSource m_AudioSource;
        private float m_CurrentLeverAngle;
        private float m_TargetLeverAngle;
        private Coroutine m_MomentaryResetCoroutine;
        private bool m_IsDisabled;

        // Cached component references for performance
        private Dictionary<GameObject, IToggleable> m_ToggleableCache = new Dictionary<GameObject, IToggleable>();
        private Dictionary<GameObject, Chest> m_ChestCache = new Dictionary<GameObject, Chest>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of this switch.
        /// </summary>
        public SwitchType Type => m_SwitchType;

        /// <summary>
        /// Gets whether this switch is currently disabled (OneShot after activation).
        /// </summary>
        public bool IsDisabled => m_IsDisabled;

        /// <inheritdoc/>
        public override bool CanInteract => base.CanInteract && !m_IsDisabled;

        /// <inheritdoc/>
        public override string InteractionPrompt
        {
            get
            {
                if (m_IsDisabled)
                    return "Already Used";

                return m_SwitchType switch
                {
                    SwitchType.Momentary => IsOn ? "Hold" : "Press",
                    SwitchType.OneShot => "Activate",
                    _ => base.InteractionPrompt
                };
            }
        }

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            m_AudioSource = GetComponent<AudioSource>();

            if (m_AudioSource == null)
            {
                Debug.LogError($"[{name}] AudioSource component missing.", this);
            }

            CacheTargetReferences();
        }

        protected override void Start()
        {
            base.Start();

            // Initialize lever to match starting state
            m_CurrentLeverAngle = IsOn ? m_LeverRotationAngle : -m_LeverRotationAngle;
            m_TargetLeverAngle = m_CurrentLeverAngle;
            ApplyLeverRotation();
        }

        private void Update()
        {
            AnimateLever();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_MomentaryResetDelay = Mathf.Max(0.1f, m_MomentaryResetDelay);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_MomentaryResetCoroutine != null)
            {
                StopCoroutine(m_MomentaryResetCoroutine);
                m_MomentaryResetCoroutine = null;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Resets a OneShot switch to be usable again (useful for debugging).
        /// </summary>
        public void ResetOneShot()
        {
            if (m_SwitchType != SwitchType.OneShot)
            {
                Debug.LogWarning($"[{name}] ResetOneShot called on non-OneShot switch.", this);
                return;
            }

            m_IsDisabled = false;
            SetState(false);
        }

        #endregion

        #region Interaction Handling

        protected override void OnInteractInternal(InteractionDetector interactionDetector)
        {
            switch (m_SwitchType)
            {
                case SwitchType.Toggle:
                    base.OnInteractInternal(interactionDetector);
                    break;

                case SwitchType.Momentary:
                    HandleMomentary();
                    break;

                case SwitchType.OneShot:
                    HandleOneShot();
                    break;
            }
        }

        private void HandleMomentary()
        {
            // Cancel any existing reset
            if (m_MomentaryResetCoroutine != null)
            {
                StopCoroutine(m_MomentaryResetCoroutine);
            }

            // Activate
            SetState(true);

            // Schedule automatic deactivation
            m_MomentaryResetCoroutine = StartCoroutine(MomentaryResetRoutine());
        }

        private void HandleOneShot()
        {
            if (m_IsDisabled)
            {
                if (m_DisabledSound != null)
                {
                    m_AudioSource.PlayOneShot(m_DisabledSound);
                }
                return;
            }

            // Activate once and disable
            SetState(true);
            m_IsDisabled = true;
        }

        private IEnumerator MomentaryResetRoutine()
        {
            yield return new WaitForSeconds(m_MomentaryResetDelay);
            SetState(false);
            m_MomentaryResetCoroutine = null;
        }

        #endregion

        #region State Management

        protected override void ApplyState(bool isOn, bool animate)
        {
            // Update visual state
            UpdateVisuals(isOn, animate);

            // Execute connected actions
            ExecuteTargetActions(isOn);

            // Fire events
            if (isOn)
            {
                m_OnSwitchOn?.Invoke();
            }
            else
            {
                m_OnSwitchOff?.Invoke();
            }
        }

        protected override void OnToggleStateChanged(bool previousState, bool newState)
        {
            Debug.Log($"[{name}] State: {(previousState ? "On" : "Off")} → {(newState ? "On" : "Off")}", this);
        }

        #endregion

        #region Visual & Audio

        private void UpdateVisuals(bool isOn, bool animate)
        {
            // Lever animation
            m_TargetLeverAngle = isOn ? m_LeverRotationAngle : -m_LeverRotationAngle;

            if (!animate)
            {
                m_CurrentLeverAngle = m_TargetLeverAngle;
                ApplyLeverRotation();
            }

            // Audio feedback
            if (animate)
            {
                var clip = isOn ? m_SwitchOnSound : m_SwitchOffSound;
                if (clip != null && m_AudioSource != null)
                {
                    m_AudioSource.PlayOneShot(clip);
                }
            }
        }

        private void AnimateLever()
        {
            if (m_LeverPivot == null || Mathf.Approximately(m_CurrentLeverAngle, m_TargetLeverAngle))
            {
                return;
            }

            m_CurrentLeverAngle = Mathf.MoveTowards(
                m_CurrentLeverAngle,
                m_TargetLeverAngle,
                k_LeverAnimationSpeed * m_LeverRotationAngle * 2f * Time.deltaTime
            );

            ApplyLeverRotation();
        }

        private void ApplyLeverRotation()
        {
            if (m_LeverPivot != null)
            {
                m_LeverPivot.localRotation = Quaternion.Euler(m_CurrentLeverAngle, 0f, 0f);
            }
        }

        #endregion

        #region Target Actions

        private void CacheTargetReferences()
        {
            m_ToggleableCache.Clear();
            m_ChestCache.Clear();

            foreach (var target in m_Targets)
            {
                if (target.TargetObject == null)
                    continue;

                // Cache toggleable reference
                var toggleable = target.TargetObject.GetComponent<IToggleable>();
                if (toggleable != null)
                {
                    m_ToggleableCache[target.TargetObject] = toggleable;
                }

                // Cache chest reference
                var chest = target.TargetObject.GetComponent<Chest>();
                if (chest != null)
                {
                    m_ChestCache[target.TargetObject] = chest;
                }
            }
        }

        private void ExecuteTargetActions(bool switchIsOn)
        {
            foreach (var target in m_Targets)
            {
                if (target.TargetObject == null || !target.Enabled)
                    continue;

                switch (target.ActionType)
                {
                    case TargetActionType.Toggle:
                        ExecuteToggleAction(target, switchIsOn);
                        break;

                    case TargetActionType.OpenChest:
                        if (switchIsOn) // Only open on switch activation
                        {
                            ExecuteOpenChestAction(target);
                        }
                        break;
                }
            }
        }

        private void ExecuteToggleAction(ConnectedTarget target, bool switchIsOn)
        {
            // Use cached reference for performance
            if (m_ToggleableCache.TryGetValue(target.TargetObject, out var toggleable))
            {
                toggleable.SetState(switchIsOn);
                return;
            }

            // Fallback for Door (locked doors block IToggleable.SetState)
            var door = target.TargetObject.GetComponent<Door>();
            if (door != null)
            {
                if (switchIsOn)
                    door.Open();
                else
                    door.Close();
            }
        }

        private void ExecuteOpenChestAction(ConnectedTarget target)
        {
            // Use cached reference for performance
            if (!m_ChestCache.TryGetValue(target.TargetObject, out var chest))
            {
                Debug.LogWarning($"[{name}] Target '{target.TargetObject.name}' has no Chest component.", this);
                return;
            }

            if (chest.IsOpened)
            {
                Debug.Log($"[{name}] Chest '{target.TargetObject.name}' is already open.", this);
                return;
            }

            // Start coroutine to simulate player hold
            StartCoroutine(SimulateChestHold(chest));
        }

        /// <summary>
        /// Simulates a player holding to open a chest over time.
        /// Note: This only OPENS the chest - player must interact to collect contents.
        /// </summary>
        private IEnumerator SimulateChestHold(Chest chest)
        {
            IInteractable interactable = chest;
            if (interactable == null)
            {
                Debug.LogError($"[{name}] Chest does not implement IInteractable!", this);
                yield break;
            }

            // Start the hold
            interactable.OnHoldStart(null);

            // Simulate hold progress over the chest's hold duration
            var holdDuration = chest.HoldDuration;
            var elapsed = 0f;

            while (elapsed < holdDuration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / holdDuration);

                // Update progress (this animates the lid)
                interactable.OnHoldProgress(progress);

                yield return null;
            }

            // Complete the hold WITHOUT auto-collecting items
            // Player will need to interact with open chest to collect
            interactable.OnHoldComplete(null);
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Defines how the switch behaves when interacted with.
        /// </summary>
        public enum SwitchType
        {
            /// <summary>
            /// Standard toggle switch - stays in position until toggled again.
            /// </summary>
            Toggle,

            /// <summary>
            /// Momentary switch - automatically returns to OFF after a delay.
            /// </summary>
            Momentary,

            /// <summary>
            /// One-shot switch - can only be activated once, then becomes disabled.
            /// </summary>
            OneShot
        }

        /// <summary>
        /// Defines what action to perform on a connected target.
        /// </summary>
        public enum TargetActionType
        {
            /// <summary>
            /// Toggle the target on/off (for doors, switches, etc).
            /// </summary>
            Toggle,

            /// <summary>
            /// Force open a chest (triggers hold interaction).
            /// </summary>
            OpenChest
        }

        /// <summary>
        /// Represents a connected object that this switch can trigger.
        /// </summary>
        [Serializable]
        public class ConnectedTarget
        {
            [Tooltip("Enable/disable this target action")]
            public bool Enabled = true;

            [Tooltip("The GameObject to interact with")]
            public GameObject TargetObject;

            [Tooltip("What action to perform on the target")]
            public TargetActionType ActionType = TargetActionType.Toggle;

            [Tooltip("Optional: Custom name for this connection (for organization)")]
            public string Notes;
        }

        #endregion
    }
}
