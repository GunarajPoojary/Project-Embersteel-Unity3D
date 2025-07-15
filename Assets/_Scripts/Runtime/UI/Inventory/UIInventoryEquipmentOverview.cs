using ProjectEmbersteel.EquipmentSystem;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.Equipment;
using ProjectEmbersteel.StatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEmbersteel.UI.Inventory
{
    public class UIInventoryEquipmentOverview : MonoBehaviour
    {
        [System.Serializable]
        public class StatOverviewUI
        {
            public StatType statType;
            public GameObject gameObject;
            public TMP_Text statValueText;
        }

        [SerializeField] private GameObject _overviewPanel;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _primaryStatTypeText;
        [SerializeField] private TMP_Text _primaryStatValueText;
        [SerializeField] private Image _equipmentIcon;
        [SerializeField] private StatOverviewUI[] _statUI;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _closeButton;

		[Header("Publisher")]
        [SerializeField] private EquipmentSOEventChannelSO _equipEquipmentEvent;
        [SerializeField] private EquipmentSOEventChannelSO _unequipEquipmentEvent;

        private EquipmentSO _currentSelectedEquipment;

        private void OnEnable() => AddListeners();
        private void OnDisable() => RemoveListeners();

        public void DisplayEquipmentOverview(EquipmentSO equipment)
        {
            _currentSelectedEquipment = equipment;

            switch (equipment.Type)
            {
                case EquipmentType.Weapon:
                case EquipmentType.HeadArmor:
                case EquipmentType.ChestArmor:
                case EquipmentType.ArmArmor:
                case EquipmentType.BeltArmor:
                case EquipmentType.LegArmor:
                case EquipmentType.FeetArmor:
                    IEquippable equippable = PlayerEquipmentManager.Instance.PlayerEquipmentDatabase.GetEquipmentObjectBySO(equipment);
                    if (equippable != null && equippable.IsEquipped)
                    {
                        _equipButton.gameObject.SetActive(false);
                        _unequipButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        _equipButton.gameObject.SetActive(true);
                        _unequipButton.gameObject.SetActive(false);
                    }
                    _sellButton.gameObject.SetActive(true);
                    ShowStats(equipment);
                    break;
            }

            _titleText.text = _currentSelectedEquipment.DisplayName;
            _descriptionText.text = _currentSelectedEquipment.EquipmentDescription;
            _equipmentIcon.sprite = _currentSelectedEquipment.Icon;

            ToggleMenu();
        }

        private void AddListeners()
        {
            _sellButton.onClick.AddListener(OnSellButtonClick);
            _equipButton.onClick.AddListener(OnEquipButtonClick);
            _unequipButton.onClick.AddListener(OnUnequipButtonClick);

            _closeButton.onClick.AddListener(ToggleMenu);
        }

        private void RemoveListeners()
        {
            _sellButton.onClick.RemoveListener(OnSellButtonClick);
            _equipButton.onClick.RemoveListener(OnEquipButtonClick);
            _unequipButton.onClick.RemoveListener(OnUnequipButtonClick);

            _closeButton.onClick.RemoveListener(ToggleMenu);
        }

        private void ToggleMenu()
        {
            if (_overviewPanel.activeSelf)
                _overviewPanel.SetActive(false);
            else
                _overviewPanel.SetActive(true);
        }

        private void ShowStats(EquipmentSO equipment)
        {
            // First, hide all stat UI elements
            foreach (StatOverviewUI statUI in _statUI)
                statUI.gameObject.SetActive(false);

            EquipmentBaseStatsSO equipmentStats = equipment.Stats.BaseStats;

            _primaryStatTypeText.text = equipmentStats.PrimaryBaseStat.statType.ToString();
            _primaryStatValueText.text = "+" + equipmentStats.PrimaryBaseStat.value.ToString("0.##") + (equipmentStats.PrimaryBaseStat.statValueType
                                                                                                       == StatValueType.Flat ? "" : "%");

            // Then, display only the stats that are relevant
            foreach (ReadonlyBaseStat secondaryBaseStat in equipmentStats.SecondaryBaseStats)
            {
                foreach (StatOverviewUI statUI in _statUI)
                {
                    if (statUI.statType == secondaryBaseStat.statType)
                    {
                        statUI.gameObject.SetActive(true);

                        statUI.statValueText.text = "+" + secondaryBaseStat.value.ToString("0.##") + (secondaryBaseStat.statValueType == StatValueType.Flat ? "" : "%");
                        break;
                    }
                }
            }
        }

        private void OnSellButtonClick()
        {
            if (_currentSelectedEquipment != null)
                Debug.Log($"Sell {_currentSelectedEquipment.DisplayName}");
        }

        private void OnEquipButtonClick()
        {
            if (_currentSelectedEquipment != null)
            {
                _equipEquipmentEvent.RaiseEvent(_currentSelectedEquipment);
                _equipButton.gameObject.SetActive(false);
                _unequipButton.gameObject.SetActive(true);
            }
        }

        private void OnUnequipButtonClick()
        {
            if (_currentSelectedEquipment != null)
            {
                _unequipEquipmentEvent.RaiseEvent(_currentSelectedEquipment);
                _equipButton.gameObject.SetActive(true);
                _unequipButton.gameObject.SetActive(false);
            }
        }
    }
}