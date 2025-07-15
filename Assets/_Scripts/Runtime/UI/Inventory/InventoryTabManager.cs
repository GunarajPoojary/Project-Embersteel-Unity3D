using System;
using UnityEngine;

namespace ProjectEmbersteel.UI
{
    public class InventoryTabManager : MonoBehaviour
    {
        [Serializable]
        private class InventoryTabData
        {
            public InventoryTabButton tabButton;
            public GameObject tabContent;
        }

        [SerializeField] private InventoryTabData[] _inventoryTabs;
        private int _currentSelectedTabIndex;

        private void Awake() => InitializeTabs();

        private void OnEnable() => SubscribeEvents(true);
        private void OnDisable() => SubscribeEvents(false);

        private void InitializeTabs()
        {
            for (int i = 0; i < _inventoryTabs.Length; i++)
            {
                _inventoryTabs[i].tabButton.Initialize(i);
                _inventoryTabs[i].tabContent.SetActive(false);
            }

            _currentSelectedTabIndex = 0;
            _inventoryTabs[_currentSelectedTabIndex].tabContent.SetActive(true);
        }

        private void SubscribeEvents(bool subscribe)
        {
            for (int i = 0; i < _inventoryTabs.Length; i++)
            {
                if (subscribe)
                    _inventoryTabs[i].tabButton.OnClick += HandleTabSelection;
                else
                    _inventoryTabs[i].tabButton.OnClick -= HandleTabSelection;
            }
        }

        private void HandleTabSelection(int index)
        {
            _inventoryTabs[_currentSelectedTabIndex].tabContent.SetActive(false);
            _inventoryTabs[index].tabContent.SetActive(true);
            _currentSelectedTabIndex = index;
        }
    }
}