using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

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

        [Header("Debug")]
        [SerializeField] private bool m_ShowDebugRay;
        [SerializeField] private Color m_DebugRayColor = Color.green;

        // Non-serialized private instance fields
        private Core.Managers.InputManager m_InputManager;
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
        }

        private void Start()
        {
            m_InputManager = Core.Managers.InputManager.Instance;
            
            if (m_InputManager == null)
            {
                Debug.LogError("[InteractionDetector] InputManager instance not found!", this);
                enabled = false;
                return;
            }

            SubscribeToInputEvents();
        }

        private void Update()
        {
            UpdateDetection();
            UpdateHoldProgress();
        }

        private void OnDestroy()
        {
            UnsubscribeFromInputEvents();
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

            var origin = m_RaycastOrigin != null ? m_RaycastOrigin : transform;
            var direction = origin.forward;

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

        #endregion

        #region Private Methods

        private void SubscribeToInputEvents()
        {
            if (m_InputManager == null) return;

            var interactAction = m_InputManager.InputActions.Player.Interact;
            
            interactAction.started += OnInteractStarted;
            interactAction.performed += OnInteractPerformed;
            interactAction.canceled += OnInteractCanceled;
        }

        private void UnsubscribeFromInputEvents()
        {
            if (m_InputManager == null) return;

            var interactAction = m_InputManager.InputActions.Player.Interact;
            
            interactAction.started -= OnInteractStarted;
            interactAction.performed -= OnInteractPerformed;
            interactAction.canceled -= OnInteractCanceled;
        }

        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            if (m_CurrentTarget == null || !m_CurrentTarget.CanInteract)
            {
                return;
            }

            switch (m_CurrentTarget.InteractionType)
            {
                case InteractionType.Instant:
                case InteractionType.Toggle:
                    PerformInteraction();
                    break;

                case InteractionType.Hold:
                    if (!m_IsHolding)
                    {
                        StartHold();
                    }
                    break;
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            // This fires when the button is fully pressed (after started)
            // Can be used for additional logic if needed
        }

        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            if (m_IsHolding)
            {
                CancelHold();
            }
        }

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

        private void UpdateDetection()
        {
            if (m_RaycastOrigin == null)
            {
                return;
            }

            IInteractable detectedInteractable = null;
            GameObject detectedObject = null;

            // Perform raycast
            var ray = new Ray(m_RaycastOrigin.position, m_RaycastOrigin.forward);
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
            
            // Fire event
            OnTargetChanged?.Invoke(m_CurrentTarget);
        }

        private void StartHold()
        {
            m_IsHolding = true;
            m_HoldStartTime = Time.time;
            m_CurrentHoldProgress = 0f;

            m_CurrentTarget.OnHoldStart();
        }

        private void UpdateHoldProgress()
        {
            if (!m_IsHolding || m_CurrentTarget == null)
            {
                return;
            }

            var holdDuration = m_CurrentTarget.HoldDuration;
            
            if (holdDuration <= 0f)
            {
                CompleteHold();
                return;
            }

            var elapsed = Time.time - m_HoldStartTime;
            m_CurrentHoldProgress = Mathf.Clamp01(elapsed / holdDuration);

            // Notify target of progress
            m_CurrentTarget.OnHoldProgress(m_CurrentHoldProgress);

            // Fire progress event
            OnHoldProgressChanged?.Invoke(m_CurrentHoldProgress);

            // Check if hold is complete
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
        }

        private void PerformInteraction()
        {
            if (m_CurrentTarget == null)
            {
                return;
            }

            m_CurrentTarget.OnInteract();
            OnInteractionPerformed?.Invoke(m_CurrentTarget);
        }

        #endregion
    }
}