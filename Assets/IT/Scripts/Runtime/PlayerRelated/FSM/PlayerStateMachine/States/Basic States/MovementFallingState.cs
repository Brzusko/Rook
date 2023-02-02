using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using IT.Data.Networking;
using IT.FSM;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementFallingState : IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _stateMachine;
        private float _inTheAir = 0f;
        
        public MovementFallingState(IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _stateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            CharacterMovement characterMovement = context.CharacterMovement;
                
            context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

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

            int layerMask = ~0;

            if (characterMovement.Raycast(characterMovement.GetFootPosition(), Vector3.down, 1f, layerMask,
                    out RaycastHit hit, 0.1f) && _inTheAir > 0.1f)
            {
                context.PlayerAnimations.PlayAnimation(PlayerAnimationStateID.LANDING);
            }
            
            characterMovement.SimpleMove(desiredVelocity,
                maxSpeed,
                acceleration,
                deceleration,
                friction,
                drag,
                new Vector3(0f, movementStatsModule.Gravity, 0f),
                true,
                deltaTime);

            _inTheAir += deltaTime;
        }

        public void Enter(bool onReconcile, bool asReplay = false)
        {
            _stateMachine.Context.PlayerAnimations.PlayAnimation(PlayerAnimationStateID.FALLING);
            _inTheAir = 0f;
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asReplay = false)
        {
            CharacterMovement characterMovement = _stateMachine.Context.CharacterMovement;

            if (characterMovement.wasGrounded || !characterMovement.isGrounded)
                return;

            if (input.MovementInput.sqrMagnitude == 0)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.IDLE, onReconcile, asReplay);
                return;
            }

            if (input.IsWalkingPressed)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.WALKING, onReconcile, asReplay);
                return;
            }
            
            _stateMachine.ChangeBaseState(PlayerBaseStateID.SCUTTER, onReconcile, asReplay);
        }
    }

}