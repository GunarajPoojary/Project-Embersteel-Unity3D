namespace ProjectEmbersteel.StateMachine
{
    /// <summary>
    /// Abstract base class for managing state transitions and delegating behavior to the current state.
    /// It provides a consistent interface for handling input, updates, collisions, and animation events.
    /// </summary>
    public abstract class StateMachine
    {
        protected IState _currentState;

        public void SwitchState(IState newState)
        {
            _currentState?.Exit();

            _currentState = newState;

            _currentState.Enter();
        }

        public void HandleInput() => _currentState?.HandleInput();

        public void UpdateState() => _currentState?.UpdateState();

        public void OnAnimationEnterEvent() => _currentState?.OnAnimationEnterEvent();
        public void OnAnimationExitEvent() => _currentState?.OnAnimationExitEvent();
        public void OnAnimationTransitionEvent() => _currentState?.OnAnimationTransitionEvent();
    }
}