using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.UI;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player
{
    /// <summary>
    /// Detects interactable objects in the world and handles player input for interactions.
    /// Attach to the player and assign the camera for raycasting.
    /// </summary>
    public class InteractionDetector : MonoBehaviour
    {
        #region Fields

        // Private constant fields
        private const float k_MinInteractionRange = 0.5f;
        private const float k_MaxInteractionRange = 10f;
        private const float k_DefaultSpherecastRadius = 0.1f;

        // Serialized private instance fields
        [Header("Detection Settings")]
        [SerializeField] private float m_InteractionRange = 3f;
        [SerializeField] private LayerMask m_InteractionLayerMask = -1;
        [SerializeField] private Transform m_RaycastOrigin;
        [SerializeField] private bool m_UseSpherecast = true;
        [SerializeField] private float m_SpherecastRadius = 0.1f;

        [Header("Input Settings")]
        [SerializeField] private KeyCode m_InteractionKey = KeyCode.E;
        [SerializeField] private bool m_UseInputSystem;

        [Header("UI Reference")]
        [SerializeField] private InteractionUIManager m_UIManager;

        [Header("Debug")]
        [SerializeField] private bool m_ShowDebugRay;
        [SerializeField] private Color m_DebugRayColor = Color.green;

        // Non-serialized private instance fields
        private IInteractable m_CurrentTarget;
        private GameObject m_CurrentTargetObject;
        private bool m_IsHolding;
        private float m_HoldStartTime;
        private float m_CurrentHoldProgress;
        private Camera m_Camera;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a new interactable is targeted.
        /// </summary>
        public event Action<IInteractable> OnTargetChanged;

        /// <summary>
        /// Event fired when an interaction is performed.
        /// </summary>
        public event Action<IInteractable> OnInteractionPerformed;

        /// <summary>
        /// Event fired when hold progress updates.
        /// </summary>
        public event Action<float> OnHoldProgressChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the currently targeted interactable.
        /// </summary>
        public IInteractable CurrentTarget => m_CurrentTarget;

        /// <summary>
        /// Gets whether the player is currently holding to interact.
        /// </summary>
        public bool IsHolding => m_IsHolding;

        /// <summary>
        /// Gets the current hold progress (0-1).
        /// </summary>
        public float CurrentHoldProgress => m_CurrentHoldProgress;

        /// <summary>
        /// Gets or sets the interaction range.
        /// </summary>
        public float InteractionRange
        {
            get => m_InteractionRange;
            set => m_InteractionRange = Mathf.Clamp(value, k_MinInteractionRange, k_MaxInteractionRange);
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            InitializeCamera();
            ValidateSetup();
        }

        private void Update()
        {
            UpdateDetection();
            HandleInput();
            UpdateHoldProgress();
        }

        private void OnValidate()
        {
            m_InteractionRange = Mathf.Clamp(m_InteractionRange, k_MinInteractionRange, k_MaxInteractionRange);
            m_SpherecastRadius = Mathf.Max(0f, m_SpherecastRadius);
        }

        private void OnDrawGizmosSelected()
        {
            if (!m_ShowDebugRay)
            {
                return;
            }

            Transform origin = m_RaycastOrigin != null ? m_RaycastOrigin : transform;
            Vector3 direction = origin.forward;

            Gizmos.color = m_DebugRayColor;
            Gizmos.DrawRay(origin.position, direction * m_InteractionRange);
            Gizmos.DrawWireSphere(origin.position + direction * m_InteractionRange, m_SpherecastRadius);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Forces interaction with the current target.
        /// </summary>
        public void ForceInteract()
        {
            if (m_CurrentTarget != null && m_CurrentTarget.CanInteract)
            {
                PerformInteraction();
            }
        }

        /// <summary>
        /// Clears the current target.
        /// </summary>
        public void ClearTarget()
        {
            SetTarget(null, null);
        }

        /// <summary>
        /// Sets the interaction key.
        /// </summary>
        /// <param name="key">The new interaction key.</param>
        public void SetInteractionKey(KeyCode key)
        {
            m_InteractionKey = key;
        }

        #endregion

        #region Private Methods

        private void InitializeCamera()
        {
            if (m_RaycastOrigin == null)
            {
                m_Camera = Camera.main;
                
                if (m_Camera != null)
                {
                    m_RaycastOrigin = m_Camera.transform;
                }
                else
                {
                    Debug.LogError("[InteractionDetector] No camera found. Please assign RaycastOrigin.", this);
                }
            }
        }

        private void ValidateSetup()
        {
            if (m_UIManager == null)
            {
                m_UIManager = FindAnyObjectByType<InteractionUIManager>();
                
                if (m_UIManager == null)
                {
                    Debug.LogWarning("[InteractionDetector] No InteractionUIManager found in scene.", this);
                }
            }
        }

        private void UpdateDetection()
        {
            if (m_RaycastOrigin == null)
            {
                return;
            }

            IInteractable detectedInteractable = null;
            GameObject detectedObject = null;

            // Perform raycast
            Ray ray = new Ray(m_RaycastOrigin.position, m_RaycastOrigin.forward);
            RaycastHit hit;
            bool didHit;

            if (m_UseSpherecast && m_SpherecastRadius > 0f)
            {
                didHit = Physics.SphereCast(ray, m_SpherecastRadius, out hit, m_InteractionRange, m_InteractionLayerMask);
            }
            else
            {
                didHit = Physics.Raycast(ray, out hit, m_InteractionRange, m_InteractionLayerMask);
            }

            // Debug ray
            if (m_ShowDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * m_InteractionRange, didHit ? Color.green : Color.red);
            }

            // Check for interactable
            if (didHit)
            {
                detectedInteractable = hit.collider.GetComponentInParent<IInteractable>();
                
                if (detectedInteractable != null)
                {
                    detectedObject = hit.collider.gameObject;
                }
            }

            // Update target if changed
            if (detectedInteractable != m_CurrentTarget)
            {
                SetTarget(detectedInteractable, detectedObject);
            }
        }

        private void SetTarget(IInteractable newTarget, GameObject targetObject)
        {
            // Exit previous target
            if (m_CurrentTarget != null)
            {
                m_CurrentTarget.OnFocusExit();
                CancelHold();
            }

            // Set new target
            m_CurrentTarget = newTarget;
            m_CurrentTargetObject = targetObject;

            // Enter new target
            if (m_CurrentTarget != null)
            {
                m_CurrentTarget.OnFocusEnter();
            }

            // Update UI
            UpdateUI();

            // Fire event
            OnTargetChanged?.Invoke(m_CurrentTarget);
        }

        private void HandleInput()
        {
            if (m_CurrentTarget == null || !m_CurrentTarget.CanInteract)
            {
                if (m_IsHolding)
                {
                    CancelHold();
                }
                return;
            }

            bool interactPressed = Input.GetKeyDown(m_InteractionKey);
            bool interactHeld = Input.GetKey(m_InteractionKey);
            bool interactReleased = Input.GetKeyUp(m_InteractionKey);

            switch (m_CurrentTarget.InteractionType)
            {
                case InteractionType.Instant:
                case InteractionType.Toggle:
                    if (interactPressed)
                    {
                        PerformInteraction();
                    }
                    break;

                case InteractionType.Hold:
                    HandleHoldInput(interactPressed, interactHeld, interactReleased);
                    break;
            }
        }

        private void HandleHoldInput(bool pressed, bool held, bool released)
        {
            if (pressed && !m_IsHolding)
            {
                StartHold();
            }
            else if (released && m_IsHolding)
            {
                CancelHold();
            }
        }

        private void StartHold()
        {
            m_IsHolding = true;
            m_HoldStartTime = Time.time;
            m_CurrentHoldProgress = 0f;

            m_CurrentTarget.OnHoldStart();
            UpdateUI();
        }

        private void UpdateHoldProgress()
        {
            if (!m_IsHolding || m_CurrentTarget == null)
            {
                return;
            }

            float holdDuration = m_CurrentTarget.HoldDuration;
            
            if (holdDuration <= 0f)
            {
                CompleteHold();
                return;
            }

            float elapsed = Time.time - m_HoldStartTime;
            m_CurrentHoldProgress = Mathf.Clamp01(elapsed / holdDuration);

            // Notify target of progress
            m_CurrentTarget.OnHoldProgress(m_CurrentHoldProgress);

            // Fire progress event
            OnHoldProgressChanged?.Invoke(m_CurrentHoldProgress);

            // Update UI
            if (m_UIManager != null)
            {
                m_UIManager.UpdateHoldProgress(m_CurrentHoldProgress);
            }

            // Check for completion
            if (m_CurrentHoldProgress >= 1f)
            {
                CompleteHold();
            }
        }

        private void CompleteHold()
        {
            if (m_CurrentTarget == null)
            {
                return;
            }

            m_IsHolding = false;
            m_CurrentHoldProgress = 0f;

            m_CurrentTarget.OnHoldComplete();
            OnInteractionPerformed?.Invoke(m_CurrentTarget);

            UpdateUI();
        }

        private void CancelHold()
        {
            if (!m_IsHolding)
            {
                return;
            }

            m_IsHolding = false;
            m_CurrentHoldProgress = 0f;

            if (m_CurrentTarget != null)
            {
                m_CurrentTarget.OnHoldCancel();
            }

            UpdateUI();
        }

        private void PerformInteraction()
        {
            if (m_CurrentTarget == null)
            {
                return;
            }

            m_CurrentTarget.OnInteract();
            OnInteractionPerformed?.Invoke(m_CurrentTarget);

            // Refresh UI in case prompt changed
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (m_UIManager == null)
            {
                return;
            }

            if (m_CurrentTarget != null && m_CurrentTarget.CanInteract)
            {
                m_UIManager.ShowPrompt(
                    m_CurrentTarget.InteractionPrompt,
                    m_InteractionKey.ToString(),
                    m_CurrentTarget.InteractionType
                );

                if (m_CurrentTarget.InteractionType == InteractionType.Hold)
                {
                    m_UIManager.ShowHoldProgress(m_IsHolding);
                    m_UIManager.UpdateHoldProgress(m_CurrentHoldProgress);
                }
                else
                {
                    m_UIManager.HideHoldProgress();
                }
            }
            else if (m_CurrentTarget != null && !m_CurrentTarget.CanInteract)
            {
                // Show "cannot interact" feedback
                m_UIManager.ShowCannotInteract(m_CurrentTarget.InteractionPrompt);
            }
            else
            {
                m_UIManager.HidePrompt();
                m_UIManager.HideHoldProgress();
            }
        }

        #endregion
    }
}
