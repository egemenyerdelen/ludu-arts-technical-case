using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Base
{
    public class InteractableHighlight : MonoBehaviour
    {
        // Serialized private instance fields
        [Header("Visual Feedback")]
        [SerializeField] private bool m_UseHighlight;
        [SerializeField] private Material m_HighlightMaterial;
        
        // Non-serialized private instance fields
        private Renderer[] m_Renderers;
        private Material[] m_OriginalMaterials;
        
        protected bool UseHighlight
        {
            get => m_UseHighlight;
            set => m_UseHighlight = value;
        }
        
        #region Unity Methods
        
        private void Awake()
        {
            CacheRenderers();
        }
        
        #endregion
        
        #region Methods
        
        public void ApplyHighlight()
        {
            if (!m_UseHighlight || m_HighlightMaterial == null || m_Renderers == null)
            {
                return;
            }

            for (var i = 0; i < m_Renderers.Length; i++)
            {
                if (m_Renderers[i] != null)
                {
                    // Add highlight material as additional material
                    var materials = m_Renderers[i].materials;
                    var newMaterials = new Material[materials.Length + 1];
                    materials.CopyTo(newMaterials, 0);
                    newMaterials[materials.Length] = m_HighlightMaterial;
                    m_Renderers[i].materials = newMaterials;
                }
            }
        }

        public void RemoveHighlight()
        {
            for (var i = 0; i < m_Renderers.Length; i++)
            {
                if (m_Renderers[i] != null && m_OriginalMaterials[i] != null)
                {
                    // Restore ENTIRE materials array, not just first material
                    m_Renderers[i].materials = new Material[] { m_OriginalMaterials[i] };
                }
            }
        }
        
        private void CacheRenderers()
        {
            m_Renderers = GetComponentsInChildren<Renderer>();
            
            if (m_Renderers == null || m_Renderers.Length == 0)
            {
                Debug.LogWarning($"[{GetType().Name}] No renderers found for highlight system.", this);
                return;
            }

            m_OriginalMaterials = new Material[m_Renderers.Length];
            for (var i = 0; i < m_Renderers.Length; i++)
            {
                if (m_Renderers[i] != null)
                {
                    m_OriginalMaterials[i] = m_Renderers[i].material;
                }
            }
        }
        
        #endregion
    }
}