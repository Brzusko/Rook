using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementJumpIcspState : ICSPState<NetworkInput>
    {
        private readonly ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _icspStateMachine;

        public MovementJumpIcspState(ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> icspStateMachine)
        {
            _icspStateMachine = icspStateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _icspStateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            CharacterMovement characterMovement = context.CharacterMovement;

            context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

            if (input.IsJumpPressed && characterMovement.isGrounded)
            {
                characterMovement.PauseGroundConstraint();
                characterMovement.LaunchCharacter(Vector3.up * movementStatsModule.JumpForce);
            }
    
            float maxSpeed = movementStatsModule.MovementSpeed *  movementStatsModule.InAirControl;
            
            float acceleration = characterMovement.isGrounded
                ? movementStatsModule.Acceleration 
                : movementStatsModule.InAirAcceleration;
            
            float deceleration = characterMovement.isGrounded
                ? movementStatsModule.Deceleration
                : movementStatsModule.InAirDeceleration;
            
            float friction = characterMovement.isGrounded
                ? movementStatsModule.Friction
                : movementStatsModule.InAirFriction;
            
            float drag = characterMovement.isGrounded
                ? movementStatsModule.Drag
                : movementStatsModule.InAirDrag;

            Vector3 desiredVelocity = new Vector3(input.MovementInput.x, 0f, input.MovementInput.y) * maxSpeed;
            
            characterMovement.SimpleMove(desiredVelocity,
                maxSpeed,
                acceleration,
                deceleration,
                friction,
                drag,
                new Vector3(0f, movementStatsModule.Gravity, 0f),
                true,
                deltaTime);
        }

        public void Enter(bool onReconcile, bool asReplay = false)
        {
            if(asReplay)
                return;
            
            _icspStateMachine.Context.PlayerAnimations.PlayAnimation(PlayerMovementAnimID.JUMPING);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asReplay = false)
        {
            CharacterMovement characterMovement = _icspStateMachine.Context.CharacterMovement;
            Vector3 velocity = characterMovement.velocity;

            if (!characterMovement.isGrounded && velocity.y < 0)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.FALLING, onReconcile, asReplay);
                return;
            }

            if (characterMovement.isGrounded && input.MovementInput == default)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.IDLE, onReconcile, asReplay);
                return;
            }

            if (!characterMovement.isGrounded || input.MovementInput == default) return;
            
            if (input.IsWalkingPressed)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.WALKING, onReconcile, asReplay);
                return;
            }
                
            _icspStateMachine.ChangeBaseState(PlayerBaseStateID.SCUTTER, onReconcile, asReplay);

        }
    }
}
