using System;
using ProjectEmbersteel.Equipment;
using ProjectEmbersteel.StatSystem;
using UnityEngine;

namespace ProjectEmbersteel.EquipmentSystem
{
    public class PlayerEquipmentsController
    {
        private readonly RuntimeAnimatorController _defaultAnimatorController;
        //[Space]
        private ArmorSO[] _equippedArmors;
        private WeaponSO _equippedWeapon;
        private readonly IStatModifiable _playerStats;

        private readonly Animator _animator;
        private readonly PlayerEquipmentDatabase _equipmentDataBase;

        public PlayerEquipmentsController(IStatModifiable playerStats, PlayerEquipmentDatabase playerEquipmentDatabase, Animator animator, RuntimeAnimatorController defaultAnimatorController)
        {
            _equipmentDataBase = playerEquipmentDatabase;
            _playerStats = playerStats;
            _animator = animator;
            _defaultAnimatorController = defaultAnimatorController;

            InitializeEquipmentSlots();
        }

        // Equip a new armor, unequip the old one if necessary, and update the visuals
        public void EquipArmor(ArmorSO newArmor)
        {
            int slotIndex = (int)newArmor.EquipSlot;
            ArmorSO currentEquippedArmor = UnequipArmor(slotIndex);

            _equipmentDataBase.GetEquipmentObjectBySO(newArmor).Equip(_playerStats);

            _equippedArmors[slotIndex] = newArmor;
        }

        // Unequip a armor from a specific slot, destroy the associated mesh, and reset the default skin
        public ArmorSO UnequipArmor(int slotIndex)
        {
            ArmorSO currentEquippedArmor = _equippedArmors[slotIndex];

            if (currentEquippedArmor != null)
                _equipmentDataBase.GetEquipmentObjectBySO(currentEquippedArmor).Unequip(_playerStats);

            _equippedArmors[slotIndex] = null;
            return currentEquippedArmor;
        }

        // Equip a new weapon, unequip the old one if necessary, and update the visuals and animator controller
        public void EquipWeapon(WeaponSO newWeapon)
        {
            WeaponSO currentEquippedWeapon = UnequipWeapon();

            _equipmentDataBase.GetEquipmentObjectBySO(newWeapon).Equip(_playerStats);

            _equippedWeapon = newWeapon;
            UpdateAnimatorController();
        }

        // Unequip the currently equipped weapon, destroy its game objects, and reset the animator controller
        public WeaponSO UnequipWeapon()
        {
            if (_equippedWeapon == null) return null;

            WeaponSO currentEquippedWeapon = _equippedWeapon;

            _equipmentDataBase.GetEquipmentObjectBySO(currentEquippedWeapon).Unequip(_playerStats);

            _equippedWeapon = null;

            UpdateAnimatorController();

            return currentEquippedWeapon;
        }

        // Unequip all armors and weapons, and reset the player to the default skins and animator controller
        public void UnequipAll()
        {
            UnequipWeapon();

            _animator.runtimeAnimatorController = _defaultAnimatorController;
        }

        private void InitializeEquipmentSlots()
        {
            int numSlots = Enum.GetNames(typeof(ArmorEquipSlot)).Length;
            _equippedArmors = new ArmorSO[numSlots];
        }

        // Update the animator controller based on the equipped weapon and current scene
        private void UpdateAnimatorController() => _animator.runtimeAnimatorController = _equippedWeapon != null ? _equippedWeapon.AnimatorOverrideController : _defaultAnimatorController;
    }
}