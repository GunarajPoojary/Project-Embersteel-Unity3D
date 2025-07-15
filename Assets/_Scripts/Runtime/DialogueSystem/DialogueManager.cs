using ProjectEmbersteel.DialogueSystem.UI;
using ProjectEmbersteel.Events.EventChannel;
using UnityEngine;

namespace ProjectEmbersteel.DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueSOEventChannelSO _startDialogue;
        [SerializeField] private DialogueUI _dialogueUI;
        [SerializeField] private VoidEventChannelSO _playNextDialogueLine;

        private void OnEnable() => SubscribeToEvents(true);
        private void OnDisable() => SubscribeToEvents(false);

        private void SubscribeToEvents(bool subscribe)
        {
            if (subscribe)
                _startDialogue.OnEventRaised += StartDialogue;
            else
                _startDialogue.OnEventRaised -= StartDialogue;
        }

        public void StartDialogue(DialogueSO dialogueData)
        {
            Debug.Log("Do Nothing for now");
        }
    }
}