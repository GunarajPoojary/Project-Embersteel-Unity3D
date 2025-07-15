using System;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.InteractionSystem;
using ProjectEmbersteel.UI.Interaction;
using ProjectEmbersteel.UI.Inventory;
using ProjectEmbersteel.Utilities.Inputs.ScriptableObjects;
using UnityEngine;

namespace ProjectEmbersteel.UI
{
	/// <summary>
	/// Manages all UI panels in the game (Drops preview, interaction, inventory).
	/// Listens to input and interaction events to toggle appropriate UI elements.
	/// </summary>
	public class UIManager : MonoBehaviour
	{
		[Header(" UI omponents")]
		[SerializeField] private UIDropsPreview _uIDropsPreview;
		[SerializeField] private UIInteraction _uIInteraction;
		[SerializeField] private UIInventory _uIInventory;
		[SerializeField] private InputReader _input;

        [Header("Listener")]
		[SerializeField] private IInteractableEventChannelSO _triggerInteractableEvent;

		public event Action OnCloseDropsPreviewUI;

		private void OnEnable() => SubscribeToEvents(true);
		private void OnDisable() => SubscribeToEvents(false);

		public void CloseInventory() => _uIInventory.CloseInventoryUI();

		private void SubscribeToEvents(bool subscribe)
		{
			if (subscribe)
			{
				_triggerInteractableEvent.OnEventRaised += ToggleInteractionUI;
				_input.InteractPerformedAction += CloseInteractionUI;
				_uIDropsPreview.OnCloseDropsPreviewUI += CloseDropsPreview;
			}
			else
			{
				_triggerInteractableEvent.OnEventRaised -= ToggleInteractionUI;
				_input.InteractPerformedAction -= CloseInteractionUI;
				_uIDropsPreview.OnCloseDropsPreviewUI -= CloseDropsPreview;
			}
		}

		// Called when the drop preview UI is closed.
		private void CloseDropsPreview() => OnCloseDropsPreviewUI?.Invoke();

		// Hides the interaction UI.
		// Used when game state changed.
		public void CloseInteractionUI() => _uIInteraction.gameObject.SetActive(false);
		public void CloseDropsPreviewUI() => _uIDropsPreview.gameObject.SetActive(false);

		// Toggles the interaction UI visibility based on whether the player is near an interactable.
		private void ToggleInteractionUI(bool toggle, IInteractable interactable)
		{
			// Update the interaction type displayed on the UI
			_uIInteraction.SetInteractionType(interactable.InteractionType);

			// Show or hide the interaction panel
			_uIInteraction.gameObject.SetActive(toggle);
		}
	}
}