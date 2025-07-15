using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.Equipment;
using UnityEngine;
using ProjectEmbersteel.Utilities.Inputs.ScriptableObjects;

namespace ProjectEmbersteel.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        [SerializeField] public Inventory _inventory;
        
        [Header("Listener")]
        [SerializeField] private EquipmentSOEventChannelSO _addEquipmentToInventoryEvent;

        [Header("Publishers")]
        [SerializeField] private EquipmentSOEventChannelSO _addEquipmentToInvetorySuccessEvent;
        [SerializeField] private VoidEventChannelSO _toggleInventoryMenuEvent;

        private InventoryController _controller;

        private void Awake() => CreateInventoryController();

        private void OnEnable() => SubscribeToControllerEvents();
        private void OnDisable() => UnsubscribeToControllerEvents();

        private void AddEquipment(EquipmentSO equipment)
        {
            if (_controller.TryAddEquipment(equipment))
                _addEquipmentToInvetorySuccessEvent.RaiseEvent(equipment);
        }

        private void CreateInventoryController() => _controller = new InventoryController(_inventory, _inputReader, _toggleInventoryMenuEvent);

        private void SubscribeToControllerEvents()
        {
            _controller.AddListener();
            _addEquipmentToInventoryEvent.OnEventRaised += AddEquipment;
        }

        private void UnsubscribeToControllerEvents()
        {
            _controller.RemoveListener();
            _addEquipmentToInventoryEvent.OnEventRaised -= AddEquipment;
        }
    }
}