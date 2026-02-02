using UnityEngine;
using UnityEngine.InputSystem;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Player
{
    /// <summary>
    /// Simple first-person player controller for testing the interaction system.
    /// Provides basic WASD movement and mouse look using the new Input System.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimplePlayerController : MonoBehaviour
    {
        #region Fields

        // Private constant fields
        private const float k_MinMoveSpeed = 1f;
        private const float k_MaxMoveSpeed = 20f;
        private const float k_MinSensitivity = 0.1f;
        private const float k_MaxSensitivity = 100f;
        private const float k_Gravity = -100f;
        private const float k_GroundedDownForce = -5f;
        private const float k_DefaultCameraHeight = 0.8f;
        private const float k_DefaultGroundCheckOffset = -0.9f;

        // Serialized private instance fields
        [Header("Movement")]
        [SerializeField] private float m_MoveSpeed = 5f;
        [SerializeField] private float m_SprintMultiplier = 1.5f;
        [SerializeField] private float m_JumpHeight = 1.2f;

        [Header("Look")]
        [SerializeField] private float m_MouseSensitivity = 2f;
        [SerializeField] private float m_VerticalLookLimit = 90f;
        [SerializeField] private Transform m_CameraTransform;

        [Header("Ground Check")]
        [SerializeField] private Transform m_GroundCheck;
        [SerializeField] private float m_GroundDistance = 0.4f;
        [SerializeField] private LayerMask m_GroundMask = -1;

        [Header("Settings")]
        [SerializeField] private bool m_LockCursor = true;

        // Non-serialized private instance fields
        private CharacterController m_CharacterController;
        private Core.Managers.InputManager m_InputManager;
        private Vector3 m_Velocity;
        private float m_VerticalRotation;
        private bool m_IsGrounded;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the movement speed.
        /// </summary>
        public float MoveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = Mathf.Clamp(value, k_MinMoveSpeed, k_MaxMoveSpeed);
        }

        /// <summary>
        /// Gets or sets the mouse sensitivity.
        /// </summary>
        public float MouseSensitivity
        {
            get => m_MouseSensitivity;
            set => m_MouseSensitivity = Mathf.Clamp(value, k_MinSensitivity, k_MaxSensitivity);
        }

        /// <summary>
        /// Gets whether the player is grounded.
        /// </summary>
        public bool IsGrounded => m_IsGrounded;

        /// <summary>
        /// Gets the character controller component.
        /// </summary>
        public CharacterController CharacterController => m_CharacterController;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();

            if (m_CharacterController == null)
            {
                Debug.LogError("[SimplePlayerController] CharacterController component is missing.", this);
            }

            InitializeCamera();
            InitializeGroundCheck();
        }

        private void Start()
        {
            m_InputManager = Core.Managers.InputManager.Instance;

            if (m_InputManager == null)
            {
                Debug.LogError("[SimplePlayerController] InputManager instance not found!", this);
                enabled = false;
                return;
            }

            SubscribeToInputEvents();

            if (m_LockCursor)
            {
                LockCursor();
            }
        }

        private void Update()
        {
            CheckGround();
            HandleMovement();
            HandleLook();
        }

        private void OnDestroy()
        {
            UnsubscribeFromInputEvents();
        }

        private void OnValidate()
        {
            m_MoveSpeed = Mathf.Clamp(m_MoveSpeed, k_MinMoveSpeed, k_MaxMoveSpeed);
            m_MouseSensitivity = Mathf.Clamp(m_MouseSensitivity, k_MinSensitivity, k_MaxSensitivity);
            m_VerticalLookLimit = Mathf.Clamp(m_VerticalLookLimit, 0f, 90f);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Locks the cursor to the center of the screen.
        /// </summary>
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Unlocks the cursor.
        /// </summary>
        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Toggles cursor lock state.
        /// </summary>
        public void ToggleCursorLock()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        #endregion

        #region Private Methods

        private void SubscribeToInputEvents()
        {
            if (m_InputManager == null)
            {
                Debug.LogError("[SimplePlayerController] Cannot subscribe to input events - InputManager is null.", this);
                return;
            }

            m_InputManager.InputActions.Player.Jump.performed += OnJumpPerformed;
            m_InputManager.InputActions.UI.Cancel.performed += OnCancelPerformed;
            m_InputManager.InputActions.UI.Click.performed += OnClickPerformed;
        }

        private void UnsubscribeFromInputEvents()
        {
            if (m_InputManager == null)
            {
                return;
            }

            m_InputManager.InputActions.Player.Jump.performed -= OnJumpPerformed;
            m_InputManager.InputActions.UI.Cancel.performed -= OnCancelPerformed;
            m_InputManager.InputActions.UI.Click.performed -= OnClickPerformed;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (m_IsGrounded)
            {
                m_Velocity.y = Mathf.Sqrt(m_JumpHeight * -2f * k_Gravity);
            }
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            ToggleCursorLock();
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        private void InitializeCamera()
        {
            if (m_CameraTransform == null)
            {
                var mainCamera = Camera.main;

                if (mainCamera != null)
                {
                    m_CameraTransform = mainCamera.transform;

                    // Parent camera to player if not already
                    if (m_CameraTransform.parent != transform)
                    {
                        var cameraHolder = new GameObject("CameraHolder");
                        cameraHolder.transform.SetParent(transform);
                        cameraHolder.transform.localPosition = new Vector3(0f, k_DefaultCameraHeight, 0f);
                        cameraHolder.transform.localRotation = Quaternion.identity;

                        m_CameraTransform.SetParent(cameraHolder.transform);
                        m_CameraTransform.localPosition = Vector3.zero;
                        m_CameraTransform.localRotation = Quaternion.identity;

                        m_CameraTransform = cameraHolder.transform;
                    }
                }
                else
                {
                    Debug.LogWarning("[SimplePlayerController] No camera assigned and no main camera found.", this);
                }
            }
        }

        private void InitializeGroundCheck()
        {
            if (m_GroundCheck == null)
            {
                var groundCheck = new GameObject("GroundCheck");
                groundCheck.transform.SetParent(transform);
                groundCheck.transform.localPosition = new Vector3(0f, k_DefaultGroundCheckOffset, 0f);
                m_GroundCheck = groundCheck.transform;
            }
        }

        private void CheckGround()
        {
            m_IsGrounded = Physics.CheckSphere(m_GroundCheck.position, m_GroundDistance, m_GroundMask);

            if (m_IsGrounded && m_Velocity.y < 0f)
            {
                m_Velocity.y = k_GroundedDownForce;
            }
        }

        private void HandleMovement()
        {
            if (m_InputManager == null)
            {
                return;
            }

            var moveInput = m_InputManager.InputActions.Player.Move.ReadValue<Vector2>();

            var moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            moveDirection.Normalize();

            var currentSpeed = m_MoveSpeed;
            var isSprinting = m_InputManager.InputActions.Player.Sprint.IsPressed();
            if (isSprinting)
            {
                currentSpeed *= m_SprintMultiplier;
            }

            m_CharacterController.Move(moveDirection * (currentSpeed * Time.deltaTime));

            // Apply gravity
            m_Velocity.y += k_Gravity * Time.deltaTime;
            m_CharacterController.Move(m_Velocity * Time.deltaTime);
        }

        private void HandleLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            if (m_CameraTransform == null || m_InputManager == null)
            {
                return;
            }

            var lookInput = m_InputManager.InputActions.Player.Look.ReadValue<Vector2>();
            var mouseX = lookInput.x * m_MouseSensitivity * Time.deltaTime;
            var mouseY = lookInput.y * m_MouseSensitivity * Time.deltaTime;

            // Horizontal rotation (rotate player body)
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation (rotate camera)
            m_VerticalRotation -= mouseY;
            m_VerticalRotation = Mathf.Clamp(m_VerticalRotation, -m_VerticalLookLimit, m_VerticalLookLimit);
            m_CameraTransform.localRotation = Quaternion.Euler(m_VerticalRotation, 0f, 0f);
        }

        #endregion
    }
}
