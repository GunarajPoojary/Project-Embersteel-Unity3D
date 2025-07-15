using System;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.Utilities.Inputs.ScriptableObjects;
using UnityEngine;

namespace ProjectEmbersteel.InteractionSystem
{
    /// <summary>
    /// Manages player interaction with IInteractable objects.
    /// Listens for input and interaction events and delegates interaction logic to the InteractionController.
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;

        [Header("Listener")]
        [SerializeField] private IInteractableEventChannelSO _triggerInteractableEvent;                          

        private InteractionController _controller; 

        public event Action OnInteract;

        private void Awake() => _controller = new(_inputReader, HandleInteract);

        private void OnEnable() => SubscribeToEvents(true);
        private void OnDisable() => SubscribeToEvents(false);

        private void SubscribeToEvents(bool subscribe)
        {
            if (subscribe)
            {
                _triggerInteractableEvent.OnEventRaised += HandleInteractableTrigger;
                _controller.AddInputActionCallback();
            }
            else
            {
                _triggerInteractableEvent.OnEventRaised -= HandleInteractableTrigger;
                _controller.RemoveInputActionCallback();
            }
        }

        // Adds or removes an IInteractable to the controller based on trigger event.
        private void HandleInteractableTrigger(bool toggle, IInteractable interactable)
        {
            if (toggle)
                _controller.AddInteractable(interactable);
            else
                _controller.RemoveInteractable(interactable);
        }

        // Handles the interact invoked by controller when input is pressed.
        private void HandleInteract() => OnInteract?.Invoke();
    }
}