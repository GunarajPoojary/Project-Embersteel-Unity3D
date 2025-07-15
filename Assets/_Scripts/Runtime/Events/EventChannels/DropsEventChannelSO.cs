using UnityEngine.Events;
using UnityEngine;
using ProjectEmbersteel.Equipment;
using System.Collections.Generic;

namespace ProjectEmbersteel.Events.EventChannel
{
    /// <summary>
    /// This class is used for Events that have a list of EquipmentSO argument.
    /// Example: An event to notify hen drops are collected when chest is opened
    /// </summary>
    [CreateAssetMenu(menuName = "Custom/Events/Drops Event Channel")]
    public class DropsEventChannelSO : DescriptionBaseSO
    {
        public event UnityAction<List<EquipmentSO>> OnEventRaised;

        public void RaiseEvent(List<EquipmentSO> equipments) => OnEventRaised?.Invoke(equipments);
    }
}