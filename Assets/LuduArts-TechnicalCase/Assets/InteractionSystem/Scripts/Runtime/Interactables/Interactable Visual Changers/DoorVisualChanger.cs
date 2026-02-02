using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables.Interactable_Visual_Changers
{
    /// <summary>
    /// Visual changer for door rotation.
    /// Rotates the door around its hinge point.
    /// </summary>
    public class DoorVisualChanger : MonoBehaviour
    {
        #region Fields

        // Private constant fields
        private const float k_DefaultRotationAngle = -90f;

        // Serialized private instance fields
        [Header("References")]
        [SerializeField] private Transform m_DoorHingeTransform;

        [Header("Settings")]
        [SerializeField] private bool m_IsLocked;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether the door is locked.
        /// </summary>
        public bool IsLocked
        {
            get => m_IsLocked;
            set => m_IsLocked = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Rotates the door around its hinge point.
        /// </summary>
        public void RotateDoor()
        {
            if (m_DoorHingeTransform == null)
            {
                Debug.LogError("[DoorVisualChanger] Door hinge transform is not assigned.", this);
                return;
            }

            if (m_IsLocked)
            {
                Debug.LogWarning("[DoorVisualChanger] Cannot rotate - door is locked.", this);
                return;
            }

            transform.RotateAround(m_DoorHingeTransform.position, Vector3.up, k_DefaultRotationAngle);
        }

        #endregion
    }
}
