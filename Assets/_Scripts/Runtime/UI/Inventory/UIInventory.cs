using System;
using System.Collections.Generic;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.Equipment;
using ProjectEmbersteel.StatSystem;
using ProjectEmbersteel.Utilities;
using UnityEngine;

namespace ProjectEmbersteel.UI.Inventory
{
    public class UIInventory : MonoBehaviour
    {

        [Serializable]
        private struct EquipmentContentPanel
        {
            public EquipmentType Type;
            public Transform ContentPanel;
        }

        [Header("UI References")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private EquipmentContentPanel[] _equipmentContentPanels;
        [SerializeField] private UIInventorySlot _slotPrefab;

        [Header("Equipment Details Panel")]
        [SerializeField] private UIInventoryEquipmentOverview _equipmentOverviewPanel;

        [SerializeField] private PlayerAttributesUI _attributesUI;
        [SerializeField] private AudioSource _uiAudioSource;

        [Header("Configuration")]
        [SerializeField] private int _poolSize = 100;

        [Header("Publishers")]
        [SerializeField] private VoidEventChannelSO _toggleInventoryMenuEvent;
        [SerializeField] private VoidEventChannelSO _openInventoryMenuEvent;
        [SerializeField] private VoidEventChannelSO _closeInventoryMenuEvent;

        [Header("Listener")]
        [SerializeField] private StatEventChannelSO _statUpdateEvent;
        [SerializeField] private EquipmentSOEventChannelSO _addEquipmentToInventorySuccessEvent;

        // Object pooling
        private ObjectPool<UIInventorySlot> _slotsPool;
        private RectTransform _equipmentPoolContainer;

        // Data structures for efficient inventory management
        private readonly Dictionary<EquipmentType, List<UIInventorySlot>> _slotUIsByType = new();
        private Dictionary<EquipmentType, Transform> _equipmentContentPanelsMap;

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeInventory();
            SetContentPanelsByEquipmentType();
        }

        private void OnEnable() => SubscribeToEvents(true);
        private void OnDisable() => SubscribeToEvents(false);

        private void OnDestroy() => CleanupSlotEvents();
        #endregion

        private void InitializeInventory()
        {
            _inventoryPanel.SetActive(false);

            CreateSlotPoolContainer();
            InitializeObjectPool();
        }

        private void SubscribeToEvents(bool subscribe)
        {
            if (subscribe)
            {
                _statUpdateEvent.OnEventRaised += UpdateBaseStats;
                _toggleInventoryMenuEvent.OnEventRaised += ToggleInventoryMenu;
                _addEquipmentToInventorySuccessEvent.OnEventRaised += AddSlotUI;
            }
            else
            {
                _statUpdateEvent.OnEventRaised -= UpdateBaseStats;
                _toggleInventoryMenuEvent.OnEventRaised -= ToggleInventoryMenu;
                _addEquipmentToInventorySuccessEvent.OnEventRaised -= AddSlotUI;
            }
        }

        public void CloseInventoryUI() => _inventoryPanel.SetActive(false);

        private void ToggleInventoryMenu()
        {
            _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);

            if (_inventoryPanel.activeSelf)
                _openInventoryMenuEvent.RaiseEvent();
            else
                _closeInventoryMenuEvent.RaiseEvent();
        }


        private void UpdateBaseStats(StatType statType, Stat stat) => _attributesUI.UpdateBaseStats(statType, stat);

        private void CreateSlotPoolContainer()
        {
            GameObject containerGO = new("InventorySlotPool", typeof(RectTransform));
            _equipmentPoolContainer = containerGO.GetComponent<RectTransform>();
            _equipmentPoolContainer.SetParent((RectTransform)transform);
        }

        private void InitializeObjectPool()
        {
            _slotsPool = new ObjectPool<UIInventorySlot>(
                createFunc: CreateSlotObject,
                onGet: OnSlotRetrieved,
                onRelease: OnSlotReleased,
                poolSize: _poolSize
            );
        }

        private void SetContentPanelsByEquipmentType()
        {
            _equipmentContentPanelsMap = new Dictionary<EquipmentType, Transform>(_equipmentContentPanels.Length);

            foreach (EquipmentContentPanel equipmentContentEntry in _equipmentContentPanels)
                _equipmentContentPanelsMap[equipmentContentEntry.Type] = equipmentContentEntry.ContentPanel;
        }

        #region Public Interface
        public void AddSlotUI(EquipmentSO equipment)
        {
            if (equipment == null) return;

            EquipmentType equipmentType = equipment.Type;

            if (!_equipmentContentPanelsMap.TryGetValue(equipmentType, out Transform contentPanel))
            {
                Debug.LogError($"No container found for equipment type: {equipmentType}");
                return;
            }

            EnsureSlotListExists(equipmentType);

            CreateAndSetupSlot(equipment, equipmentType, contentPanel);
        }
        #endregion

        #region Equipment Management
        private void EnsureSlotListExists(EquipmentType equipmentType)
        {
            if (!_slotUIsByType.ContainsKey(equipmentType))
                _slotUIsByType[equipmentType] = new List<UIInventorySlot>();
        }

        private UIInventorySlot CreateAndSetupSlot(EquipmentSO equipment, EquipmentType equipmentType, Transform container)
        {
            UIInventorySlot slot = _slotsPool.Get();
            slot.Initialize(equipment, container, _uiAudioSource);
            slot.OnClick += OnEquipmentClicked;

            _slotUIsByType[equipmentType].Add(slot);
            return slot;
        }
        #endregion

        #region Event Handlers
        private void OnEquipmentClicked(EquipmentSO equipment) => _equipmentOverviewPanel.DisplayEquipmentOverview(equipment);
        #endregion

        #region Object Pool Management
        private UIInventorySlot CreateSlotObject()
        {
            UIInventorySlot slot = Instantiate(_slotPrefab, _equipmentPoolContainer);
            slot.gameObject.SetActive(false);
            return slot;
        }

        private void OnSlotRetrieved(UIInventorySlot slot) => slot.gameObject.SetActive(true);

        private void OnSlotReleased(UIInventorySlot slot)
        {
            slot.gameObject.SetActive(false);
            slot.OnClick -= OnEquipmentClicked; // Prevent memory leaks
        }
        #endregion

        #region Cleanup
        private void CleanupSlotEvents()
        {
            foreach (List<UIInventorySlot> slotList in _slotUIsByType.Values)
            {
                foreach (UIInventorySlot slot in slotList)
                {
                    if (slot != null)
                        slot.OnClick -= OnEquipmentClicked;
                }
            }
        }
        #endregion
    }
}