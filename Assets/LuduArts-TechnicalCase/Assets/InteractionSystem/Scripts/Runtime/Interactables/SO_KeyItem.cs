using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    /// <summary>
    /// ScriptableObject defining a key item's properties.
    /// Create instances via Assets > Create > Interaction > Key Item.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Key_New", menuName = "Interaction/Key Item", order = 1)]
    public class SO_KeyItem : ScriptableObject
    {
        #region Fields

        // Serialized private instance fields
        [Header("Key Information")]
        [SerializeField] private string m_DisplayName = "Key";
        [SerializeField] private KeyType m_KeyType;
        [SerializeField] [TextArea(2, 4)] private string m_Description = "A key.";

        [Header("Visuals")]
        [SerializeField] private Sprite m_Icon;
        [SerializeField] private Color m_KeyColor;
        [SerializeField] private GameObject m_WorldPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip m_PickupSound;
        [SerializeField] private AudioClip m_UseSound;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the display name of the key.
        /// </summary>
        public string DisplayName => m_DisplayName;

        /// <summary>
        /// Gets the type of this key.
        /// </summary>
        public KeyType KeyType => m_KeyType;

        /// <summary>
        /// Gets the description of the key.
        /// </summary>
        public string Description => m_Description;

        /// <summary>
        /// Gets the UI icon for this key.
        /// </summary>
        public Sprite Icon => m_Icon;

        /// <summary>
        /// Gets the color of this key.
        /// </summary>
        public Color KeyColor => m_KeyColor;

        /// <summary>
        /// Gets the world prefab for this key.
        /// </summary>
        public GameObject WorldPrefab => m_WorldPrefab;

        /// <summary>
        /// Gets the pickup sound for this key.
        /// </summary>
        public AudioClip PickupSound => m_PickupSound;

        /// <summary>
        /// Gets the use sound for this key.
        /// </summary>
        public AudioClip UseSound => m_UseSound;

        #endregion

        #region Methods

        /// <summary>
        /// Gets a formatted string representation of this key.
        /// </summary>
        /// <returns>Formatted key description.</returns>
        public override string ToString()
        {
            return $"{m_DisplayName} ({m_KeyType})";
        }

        #endregion
    }
}
