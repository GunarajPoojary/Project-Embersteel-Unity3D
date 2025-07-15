using System;
using ProjectEmbersteel.StatSystem;
using UnityEngine;

namespace ProjectEmbersteel.Equipment
{
    public enum EquipmentType
    {
        Weapon,
        HeadArmor,
        ArmArmor,
        ChestArmor,
        BeltArmor,
        LegArmor,
        FeetArmor
    }

    public abstract class EquipmentSO : DescriptionBaseSO
    {
        [Header("Basic Info")]
        [SerializeField] private EquipmentStatsSO _stats;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField, TextArea] private string _equipmentDescription;

        [Header("Classification")]
        [SerializeField] protected EquipmentType _type;

        [HideInInspector]
        [SerializeField] private string _id;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public string EquipmentDescription => _equipmentDescription;
        public EquipmentStatsSO Stats => _stats;
        public EquipmentType Type => _type;
        public string ID => _id;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}