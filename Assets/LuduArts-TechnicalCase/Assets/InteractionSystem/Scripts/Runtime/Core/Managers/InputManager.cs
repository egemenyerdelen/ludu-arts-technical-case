using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Managers
{
    /// <summary>
    /// Singleton manager for the Unity Input System.
    /// Handles creation, lifecycle, and access to input actions.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Gets the singleton instance of the InputManager.
        /// </summary>
        public static InputManager Instance { get; private set; }

        /// <summary>
        /// Gets the input actions asset for reading player input.
        /// </summary>
        public InputSystem_Actions InputActions { get; private set; }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[InputManager] Duplicate InputManager detected, destroying this instance.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InputActions = new InputSystem_Actions();
            InputActions.Enable();
        }

        private void OnDisable()
        {
            InputActions?.Disable();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                InputActions?.Dispose();
                Instance = null;
            }
        }

        #endregion
    }
}
