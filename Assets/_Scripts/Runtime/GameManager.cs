using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.InteractionSystem;
using ProjectEmbersteel.Inventory;
using ProjectEmbersteel.StateMachine;
using ProjectEmbersteel.UI;
using ProjectEmbersteel.Utilities.Inputs.ScriptableObjects;
using UnityEngine;

namespace ProjectEmbersteel
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private UIManager _uIManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private InteractionManager _interactionManager;
        [SerializeField] private InputReader _input;

        [Header("Listeners")]
        [SerializeField] private VoidEventChannelSO _openInventoryMenuEvent;
        [SerializeField] private VoidEventChannelSO _closeInventoryMenuEvent;
        [SerializeField] private IInteractableEventChannelSO _triggerInteractableEvent;

        private GameStateMachine _gameStateMachine;

        private void Awake() => InitializeGameStateMachine();

        private void OnEnable()
        {
            _interactionManager.OnInteract += SwitchToDropCollectState;
            _uIManager.OnCloseDropsPreviewUI += HandleCloseDropsPreviewUI;

            _triggerInteractableEvent.OnEventRaised += SwitchToInteractState;

            _openInventoryMenuEvent.OnEventRaised += SwitchToInventoryState;
            _closeInventoryMenuEvent.OnEventRaised += SwitchToGameplayState;

            _gameStateMachine.AddEnterActionCallbacks(
                _input.DisablePlayerMovementActions,
                _input.DisablePlayerMovementActions,
                _input.DisablePlayerMovementActions,
                _input.DisablePlayerMovementActions,
                _input.DisablePlayerMovementActions);

            _gameStateMachine.AddExitActionCallbacks(
                InventoryExitAction,
                _input.EnablePlayerMovementActions,
                _input.EnablePlayerMovementActions,
                DropCollectExitAction,
                InteractExitAction);
        }

        private void SwitchToInteractState(bool arg0, IInteractable arg1)
        {
            Debug.Log("Switch To Interact");
        }

        private void OnDisable()
        {
            _interactionManager.OnInteract -= SwitchToDropCollectState;
            _uIManager.OnCloseDropsPreviewUI -= HandleCloseDropsPreviewUI;

            _openInventoryMenuEvent.OnEventRaised -= SwitchToInventoryState;
            _closeInventoryMenuEvent.OnEventRaised -= SwitchToGameplayState;

            _gameStateMachine.RemoveAllActionCallbacks();
        }

        private void InitializeGameStateMachine() => _gameStateMachine = new GameStateMachine();

        private void SwitchToInventoryState() => _gameStateMachine.SwitchState(GameState.Inventory);
        private void SwitchToGameplayState() => _gameStateMachine.SwitchState(GameState.Gameplay);
        private void SwitchToDropCollectState() => _gameStateMachine.SwitchState(GameState.DropCollect);

        private void HandleCloseDropsPreviewUI() => _gameStateMachine.SwitchState(GameState.Gameplay);

        private void InventoryExitAction()
        {
            _uIManager.CloseInventory();
            _input.EnablePlayerMovementActions();
        }

        private void DropCollectExitAction()
        {
            _uIManager.CloseDropsPreviewUI();
            _input.EnablePlayerMovementActions();
        }

        private void InteractExitAction()
        {
            _uIManager.CloseInteractionUI();
            _input.EnablePlayerMovementActions();
        }
    }
}