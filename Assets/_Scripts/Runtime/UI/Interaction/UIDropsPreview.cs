using System;
using System.Collections.Generic;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.Equipment;
using ProjectEmbersteel.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEmbersteel.UI.Interaction
{
    /// <summary>
    /// Controls the UI panel that previews a list of collected equipments drops from chest.
    /// Uses an object pool to efficiently reuse UIDrop instances.
    /// </summary>
    public class UIDropsPreview : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _containerPanel;
        [SerializeField] private UIDrop _dropPrefab;
        [SerializeField] private int _poolSize = 10;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _dropsContainer;

        [Header("Listener")]
        [SerializeField] private DropsEventChannelSO _dropsCollectedEvent;

        private ObjectPool<UIDrop> _dropsPool;
        private RectTransform _equipmentPoolContainer;
        private readonly List<UIDrop> _currentDrops = new();

        public event Action OnCloseDropsPreviewUI;

        private void Awake()
        {
            _containerPanel.SetActive(false);

            CreateDropPoolContainer();
            InitializeObjectPool();
        }

        private void OnEnable() => SubscribeToEvents(true);
        private void OnDisable() => SubscribeToEvents(false);

        private void SubscribeToEvents(bool subscribe)
        {
            if (subscribe)
            {
                _dropsCollectedEvent.OnEventRaised += DisplayCollectedDrops;
                _closeButton.onClick.AddListener(CloseUI);
            }
            else
            {
                _dropsCollectedEvent.OnEventRaised -= DisplayCollectedDrops;
                _closeButton.onClick.RemoveListener(CloseUI);
            }
        }

        // Handles closing the UI: hides panel, releases drops back to the pool, and raises close event.
        private void CloseUI()
        {
            _containerPanel.SetActive(false);

            OnCloseDropsPreviewUI?.Invoke();

            foreach (var drop in _currentDrops)
                _dropsPool.Release(drop);

            _currentDrops.Clear();
        }

        // Displays the given list of drops in the UI.
        private void DisplayCollectedDrops(List<EquipmentSO> drops)
        {
            _containerPanel.SetActive(true);

            foreach (EquipmentSO drop in drops)
            {
                UIDrop uIDrop = _dropsPool.Get();
                uIDrop.Initialize(drop.Icon, drop.DisplayName);
                uIDrop.transform.SetParent(_dropsContainer);
                _currentDrops.Add(uIDrop);
            }
        }

        private void InitializeObjectPool()
        {
            _dropsPool = new ObjectPool<UIDrop>(
                createFunc: CreateDropObject,
                onGet: OnDropRetrieved,
                onRelease: OnDropReleased,
                poolSize: _poolSize
            );
        }

        private void CreateDropPoolContainer()
        {
            GameObject containerGO = new("DropPool", typeof(RectTransform));
            _equipmentPoolContainer = containerGO.GetComponent<RectTransform>();
            _equipmentPoolContainer.SetParent((RectTransform)transform);
        }

        private UIDrop CreateDropObject()
        {
            UIDrop drop = Instantiate(_dropPrefab, _equipmentPoolContainer);
            drop.gameObject.SetActive(false);
            return drop;
        }

        private void OnDropRetrieved(UIDrop drop) => drop.gameObject.SetActive(true);
        private void OnDropReleased(UIDrop drop) => drop.gameObject.SetActive(false);
    }
}