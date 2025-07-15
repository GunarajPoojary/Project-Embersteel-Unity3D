using System;
using System.Collections.Generic;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.Equipment;
using ProjectEmbersteel.StatSystem;
using UnityEngine;

namespace ProjectEmbersteel.EquipmentSystem
{
    [Serializable]
    public class EquipmentDatabase
    {
        public EquipmentSO equipmentSO;
        public Equipment equippable;
    }
    public class PlayerEquipmentManager : MonoBehaviour
    {
        public static PlayerEquipmentManager Instance { get; private set; }

        [SerializeField] private RuntimeAnimatorController _defaultAnimator;
        [SerializeField] private EquipmentDatabase[] _equipmentDatabase;

        [Header("Listeners")]
        [SerializeField] private EquipmentSOEventChannelSO _equipEquipmentEvent;
        [SerializeField] private EquipmentSOEventChannelSO _unequipEquipmentEvent;

        public PlayerEquipmentDatabase PlayerEquipmentDatabase { get; private set; }

        private PlayerEquipmentsController _controller;

        private void Awake() => Instance = this;

        private void Start() => InitializePlayerEquipmentsController();

        private void OnEnable() => SubscribeToEquipEquipmentEvent(true);
        private void OnDisable() => SubscribeToEquipEquipmentEvent(false);

        private void InitializePlayerEquipmentsController()
        {
            Animator animator = GetComponent<Animator>();
            IStatModifiable playerStats = GetComponentInParent<Player.Player>().StatModifiable;

            Dictionary<EquipmentSO, IEquippable> equipmentEntries = new(_equipmentDatabase.Length);

            foreach (EquipmentDatabase equipmentEntry in _equipmentDatabase)
            {
                if (equipmentEntry.equipmentSO != null && equipmentEntry.equippable != null)
                    equipmentEntries[equipmentEntry.equipmentSO] = equipmentEntry.equippable;
                else
                    Debug.LogWarning("Null key or value found in equipment dictionary.");
            }

            PlayerEquipmentDatabase = new(equipmentEntries);

            _controller = new PlayerEquipmentsController(playerStats, PlayerEquipmentDatabase, animator, _defaultAnimator);
        }

        private void SubscribeToEquipEquipmentEvent(bool subscribe)
        {
            if (subscribe)
            {
                _equipEquipmentEvent.OnEventRaised += HandleEquipEquipment;
                _unequipEquipmentEvent.OnEventRaised += HandleUnequipEquipment;
            }
            else
            {
                _equipEquipmentEvent.OnEventRaised -= HandleEquipEquipment;
                _unequipEquipmentEvent.OnEventRaised -= HandleUnequipEquipment;
            }
        }

        private void HandleEquipEquipment(EquipmentSO equipment)
        {
            switch (equipment.Type)
            {
                case EquipmentType.Weapon:
                    _controller.EquipWeapon(equipment as WeaponSO);
                    break;
                case EquipmentType.HeadArmor:
                case EquipmentType.ChestArmor:
                case EquipmentType.ArmArmor:
                case EquipmentType.BeltArmor:
                case EquipmentType.LegArmor:
                case EquipmentType.FeetArmor:
                    _controller.EquipArmor(equipment as ArmorSO);
                    break;
            }
        }

        private void HandleUnequipEquipment(EquipmentSO equipment)
        {
            switch (equipment.Type)
            {
                case EquipmentType.Weapon:
                    _controller.UnequipWeapon();
                    break;
                case EquipmentType.HeadArmor:
                case EquipmentType.ChestArmor:
                case EquipmentType.ArmArmor:
                case EquipmentType.BeltArmor:
                case EquipmentType.LegArmor:
                case EquipmentType.FeetArmor:
                    ArmorSO armor = equipment as ArmorSO;
                    _controller.UnequipArmor((int)armor.EquipSlot);
                    break;
            }
        }
    }
}