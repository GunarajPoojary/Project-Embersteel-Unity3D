using UnityEngine.Events;
using UnityEngine;
using ProjectEmbersteel.InteractionSystem;

namespace ProjectEmbersteel.Events.EventChannel
{
    /// <summary>
    /// This class is used for Events that have a IInteractable argument.
    /// Example: An event to toggle a interactable UI interface
    /// </summary>
    [CreateAssetMenu(fileName = "newIInteractableEvent", menuName = "Custom/Events/IInteractable Event Channel")]
    public class IInteractableEventChannelSO : DescriptionBaseSO
    {
        public event UnityAction<bool, IInteractable> OnEventRaised;

        public void RaiseEvent(bool value, IInteractable interactable) => OnEventRaised?.Invoke(value, interactable);
    }
}