using UnityEngine.Events;
using UnityEngine;
using ProjectEmbersteel.Equipment;

namespace ProjectEmbersteel.Events.EventChannel
{
    /// <summary>
    /// This class is used for Events that have a EquipmentSO argument.
    /// Example: An event to notify Inventory when EquipmentSO in the world picked up
    /// </summary>
    [CreateAssetMenu(menuName = "Custom/Events/EquipmentSO Event Channel")]
    public class EquipmentSOEventChannelSO : DescriptionBaseSO
    {
        public event UnityAction<EquipmentSO> OnEventRaised;

        public void RaiseEvent(EquipmentSO equipment) => OnEventRaised?.Invoke(equipment);
    }
}