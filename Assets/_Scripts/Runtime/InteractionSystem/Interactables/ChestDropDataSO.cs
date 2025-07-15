using ProjectEmbersteel.Equipment;
using UnityEngine;

namespace ProjectEmbersteel
{
    [CreateAssetMenu(fileName = "ChestDropData", menuName = "Custom/Chest/Data")]
    public class ChestDropDataSO : ScriptableObject
    {
        [field: SerializeField] public EquipmentSO[] Equipments { get; private set; }
        [field: SerializeField] public int MinimumDropCount { get; private set; } = 3;
        [field: SerializeField] public int MaximumDropCount { get; private set; } = 8;
    }
}