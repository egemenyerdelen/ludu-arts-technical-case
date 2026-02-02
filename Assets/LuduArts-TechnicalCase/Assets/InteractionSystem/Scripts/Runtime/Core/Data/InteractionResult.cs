namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Data
{
    /// <summary>
    /// Contains the result of an interaction attempt.
    /// </summary>
    [System.Serializable]
    public struct InteractionResult
    {
        #region Fields

        private bool m_Success;
        private string m_Message;
        private InteractionFailReason m_FailReason;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the interaction was successful.
        /// </summary>
        public bool Success => m_Success;

        /// <summary>
        /// Gets the message describing the result (for UI feedback).
        /// </summary>
        public string Message => m_Message;

        /// <summary>
        /// Gets the reason for failure, if the interaction failed.
        /// </summary>
        public InteractionFailReason FailReason => m_FailReason;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a successful interaction result.
        /// </summary>
        /// <param name="message">Optional success message.</param>
        /// <returns>A successful InteractionResult.</returns>
        public static InteractionResult Succeeded(string message = "")
        {
            return new InteractionResult
            {
                m_Success = true,
                m_Message = message,
                m_FailReason = InteractionFailReason.None
            };
        }

        /// <summary>
        /// Creates a failed interaction result.
        /// </summary>
        /// <param name="reason">The reason for failure.</param>
        /// <param name="message">Optional failure message.</param>
        /// <returns>A failed InteractionResult.</returns>
        public static InteractionResult Failed(InteractionFailReason reason, string message = "")
        {
            return new InteractionResult
            {
                m_Success = false,
                m_Message = message,
                m_FailReason = reason
            };
        }

        #endregion
    }

    /// <summary>
    /// Reasons why an interaction might fail.
    /// </summary>
    public enum InteractionFailReason
    {
        /// <summary>
        /// No failure - interaction succeeded.
        /// </summary>
        None = 0,

        /// <summary>
        /// Object is locked and requires a key.
        /// </summary>
        Locked = 1,

        /// <summary>
        /// Player is out of interaction range.
        /// </summary>
        OutOfRange = 2,

        /// <summary>
        /// Object is currently disabled or inactive.
        /// </summary>
        Disabled = 3,

        /// <summary>
        /// Object has already been used (e.g., opened chest).
        /// </summary>
        AlreadyUsed = 4,

        /// <summary>
        /// Player doesn't have the required item.
        /// </summary>
        MissingRequiredItem = 5,

        /// <summary>
        /// Interaction was cancelled by the player.
        /// </summary>
        Cancelled = 6
    }
}
