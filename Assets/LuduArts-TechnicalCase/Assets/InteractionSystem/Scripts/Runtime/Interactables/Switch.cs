using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    public class Switch : MonoBehaviour
    {
        [SerializeField] private Transform m_LeverTransform;
        [SerializeField] private ToggleState m_SwitchState = ToggleState.Closed;

        private void UseSwitch()
        {
            ChangeSwitchVisual();
        }
        
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