using UnityEngine;

namespace ProjectEmbersteel.Equipment
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Custom/Equipment/Weapon", order = 1)]
    public class WeaponSO : EquipmentSO
    {
        [SerializeField] private AnimatorOverrideController _animatorOverrideController;
        public AnimatorOverrideController AnimatorOverrideController => _animatorOverrideController;
    }
}