using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Data
{
    /// <summary>
    /// Contains data about an interaction event.
    /// Passed to event handlers when interactions occur.
    /// </summary>
    [System.Serializable]
    public struct InteractionData
    {
        #region Fields

        [SerializeField] private GameObject m_Target;
        [SerializeField] private InteractionType m_InteractionType;
        [SerializeField] private GameObject m_Interactor;
        [SerializeField] private Vector3 m_InteractionPoint;
        [SerializeField] private float m_Timestamp;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the GameObject that was interacted with.
        /// </summary>
        public GameObject Target => m_Target;

        /// <summary>
        /// Gets the type of interaction that occurred.
        /// </summary>
        public InteractionType InteractionType => m_InteractionType;

        /// <summary>
        /// Gets the GameObject that initiated the interaction (usually the player).
        /// </summary>
        public GameObject Interactor => m_Interactor;

        /// <summary>
        /// Gets the world position where the interaction occurred.
        /// </summary>
        public Vector3 InteractionPoint => m_InteractionPoint;

        /// <summary>
        /// Gets the time when the interaction occurred.
        /// </summary>
        public float Timestamp => m_Timestamp;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new InteractionData instance.
        /// </summary>
        /// <param name="target">The target of the interaction.</param>
        /// <param name="interactionType">The type of interaction.</param>
        /// <param name="interactor">The initiator of the interaction.</param>
        /// <param name="interactionPoint">The world position of the interaction.</param>
        public InteractionData(
            GameObject target,
            InteractionType interactionType,
            GameObject interactor,
            Vector3 interactionPoint)
        {
            m_Target = target;
            m_InteractionType = interactionType;
            m_Interactor = interactor;
            m_InteractionPoint = interactionPoint;
            m_Timestamp = Time.time;
        }

        #endregion
    }
}
