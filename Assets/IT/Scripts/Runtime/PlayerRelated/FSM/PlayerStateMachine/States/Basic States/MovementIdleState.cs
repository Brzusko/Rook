using EasyCharacterMovement;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementIdleState: IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerStateMachineContext> _stateMachine;

        public MovementIdleState(IStateMachine<PlayerBaseStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _stateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            CharacterMovement characterMovement = context.CharacterMovement;
            
            context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

            float maxSpeed = movementStatsModule.MovementSpeed;
            
            characterMovement.SimpleMove(Vector3.zero, 
                maxSpeed, 
                movementStatsModule.Acceleration, 
                movementStatsModule.Deceleration, 
                movementStatsModule.Friction, 
                movementStatsModule.Drag, 
                new Vector3(0f, movementStatsModule.Gravity, 0f), 
                true, 
                deltaTime);
        }

        public void Enter()
        {
            _stateMachine.Context.PlayerAnimations.PlayAnimation(PlayerAnimationStateID.GROUNDED);
        }

        public void Exit()
        {
            
        }

        public void CheckStateChange(NetworkInput input)
        {
            CharacterMovement characterMovement = _stateMachine.Context.CharacterMovement;

            if (characterMovement.isGrounded && input.isJumpPressed)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.JUMPING);
                return;
            }
            
            if (characterMovement.wasGrounded && !characterMovement.isGrounded)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.FALLING);
                return;
            }
            
            if (input.MovementInput.sqrMagnitude == 0 && characterMovement.isGrounded)
                return;

            if (input.IsWalkingPressed)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.WALKING);
                return;
            }
            
            _stateMachine.ChangeBaseState(PlayerBaseStateID.SCUTTER);
        }
    }
}