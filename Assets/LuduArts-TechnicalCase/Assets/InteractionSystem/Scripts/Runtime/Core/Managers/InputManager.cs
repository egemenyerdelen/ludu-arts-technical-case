using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Managers
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        
        public InputSystem_Actions InputActions { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            
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
    }
}