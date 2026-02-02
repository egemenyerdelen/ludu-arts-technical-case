using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables.Interactable_Visual_Changers
{
    public class DoorVisualChanger : MonoBehaviour
    {
        [SerializeField] private Transform m_DoorHingeTransform;
        [SerializeField] private bool m_IsLocked;

        private void RotateDoor()
        {
            transform.RotateAround(m_DoorHingeTransform.position, Vector3.up, -90);
        }
    }
}