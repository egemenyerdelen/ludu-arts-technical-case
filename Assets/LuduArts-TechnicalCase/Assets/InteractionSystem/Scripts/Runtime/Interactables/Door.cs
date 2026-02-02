using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private Transform m_DoorHingeTransform;
        [SerializeField] private bool m_IsLocked;
        [SerializeField] private ToggleState m_DoorState = ToggleState.Closed;

        private void RotateDoor()
        {
            transform.RotateAround(m_DoorHingeTransform.position, Vector3.up, -90);
        }
    }
}