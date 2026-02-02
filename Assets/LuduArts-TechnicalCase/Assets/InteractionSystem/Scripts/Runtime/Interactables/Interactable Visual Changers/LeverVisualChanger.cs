using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables.Interactable_Visual_Changers
{
    /// <summary>
    /// Visual changer for lever/switch visuals.
    /// Toggles the lever position and rotation to represent state changes.
    /// </summary>
    public class LeverVisualChanger : MonoBehaviour
    {
        #region Fields

        // Serialized private instance fields
        [Header("References")]
        [SerializeField] private Transform m_LeverTransform;

        #endregion

        #region Methods

        /// <summary>
        /// Toggles the lever visual state by inverting its position and rotation.
        /// </summary>
        public void ChangeSwitchVisual()
        {
            if (m_LeverTransform == null)
            {
                Debug.LogError("[LeverVisualChanger] Lever transform is not assigned.", this);
                return;
            }

            var position = m_LeverTransform.position;
            position.z *= -1;
            m_LeverTransform.position = position;

            var rotation = m_LeverTransform.rotation;
            rotation.x *= -1;
            m_LeverTransform.rotation = rotation;
        }

        #endregion
    }
}
