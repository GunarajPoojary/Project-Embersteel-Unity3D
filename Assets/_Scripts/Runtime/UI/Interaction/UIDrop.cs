using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEmbersteel.UI.Interaction
{
    public class UIDrop : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _equipmentIcon;
        [SerializeField] private TextMeshProUGUI _equipmentNameText;

        public void Initialize(Sprite equipmentIcon, string equipmentName)
        {
            _equipmentIcon.sprite = equipmentIcon;
            _equipmentNameText.text = equipmentName;
        }
    }
}