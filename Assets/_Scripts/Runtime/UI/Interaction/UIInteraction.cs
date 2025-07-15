using ProjectEmbersteel.InteractionSystem;
using TMPro;
using UnityEngine;

namespace ProjectEmbersteel.UI.Interaction
{
    public class UIInteraction : MonoBehaviour
    {
        [SerializeField] private TMP_Text _interactionTypeText;

        public void SetInteractionType(InteractionType type)
        {
            string interactionType = "";

            switch (type)
            {
                case InteractionType.Chest:
                    interactionType = "Collect Drops";
                    break;
                case InteractionType.TrialSword:
                    interactionType = "Start Combat";
                    break;
            }

            _interactionTypeText.text = interactionType;
        }
    }
}