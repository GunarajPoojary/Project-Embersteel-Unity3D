using PrimeTween;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.Equipment;
using UnityEngine;
using System.Collections.Generic;

namespace ProjectEmbersteel.InteractionSystem
{
    /// <summary>
    /// Represents a chest in the world that can be interacted with to drop Equipments.
    /// Plays an opening animation, awards random drops to the player, and raises relevant events.
    /// </summary>
    public class Chest : MonoBehaviour, IInteractable
    {
        [Header("Chest Settings")]
        [SerializeField] private Transform _chestLid;
        [SerializeField] private float _lidOpenAngle = 180f;
        [SerializeField] private float _lidCloseAngle = -90f;
        [SerializeField] private float _lidOpenDuration = 0.5f;

        [Header("Drop Settings")]
        [SerializeField] private ChestDropDataSO _dropsData;

        [Header("Publishers")]
        [SerializeField] private EquipmentSOEventChannelSO _addEquipmentToInventoryEvent;
        [SerializeField] private DropsEventChannelSO _dropsCollectedEvent;

        public InteractionType InteractionType { get; private set; } = InteractionType.Chest;

        private readonly List<EquipmentSO> _drops = new();

        public bool Interactable { get; private set; } = true;

        /// <summary>
        /// Called when the player interacts with this chest.
        /// Triggers the lid opening animation and handles drops.
        /// </summary>
        public void Interact()
        {
            if (!Interactable) return;

            Interactable = false;

            // Animate the lid opening and, on completion, generate drops
            if (_chestLid)
                Tween.LocalRotation(
                    _chestLid,
                    Quaternion.Euler(-_lidOpenAngle, 0, 0),
                    _lidOpenDuration,
                    Ease.OutBack
                ).OnComplete(CollectDrops);
        }

        private void CollectDrops()
        {
            int dropCount = Random.Range(_dropsData.MinimumDropCount, _dropsData.MaximumDropCount);

            _drops.Clear();

            for (int i = 0; i < dropCount; i++)
            {
                EquipmentSO drop = _dropsData.Equipments[Random.Range(0, _dropsData.Equipments.Length)];
                _drops.Add(drop);
                _addEquipmentToInventoryEvent.RaiseEvent(drop);
            }

            _dropsCollectedEvent.RaiseEvent(_drops);
        }
    }
}