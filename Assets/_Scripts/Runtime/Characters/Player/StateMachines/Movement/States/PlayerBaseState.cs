using ProjectEmbersteel.Player.Data.States.Airborne;
using ProjectEmbersteel.Player.Data.States.Grounded;
using ProjectEmbersteel.StateMachine;
using UnityEngine;

namespace ProjectEmbersteel.Player.StateMachines.Movement.States
{
    /// <summary>
    /// Base class for player movement-related states.
    /// </summary>
    public class PlayerBaseState : IState
    {
        protected PlayerStateMachine _stateMachine;

        private const float GRAVITY = -9.81f;

        protected readonly PlayerGroundedData _groundedData;
        protected readonly PlayerAirborneData _airborneData;

        public PlayerBaseState(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;

            _groundedData = _stateMachine.PlayerController.Data.GroundedData;
            _airborneData = _stateMachine.PlayerController.Data.AirborneData;
        }

        #region IState Methods
        public virtual void Enter() => AddInputActionsCallbacks();

        public virtual void Exit() => RemoveInputActionsCallbacks();

        public virtual void HandleInput() { }

        public virtual void UpdateState() { }

        public virtual void OnAnimationEnterEvent() { }
        public virtual void OnAnimationExitEvent() { }
        public virtual void OnAnimationTransitionEvent() { }
        #endregion

        #region Main Methods
        // Applies motion to the character controller based on current vertical and horizontal velocities.
        protected virtual void ApplyMotion()
        {
            Vector3 targetMotion = _stateMachine.ReusableData.CurrentVerticalVelocity * Vector3.up + _stateMachine.ReusableData.CurrentHorizontalVelocity;

            _stateMachine.PlayerController.Controller.Move(targetMotion * Time.deltaTime);
        }
        #endregion

        #region Reusable Methods
        protected virtual void AddInputActionsCallbacks()
        {
            _stateMachine.PlayerController.Input.RunPerformedAction += OnRunPerformed;
            _stateMachine.PlayerController.Input.RunCanceledAction += OnRunCanceled;

            _stateMachine.PlayerController.Input.MovePerformedAction += OnMovementPerformed;
            _stateMachine.PlayerController.Input.MoveCanceledAction += OnMovementCanceled;
        }

        protected virtual void RemoveInputActionsCallbacks()
        {
            _stateMachine.PlayerController.Input.RunPerformedAction -= OnRunPerformed;
            _stateMachine.PlayerController.Input.RunCanceledAction -= OnRunCanceled;

            _stateMachine.PlayerController.Input.MovePerformedAction -= OnMovementPerformed;
            _stateMachine.PlayerController.Input.MoveCanceledAction -= OnMovementCanceled;
        }

        protected void ResetVerticalVelocity() =>
            _stateMachine.ReusableData.CurrentVerticalVelocity = 0;

        protected void ResetHorizontalVelocity() =>
            _stateMachine.ReusableData.CurrentHorizontalVelocity = Vector3.zero;

        protected void ResetVelocity()
        {
            ResetVerticalVelocity();
            ResetHorizontalVelocity();
        }

        protected void StartAnimation(int animationHash) => _stateMachine.PlayerController.Animator.SetBool(animationHash, true);

        protected void StopAnimation(int animationHash) => _stateMachine.PlayerController.Animator.SetBool(animationHash, false);
        #endregion

        #region Input Methods
        protected virtual void OnRunPerformed() => _stateMachine.ReusableData.ShouldRun = true;
        protected virtual void OnRunCanceled() => _stateMachine.ReusableData.ShouldRun = false;

        protected virtual void OnMovementPerformed(Vector2 moveInput) { }
        protected virtual void OnMovementCanceled() => _stateMachine.ReusableData.MovementInput = Vector2.zero;
        #endregion
    }
}