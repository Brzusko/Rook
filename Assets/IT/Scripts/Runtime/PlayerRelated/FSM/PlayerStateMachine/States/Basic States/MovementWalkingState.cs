using EasyCharacterMovement;
using IT.Data.Networking;
using IT.Input;
using IT.Interfaces;
using IT.Interfaces.FSM;
using IT.Utils;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementWalkingState: IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerStateMachineContext> _stateMachine;

        public MovementWalkingState(IStateMachine<PlayerBaseStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _stateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            CharacterMovement characterMovement = context.CharacterMovement;

            context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

            float maxSpeed = movementStatsModule.MovementSpeed * movementStatsModule.WalkingSpeedModifier;

            Vector3 desiredVelocity = new Vector3(input.MovementInput.x, 0f, input.MovementInput.y) * maxSpeed;
                                      
            characterMovement.SimpleMove(desiredVelocity, 
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
            
            if (!input.IsWalkingPressed && input.MovementInput.sqrMagnitude > 0f)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.SCUTTER);
                return;
            }
            
            if (input.MovementInput.sqrMagnitude == 0f)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.IDLE);
            }
        }
    }
}