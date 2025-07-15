using ProjectEmbersteel.DialogueSystem;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.InteractionSystem;
using UnityEngine;

namespace ProjectEmbersteel.Characters
{
    public class NPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueSO _defaultDialogueDataSO;

        [Header("Publisher")]
        [SerializeField] private DialogueSOEventChannelSO _startDialogueEvent;

        public InteractionType InteractionType => throw new System.NotImplementedException();

        public bool Interactable { get; private set; } = true;

        public void Interact()
        {
            _startDialogueEvent.RaiseEvent(_defaultDialogueDataSO);
        }
    }
}