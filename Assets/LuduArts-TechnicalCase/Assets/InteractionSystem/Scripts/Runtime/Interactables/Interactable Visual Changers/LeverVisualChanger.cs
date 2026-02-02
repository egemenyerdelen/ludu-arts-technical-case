using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables.Interactable_Visual_Changers
{
    public class LeverVisualChanger : MonoBehaviour
    {
        [SerializeField] private Transform m_LeverTransform;
        
        private void ChangeSwitchVisual()
        {
            var vector3 = m_LeverTransform.position;
            vector3.z *= -1;
            m_LeverTransform.position = vector3;

            var rotation = m_LeverTransform.rotation;
            rotation.x *= -1;
            m_LeverTransform.rotation = rotation;
        }
    }
}