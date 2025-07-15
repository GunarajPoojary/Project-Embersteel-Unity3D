using System;
using ProjectEmbersteel.Equipment;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.Utilities.Inputs.ScriptableObjects;

namespace ProjectEmbersteel.Inventory
{
    public class InventoryController
    {
        private readonly Inventory _inventory;

        public event Action<EquipmentSO> OnEquipmentAdded;
        private readonly InputReader _inputReader;

        // Publisher
        private readonly VoidEventChannelSO _toggleInventoryMenuEvent;

        public InventoryController(Inventory inventory, InputReader inputReader, VoidEventChannelSO toggleInventoryMenuEvent)
        {
            _inventory = inventory;
            _inputReader = inputReader;
            _toggleInventoryMenuEvent = toggleInventoryMenuEvent;
        }

        public void AddListener() => _inputReader.InventoryAction += HandleInventoryAction;
        public void RemoveListener() => _inputReader.InventoryAction -= HandleInventoryAction;

        private void HandleInventoryAction() => _toggleInventoryMenuEvent.RaiseEvent();

        public bool TryAddEquipment(EquipmentSO equipment) => _inventory.TryAddEquipment(equipment);

        public void RemoveEquipment(string equipmentName) => _inventory.RemoveEquipment(equipmentName);
    }
}