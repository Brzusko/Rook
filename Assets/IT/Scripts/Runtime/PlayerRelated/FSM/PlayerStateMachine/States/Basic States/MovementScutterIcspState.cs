using System.Collections;
using System.Collections.Generic;
using IT.FSM;
using IT.Interfaces.FSM;
using UnityEngine;
using EasyCharacterMovement;
using IT.Data.Networking;

namespace IT.FSM.States
{
    public class MovementScutterIcspState : ICSPState<NetworkInput>
    {
        private readonly ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _icspStateMachine;

        public MovementScutterIcspState(ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> icspStateMachine)
        {
            _icspStateMachine = icspStateMachine;
        }

        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _icspStateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            CharacterMovement characterMovement = context.CharacterMovement;

            context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

            float maxSpeed = movementStatsModule.MovementSpeed * movementStatsModule.AdditionalSpeedModifiers;

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

        public void Enter(bool onReconcile, bool asServer, bool asReplay = false)
        {
            if(asReplay)
                return;
            
            _icspStateMachine.Context.PlayerAnimations.PlayAnimation(PlayerMovementAnimID.GROUNDED);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {

        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asServer, bool asReplay = false)
        {
            CharacterMovement characterMovement = _icspStateMachine.Context.CharacterMovement;
            
            if (characterMovement.isGrounded && input.IsJumpPressed)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.JUMPING,  onReconcile, asServer, asReplay);
                return;
            }
            
            if (characterMovement.wasGrounded && !characterMovement.isGrounded)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.FALLING, onReconcile, asServer, asReplay);
                return;
            }

            if (input.IsWalkingPressed && input.MovementInput.sqrMagnitude > 0f)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.WALKING,  onReconcile, asServer, asReplay);
                return;
            }

            if (input.MovementInput.sqrMagnitude == 0f)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.IDLE,  onReconcile, asServer, asReplay);
            }
        }
    }
}
