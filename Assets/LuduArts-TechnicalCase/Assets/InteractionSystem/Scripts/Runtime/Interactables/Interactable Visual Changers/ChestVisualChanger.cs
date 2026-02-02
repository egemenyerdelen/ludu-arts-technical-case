using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables.Interactable_Visual_Changers
{
    /// <summary>
    /// Visual changer for chest lid rotation.
    /// Opens the chest lid by rotating it around its hinge point.
    /// </summary>
    public class ChestVisualChanger : MonoBehaviour
    {
        #region Fields

        // Private constant fields
        private const float k_DefaultOpenAngle = 90f;

        // Serialized private instance fields
        [Header("References")]
        [SerializeField] private GameObject m_ChestLid;
        [SerializeField] private Transform m_ChestHingeTransform;

        #endregion

        #region Methods

        /// <summary>
        /// Opens the chest lid by rotating it around the hinge.
        /// </summary>
        public void OpenLid()
        {
            if (m_ChestLid == null)
            {
                Debug.LogError("[ChestVisualChanger] Chest lid is not assigned.", this);
                return;
            }

            if (m_ChestHingeTransform == null)
            {
                Debug.LogError("[ChestVisualChanger] Chest hinge transform is not assigned.", this);
                return;
            }

            m_ChestLid.transform.RotateAround(m_ChestHingeTransform.position, Vector3.right, k_DefaultOpenAngle);
        }

        #endregion
    }
}
