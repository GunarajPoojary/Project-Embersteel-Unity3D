using UnityEngine.Events;
using UnityEngine;
using ProjectEmbersteel.DialogueSystem;

namespace ProjectEmbersteel.Events.EventChannel
{
    /// <summary>
    /// This class is used for Events that have a DialogueDataSO argument.
    /// Example: An event to start Dialogue
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueEvent", menuName = "Custom/Events/Dialogue Event Channel")]
    public class DialogueSOEventChannelSO : DescriptionBaseSO
    {
        public event UnityAction<DialogueSO> OnEventRaised;

        public void RaiseEvent(DialogueSO dialogue) => OnEventRaised?.Invoke(dialogue);
    }
}