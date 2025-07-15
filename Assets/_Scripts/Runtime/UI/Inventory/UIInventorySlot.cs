using System;
using System.Text;
using ProjectEmbersteel.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectEmbersteel.UI.Inventory
{
    public class UIInventorySlot : UISelectableButton<EquipmentSO>, IDeselectHandler
    {
        [Header("UI References")]
        [SerializeField] private Image _equipmentIcon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _selectedImage;
        [SerializeField] private Image _hoverImage;
        [SerializeField] private AudioClip _selectedSound;
        [SerializeField] private AudioClip _hoverSound;

        private static IDeselectHandler _currentSelected;
        private AudioSource _audioSource;
        private EquipmentSO _equipment;
        private readonly StringBuilder _stringBuilder = new(16);

        public void Initialize(EquipmentSO equipment, Transform parentTransform, AudioSource audioSource)
        {
            _equipment = equipment;
            _audioSource = audioSource;

            transform.SetParent(parentTransform);

            SetIcon();
            SetName();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (_audioSource != null)
                _audioSource.PlayOneShot(_hoverSound);

            _hoverImage.gameObject.SetActive(true);
        }

        public override void OnPointerExit(PointerEventData eventData) => _hoverImage.gameObject.SetActive(false);

        public void OnDeselect(BaseEventData eventData) => _selectedImage.gameObject.SetActive(false);

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (_currentSelected != null && _currentSelected != this as IDeselectHandler)
                _currentSelected.OnDeselect(eventData);

            if (_audioSource != null)
                _audioSource.PlayOneShot(_selectedSound);

            _selectedImage.gameObject.SetActive(true);

            base.OnPointerClick(eventData);

            _currentSelected = this;
        }

        protected override EquipmentSO GetValue() => _equipment;

        private void SetIcon() => _equipmentIcon.sprite = _equipment.Icon;

        private void SetName() => _nameText.text = _equipment.DisplayName;
    }
}