using System;
using ProjectEmbersteel.Utilities.Inputs.ScriptableObjects;

namespace ProjectEmbersteel.InteractionSystem
{
    /// <summary>
    /// Controls player interaction with IInteractable objects.
    /// Manages input callbacks and triggers interaction logic.
    /// </summary>
    public class InteractionController
    {
        private readonly InputReader _inputReader;                 
        private IInteractable _currentInteractable;              
        private readonly Action _onInteract = default;            

        public InteractionController(InputReader inputReader, Action onInteract)
        {
            _onInteract = onInteract;
            _inputReader = inputReader;

            // Disable interaction input by default
            _inputReader.DisableActionFor(InputActionType.Interact);
        }

        /// <summary>
        /// Subscribes to the input event for interaction.
        /// </summary>
        public void AddInputActionCallback() => _inputReader.InteractPerformedAction += Interact;

        /// <summary>
        /// Unsubscribes from the input event for interaction.
        /// </summary>
        public void RemoveInputActionCallback() => _inputReader.InteractPerformedAction -= Interact;

        /// <summary>
        /// Sets the current interactable and enables the input action.
        /// </summary>
        /// <param name="interactable">The interactable object to track.</param>
        public void AddInteractable(IInteractable interactable)
        {
            _currentInteractable = interactable;
            _inputReader.EnableActionFor(InputActionType.Interact);
        }

        /// <summary>
        /// Clears the current interactable and disables the input action.
        /// </summary>
        /// <param name="interactable">The interactable to remove (if it's the current one).</param>
        public void RemoveInteractable(IInteractable interactable)
        {
            if (interactable == _currentInteractable)
            {
                _currentInteractable = null;
                _inputReader.DisableActionFor(InputActionType.Interact);
            }
        }

        // Handles the actual interaction when input is triggered if the player is within th radius.
        private void Interact()
        {
            if (_currentInteractable != null)
            {
                // Perform the interaction
                _currentInteractable.Interact();
                RemoveInteractable(_currentInteractable);

                _onInteract?.Invoke();
            }
        }
    }
}