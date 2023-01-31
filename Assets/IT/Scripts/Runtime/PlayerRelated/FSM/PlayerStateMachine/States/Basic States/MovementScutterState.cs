using System.Collections;
using System.Collections.Generic;
using IT.FSM;
using IT.Interfaces.FSM;
using UnityEngine;
using EasyCharacterMovement;
using IT.Data.Networking;

namespace IT.FSM.States
{
    public class MovementScutterState : IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _stateMachine;

        public MovementScutterState(IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> stateMachine)
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

            if (input.IsWalkingPressed && input.MovementInput.sqrMagnitude > 0f)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.WALKING);
                return;
            }

            if (input.MovementInput.sqrMagnitude == 0f)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.IDLE);
            }
        }
    }
}
