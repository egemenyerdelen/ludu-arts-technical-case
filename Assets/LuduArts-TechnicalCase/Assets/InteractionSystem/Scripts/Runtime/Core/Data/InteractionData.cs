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

        /// <summary>
        /// The GameObject that was interacted with.
        /// </summary>
        public GameObject m_Target;

        /// <summary>
        /// The type of interaction that occurred.
        /// </summary>
        public InteractionType m_InteractionType;

        /// <summary>
        /// The GameObject that initiated the interaction (usually the player).
        /// </summary>
        public GameObject m_Interactor;

        /// <summary>
        /// The world position where the interaction occurred.
        /// </summary>
        public Vector3 m_InteractionPoint;

        /// <summary>
        /// The time when the interaction occurred.
        /// </summary>
        public float m_Timestamp;

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
