using System;
using LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Core.Enums;
using UnityEngine;

namespace LuduArts_TechnicalCase.Assets.InteractionSystem.Scripts.Runtime.Interactables
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private ToggleState m_ChestToggle =  ToggleState.Closed;
        [SerializeField] private GameObject m_ChestLid;
        [SerializeField] private Transform m_ChestHingeTransform;

        private void Start()
        {
            OpenChestLid();
        }

        private void OpenChestLid()
        {
            m_ChestLid.transform.RotateAround(m_ChestHingeTransform.position, Vector3.right, 90);
        }
    }
}