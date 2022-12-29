using IT.Input;
using IT.Interfaces.FSM;
using IT.Utils;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementIdleState: IState<NetworkedInput>
    {
        private IStateMachine<MovementStateID, MovementContext> _stateMachine;

        public MovementIdleState(IStateMachine<MovementStateID, MovementContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        public void Tick(NetworkedInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            Vector2 decodedInput = input.MovementInput;
            Vector3 currentVelocity = _stateMachine.Context.CharacterMovement.velocity;
            float maxSpeed = _stateMachine.Context.MaxSpeed * Constants.FRICTION;
            float acceleration = _stateMachine.Context.Acceleration * Constants.AIR_CONTROL;

            float xComponent = Mathf.MoveTowards(currentVelocity.x, decodedInput.x * maxSpeed, acceleration * deltaTime);
            float zComponent =
                Mathf.MoveTowards(currentVelocity.z, decodedInput.y * maxSpeed, acceleration * deltaTime);

            currentVelocity.x = xComponent;
            currentVelocity.z = zComponent;

            _stateMachine.Context.CharacterMovement.Move(currentVelocity, deltaTime);
        }

        public void Enter()
        {
            
        }

        public void Exit()
        {
            
        }

        public void CheckStateChange()
        {
            Vector3 velocity = _stateMachine.Context.CharacterMovement.velocity;

            if (velocity.sqrMagnitude > 0)
            {
                _stateMachine.ChangeState(MovementStateID.MOVING);
                return;
            }

        }
    }
}