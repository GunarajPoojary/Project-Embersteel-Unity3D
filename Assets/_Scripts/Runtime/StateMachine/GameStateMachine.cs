using System;

namespace ProjectEmbersteel.StateMachine
{
    /// <summary>
    /// Represents the finite state machine for the game.
    /// </summary>
    public class GameStateMachine
    {
        private GameState _currentState;
        private GameState _previousState;

        // Actions to invoke when entering a specific state
        public Action InventoryEnterAction;
        public Action DialogueEnterAction;
        public Action CombatEnterAction;
        public Action InteractEnterAction;
        public Action DropCollectEnterAction;

        // Actions to invoke when exiting a specific state
        public Action InventoryExitAction;
        public Action DialogueExitAction;
        public Action CombatExitAction;
        public Action InteractExitAction;
        public Action DropCollectExitAction;

        public void AddEnterActionCallbacks(
            Action inventoryEnterAction,
            Action dialogueEnterAction,
            Action combatEnterAction,
            Action dropCollectEnterAction,
            Action interactEnterAction)
        {
            InventoryEnterAction += inventoryEnterAction;
            DialogueEnterAction += dialogueEnterAction;
            CombatEnterAction += combatEnterAction;
            InteractEnterAction += interactEnterAction;
            DropCollectEnterAction += dropCollectEnterAction;
        }

        public void AddExitActionCallbacks(
            Action inventoryExitAction,
            Action dialogueExitAction,
            Action combatExitAction,
            Action dropCollectExitAction,
            Action interactExitAction)
        {
            InventoryExitAction += inventoryExitAction;
            DialogueExitAction += dialogueExitAction;
            CombatExitAction += combatExitAction;
            InteractExitAction += interactExitAction;
            DropCollectExitAction += dropCollectExitAction;
        }

        public void RemoveAllActionCallbacks()
        {
            InventoryEnterAction = null;
            DialogueEnterAction = null;
            CombatEnterAction = null;
            InteractEnterAction = null;

            InventoryExitAction = null;
            DialogueExitAction = null;
            CombatExitAction = null;
            InteractExitAction = null;
        }

        public void SwitchState(GameState newState)
        {
            if (_currentState == newState)
            {
                SwitchToPreviousState();
                return;
            }

            ExitState(_currentState);
            EnterState(newState);
        }

        public void EnterState(GameState newState)
        {
            switch (newState)
            {
                case GameState.Inventory:
                    InventoryEnterAction?.Invoke();
                    break;
                case GameState.DropCollect:
                    DropCollectEnterAction?.Invoke();
                    break;
                case GameState.Interact:
                    InteractEnterAction?.Invoke();
                    break;
                case GameState.Dialogue:
                    // DialogueEnterAction?.Invoke(); — uncomment if/when implemented
                    break;
                case GameState.Combat:
                    // CombatEnterAction?.Invoke(); — uncomment if/when implemented
                    break;
            }

            _currentState = newState;
        }

        public void ExitState(GameState currentState)
        {
            switch (currentState)
            {
                case GameState.Inventory:
                    InventoryExitAction?.Invoke();
                    break;
                case GameState.DropCollect:
                    DropCollectExitAction?.Invoke();
                    break;
                case GameState.Interact:
                    InteractExitAction?.Invoke();
                    break;
                case GameState.Dialogue:
                    // DialogueExitAction?.Invoke(); — uncomment if/when implemented
                    break;
                case GameState.Combat:
                    // CombatExitAction?.Invoke(); — uncomment if/when implemented
                    break;
            }

            _previousState = currentState;
        }

        private void SwitchToPreviousState()
        {
            ExitState(_currentState);
            EnterState(_previousState);
        }
    }
}