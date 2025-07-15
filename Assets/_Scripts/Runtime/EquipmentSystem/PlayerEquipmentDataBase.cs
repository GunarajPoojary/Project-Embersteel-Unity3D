using System.Collections.Generic;
using ProjectEmbersteel.Equipment;
using UnityEngine;

namespace ProjectEmbersteel.EquipmentSystem
{
    public class PlayerEquipmentDatabase
    {
        private readonly Dictionary<EquipmentSO, IEquippable> _equipmentLookup;

        public PlayerEquipmentDatabase(Dictionary<EquipmentSO, IEquippable> equipmentEntries)
        {
            // Initialize the lookup table for fast access
            _equipmentLookup = equipmentEntries;
        }

        /// <summary>
        /// Gets the corresponding GameObject (already in the scene) for the given EquipmentSO.
        /// </summary>
        public IEquippable GetEquipmentObjectBySO(EquipmentSO equipmentSO)
        {
            if (_equipmentLookup == null)
            {
                Debug.LogError("Equipment lookup table not initialized.");
                return null;
            }

            if (equipmentSO == null)
            {
                Debug.LogError("Provided EquipmentSO is null.");
                return null;
            }

            if (_equipmentLookup.TryGetValue(equipmentSO, out IEquippable equippable))
                return equippable;

            Debug.LogWarning($"EquipmentSO not found in lookup: {equipmentSO.name}");
            return null;
        }
    }
}