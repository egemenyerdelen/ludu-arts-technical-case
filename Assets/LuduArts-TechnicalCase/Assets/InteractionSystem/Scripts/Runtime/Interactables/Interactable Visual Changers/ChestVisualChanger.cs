using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables.Interactable_Visual_Changers
{
    public class ChestVisualChanger : MonoBehaviour
    {
        [SerializeField] private GameObject m_ChestLid;
        [SerializeField] private Transform m_ChestHingeTransform;
        
        private void OpenLid()
        {
            m_ChestLid.transform.RotateAround(m_ChestHingeTransform.position, Vector3.right, 90);
        }
    }
}