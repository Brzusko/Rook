using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementJumpState : IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _stateMachine;

        public MovementJumpState(IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _stateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            CharacterMovement characterMovement = context.CharacterMovement;

            context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

            if (input.isJumpPressed && characterMovement.isGrounded)
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

        public void Enter()
        {
            PlayerAnimations playerAnimations = _stateMachine.Context.PlayerAnimations;
            playerAnimations.PlayAnimation(PlayerAnimationStateID.JUMPING);
        }

        public void Exit()
        {
            
        }

        public void CheckStateChange(NetworkInput input)
        {
            CharacterMovement characterMovement = _stateMachine.Context.CharacterMovement;
            Vector3 velocity = characterMovement.velocity;

            if (!characterMovement.isGrounded && velocity.y < 0)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.FALLING);
                return;
            }

            if (characterMovement.isGrounded && input.MovementInput == default)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.IDLE);
                return;
            }

            if (!characterMovement.isGrounded || input.MovementInput == default) return;
            
            if (input.IsWalkingPressed)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.WALKING);
                return;
            }
                
            _stateMachine.ChangeBaseState(PlayerBaseStateID.SCUTTER);

        }
    }
}
