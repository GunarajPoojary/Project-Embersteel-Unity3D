using System;
using System.Collections.Generic;
using ProjectEmbersteel.Equipment;
using UnityEngine;

namespace ProjectEmbersteel.Inventory
{
    /// <summary>
    /// Inventory model class that manages collections of equipments
    /// </summary>
    [Serializable]
    public class Inventory
    {
        [Range(1, 2000)] public int MaxCapacity = 100;

        private readonly Dictionary<string, EquipmentSO> _equipmentsById = new();
        private int _currentCapacity = 0;

        /// <summary>
        /// Adds an equipment to the inventory with specified quantity
        /// Handles capacity checking, stacking logic, and appropriate event notifications
        /// </summary>
        /// <param name="equipment">The equipment scriptable object to add to the inventory</param>
        /// <param name="quantity">Number of equipments to add (default: 1)</param>
        public bool TryAddEquipment(EquipmentSO equipment, int quantity = 1)
        {
            if (equipment == null || quantity < 1) return false;

            int remaining = quantity;
            int availableCapacity = MaxCapacity - _currentCapacity;

            // Limit quantity to available capacity to prevent overflow
            remaining = Mathf.Min(remaining, availableCapacity);

            _equipmentsById[equipment.ID] = equipment;
            _currentCapacity += remaining;

            return true;
        }

        /// <summary>
        /// Method to remove equipments from inventory by name and quantity
        /// Currently returns false as implementation is pending
        /// </summary>
        /// <param name="equipmentName">Name of the equipment to remove from inventory</param>
        /// <param name="quantity">Number of equipments to remove (default: 1)</param>
        /// <returns>True if equipments were successfully removed, false otherwise</returns>
        public bool RemoveEquipment(string equipmentName, int quantity = 1)
        {
            // TODO: Implement equipment removal logic
            // Should handle quantity validation, equipment existence checking, and capacity updates
            return false;
        }
    }
}